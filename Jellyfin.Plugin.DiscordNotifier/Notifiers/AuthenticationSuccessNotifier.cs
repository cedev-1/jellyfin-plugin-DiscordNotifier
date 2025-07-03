using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Templates;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier for successful authentication attempts that sends notifications to Discord.
/// </summary>
public class AuthenticationSuccessNotifier : IEventConsumer<AuthenticationResultEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<AuthenticationSuccessNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationSuccessNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthenticationSuccessNotifier(DiscordSender sender, ILogger<AuthenticationSuccessNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Handles the authentication success event.
    /// </summary>
    /// <param name="eventArgs">The authentication result event arguments.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnEvent(AuthenticationResultEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.AuthenticationSuccessNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _logger.LogInformation("AuthenticationSuccessNotifier is disabled or Webhook URL is missing.");
            return;
        }

        _logger.LogInformation("AuthenticationSuccessNotifier: Success for user {Username}", eventArgs.User.Name);

        var message = AuthenticationSuccessTemplate.CreateMessage(eventArgs, config);
        var json = JsonSerializer.Serialize(message);

        await _sender.SendPostToWebhook(config.WebhookUrl, json).ConfigureAwait(false);
    }
}
