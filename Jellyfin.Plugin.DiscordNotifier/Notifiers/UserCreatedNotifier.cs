using System.Text.Json;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a new user is created.
/// </summary>
public class UserCreatedNotifier : IEventConsumer<UserCreatedEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<UserCreatedNotifier> _logger;
    private readonly IServerApplicationHost _applicationHost;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCreatedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="applicationHost">The server application host.</param>
    public UserCreatedNotifier(
        DiscordSender sender,
        ILogger<UserCreatedNotifier> logger,
        IServerApplicationHost applicationHost)
    {
        _sender = sender;
        _logger = logger;
        _applicationHost = applicationHost;
    }

    /// <summary>
    /// Handles the user created event by sending a notification to Discord.
    /// </summary>
    /// <param name="eventArgs">The event arguments containing user creation details.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnEvent(UserCreatedEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        var config = Plugin.Config;
        var webhookUrl = config.WebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogError("Webhook URL is not set.");
            return;
        }

        if (!config.EnablePlugin)
        {
            _logger.LogInformation("Plugin is disabled.");
            return;
        }

        if (!config.UserCreatedNotifier)
        {
            _logger.LogInformation("UserCreatedNotifier is disabled.");
            return;
        }

        _logger.LogInformation("UserCreatedNotifier: User created event received for user {Username}", eventArgs.Argument.Username);

        var messageObject = UserCreatedTemplate.CreateMessage(eventArgs, config);
        var jsonMessage = JsonSerializer.Serialize(messageObject);

        _logger.LogInformation("UserCreatedNotifier: Sending message to Discord");
        await _sender.SendPostToWebhook(webhookUrl, jsonMessage).ConfigureAwait(false);
    }
}