using MediaBrowser.Controller.Events.Authentication;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

public static class AuthenticationSuccessTemplate
{
    private const int ColorGreen = 65280;

    private static readonly ILogger _logger = Plugin.Logger;

    public static object CreateMessage(AuthenticationResultEventArgs eventArgs, PluginConfiguration config)
    {
        var user = eventArgs.User;
        var session = eventArgs.SessionInfo;

        _logger.LogInformation("Creating Discord message for successful login: {Username}", user.Name);

        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        string userUrl = $"{serverUrl}/web/index.html#!/dashboard/users/profile?userId={user.Id}";
        string ipAddress = session?.RemoteEndPoint?.Split(':')[0] ?? "Unknown";
        string deviceName = session?.DeviceName ?? "Unknown";
        string clientName = session?.Client ?? "Unknown";

        return new
        {
            embeds = new[]
            {
                new
                {
                    title = ":unlock: Login Successful",
                    description = $"User **{user.Name}** has logged in successfully.",
                    url = userUrl,
                    color = ColorGreen,
                    author = new
                    {
                        name = "Jellyfin",
                        icon_url = "https://i.imgur.com/ZGPxFN2.jpg"
                    },
                    thumbnail = new
                    {
                        url = "https://i.imgur.com/ZGPxFN2.jpg"
                    },
                    fields = new[]
                    {
                        new { name = "User", value = $"**{user.Name}**", inline = true },
                        new { name = "IP Address", value = $"||{ipAddress}||", inline = true },
                        new { name = "\u200B", value = "\u200B", inline = true },
                        new { name = "Device", value = deviceName, inline = true },
                        new { name = "Client", value = clientName, inline = true }
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
    }
}
