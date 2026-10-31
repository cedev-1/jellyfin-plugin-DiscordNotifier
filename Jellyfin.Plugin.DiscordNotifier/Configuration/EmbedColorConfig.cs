namespace Jellyfin.Plugin.DiscordNotifier.Configuration
{
    /// <summary>
    /// A custom embed colour for one notification event.
    /// </summary>
    public class EmbedColorConfig
    {
        /// <summary>Gets or sets the name of the <see cref="NotifierEvent"/> this colour applies to.</summary>
        public string Event { get; set; } = string.Empty;

        /// <summary>Gets or sets the colour as a hex string (e.g. <c>#9B59B6</c>).</summary>
        public string Color { get; set; } = string.Empty;
    }
}
