using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message at a configured interval during playback.
/// Seeds the throttle cache on the first progress event so the initial duplicate
/// (which fires ~1s after PlaybackStart) is suppressed. Also skips paused sessions.
/// </summary>
public class PlaybackProgressNotifier : IEventConsumer<PlaybackProgressEventArgs>
{
    private static readonly ConcurrentDictionary<string, DateTime> _lastNotified = new();
    private static readonly TimeSpan CleanupThreshold = TimeSpan.FromHours(2);

    private readonly DiscordSender _sender;
    private readonly ILogger<PlaybackProgressNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackProgressNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PlaybackProgressNotifier(DiscordSender sender, ILogger<PlaybackProgressNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PlaybackProgressEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PlaybackProgressNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        if (eventArgs.Item is null || eventArgs.Item.IsThemeMedia)
        {
            return;
        }

        // Skip paused sessions — no point reporting progress when nothing is moving.
        if (eventArgs.Session?.PlayState?.IsPaused == true)
        {
            return;
        }

        var itemType = eventArgs.Item.GetType().Name;
        var isMovie = itemType == "Movie";
        var isEpisode = itemType == "Episode";

        if (!isMovie && !isEpisode)
        {
            return;
        }

        if (isMovie && !config.PlaybackNotifyMovies)
        {
            return;
        }

        if (isEpisode && !config.PlaybackNotifySeries)
        {
            return;
        }

        if (config.PlaybackTranscodeOnlyNotifier)
        {
            var playMethod = eventArgs.Session?.PlayState?.PlayMethod;
            if (playMethod is not (PlayMethod.Transcode or PlayMethod.DirectStream))
            {
                return;
            }
        }

        var sessionId = eventArgs.Session?.Id ?? string.Empty;
        var interval = TimeSpan.FromMinutes(config.PlaybackProgressIntervalMinutes > 0 ? config.PlaybackProgressIntervalMinutes : 10);

        CleanupOldEntries();

        if (_lastNotified.TryGetValue(sessionId, out var lastTime) &&
            DateTime.UtcNow - lastTime < interval)
        {
            return;
        }

        // First progress event for this session: seed the cache without sending.
        // This suppresses the initial duplicate that fires ~1s after PlaybackStart.
        if (!_lastNotified.ContainsKey(sessionId))
        {
            _lastNotified[sessionId] = DateTime.UtcNow;
            return;
        }

        try
        {
            _lastNotified[sessionId] = DateTime.UtcNow;

            _logger.LogInformation(
                "PlaybackProgressNotifier: {UserName} is watching {ItemName}",
                eventArgs.Session?.UserName ?? "Unknown",
                eventArgs.Item.Name);

            var message = PlaybackProgressTemplate.CreateMessage(eventArgs, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PlaybackProgressNotifier: Error sending notification");
        }
    }

    private static void CleanupOldEntries()
    {
        var threshold = DateTime.UtcNow - CleanupThreshold;
        foreach (var key in _lastNotified.Where(kvp => kvp.Value < threshold).Select(kvp => kvp.Key).ToList())
        {
            _lastNotified.TryRemove(key, out _);
        }
    }
}
