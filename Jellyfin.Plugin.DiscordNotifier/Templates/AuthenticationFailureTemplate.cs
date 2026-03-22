using MediaBrowser.Controller.Events.Authentication;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

public static class AuthenticationFailureTemplate
{
    private const int ColorRed = 16711680;

    private static readonly ILogger _logger = Plugin.Logger;

    public static object CreateMessage(AuthenticationRequestEventArgs eventArgs, PluginConfiguration config)
    {
        _logger.LogInformation("Creating Discord message for failed login attempt: {Username}", eventArgs.Username);

        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        string ipAddress = eventArgs.RemoteEndPoint?.Split(':')[0] ?? "Unknown";
        string deviceName = eventArgs.DeviceName ?? "Unknown";

        return new
        {
            embeds = new[]
            {
                new
                {
                    title = ":lock: Login Failed",
                    description = $"Failed login attempt for user **{eventArgs.Username}**.",
                    url = $"{serverUrl}/web/index.html#!/dashboard/users",
                    color = ColorRed,
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
                        new { name = "Username", value = $"**{eventArgs.Username}**", inline = true },
                        new { name = "IP Address", value = $"||{ipAddress}||", inline = true },
                        new { name = "\u200B", value = "\u200B", inline = true },
                        new { name = "Device", value = deviceName, inline = true }
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
