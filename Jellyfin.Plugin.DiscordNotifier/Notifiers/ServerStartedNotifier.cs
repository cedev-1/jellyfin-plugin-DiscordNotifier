using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when the Jellyfin server starts.
/// </summary>
public class ServerStartedNotifier : IHostedService
{
    private readonly DiscordSender _sender;
    private readonly ILogger<ServerStartedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerStartedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public ServerStartedNotifier(DiscordSender sender, ILogger<ServerStartedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.EnablePlugin || !config.ServerStartedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return Task.CompletedTask;
        }

        // Fire-and-forget so we never block Jellyfin startup (Kestrel, other hosted services).
        _ = SendAfterDelayAsync(config, cancellationToken);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SendAfterDelayAsync(Configuration.PluginConfiguration config, CancellationToken cancellationToken)
    {
        try
        {
            // Small delay to let the server finish initialization.
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("ServerStartedNotifier: sending server started notification");

            var message = ServerStartedTemplate.CreateMessage(config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancelled during startup delay — skip notification.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ServerStartedNotifier: Error sending notification");
        }
    }
}
