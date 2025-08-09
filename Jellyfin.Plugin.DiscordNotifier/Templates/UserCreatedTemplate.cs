using Jellyfin.Data.Events.Users;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates
{
    /// <summary>
    /// Template for UserCreated notification.
    /// </summary>
    public static class UserCreatedTemplate
    {
        private static readonly ILogger _logger = Plugin.Logger;

        /// <summary>
        /// Creates the Discord message for UserCreated event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>The Discord message.</returns>
        public static object CreateMessage(UserCreatedEventArgs eventArgs, PluginConfiguration config)
        {
            try
            {
                _logger.LogInformation("Creating Discord message for new user: {Username}", eventArgs.Argument.Username);

                string serverUrl = ServerUrlHelper.GetServerUrl(config);

                var message = new
                {
                    content = string.Empty,
                    embeds = new[]
                    {
                        new
                        {
                            title = "🪼 New User Created",
                            description = "A new account has been created on the server.",
                            url = $"{serverUrl}/web/#/dashboard/users/profile?userId={eventArgs.Argument.Id}",
                            color = 5027327,
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
                _logger.LogError(ex, "Error creating Discord message for new user {Username}", eventArgs.Argument.Username);
                throw;
            }
        }
    }
}