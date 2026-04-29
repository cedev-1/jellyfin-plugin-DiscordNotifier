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
            ServerName = string.Empty;
            UserCreatedNotifier = true;
            UserDeletedNotifier = true;
            AuthenticationSuccessNotifier = true;
            AuthenticationFailureNotifier = true;
            ItemAddedNotifier = true;
            PlaybackStartNotifier = false;
            PlaybackTranscodeOnlyNotifier = false;
            PlaybackNotifyMovies = true;
            PlaybackNotifySeries = false;
            PlaybackDetailedMode = false;
            PlaybackStopNotifier = false;
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
        /// Gets or sets the display name shown in Discord notification footers.
        /// </summary>
        public string ServerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the UserCreatedNotifier is enabled.
        /// </summary>
        public bool UserCreatedNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the UserDeletedNotifier is enabled.
        /// </summary>
        public bool UserDeletedNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the AuthenticationSuccessNotifier is enabled.
        /// </summary>
        public bool AuthenticationSuccessNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the AuthenticationFailureNotifier is enabled.
        /// </summary>
        public bool AuthenticationFailureNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ItemAddedNotifier is enabled.
        /// </summary>
        public bool ItemAddedNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the PlaybackStartNotifier is enabled.
        /// </summary>
        public bool PlaybackStartNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only notify on transcoding sessions.
        /// </summary>
        public bool PlaybackTranscodeOnlyNotifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify for movie playback.
        /// </summary>
        public bool PlaybackNotifyMovies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify for series episode playback.
        /// </summary>
        public bool PlaybackNotifySeries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show detailed playback info (codecs, client, transcode reasons).
        /// When false, a simple notification is sent instead.
        /// </summary>
        public bool PlaybackDetailedMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the PlaybackStopNotifier is enabled.
        /// </summary>
        public bool PlaybackStopNotifier { get; set; }
    }
}