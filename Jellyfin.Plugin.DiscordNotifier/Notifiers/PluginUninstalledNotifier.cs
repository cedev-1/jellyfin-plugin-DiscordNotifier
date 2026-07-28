using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Updates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin is uninstalled.
/// </summary>
public class PluginUninstalledNotifier : IEventConsumer<PluginUninstalledEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginUninstalledNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginUninstalledNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginUninstalledNotifier(DiscordSender sender, ILogger<PluginUninstalledNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(PluginUninstalledEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginUninstalledNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginUninstalledNotifier: {PluginName}", eventArgs.Argument.Name);

            var message = PluginEventTemplate.CreateUninstalledMessage(eventArgs.Argument, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginUninstalledNotifier: Error sending notification");
        }
    }
}
