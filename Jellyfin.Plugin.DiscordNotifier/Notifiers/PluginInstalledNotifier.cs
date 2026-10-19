using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin is installed.
/// </summary>
public class PluginInstalledNotifier : IEventConsumer<PluginInstalledEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginInstalledNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstalledNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginInstalledNotifier(DiscordSender sender, ILogger<PluginInstalledNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginInstalledEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginInstalledNotifier || !config.HasWebhookFor(NotifierEvent.PluginInstalled))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginInstalledNotifier: {PluginName} v{Version}", eventArgs.Argument.Name, eventArgs.Argument.Version);

            var message = PluginEventTemplate.CreateInstalledMessage(eventArgs.Argument, config);
            await _sender.SendAsync(NotifierEvent.PluginInstalled, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginInstalledNotifier: Error sending notification");
        }
    }
}
