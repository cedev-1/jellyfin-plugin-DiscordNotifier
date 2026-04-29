using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a playback session starts.
/// </summary>
public class PlaybackStartNotifier : IEventConsumer<PlaybackStartEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PlaybackStartNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackStartNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PlaybackStartNotifier(DiscordSender sender, ILogger<PlaybackStartNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PlaybackStartEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PlaybackStartNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
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

        var playMethod = eventArgs.Session?.PlayState?.PlayMethod;
        var isTranscoding = playMethod == PlayMethod.Transcode;

        if (config.PlaybackDetailedMode && config.PlaybackTranscodeOnlyNotifier && !isTranscoding)
        {
            return;
        }

        _logger.LogInformation(
            "PlaybackStartNotifier: {UserName} started playback of {ItemName} via {PlayMethod}",
            eventArgs.Session?.UserName ?? "Unknown",
            eventArgs.Item.Name,
            playMethod?.ToString() ?? "Unknown");

        var message = PlaybackStartTemplate.CreateMessage(eventArgs, config);
        var json = JsonSerializer.Serialize(message);
        await _sender.SendPostToWebhook(config.WebhookUrl, json).ConfigureAwait(false);
    }
}
