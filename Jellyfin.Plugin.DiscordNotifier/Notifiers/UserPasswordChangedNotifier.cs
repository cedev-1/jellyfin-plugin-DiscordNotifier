using System;
using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a user changes their password.
/// </summary>
public class UserPasswordChangedNotifier : IEventConsumer<UserPasswordChangedEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<UserPasswordChangedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPasswordChangedNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public UserPasswordChangedNotifier(DiscordSender sender, ILogger<UserPasswordChangedNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(UserPasswordChangedEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.UserPasswordChangedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            _logger.LogInformation("UserPasswordChangedNotifier: {Username} changed their password", eventArgs.Argument.Username);

            var message = UserPasswordChangedTemplate.CreateMessage(eventArgs, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserPasswordChangedNotifier: Error sending notification");
        }
    }
}
