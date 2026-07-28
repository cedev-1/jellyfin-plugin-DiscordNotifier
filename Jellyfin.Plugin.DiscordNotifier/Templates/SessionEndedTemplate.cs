using System;
using System.Collections.Generic;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Session;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for SessionEnded notification.
/// </summary>
public static class SessionEndedTemplate
{
    /// <summary>
    /// Creates a Discord message when a client session ends.
    /// </summary>
    /// <param name="session">The session info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(SessionInfo session, PluginConfiguration config)
    {
        var fields = new List<DiscordEmbedField>
        {
            new() { Name = "Client", Value = DiscordText.Safe(session.Client), Inline = true },
            new() { Name = "Device", Value = DiscordText.Safe(session.DeviceName), Inline = true }
        };

        if (config.IncludeIpAddresses)
        {
            fields.Add(new DiscordEmbedField { Name = "IP Address", Value = DiscordText.Safe(session.RemoteEndPoint), Inline = true });
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "Session Ended",
                    Description = $"**{DiscordText.Safe(session.UserName)}** closed their session.",
                    Color = 0x95A5A6,
                    Fields = fields.ToArray(),
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
