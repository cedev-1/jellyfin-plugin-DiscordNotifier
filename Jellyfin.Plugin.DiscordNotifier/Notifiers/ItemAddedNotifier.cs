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
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when new content is added to the library.
/// Episodes from the same series (and tracks from the same album) are batched into a
/// single notification. A batch is flushed when the library scan that produced it
/// completes; for additions made outside a scan (real-time folder monitoring) it is
/// flushed after a configurable quiet window, with a hard upper bound so a continuous
/// trickle can never delay the notification indefinitely.
/// </summary>
public class ItemAddedNotifier : IHostedService
{
    private const int BurstCap = 25;
    private static readonly TimeSpan InterSendDelay = TimeSpan.FromMilliseconds(250);

    private static readonly HashSet<string> NotifiableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Movie", "Episode", "Series", "Audio", "MusicAlbum", "Book"
    };

    private readonly ILibraryManager _libraryManager;
    private readonly ITaskManager _taskManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<ItemAddedNotifier> _logger;

    // itemId → (item, groupKey). The group key is captured once at enqueue time
    // so it is stable even if metadata providers populate SeriesId/AlbumEntity later.
    private readonly ConcurrentDictionary<Guid, (BaseItem Item, Guid GroupKey)> _queue = new();

    // groupKey → last activity timestamp
    private readonly ConcurrentDictionary<Guid, DateTime> _lastActivity = new();

    // groupKey → timestamp the group was first seen (used for the max-wait upper bound)
    private readonly ConcurrentDictionary<Guid, DateTime> _firstSeen = new();

    // Serializes flushes triggered by the periodic loop and by scan completion.
    private readonly SemaphoreSlim _flushLock = new(1, 1);

    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAddedNotifier"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="taskManager">The scheduled task manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemAddedNotifier(ILibraryManager libraryManager, ITaskManager taskManager, DiscordSender sender, ILogger<ItemAddedNotifier> logger)
    {
        _libraryManager = libraryManager;
        _taskManager = taskManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded += OnItemAdded;
        _taskManager.TaskCompleted += OnTaskCompleted;
        _cts = new CancellationTokenSource();
        _ = ProcessLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemAdded -= OnItemAdded;
        _taskManager.TaskCompleted -= OnTaskCompleted;
        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();
        }
    }

    private void OnItemAdded(object? sender, ItemChangeEventArgs e)
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

        if (!IsTypeEnabled(item, Plugin.Config))
        {
            return;
        }

        // Compute and store the group key once. At ItemAdded time, Episode.SeriesId
        // may still be Guid.Empty (metadata providers haven't run yet), so we fall
        // back to the parent folder id which is already set.
        var groupKey = ResolveGroupKey(item);

        _queue.TryAdd(item.Id, (item, groupKey));

        var now = DateTime.UtcNow;
        _firstSeen.TryAdd(groupKey, now);
        _lastActivity[groupKey] = now;

        _logger.LogDebug("ItemAddedNotifier: Queued {ItemName} ({ItemType}) with group {GroupKey}", item.Name, item.GetType().Name, groupKey);
    }

    // Fired when any scheduled task completes. A finished library scan means every item
    // it discovered is already queued, so the whole batch can be flushed at once.
    private void OnTaskCompleted(object? sender, TaskCompletionEventArgs e)
    {
        if (!string.Equals(e.Task?.ScheduledTask?.Key, "RefreshLibrary", StringComparison.Ordinal))
        {
            return;
        }

        if (_queue.IsEmpty)
        {
            return;
        }

        _logger.LogDebug("ItemAddedNotifier: Library scan completed, flushing queued groups");
        _ = SafeFlushAsync(force: true);
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

    // For a track, group by its parent album so a whole album added at once
    // produces a single notification instead of one per track.
    private static Guid GetAlbumKey(Audio audio)
    {
        var albumId = audio.AlbumEntity?.Id ?? Guid.Empty;
        if (albumId != Guid.Empty)
        {
            return albumId;
        }

        return audio.ParentId != Guid.Empty ? audio.ParentId : audio.Id;
    }

    // Per-media-type opt-out for new-content notifications.
    private static bool IsTypeEnabled(BaseItem item, Configuration.PluginConfiguration config)
    {
        return item.GetType().Name switch
        {
            "Movie" => config.ItemAddedNotifyMovies,
            "Episode" or "Series" => config.ItemAddedNotifySeries,
            "Audio" or "MusicAlbum" => config.ItemAddedNotifyMusic,
            "Book" => config.ItemAddedNotifyBooks,
            _ => true
        };
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        // Check every 10 seconds — actual sending is gated by the flush rules.
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    await FlushReadyGroupsAsync(force: false).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ItemAddedNotifier: Flush iteration failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task SafeFlushAsync(bool force)
    {
        try
        {
            await FlushReadyGroupsAsync(force).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ItemAddedNotifier: Flush failed");
        }
    }

    private async Task FlushReadyGroupsAsync(bool force)
    {
        if (_queue.IsEmpty)
        {
            return;
        }

        await _flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await FlushInternalAsync(force).ConfigureAwait(false);
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private async Task FlushInternalAsync(bool force)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.ItemAddedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _queue.Clear();
            _lastActivity.Clear();
            _firstSeen.Clear();
            return;
        }

        var now = DateTime.UtcNow;
        int sent = 0;

        // --- Episodes: group by stored group key, flush ready series ---
        var episodeGroups = _queue.Values
            .Where(e => e.Item is Episode)
            .GroupBy(e => e.GroupKey)
            .ToList();

        foreach (var group in episodeGroups)
        {
            var groupKey = group.Key;

            if (!ShouldFlushGroup(groupKey, now, force, config))
            {
                continue;
            }

            // Remove these episodes from the queue.
            var episodeList = new List<Episode>();
            foreach (var entry in group)
            {
                if (_queue.TryRemove(entry.Item.Id, out var queued))
                {
                    var updated = (_libraryManager.GetItemById(queued.Item.Id) ?? queued.Item) as Episode ?? (Episode)entry.Item;
                    episodeList.Add(updated);
                }
            }

            // A Series item shares its Id with its episodes' SeriesId. When a brand-new
            // series is added alongside its episodes, absorb that standalone Series item
            // here so the whole series produces a single notification instead of one for
            // the series plus one for its episodes.
            bool isNewSeries = _queue.TryRemove(groupKey, out _);

            CleanupGroupMaps(groupKey);

            if (episodeList.Count == 0)
            {
                continue;
            }

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemAddedNotifier: Burst cap reached, skipping remaining notifications");
                break;
            }

            _logger.LogInformation(
                "ItemAddedNotifier: Sending notification for {Count} episode(s) (newSeries={IsNewSeries})",
                episodeList.Count,
                isNewSeries);

            try
            {
                var message = episodeList.Count == 1 && !isNewSeries
                    ? ItemAddedTemplate.CreateMessage(episodeList[0], config)
                    : ItemAddedTemplate.CreateEpisodeGroupMessage(episodeList, config, isNewSeries);

                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemAddedNotifier: Error sending episode group notification");
            }
        }

        // --- Audio: group by stored group key, flush ready albums ---
        var trackGroups = _queue.Values
            .Where(e => e.Item is Audio)
            .GroupBy(e => e.GroupKey)
            .ToList();

        foreach (var group in trackGroups)
        {
            var groupKey = group.Key;

            if (!ShouldFlushGroup(groupKey, now, force, config))
            {
                continue;
            }

            var trackList = new List<Audio>();
            foreach (var entry in group)
            {
                if (_queue.TryRemove(entry.Item.Id, out var queued))
                {
                    var updated = (_libraryManager.GetItemById(queued.Item.Id) ?? queued.Item) as Audio ?? (Audio)entry.Item;
                    trackList.Add(updated);
                }
            }

            CleanupGroupMaps(groupKey);

            if (trackList.Count == 0)
            {
                continue;
            }

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemAddedNotifier: Burst cap reached, skipping remaining notifications");
                break;
            }

            _logger.LogInformation(
                "ItemAddedNotifier: Sending notification for {Count} track(s) of album {AlbumKey}",
                trackList.Count,
                groupKey);

            try
            {
                var message = trackList.Count == 1
                    ? ItemAddedTemplate.CreateMessage(trackList[0], config)
                    : ItemAddedTemplate.CreateTrackGroupMessage(trackList, config);

                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemAddedNotifier: Error sending track group notification");
            }
        }

        // --- Other items (movies, books, series, albums without tracks): each item is
        // its own group, so flush individually once ready. Like episodes/albums they wait
        // for scan completion or the quiet window — this ensures metadata (overview,
        // images) has been fetched before the notification is sent. ---
        var others = _queue.Values.Where(e => e.Item is not Episode && e.Item is not Audio).ToList();
        foreach (var entry in others)
        {
            if (!ShouldFlushGroup(entry.GroupKey, now, force, config))
            {
                continue;
            }

            if (!_queue.TryRemove(entry.Item.Id, out var queued))
            {
                continue;
            }

            CleanupGroupMaps(entry.GroupKey);

            if (sent >= BurstCap)
            {
                _logger.LogWarning("ItemAddedNotifier: Burst cap reached, skipping remaining notifications");
                break;
            }

            var updated = _libraryManager.GetItemById(queued.Item.Id) ?? queued.Item;

            try
            {
                var message = ItemAddedTemplate.CreateMessage(updated, config);
                await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                sent++;
                await Task.Delay(InterSendDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemAddedNotifier: Error processing notification for {ItemName}", updated?.Name);
            }
        }
    }

    // Only remove the activity maps if no queued item still uses this group key.
    private void CleanupGroupMaps(Guid groupKey)
    {
        if (!_queue.Values.Any(e => e.GroupKey == groupKey))
        {
            _lastActivity.TryRemove(groupKey, out _);
            _firstSeen.TryRemove(groupKey, out _);
        }
    }

    // Decides whether a grouped set (series/album) is ready to be sent.
    private bool ShouldFlushGroup(Guid groupKey, DateTime now, bool force, Configuration.PluginConfiguration config)
    {
        // A scan just finished — send the whole batch now.
        if (force)
        {
            return true;
        }

        if (!_lastActivity.TryGetValue(groupKey, out var last))
        {
            // Orphaned group key with no activity record — flush rather than leak.
            return true;
        }

        // While a scan is running, hold the group so the entire import lands in a
        // single notification; it is flushed when the scan completes.
        if (_libraryManager.IsScanRunning)
        {
            return false;
        }

        // No scan involved (real-time monitor / manual copy): send once the group has
        // been quiet for the configured window...
        var quietWindow = TimeSpan.FromMinutes(config.ItemAddedGroupingDelayMinutes);
        if (now - last >= quietWindow)
        {
            return true;
        }

        // ...or once it has waited longer than the hard upper bound.
        var maxWait = TimeSpan.FromMinutes(config.ItemAddedMaxWaitMinutes);
        if (_firstSeen.TryGetValue(groupKey, out var first) && now - first >= maxWait)
        {
            return true;
        }

        return false;
    }
}
