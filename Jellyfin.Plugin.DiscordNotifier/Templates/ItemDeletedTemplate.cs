using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for ItemDeleted notification.
/// </summary>
public static class ItemDeletedTemplate
{
    /// <summary>
    /// Creates a Discord message when a library item is deleted.
    /// </summary>
    /// <param name="item">The item that was deleted.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(BaseItem item, PluginConfiguration config)
    {
        var typeLabel = item.GetType().Name switch
        {
            "Movie" => "Movie",
            "Episode" => "Episode",
            "Series" => "Series",
            "Audio" => "Track",
            "MusicAlbum" => "Album",
            "Book" => "Book",
            _ => item.GetType().Name
        };

        string titleText;
        if (item is Episode episode && !string.IsNullOrEmpty(episode.SeriesName))
        {
            var s = episode.ParentIndexNumber.GetValueOrDefault();
            var e = episode.IndexNumber.GetValueOrDefault();
            titleText = $"Deleted {typeLabel}: {DiscordText.Safe(episode.SeriesName)} — S{s:D2}E{e:D2} ~ {DiscordText.Safe(item.Name)}";
        }
        else
        {
            var name = DiscordText.Safe(item.Name);
            titleText = item.ProductionYear.HasValue
                ? $"Deleted {typeLabel}: {name} ({item.ProductionYear})"
                : $"Deleted {typeLabel}: {name}";
        }

        titleText = DiscordText.Truncate(titleText, 256);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Color = 0xE74C3C,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
