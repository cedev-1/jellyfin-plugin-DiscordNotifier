using System.Collections.Generic;

namespace Jellyfin.Plugin.DiscordNotifier.Configuration
{
    /// <summary>
    /// A named Discord webhook and the notification events it receives.
    /// </summary>
    public class WebhookConfig
    {
        /// <summary>Gets or sets the display name of the webhook (only used in the configuration page).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the Discord webhook URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Gets or sets the bot display name for this webhook. Empty falls back to the global value.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets the bot avatar URL for this webhook. Empty falls back to the global value.</summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the names of the <see cref="NotifierEvent"/> values this webhook receives.</summary>
        public List<string> Events { get; set; } = new();
    }
}
