using Jellyfin.Plugin.DiscordNotifier.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier
{
    /// <summary>
    /// Main plugin class for Discord Notifier that handles configuration and web pages.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger<Plugin> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="logger">The logger instance.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            _logger = logger;
            Instance = this;
        }

        /// <summary>
        /// Gets the logger instance for the plugin.
        /// </summary>
        public static ILogger<Plugin> Logger => Instance!._logger;

        /// <summary>
        /// Gets the plugin configuration.
        /// </summary>
        public static PluginConfiguration Config => Instance!.Configuration;

        /// <inheritdoc />
        public override string Name => "Discord Notifier";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("8f3c85d3-19cf-4e50-a57e-7d2ce3c0c413");

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.Web.configPage.html"
            };
            yield return new PluginPageInfo
            {
                Name = "DiscordNotifierConfig",
                EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.Web.configPage.js"
            };
        }
    }
}
