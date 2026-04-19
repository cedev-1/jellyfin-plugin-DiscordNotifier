using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Events.Authentication;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Static class for creating authentication success notification messages.
/// </summary>
public static class AuthenticationSuccessTemplate
{
    /// <summary>
    /// Creates a Discord notification message for successful authentication.
    /// </summary>
    /// <param name="eventArgs">The authentication result event arguments containing user and session information.</param>
    /// <param name="config">The plugin configuration containing server settings.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(AuthenticationResultEventArgs eventArgs, PluginConfiguration config)
    {
        var user = eventArgs.User;
        var session = eventArgs.SessionInfo;
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "🪼 Login Successful",
                    Description = $"User **{user.Name}** has logged in successfully.",
                    Url = $"{serverUrl}/web/index.html#!/dashboard/users",
                    Color = 0x2ECC71,
                    Fields =
                    [
                        new DiscordEmbedField { Name = "IP Address", Value = session?.RemoteEndPoint ?? "Unknown", Inline = true },
                        new DiscordEmbedField { Name = "Device", Value = session?.DeviceName ?? "Unknown", Inline = true }
                    ],
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
