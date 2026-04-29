using System;
using System.Globalization;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for PlaybackStop notification.
/// </summary>
public static class PlaybackStopTemplate
{
    /// <summary>
    /// Creates a Discord message when a playback session stops.
    /// </summary>
    /// <param name="eventArgs">The playback stop event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(PlaybackStopEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var session = eventArgs.Session;
        var item = eventArgs.Item;
        var positionTicks = eventArgs.PlaybackPositionTicks;
        var runtimeTicks = item?.RunTimeTicks;

        var userName = session?.UserName ?? "Unknown";

        var percent = positionTicks.HasValue && runtimeTicks.HasValue && runtimeTicks.Value > 0
            ? (double)positionTicks.Value / runtimeTicks.Value
            : 0;

        var finished = eventArgs.PlayedToCompletion || percent >= 0.9;
        var action = finished ? "Finished Watching" : "Playback Stopped";

        var authorName = BuildAuthorName(action, item);
        var authorUrl = item is not null ? $"{serverUrl}/web/index.html#!/details?id={item.Id}" : null;

        var overview = item?.Overview;
        var progressBar = PlaybackUtils.BuildProgressBar(positionTicks ?? 0, runtimeTicks);
        var description = BuildDescription(userName, overview, progressBar, finished);

        var thumbnail = item is not null
            ? new DiscordEmbedMedia { Url = $"{serverUrl}/Items/{item.Id}/Images/Primary" }
            : null;

        var color = finished ? 0x2ECC71 : 0x95A5A6;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Author = new DiscordEmbedAuthor { Name = authorName, Url = authorUrl },
                    Description = description,
                    Color = color,
                    Thumbnail = thumbnail,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }

    private static string BuildAuthorName(string action, MediaBrowser.Controller.Entities.BaseItem? item)
    {
        if (item is null)
        {
            return action;
        }

        if (item is Episode episode)
        {
            var season = episode.ParentIndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            var ep = episode.IndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            return $"{action} • {episode.SeriesName} S{season}E{ep} ~ {episode.Name}";
        }

        return item.ProductionYear.HasValue
            ? $"{action} • {item.Name} ({item.ProductionYear})"
            : $"{action} • {item.Name}";
    }

    private static string BuildDescription(string userName, string? overview, string progressBar, bool finished)
    {
        var icon = "🪼";
        var lines = new System.Collections.Generic.List<string> { $"{icon} **{userName}**" };

        if (!string.IsNullOrWhiteSpace(overview))
        {
            var truncated = overview.Length > 300 ? overview[..300] + "…" : overview;
            lines.Add($"> {truncated}");
        }

        if (!string.IsNullOrEmpty(progressBar))
        {
            lines.Add(progressBar);
        }

        return string.Join("\n\n", lines);
    }
}
