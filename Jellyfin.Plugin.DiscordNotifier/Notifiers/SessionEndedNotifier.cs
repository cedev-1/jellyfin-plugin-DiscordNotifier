using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a client session ends.
/// Includes deduplication to avoid spam from websocket reconnects and mobile backgrounding.
/// </summary>
public class SessionEndedNotifier : IEventConsumer<SessionEndedEventArgs>
{
    private static readonly ConcurrentDictionary<string, DateTime> _recentSessions = new();
    private static readonly TimeSpan DeduplicateThreshold = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan CleanupThreshold = TimeSpan.FromMinutes(10);

    private readonly DiscordSender _sender;
    private readonly ILogger<SessionEndedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionEndedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public SessionEndedNotifier(DiscordSender sender, ILogger<SessionEndedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(SessionEndedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.SessionEndedNotifier || !config.HasWebhookFor(NotifierEvent.SessionEnded))
        {
            return;
        }

        if (eventArgs.Argument is null)
        {
            return;
        }

        CleanupOldEntries();

        var sessionKey = eventArgs.Argument.Id;
        if (_recentSessions.TryGetValue(sessionKey, out var lastTime) &&
            DateTime.UtcNow - lastTime < DeduplicateThreshold)
        {
            return;
        }

        try
        {
            _recentSessions[sessionKey] = DateTime.UtcNow;

            _logger.LogInformation(
                "SessionEndedNotifier: {UserName} ended a session on {Device}",
                eventArgs.Argument.UserName ?? "Unknown",
                eventArgs.Argument.DeviceName ?? "Unknown");

            var message = SessionEndedTemplate.CreateMessage(eventArgs.Argument, config);
            await _sender.SendAsync(NotifierEvent.SessionEnded, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SessionEndedNotifier: Error sending notification");
        }
    }

    private static void CleanupOldEntries()
    {
        var threshold = DateTime.UtcNow - CleanupThreshold;
        foreach (var key in _recentSessions.Where(kvp => kvp.Value < threshold).Select(kvp => kvp.Key).ToList())
        {
            _recentSessions.TryRemove(key, out _);
        }
    }
}
