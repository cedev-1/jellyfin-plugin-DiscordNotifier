using Jellyfin.Plugin.DiscordNotifier.Notifiers;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DiscordNotifier.Configuration
{
    /// <summary>
    /// Represents the configuration settings for the Discord Notifier plugin.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            EnablePlugin = true;
            WebhookUrl = string.Empty;
            ServerUrl = string.Empty;
            UserCreatedNotifier = true;
            UserDeletedNotifier = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled.
        /// </summary>
        public bool EnablePlugin { get; set; }

        /// <summary>
        /// Gets or sets the Discord webhook URL.
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Jellyfin server URL.
        /// </summary>
        public string ServerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the UserCreatedNotifier is enabled.
        /// </summary>
        public bool UserCreatedNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the UserDeletedNotifier is enabled.
        /// </summary>
        public bool UserDeletedNotifier { get; set; }
    }
}