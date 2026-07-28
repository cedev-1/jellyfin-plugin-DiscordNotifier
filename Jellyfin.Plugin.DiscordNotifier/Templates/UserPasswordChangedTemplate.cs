using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for UserPasswordChanged notification.
/// </summary>
public static class UserPasswordChangedTemplate
{
    /// <summary>
    /// Creates a Discord message when a user changes their password.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(UserPasswordChangedEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        var safeUsername = DiscordText.Safe(eventArgs.Argument.Username);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = "Password Changed",
                    Description = $"The password for **{safeUsername}** has been changed.",
                    Url = $"{serverUrl}/web/#/dashboard/users/profile?userId={eventArgs.Argument.Id}",
                    Color = 0xE67E22,
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
