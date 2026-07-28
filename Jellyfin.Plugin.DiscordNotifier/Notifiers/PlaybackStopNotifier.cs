using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a playback session stops.
/// </summary>
public class PlaybackStopNotifier : IEventConsumer<PlaybackStopEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PlaybackStopNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackStopNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PlaybackStopNotifier(DiscordSender sender, ILogger<PlaybackStopNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PlaybackStopEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PlaybackStopNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        if (eventArgs.Item is null || eventArgs.Item.IsThemeMedia)
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

        try
        {
            _logger.LogInformation(
                "PlaybackStopNotifier: {UserName} stopped playback of {ItemName} (completed: {Completed})",
                eventArgs.Session?.UserName ?? "Unknown",
                eventArgs.Item.Name,
                eventArgs.PlayedToCompletion);

            var message = PlaybackStopTemplate.CreateMessage(eventArgs, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PlaybackStopNotifier: Error sending notification");
        }
    }
}
