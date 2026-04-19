using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Events.Authentication;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Static class for creating authentication failure notification messages.
/// </summary>
public static class AuthenticationFailureTemplate
{
    /// <summary>
    /// Creates a Discord notification message for failed authentication attempts.
    /// </summary>
    /// <param name="eventArgs">The authentication request event arguments containing user information.</param>
    /// <param name="config">The plugin configuration containing server settings.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(AuthenticationRequestEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "🪼 Login Failed",
                    Description = $"Failed login attempt for user **{eventArgs.Username}**.",
                    Url = $"{serverUrl}/web/index.html#!/dashboard/users",
                    Color = 0xE74C3C,
                    Fields =
                    [
                        new DiscordEmbedField { Name = "IP Address", Value = eventArgs.RemoteEndPoint ?? "Unknown", Inline = true },
                        new DiscordEmbedField { Name = "Device", Value = eventArgs.DeviceName ?? "Unknown", Inline = true }
                    ],
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
