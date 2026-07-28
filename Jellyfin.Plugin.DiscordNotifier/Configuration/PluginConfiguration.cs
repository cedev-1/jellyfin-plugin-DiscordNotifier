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
            WebhookUsername = "Jellyfin Notifier";
            WebhookAvatarUrl = "https://raw.githubusercontent.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/main/media/banner.png";
            ServerUrl = string.Empty;
            ServerName = string.Empty;
            UserCreatedNotifier = true;
            UserDeletedNotifier = true;
            AuthenticationSuccessNotifier = true;
            AuthenticationFailureNotifier = true;
            UserLockedOutNotifier = true;
            UserPasswordChangedNotifier = true;
            UserUpdatedNotifier = false;
            ItemAddedNotifier = true;
            ItemAddedGroupingDelayMinutes = 5;
            ItemAddedMaxWaitMinutes = 60;
            ItemAddedNotifyMovies = true;
            ItemAddedNotifySeries = true;
            ItemAddedNotifyMusic = true;
            ItemAddedNotifyBooks = true;
            ItemDeletedNotifier = false;
            ItemUpdatedNotifier = false;
            PlaybackStartNotifier = false;
            PlaybackStopNotifier = false;
            PlaybackTranscodeOnlyNotifier = false;
            PlaybackNotifyMovies = true;
            PlaybackNotifySeries = false;
            PlaybackDetailedMode = false;
            PlaybackProgressNotifier = false;
            PlaybackProgressIntervalMinutes = 10;
            SessionStartNotifier = false;
            SessionEndedNotifier = false;
            TaskCompletedNotifier = false;
            TaskCompletedOnlyOnFailure = true;
            PluginInstalledNotifier = true;
            PluginUpdatedNotifier = true;
            PluginUninstalledNotifier = true;
            PluginInstallationFailedNotifier = true;
            PluginInstallationCancelledNotifier = false;
            PluginInstallingNotifier = false;
            PendingRestartNotifier = true;
            ServerStartedNotifier = false;
            LyricDownloadFailureNotifier = false;
            SubtitleDownloadFailureNotifier = false;
            UserDataSavedNotifier = false;
            IncludeIpAddresses = false;
        }

        /// <summary>Gets or sets a value indicating whether the plugin is enabled.</summary>
        public bool EnablePlugin { get; set; }

        /// <summary>Gets or sets the Discord webhook URL.</summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the display name override for the Discord webhook bot.</summary>
        public string WebhookUsername { get; set; } = string.Empty;

        /// <summary>Gets or sets the avatar URL override for the Discord webhook bot.</summary>
        public string WebhookAvatarUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the Jellyfin server URL.</summary>
        public string ServerUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the display name shown in Discord notification footers.</summary>
        public string ServerName { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether the UserCreatedNotifier is enabled.</summary>
        public bool UserCreatedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the UserDeletedNotifier is enabled.</summary>
        public bool UserDeletedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the AuthenticationSuccessNotifier is enabled.</summary>
        public bool AuthenticationSuccessNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the AuthenticationFailureNotifier is enabled.</summary>
        public bool AuthenticationFailureNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the ItemAddedNotifier is enabled.</summary>
        public bool ItemAddedNotifier { get; set; }

        /// <summary>Gets or sets the debounce delay in minutes before sending a grouped new-content notification.</summary>
        public int ItemAddedGroupingDelayMinutes { get; set; }

        /// <summary>Gets or sets the maximum minutes a grouped new-content notification can be held before it is sent regardless of ongoing activity.</summary>
        public int ItemAddedMaxWaitMinutes { get; set; }

        /// <summary>Gets or sets a value indicating whether new-content notifications are sent for movies.</summary>
        public bool ItemAddedNotifyMovies { get; set; }

        /// <summary>Gets or sets a value indicating whether new-content notifications are sent for series and episodes.</summary>
        public bool ItemAddedNotifySeries { get; set; }

        /// <summary>Gets or sets a value indicating whether new-content notifications are sent for music (albums and tracks).</summary>
        public bool ItemAddedNotifyMusic { get; set; }

        /// <summary>Gets or sets a value indicating whether new-content notifications are sent for books.</summary>
        public bool ItemAddedNotifyBooks { get; set; }

        /// <summary>Gets or sets a value indicating whether the ItemDeletedNotifier is enabled.</summary>
        public bool ItemDeletedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the ItemUpdatedNotifier is enabled.</summary>
        public bool ItemUpdatedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PlaybackStartNotifier is enabled.</summary>
        public bool PlaybackStartNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether to only notify on transcoding sessions.</summary>
        public bool PlaybackTranscodeOnlyNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether to notify for movie playback.</summary>
        public bool PlaybackNotifyMovies { get; set; }

        /// <summary>Gets or sets a value indicating whether to notify for series episode playback.</summary>
        public bool PlaybackNotifySeries { get; set; }

        /// <summary>Gets or sets a value indicating whether to show detailed playback info.</summary>
        public bool PlaybackDetailedMode { get; set; }

        /// <summary>Gets or sets a value indicating whether the PlaybackStopNotifier is enabled.</summary>
        public bool PlaybackStopNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the UserLockedOutNotifier is enabled.</summary>
        public bool UserLockedOutNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the UserUpdatedNotifier is enabled.</summary>
        public bool UserUpdatedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the UserPasswordChangedNotifier is enabled.</summary>
        public bool UserPasswordChangedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the SessionStartNotifier is enabled.</summary>
        public bool SessionStartNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the SessionEndedNotifier is enabled.</summary>
        public bool SessionEndedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PlaybackProgressNotifier is enabled.</summary>
        public bool PlaybackProgressNotifier { get; set; }

        /// <summary>Gets or sets the interval in minutes between playback progress notifications per session.</summary>
        public int PlaybackProgressIntervalMinutes { get; set; }

        /// <summary>Gets or sets a value indicating whether the TaskCompletedNotifier is enabled.</summary>
        public bool TaskCompletedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether task-completed notifications are limited to failures only. Default true to reduce noise.</summary>
        public bool TaskCompletedOnlyOnFailure { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginInstalledNotifier is enabled.</summary>
        public bool PluginInstalledNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginUpdatedNotifier is enabled.</summary>
        public bool PluginUpdatedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginUninstalledNotifier is enabled.</summary>
        public bool PluginUninstalledNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginInstallationFailedNotifier is enabled.</summary>
        public bool PluginInstallationFailedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginInstallationCancelledNotifier is enabled.</summary>
        public bool PluginInstallationCancelledNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PendingRestartNotifier is enabled.</summary>
        public bool PendingRestartNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the ServerStartedNotifier is enabled.</summary>
        public bool ServerStartedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the SubtitleDownloadFailureNotifier is enabled.</summary>
        public bool SubtitleDownloadFailureNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the UserDataSavedNotifier is enabled.</summary>
        public bool UserDataSavedNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the PluginInstallingNotifier is enabled.</summary>
        public bool PluginInstallingNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether the LyricDownloadFailureNotifier is enabled.</summary>
        public bool LyricDownloadFailureNotifier { get; set; }

        /// <summary>Gets or sets a value indicating whether IP addresses are included in notification embeds (login, session). Default false for GDPR.</summary>
        public bool IncludeIpAddresses { get; set; }
    }
}
