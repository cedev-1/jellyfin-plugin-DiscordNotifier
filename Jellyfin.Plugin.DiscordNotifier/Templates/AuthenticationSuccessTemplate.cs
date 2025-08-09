using MediaBrowser.Controller.Events.Authentication;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Utils;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Static class for creating authentication success notification messages.
/// </summary>
public static class AuthenticationSuccessTemplate
{
    private static readonly ILogger _logger = Plugin.Logger;

    /// <summary>
    /// Creates a Discord notification message for successful authentication.
    /// </summary>
    /// <param name="eventArgs">The authentication result event arguments containing user and session information.</param>
    /// <param name="config">The plugin configuration containing server settings.</param>
    /// <returns>An anonymous object representing the Discord webhook message.</returns>
    public static object CreateMessage(AuthenticationResultEventArgs eventArgs, PluginConfiguration config)
    {
        var user = eventArgs.User;
        var session = eventArgs.SessionInfo;

        _logger.LogInformation("Creating Discord message for successful login: {Username}", user.Name);

        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        return new
        {
            content = string.Empty,
            embeds = new[]
            {
                new
                {
                    title = "🪼 Login successful",
                    description = $"User **{user.Name}** has logged in successfully.",
                    url = $"{serverUrl}/web/index.html#!/dashboard/users",
                    color = 0x00FF00,
                    fields = new[]
                    {
                        new { name = "IP Address", value = session?.RemoteEndPoint ?? "Unknown", inline = true },
                        new { name = "Device", value = session?.DeviceName ?? "Unknown", inline = true },
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
