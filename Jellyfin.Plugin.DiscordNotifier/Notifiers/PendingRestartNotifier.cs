using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when the server requires a restart.
/// Subscribes to IServerApplicationHost.HasPendingRestartChanged (plain .NET event)
/// because Jellyfin does not publish PendingRestartEventArgs through IEventManager.
/// </summary>
public class PendingRestartNotifier : IHostedService
{
    private readonly IServerApplicationHost _appHost;
    private readonly DiscordSender _sender;
    private readonly ILogger<PendingRestartNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingRestartNotifier"/> class.
    /// </summary>
    /// <param name="appHost">The application host.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public PendingRestartNotifier(IServerApplicationHost appHost, DiscordSender sender, ILogger<PendingRestartNotifier> logger)
    {
        _appHost = appHost;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appHost.HasPendingRestartChanged += OnPendingRestartChanged;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _appHost.HasPendingRestartChanged -= OnPendingRestartChanged;
        return Task.CompletedTask;
    }

    private void OnPendingRestartChanged(object? sender, EventArgs e)
    {
        _ = HandleAsync();
    }

    private async Task HandleAsync()
    {
        try
        {
            var config = Plugin.Config;
            if (!config.EnablePlugin || !config.PendingRestartNotifier || !config.HasWebhookFor(NotifierEvent.PendingRestart))
            {
                return;
            }

            _logger.LogInformation("PendingRestartNotifier: server restart required");

            var message = PendingRestartTemplate.CreateMessage(config);
            await _sender.SendAsync(NotifierEvent.PendingRestart, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PendingRestartNotifier: Error sending notification");
        }
    }
}
