using Jellyfin.Data.Events.Users;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates
{
    /// <summary>
    /// Provides template for creating Discord messages when a user is deleted.
    /// </summary>
    public static class UserDeletedTemplate
    {
        private static readonly ILogger _logger = Plugin.Logger;

        /// <summary>
        /// Creates a Discord message object for a user deletion event.
        /// </summary>
        /// <param name="eventArgs">The user deletion event arguments containing user details.</param>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>An object representing the Discord message to be sent.</returns>
        public static object CreateMessage(UserDeletedEventArgs eventArgs, PluginConfiguration config)
        {
            try
            {
                _logger.LogInformation("Creating Discord message for deleted user: {Username}", eventArgs.Argument.Username);

                string serverUrl = ServerUrlHelper.GetServerUrl(config);

                var message = new
                {
                    content = string.Empty,
                    embeds = new[]
                    {
                        new
                        {
                            title = "🪼 User Deleted",
                            description = "An account has been removed from the server.",
                            url = $"{serverUrl}/web/#/dashboard/users",
                            color = 15158332,
                            fields = new[]
                            {
                                new { name = "Username", value = eventArgs.Argument.Username, inline = true },
                                new { name = "User ID", value = eventArgs.Argument.Id.ToString(), inline = true }
                            },
                            footer = new
                            {
                                text = "Jellyfin Discord Notifier",
                                icon_url = "https://static-00.iconduck.com/assets.00/jellyfin-icon-256x255-u0iypdp6.png"
                            },
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                _logger.LogDebug("Successfully created Discord message for user {Username}", eventArgs.Argument.Username);
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Discord message for deleted user {Username}", eventArgs.Argument.Username);
                throw;
            }
        }
    }
}