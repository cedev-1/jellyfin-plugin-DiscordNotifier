using System;
using System.Globalization;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for PlaybackProgress notification.
/// </summary>
public static class PlaybackProgressTemplate
{
    /// <summary>
    /// Creates a Discord message for a playback progress update.
    /// </summary>
    /// <param name="eventArgs">The playback progress event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(PlaybackProgressEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var item = eventArgs.Item;
        var session = eventArgs.Session;
        var positionTicks = session?.PlayState?.PositionTicks;
        var runtimeTicks = item?.RunTimeTicks;

        var userName = DiscordText.Safe(session?.UserName);
        var authorName = DiscordText.Truncate(BuildAuthorName(item), 256);
        var authorUrl = item is not null ? $"{serverUrl}/web/#/details?id={item.Id}" : null;
        var progressBar = PlaybackUtils.BuildProgressBar(positionTicks ?? 0, runtimeTicks);

        var thumbnail = item is not null
            ? new DiscordEmbedMedia { Url = $"{serverUrl}/Items/{item.Id}/Images/Primary" }
            : null;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Author = new DiscordEmbedAuthor { Name = authorName, Url = authorUrl },
                    Description = $"**{userName}**\n\n{progressBar}",
                    Color = 0x9B59B6,
                    Thumbnail = thumbnail,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }

    private static string BuildAuthorName(MediaBrowser.Controller.Entities.BaseItem? item)
    {
        if (item is null)
        {
            return "Playback Progress";
        }

        if (item is Episode episode)
        {
            var season = episode.ParentIndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            var ep = episode.IndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            return $"Playback Progress • {episode.SeriesName} S{season}E{ep} ~ {episode.Name}";
        }

        return item.ProductionYear.HasValue
            ? $"Playback Progress • {item.Name} ({item.ProductionYear})"
            : $"Playback Progress • {item.Name}";
    }
}
