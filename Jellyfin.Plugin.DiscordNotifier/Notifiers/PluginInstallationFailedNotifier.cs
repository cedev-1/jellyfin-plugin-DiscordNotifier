using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a plugin installation fails.
/// </summary>
public class PluginInstallationFailedNotifier : IEventConsumer<InstallationFailedEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<PluginInstallationFailedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallationFailedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PluginInstallationFailedNotifier(DiscordSender sender, ILogger<PluginInstallationFailedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(InstallationFailedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.PluginInstallationFailedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            _logger.LogInformation("PluginInstallationFailedNotifier: {PluginName}", eventArgs.InstallationInfo.Name);

            var message = PluginEventTemplate.CreateFailedMessage(eventArgs.InstallationInfo, eventArgs.Exception, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PluginInstallationFailedNotifier: Error sending notification");
        }
    }
}
