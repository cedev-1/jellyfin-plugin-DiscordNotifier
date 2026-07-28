using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for UserLockedOut notification.
/// </summary>
public static class UserLockedOutTemplate
{
    /// <summary>
    /// Creates a Discord message when a user account is locked out.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(UserLockedOutEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        var safeUsername = DiscordText.Safe(eventArgs.Argument.Username);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "Account Locked Out",
                    Description = $"User **{safeUsername}** has been locked out after too many failed login attempts.",
                    Url = $"{serverUrl}/web/#/dashboard/users/profile?userId={eventArgs.Argument.Id}",
                    Color = 0xE74C3C,
                    Fields =
                    [
                        new DiscordEmbedField { Name = "Username", Value = safeUsername, Inline = true },
                        new DiscordEmbedField { Name = "User ID", Value = eventArgs.Argument.Id.ToString(), Inline = true }
                    ],
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
