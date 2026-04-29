using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when new content is added to the library.
/// </summary>
public class ItemAddedNotifier : IHostedService
{
    private static readonly HashSet<string> NotifiableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Movie", "Episode", "Series", "Audio", "MusicAlbum", "Book"
    };

    private readonly ILibraryManager _libraryManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<ItemAddedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAddedNotifier"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemAddedNotifier(ILibraryManager libraryManager, DiscordSender sender, ILogger<ItemAddedNotifier> logger)
    {
        _libraryManager = libraryManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded += OnItemAdded;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded -= OnItemAdded;
        return Task.CompletedTask;
    }

    private void OnItemAdded(object? sender, ItemChangeEventArgs e)
    {
        _ = ProcessItemAsync(e.Item);
    }

    private async Task ProcessItemAsync(BaseItem item)
    {
        try
        {
            var config = Plugin.Config;
            if (!config.EnablePlugin || !config.ItemAddedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
            {
                return;
            }

            if (item.IsVirtualItem || item is Folder)
            {
                return;
            }

            if (!NotifiableTypes.Contains(item.GetType().Name))
            {
                return;
            }

            _logger.LogInformation("ItemAddedNotifier: Queued {ItemName} ({ItemType})", item.Name, item.GetType().Name);

            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            var updatedItem = _libraryManager.GetItemById(item.Id) ?? item;

            var message = ItemAddedTemplate.CreateMessage(updatedItem, config);
            var json = JsonSerializer.Serialize(message);
            await _sender.SendPostToWebhook(config.WebhookUrl, json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ItemAddedNotifier: Error processing notification for {ItemName}", item.Name);
        }
    }
}
