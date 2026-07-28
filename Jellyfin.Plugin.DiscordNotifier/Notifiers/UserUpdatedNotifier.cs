using System;
using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a user account is updated.
/// </summary>
public class UserUpdatedNotifier : IEventConsumer<UserUpdatedEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<UserUpdatedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserUpdatedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public UserUpdatedNotifier(DiscordSender sender, ILogger<UserUpdatedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(UserUpdatedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.UserUpdatedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            _logger.LogInformation("UserUpdatedNotifier: {Username} account updated", eventArgs.Argument.Username);

            var message = UserUpdatedTemplate.CreateMessage(eventArgs, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserUpdatedNotifier: Error sending notification");
        }
    }
}
