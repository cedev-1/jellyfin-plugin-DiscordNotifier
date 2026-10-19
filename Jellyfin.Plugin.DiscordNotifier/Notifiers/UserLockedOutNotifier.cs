using System;
using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a user account is locked out.
/// </summary>
public class UserLockedOutNotifier : IEventConsumer<UserLockedOutEventArgs>
{
    private readonly DiscordSender _sender;
    private readonly ILogger<UserLockedOutNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLockedOutNotifier"/> class.
    /// </summary>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public UserLockedOutNotifier(DiscordSender sender, ILogger<UserLockedOutNotifier> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(UserLockedOutEventArgs eventArgs)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.UserLockedOutNotifier || !config.HasWebhookFor(NotifierEvent.UserLockedOut))
        {
            return;
        }

        try
        {
            _logger.LogInformation("UserLockedOutNotifier: {Username} has been locked out", eventArgs.Argument.Username);

            var message = UserLockedOutTemplate.CreateMessage(eventArgs, config);
            await _sender.SendAsync(NotifierEvent.UserLockedOut, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserLockedOutNotifier: Error sending notification");
        }
    }
}
