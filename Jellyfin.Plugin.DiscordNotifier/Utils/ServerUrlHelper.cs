using Jellyfin.Plugin.DiscordNotifier.Configuration;

namespace Jellyfin.Plugin.DiscordNotifier.Utils
{
    /// <summary>
    /// Helper class for getting the server URL from configuration.
    /// </summary>
    public static class ServerUrlHelper
    {
        /// <summary>
        /// Gets the server URL from configuration with fallback to default.
        /// </summary>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>The server URL to use for notifications.</returns>
        public static string GetServerUrl(PluginConfiguration config)
        {
            if (!string.IsNullOrWhiteSpace(config.ServerUrl))
            {
                return config.ServerUrl.Trim().TrimEnd('/');
            }

            // Fallback to localhost if no URL is configured
            return "http://localhost:8096";
        }
    }
}
