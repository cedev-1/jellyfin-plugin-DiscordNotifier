using MediaBrowser.Controller.Events.Authentication;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Static class for creating authentication failure notification messages.
/// </summary>
public static class AuthenticationFailureTemplate
{
    private static readonly ILogger _logger = Plugin.Logger;

    /// <summary>
    /// Creates a Discord notification message for failed authentication attempts.
    /// </summary>
    /// <param name="eventArgs">The authentication request event arguments containing user information.</param>
    /// <param name="config">The plugin configuration containing server settings.</param>
    /// <returns>An anonymous object representing the Discord webhook message.</returns>
    public static object CreateMessage(AuthenticationRequestEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        return new
        {
            content = string.Empty,
            embeds = new[]
            {
                new
                {
                    title = "🪼 Login failed",
                    description = $"Failed login attempt for user **{eventArgs.Username}**.",
                    url = $"{serverUrl}/web/index.html#!/dashboard/users",
                    color = 0xFF0000,
                    fields = new[]
                    {
                        new { name = "IP Address", value = eventArgs.RemoteEndPoint ?? "Unknown", inline = true },
                        new { name = "Device", value = eventArgs.DeviceName ?? "Unknown", inline = true },
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
    }
}
