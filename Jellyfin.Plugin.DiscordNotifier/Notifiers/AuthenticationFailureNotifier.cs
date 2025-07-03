using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Templates;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier for failed authentication attempts that sends notifications to Discord.
/// </summary>
public class AuthenticationFailureNotifier : IEventConsumer<AuthenticationRequestEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<AuthenticationFailureNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationFailureNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthenticationFailureNotifier(DiscordSender sender, ILogger<AuthenticationFailureNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Handles the authentication failure event.
    /// </summary>
    /// <param name="eventArgs">The authentication request event arguments.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnEvent(AuthenticationRequestEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.AuthenticationFailureNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _logger.LogInformation("AuthenticationFailureNotifier is disabled or Webhook URL is missing.");
            return;
        }

        _logger.LogWarning("AuthenticationFailureNotifier: Login attempt (success unknown) for {Username}", eventArgs.Username);

        var message = AuthenticationFailureTemplate.CreateMessage(eventArgs, config);
        var json = JsonSerializer.Serialize(message);

        await _sender.SendPostToWebhook(config.WebhookUrl, json).ConfigureAwait(false);
    }
}
