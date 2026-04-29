using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for ItemAdded notification.
/// </summary>
public static class ItemAddedTemplate
{
    /// <summary>
    /// Creates a Discord message when a new item is added to the library.
    /// </summary>
    /// <param name="item">The library item that was added.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(BaseItem item, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var (emoji, typeLabel) = item.GetType().Name switch
        {
            "Movie" => ("🪼", "Movie"),
            "Episode" => ("🪼", "Episode"),
            "Series" => ("🪼", "Series"),
            "Audio" => ("🪼", "Track"),
            "MusicAlbum" => ("🪼", "Album"),
            "Book" => ("🪼", "Book"),
            _ => ("🪼", item.GetType().Name)
        };

        var titleText = item.ProductionYear.HasValue
            ? $"{emoji} New {typeLabel}: {item.Name} ({item.ProductionYear})"
            : $"{emoji} New {typeLabel}: {item.Name}";

        string? description = null;

        if (item is Episode episode && !string.IsNullOrEmpty(episode.SeriesName))
        {
            description = $"**{episode.SeriesName}**";
            if (episode.ParentIndexNumber.HasValue && episode.IndexNumber.HasValue)
            {
                description += $" — S{episode.ParentIndexNumber:D2}E{episode.IndexNumber:D2}";
            }
        }

        if (!string.IsNullOrWhiteSpace(item.Overview))
        {
            var overview = item.Overview.Length > 300
                ? string.Concat(item.Overview.AsSpan(0, 300), "…")
                : item.Overview;

            description = description is null ? overview : $"{description}\n\n{overview}";
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Url = $"{serverUrl}/web/index.html#!/details?id={item.Id}",
                    Color = 0x9B59B6,
                    Thumbnail = new DiscordEmbedMedia
                    {
                        Url = $"{serverUrl}/Items/{item.Id}/Images/Primary"
                    },
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
