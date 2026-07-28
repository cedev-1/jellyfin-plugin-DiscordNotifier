using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin installation is cancelled.
/// </summary>
public class PluginInstallationCancelledNotifier : IEventConsumer<PluginInstallationCancelledEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginInstallationCancelledNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallationCancelledNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginInstallationCancelledNotifier(DiscordSender sender, ILogger<PluginInstallationCancelledNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginInstallationCancelledEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginInstallationCancelledNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginInstallationCancelledNotifier: {PluginName}", eventArgs.Argument.Name);

            var message = PluginEventTemplate.CreateCancelledMessage(eventArgs.Argument, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginInstallationCancelledNotifier: Error sending notification");
        }
    }
}
