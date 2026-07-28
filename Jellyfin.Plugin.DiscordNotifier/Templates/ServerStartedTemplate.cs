using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for ServerStarted notification.
/// </summary>
public static class ServerStartedTemplate
{
    /// <summary>
    /// Creates a Discord message when the Jellyfin server starts.
    /// </summary>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        string serverName = string.IsNullOrWhiteSpace(config.ServerName) ? "Jellyfin" : config.ServerName;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "🟢 Server Started",
                    Description = $"**{serverName}** is now online and ready.",
                    Url = string.IsNullOrWhiteSpace(serverUrl) ? null : serverUrl,
                    Color = 0x2ECC71,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
