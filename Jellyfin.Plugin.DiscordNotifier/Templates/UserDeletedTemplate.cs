using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates
{
    /// <summary>
    /// Provides template for creating Discord messages when a user is deleted.
    /// </summary>
    public static class UserDeletedTemplate
    {
        /// <summary>
        /// Creates a Discord message object for a user deletion event.
        /// </summary>
        /// <param name="eventArgs">The user deletion event arguments containing user details.</param>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>The Discord webhook payload.</returns>
        public static DiscordWebhookPayload CreateMessage(UserDeletedEventArgs eventArgs, PluginConfiguration config)
        {
            string serverUrl = ServerUrlHelper.GetServerUrl(config);

            return new DiscordWebhookPayload
            {
                Embeds =
                [
                    new DiscordEmbed
                    {
                        Title = "🪼 User Deleted",
                        Description = "An account has been removed from the server.",
                        Url = $"{serverUrl}/web/#/dashboard/users",
                        Color = 0xE74C3C,
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