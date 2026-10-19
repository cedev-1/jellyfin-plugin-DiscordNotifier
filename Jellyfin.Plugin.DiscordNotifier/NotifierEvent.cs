namespace Jellyfin.Plugin.DiscordNotifier
{
    /// <summary>
    /// Every notification the plugin can send. Webhooks subscribe to a set of these.
    /// </summary>
    public enum NotifierEvent
    {
        /// <summary>New library content.</summary>
        ItemAdded,

        /// <summary>Library content removed.</summary>
        ItemDeleted,

        /// <summary>Library content updated.</summary>
        ItemUpdated,

        /// <summary>Playback started.</summary>
        PlaybackStart,

        /// <summary>Playback stopped.</summary>
        PlaybackStop,

        /// <summary>Playback progress update.</summary>
        PlaybackProgress,

        /// <summary>User session started.</summary>
        SessionStart,

        /// <summary>User session ended.</summary>
        SessionEnded,

        /// <summary>Watch status / favorite changes.</summary>
        UserDataSaved,

        /// <summary>User created.</summary>
        UserCreated,

        /// <summary>User deleted.</summary>
        UserDeleted,

        /// <summary>User updated.</summary>
        UserUpdated,

        /// <summary>User locked out.</summary>
        UserLockedOut,

        /// <summary>User password changed.</summary>
        UserPasswordChanged,

        /// <summary>Successful authentication.</summary>
        AuthenticationSuccess,

        /// <summary>Failed authentication.</summary>
        AuthenticationFailure,

        /// <summary>Server started.</summary>
        ServerStarted,

        /// <summary>Server restart pending.</summary>
        PendingRestart,

        /// <summary>Scheduled task completed.</summary>
        TaskCompleted,

        /// <summary>Plugin installed.</summary>
        PluginInstalled,

        /// <summary>Plugin updated.</summary>
        PluginUpdated,

        /// <summary>Plugin uninstalled.</summary>
        PluginUninstalled,

        /// <summary>Plugin installation failed.</summary>
        PluginInstallationFailed,

        /// <summary>Plugin installation cancelled.</summary>
        PluginInstallationCancelled,

        /// <summary>Plugin installing.</summary>
        PluginInstalling,

        /// <summary>Lyric download failure.</summary>
        LyricDownloadFailure,

        /// <summary>Subtitle download failure.</summary>
        SubtitleDownloadFailure
    }
}
