using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
/// Episodes from the same series (and tracks from the same album) are batched
/// into a single notification. A batch is flushed after a configurable quiet
/// window with a hard upper bound, similar to <see cref="ItemAddedNotifier"/>.
/// </summary>
public class ItemDeletedNotifier : IHostedService
{
    private const int BurstCap = 25;
    private static readonly TimeSpan InterSendDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan MaxWait = TimeSpan.FromMinutes(5);

    private static readonly HashSet<string> NotifiableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Movie", "Episode", "Series", "Audio", "MusicAlbum", "Book"
    };

    private readonly ILibraryManager _libraryManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<ItemDeletedNotifier> _logger;

    // itemId → (item, groupKey). The group key clusters episodes by series and tracks by album.
    private readonly ConcurrentDictionary<Guid, (BaseItem Item, Guid GroupKey)> _queue = new();

    // groupKey → last activity timestamp
    private readonly ConcurrentDictionary<Guid, DateTime> _lastActivity = new();

    // groupKey → timestamp the group was first seen (used for the max-wait upper bound)
    private readonly ConcurrentDictionary<Guid, DateTime> _firstSeen = new();

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

        if (!NotifiableTypes.Contains(item.GetType().Name))
        {
            return;
        }

        var groupKey = ResolveGroupKey(item);

        _queue.TryAdd(item.Id, (item, groupKey));

        var now = DateTime.UtcNow;
        _firstSeen.TryAdd(groupKey, now);
        _lastActivity[groupKey] = now;

        _logger.LogDebug("ItemDeletedNotifier: Queued {ItemName} ({ItemType}) with group {GroupKey}", item.Name, item.GetType().Name, groupKey);
    }

    private static Guid ResolveGroupKey(BaseItem item)
    {
        return item switch
        {
            Episode ep when ep.SeriesId != Guid.Empty => ep.SeriesId,
            Episode ep => ep.ParentId != Guid.Empty ? ep.ParentId : ep.Id,
            Audio audio => GetAlbumKey(audio),
            _ => item.Id
        };
    }

    private static Guid GetAlbumKey(Audio audio)
    {
        var albumId = audio.AlbumEntity?.Id ?? Guid.Empty;
        if (albumId != Guid.Empty)
        {
            return albumId;
        }

        return audio.ParentId != Guid.Empty ? audio.ParentId : audio.Id;
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
                    if (_queue.IsEmpty)
                    {
                        continue;
                    }

                    await FlushReadyGroupsAsync().ConfigureAwait(false);
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

    private async Task FlushReadyGroupsAsync()
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.ItemDeletedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _queue.Clear();
            _lastActivity.Clear();
            _firstSeen.Clear();
            return;
        }

        var now = DateTime.UtcNow;
        int sent = 0;

        // --- Episodes: group by series ---
        var episodeGroups = _queue.Values
            .Where(e => e.Item is Episode)
            .GroupBy(e => e.GroupKey)
            .ToList();

        foreach (var group in episodeGroups)
        {
            if (!ShouldFlushGroup(group.Key, now))
            {
                continue;
            }

            var episodeList = new List<Episode>();
            foreach (var entry in group)
            {
                if (_queue.TryRemove(entry.Item.Id, out _))
                {
                    episodeList.Add((Episode)entry.Item);
                }
            }

            // Absorb a standalone Series item that shares the group key.
            bool seriesDeleted = _queue.TryRemove(group.Key, out _);

            CleanupGroupMaps(group.Key);

            if (episodeList.Count == 0)
            {
                continue;
            }

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemDeletedNotifier: Burst cap reached, {Remaining} deletions not notified", episodeList.Count);
                break;
            }

            _logger.LogInformation(
                "ItemDeletedNotifier: Sending notification for {Count} deleted episode(s) (seriesDeleted={SeriesDeleted})",
                episodeList.Count,
                seriesDeleted);

            try
            {
                var message = episodeList.Count == 1 && !seriesDeleted
                    ? ItemDeletedTemplate.CreateMessage(episodeList[0], config)
                    : ItemDeletedTemplate.CreateEpisodeGroupMessage(episodeList, config, seriesDeleted);

                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemDeletedNotifier: Error sending episode group notification");
            }
        }

        // --- Audio: group by album ---
        var trackGroups = _queue.Values
            .Where(e => e.Item is Audio)
            .GroupBy(e => e.GroupKey)
            .ToList();

        foreach (var group in trackGroups)
        {
            if (!ShouldFlushGroup(group.Key, now))
            {
                continue;
            }

            var trackList = new List<Audio>();
            foreach (var entry in group)
            {
                if (_queue.TryRemove(entry.Item.Id, out _))
                {
                    trackList.Add((Audio)entry.Item);
                }
            }

            CleanupGroupMaps(group.Key);

            if (trackList.Count == 0)
            {
                continue;
            }

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemDeletedNotifier: Burst cap reached, skipping remaining notifications");
                break;
            }

            _logger.LogInformation(
                "ItemDeletedNotifier: Sending notification for {Count} deleted track(s)",
                trackList.Count);

            try
            {
                var message = trackList.Count == 1
                    ? ItemDeletedTemplate.CreateMessage(trackList[0], config)
                    : ItemDeletedTemplate.CreateTrackGroupMessage(trackList, config);

                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemDeletedNotifier: Error sending track group notification");
            }
        }

        // --- Other items (movies, books, standalone series/albums) ---
        var others = _queue.Values.Where(e => e.Item is not Episode && e.Item is not Audio).ToList();
        foreach (var entry in others)
        {
            if (!ShouldFlushGroup(entry.GroupKey, now))
            {
                continue;
            }

            if (!_queue.TryRemove(entry.Item.Id, out _))
            {
                continue;
            }

            CleanupGroupMaps(entry.GroupKey);

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemDeletedNotifier: Burst cap reached, skipping remaining notifications");
                break;
            }

            try
            {
                var message = ItemDeletedTemplate.CreateMessage(entry.Item, config);
                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemDeletedNotifier: Error processing notification for {ItemName}", entry.Item.Name);
            }
        }
    }

    private void CleanupGroupMaps(Guid groupKey)
    {
        if (!_queue.Values.Any(e => e.GroupKey == groupKey))
        {
            _lastActivity.TryRemove(groupKey, out _);
            _firstSeen.TryRemove(groupKey, out _);
        }
    }

    private bool ShouldFlushGroup(Guid groupKey, DateTime now)
    {
        if (!_lastActivity.TryGetValue(groupKey, out var last))
        {
            return true;
        }

        if (now - last >= DebounceWindow)
        {
            return true;
        }

        if (_firstSeen.TryGetValue(groupKey, out var first) && now - first >= MaxWait)
        {
            return true;
        }

        return false;
    }
}
