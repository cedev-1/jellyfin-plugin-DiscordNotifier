using System;
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
        /// Ensures the URL always has a scheme so Discord accepts it.
        /// </summary>
        /// <param name="config">The plugin configuration.</param>
        /// <returns>The server URL to use for notifications.</returns>
        public static string GetServerUrl(PluginConfiguration config)
        {
            var raw = config.ServerUrl?.Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "http://localhost:8096";
            }

            if (!raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                raw = "http://" + raw;
            }

            return Uri.TryCreate(raw, UriKind.Absolute, out _)
                ? raw
                : "http://localhost:8096";
        }
    }
}
