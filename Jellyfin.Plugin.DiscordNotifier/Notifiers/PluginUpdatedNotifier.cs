using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin is updated.
/// </summary>
public class PluginUpdatedNotifier : IEventConsumer<PluginUpdatedEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginUpdatedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginUpdatedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginUpdatedNotifier(DiscordSender sender, ILogger<PluginUpdatedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginUpdatedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginUpdatedNotifier || !config.HasWebhookFor(NotifierEvent.PluginUpdated))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginUpdatedNotifier: {PluginName} v{Version}", eventArgs.Argument.Name, eventArgs.Argument.Version);

            var message = PluginEventTemplate.CreateUpdatedMessage(eventArgs.Argument, config);
            await _sender.SendAsync(NotifierEvent.PluginUpdated, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginUpdatedNotifier: Error sending notification");
        }
    }
}
