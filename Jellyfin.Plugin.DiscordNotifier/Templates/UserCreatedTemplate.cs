using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates
{
    /// <summary>
    /// Template for UserCreated notification.
    /// </summary>
    public static class UserCreatedTemplate
    {
        /// <summary>
        /// Creates the Discord message for UserCreated event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>The Discord webhook payload.</returns>
        public static DiscordWebhookPayload CreateMessage(UserCreatedEventArgs eventArgs, PluginConfiguration config)
        {
            string serverUrl = ServerUrlHelper.GetServerUrl(config);

            return new DiscordWebhookPayload
            {
                Embeds =
                [
                    new DiscordEmbed
                    {
                        Title = "🪼 New User Created",
                        Description = "A new account has been created on the server.",
                        Url = $"{serverUrl}/web/#/dashboard/users/profile?userId={eventArgs.Argument.Id}",
                        Color = 0x4C8BF5,
                        Fields =
                        [
                            new DiscordEmbedField { Name = "Username", Value = eventArgs.Argument.Username, Inline = true },
                            new DiscordEmbedField { Name = "User ID", Value = eventArgs.Argument.Id.ToString(), Inline = true }
                        ],
                        Footer = DiscordEmbedFooter.FromConfig(config),
                        Timestamp = DateTime.UtcNow.ToString("o")
                    }
                ]
            };
        }
    }
}