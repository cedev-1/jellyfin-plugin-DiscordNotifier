using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin installation starts.
/// </summary>
public class PluginInstallingNotifier : IEventConsumer<PluginInstallingEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginInstallingNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallingNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginInstallingNotifier(DiscordSender sender, ILogger<PluginInstallingNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginInstallingEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginInstallingNotifier || !config.HasWebhookFor(NotifierEvent.PluginInstalling))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginInstallingNotifier: {PluginName} v{Version}", eventArgs.Argument.Name, eventArgs.Argument.Version);

            var message = PluginEventTemplate.CreateInstallingMessage(eventArgs.Argument, config);
            await _sender.SendAsync(NotifierEvent.PluginInstalling, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginInstallingNotifier: Error sending notification");
        }
    }
}
