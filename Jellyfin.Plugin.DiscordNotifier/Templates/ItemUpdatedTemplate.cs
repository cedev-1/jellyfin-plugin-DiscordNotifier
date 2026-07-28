using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for ItemUpdated notification.
/// </summary>
public static class ItemUpdatedTemplate
{
    /// <summary>
    /// Creates a Discord message when a library item's metadata is updated.
    /// </summary>
    /// <param name="item">The item whose metadata was updated.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(BaseItem item, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var typeLabel = item.GetType().Name switch
        {
            "Movie" => "Movie",
            "Series" => "Series",
            "MusicAlbum" => "Album",
            "Book" => "Book",
            _ => item.GetType().Name
        };

        var name = DiscordText.Safe(item.Name);
        var titleText = item.ProductionYear.HasValue
            ? $"Metadata Updated: {name} ({item.ProductionYear})"
            : $"Metadata Updated: {name}";
        titleText = DiscordText.Truncate(titleText, 256);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = $"The metadata for this **{typeLabel}** has been refreshed.",
                    Url = $"{serverUrl}/web/#/details?id={item.Id}",
                    Color = 0x3498DB,
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
