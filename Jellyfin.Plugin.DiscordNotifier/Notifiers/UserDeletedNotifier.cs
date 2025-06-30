using System.Text.Json;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers
{
    /// <summary>
    /// Notifier that sends a Discord message when a user is deleted.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserDeletedNotifier"/> class.
    /// </remarks>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="applicationHost">The server application host.</param>
    public class UserDeletedNotifier(
        DiscordSender sender,
        ILogger<UserDeletedNotifier> logger,
        IServerApplicationHost applicationHost) : IEventConsumer<UserDeletedEventArgs>
    {
        private readonly DiscordSender _sender = sender;
        private readonly ILogger<UserDeletedNotifier> _logger = logger;
        private readonly IServerApplicationHost _applicationHost = applicationHost;

        /// <summary>
        /// Handles the user deleted event by sending a notification to Discord.
        /// </summary>
        /// <param name="eventArgs">The event arguments containing user deletion details.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnEvent(UserDeletedEventArgs eventArgs)
        {
            ArgumentNullException.ThrowIfNull(eventArgs);

            var config = Plugin.Config;
            var webhookUrl = config.WebhookUrl;

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("Webhook URL is not set.");
                return;
            }

            if (!config.EnablePlugin || !config.UserDeletedNotifier)
            {
                _logger.LogInformation("Plugin or UserDeletedNotifier is disabled.");
                return;
            }

            _logger.LogInformation("UserDeletedNotifier: User deleted event received for user {Username}", eventArgs.Argument.Username);

            var messageObject = UserDeletedTemplate.CreateMessage(eventArgs, config);
            var jsonMessage = JsonSerializer.Serialize(messageObject);

            _logger.LogInformation("UserDeletedNotifier: Sending message to Discord");
            await _sender.SendPostToWebhook(webhookUrl, jsonMessage).ConfigureAwait(false);
        }
    }
}
