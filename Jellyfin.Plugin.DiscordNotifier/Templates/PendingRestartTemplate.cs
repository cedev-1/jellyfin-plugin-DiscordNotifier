using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for PendingRestart notification.
/// </summary>
public static class PendingRestartTemplate
{
    /// <summary>
    /// Creates a Discord message when the server requires a restart.
    /// </summary>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(PluginConfiguration config)
    {
        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "Server Restart Required",
                    Description = "The Jellyfin server requires a restart to apply pending changes.",
                    Color = 0xE67E22,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
