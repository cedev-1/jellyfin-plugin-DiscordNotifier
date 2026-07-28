using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when library items are deleted.
/// Batches deletions over a debounce window to avoid flooding Discord when
/// many items are removed at once (e.g. unmounted drive, library cleanup).
/// </summary>
public class ItemDeletedNotifier : IHostedService
{
    private const int BurstCap = 25;
    private static readonly TimeSpan InterSendDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(10);

    private readonly ILibraryManager _libraryManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<ItemDeletedNotifier> _logger;

    private readonly ConcurrentDictionary<Guid, BaseItem> _queue = new();
    private DateTime _lastEnqueue = DateTime.MinValue;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDeletedNotifier"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemDeletedNotifier(ILibraryManager libraryManager, DiscordSender sender, ILogger<ItemDeletedNotifier> logger)
    {
        _libraryManager = libraryManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemRemoved += OnItemRemoved;
        _cts = new CancellationTokenSource();
        _ = ProcessLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemRemoved -= OnItemRemoved;
        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();
        }
    }

    private void OnItemRemoved(object? sender, ItemChangeEventArgs e)
    {
        var item = e.Item;

        // Allow Series and MusicAlbum through (they inherit from Folder).
        if (item.IsVirtualItem || (item is Folder && item is not Series && item is not MusicAlbum))
        {
            return;
        }

        var typeName = item.GetType().Name;
        if (typeName is not ("Movie" or "Episode" or "Series" or "Audio" or "MusicAlbum" or "Book"))
        {
            return;
        }

        _queue.TryAdd(item.Id, item);
        _lastEnqueue = DateTime.UtcNow;
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (_queue.IsEmpty || DateTime.UtcNow - _lastEnqueue < DebounceWindow)
                    {
                        continue;
                    }

                    await FlushAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ItemDeletedNotifier: Flush iteration failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task FlushAsync()
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.ItemDeletedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _queue.Clear();
            return;
        }

        var items = _queue.Values.ToList();
        _queue.Clear();

        int sent = 0;
        foreach (var item in items)
        {
            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemDeletedNotifier: Burst cap reached, {Remaining} deletions not notified", items.Count - sent);
                break;
            }

            try
            {
                _logger.LogInformation("ItemDeletedNotifier: {ItemName} ({ItemType}) removed from library", item.Name, item.GetType().Name);

                var message = ItemDeletedTemplate.CreateMessage(item, config);
                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;

                if (sent < items.Count)
                {
                    await Task.Delay(InterSendDelay).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemDeletedNotifier: Error processing notification for {ItemName}", item.Name);
            }
        }
    }
}
