using System;
using System.Collections.Generic;
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

        var fields = new List<DiscordEmbedField>
        {
            new() { Name = "Device", Value = DiscordText.Safe(eventArgs.DeviceName), Inline = true }
        };

        if (config.IncludeIpAddresses)
        {
            fields.Insert(0, new DiscordEmbedField { Name = "IP Address", Value = DiscordText.Safe(eventArgs.RemoteEndPoint), Inline = true });
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "Login Failed",
                    Description = $"Failed login attempt for user **{DiscordText.Safe(eventArgs.Username)}**.",
                    Url = $"{serverUrl}/web/#/dashboard/users",
                    Color = 0xE74C3C,
                    Fields = fields.ToArray(),
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
