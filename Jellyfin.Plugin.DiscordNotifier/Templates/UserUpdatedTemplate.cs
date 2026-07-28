using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for UserUpdated notification.
/// </summary>
public static class UserUpdatedTemplate
{
    /// <summary>
    /// Creates a Discord message when a user account is updated.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(UserUpdatedEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        var safeUsername = DiscordText.Safe(eventArgs.Argument.Username);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "User Account Updated",
                    Description = $"The account **{safeUsername}** has been updated.",
                    Url = $"{serverUrl}/web/#/dashboard/users/profile?userId={eventArgs.Argument.Id}",
                    Color = 0x3498DB,
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
