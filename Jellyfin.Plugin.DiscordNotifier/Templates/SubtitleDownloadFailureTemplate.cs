using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Subtitles;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for SubtitleDownloadFailure notification.
/// </summary>
public static class SubtitleDownloadFailureTemplate
{
    /// <summary>
    /// Creates a Discord message when a subtitle download fails.
    /// </summary>
    /// <param name="eventArgs">The subtitle download failure event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(SubtitleDownloadFailureEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        var item = eventArgs.Item;

        var titleText = DiscordText.Truncate($"Subtitle Download Failed: {DiscordText.Safe(item.Name)}", 256);

        var fields = new System.Collections.Generic.List<DiscordEmbedField>();

        if (!string.IsNullOrWhiteSpace(eventArgs.Provider))
        {
            fields.Add(new DiscordEmbedField { Name = "Provider", Value = DiscordText.Safe(eventArgs.Provider), Inline = true });
        }

        if (eventArgs.Exception is not null)
        {
            fields.Add(new DiscordEmbedField { Name = "Error", Value = DiscordText.Truncate(eventArgs.Exception.Message, 1024), Inline = false });
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Url = $"{serverUrl}/web/#/details?id={item.Id}",
                    Color = 0xE74C3C,
                    Fields = fields.Count > 0 ? fields.ToArray() : null,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
