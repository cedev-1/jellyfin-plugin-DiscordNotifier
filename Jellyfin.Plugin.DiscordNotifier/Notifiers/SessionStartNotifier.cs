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
/// Notifier that sends a Discord message when a new client session starts.
/// </summary>
public class SessionStartNotifier : IEventConsumer<SessionStartedEventArgs>
{
    private static readonly ConcurrentDictionary<string, DateTime> _recentSessions = new();
    private static readonly TimeSpan DeduplicateThreshold = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CleanupThreshold = TimeSpan.FromMinutes(10);

    private readonly DiscordSender _sender;
    private readonly ILogger<SessionStartNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionStartNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public SessionStartNotifier(DiscordSender sender, ILogger<SessionStartNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(SessionStartedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.SessionStartNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
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
                "SessionStartNotifier: {UserName} started a session on {Device}",
                eventArgs.Argument.UserName ?? "Unknown",
                eventArgs.Argument.DeviceName ?? "Unknown");

            var message = SessionStartTemplate.CreateMessage(eventArgs.Argument, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SessionStartNotifier: Error sending notification");
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
