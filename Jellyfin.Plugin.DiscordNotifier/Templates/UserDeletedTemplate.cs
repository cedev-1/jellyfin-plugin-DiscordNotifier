using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

public static class UserDeletedTemplate
{
    private const int ColorRed = 15158332;

    private static readonly ILogger _logger = Plugin.Logger;

    public static object CreateMessage(UserDeletedEventArgs eventArgs, PluginConfiguration config)
    {
        try
        {
            _logger.LogInformation("Creating Discord message for deleted user: {Username}", eventArgs.Argument.Username);

            string serverUrl = ServerUrlHelper.GetServerUrl(config);

            var message = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = ":wastebasket: User Deleted",
                        description = $"The user **{eventArgs.Argument.Username}** has been removed from the server.",
                        url = $"{serverUrl}/web/index.html#!/dashboard/users",
                        color = ColorRed,
                        author = new
                        {
                            name = "Jellyfin",
                            icon_url = "https://i.imgur.com/ZGPxFN2.jpg"
                        },
                        fields = new[]
                        {
                            new { name = "Username", value = $"**{eventArgs.Argument.Username}**", inline = true },
                            new { name = "User ID", value = $"||`{eventArgs.Argument.Id}`||", inline = true }
                        },
                        footer = new
                        {
                            text = "Jellyfin Discord Notifier",
                            icon_url = "https://raw.githubusercontent.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/main/media/jellyfin.png"
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