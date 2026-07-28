using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a user marks content as watched or rates it.
/// Debounces events per (userId, reason) over a short window so "mark season watched"
/// produces a single notification instead of one per episode.
/// </summary>
public class UserDataSavedNotifier : IHostedService
{
    private const int BurstCap = 10;
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(5);

    private readonly IUserDataManager _userDataManager;
    private readonly IUserManager _userManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<UserDataSavedNotifier> _logger;

    private readonly ConcurrentDictionary<string, List<UserDataSaveEventArgs>> _pending = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastActivity = new();
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataSavedNotifier"/> class.
    /// </summary>
    /// <param name="userDataManager">The user data manager.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public UserDataSavedNotifier(
        IUserDataManager userDataManager,
        IUserManager userManager,
        DiscordSender sender,
        ILogger<UserDataSavedNotifier> logger)
    {
        _userDataManager = userDataManager;
        _userManager = userManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _userDataManager.UserDataSaved += OnUserDataSaved;
        _cts = new CancellationTokenSource();
        _ = ProcessLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _userDataManager.UserDataSaved -= OnUserDataSaved;
        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();
        }
    }

    private void OnUserDataSaved(object? sender, UserDataSaveEventArgs eventArgs)
    {
        if (eventArgs.Item is null || eventArgs.Item.IsThemeMedia)
        {
            return;
        }

        if (eventArgs.SaveReason is not (UserDataSaveReason.TogglePlayed or UserDataSaveReason.UpdateUserRating))
        {
            return;
        }

        var key = $"{eventArgs.UserId}:{eventArgs.SaveReason}";
        _pending.AddOrUpdate(
            key,
            _ => new List<UserDataSaveEventArgs> { eventArgs },
            (_, list) =>
            {
                lock (list)
                {
                    list.Add(eventArgs);
                }

                return list;
            });
        _lastActivity[key] = DateTime.UtcNow;
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    await FlushReadyGroupsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UserDataSavedNotifier: Flush iteration failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task FlushReadyGroupsAsync()
    {
        var now = DateTime.UtcNow;
        var readyKeys = _lastActivity
            .Where(kvp => now - kvp.Value >= DebounceWindow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in readyKeys)
        {
            if (!_pending.TryRemove(key, out var events))
            {
                continue;
            }

            _lastActivity.TryRemove(key, out _);
            await FlushGroupAsync(key, events).ConfigureAwait(false);
        }
    }

    private async Task FlushGroupAsync(string key, List<UserDataSaveEventArgs> events)
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.UserDataSavedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        if (events.Count == 0)
        {
            return;
        }

        var firstEvent = events[0];
        var user = _userManager.GetUserById(firstEvent.UserId);
        if (user is null)
        {
            return;
        }

        var userName = user.Username;

        // For small batches (1-3 items), send individual notifications.
        if (events.Count <= 3)
        {
            int sent = 0;
            foreach (var e in events)
            {
                if (sent >= BurstCap)
                {
                    break;
                }

                try
                {
                    var message = UserDataSavedTemplate.CreateMessage(e, userName, config);
                    await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UserDataSavedNotifier: Error sending notification");
                }
            }

            return;
        }

        // For large batches, send a single summary to avoid flooding.
        try
        {
            var reason = firstEvent.SaveReason;
            var action = reason == UserDataSaveReason.TogglePlayed
                ? (firstEvent.UserData.Played ? "marked as watched" : "marked as unwatched")
                : "rated";

            _logger.LogInformation(
                "UserDataSavedNotifier: {Username} {Action} {Count} items (batched)",
                userName,
                action,
                events.Count);

            var summary = UserDataSavedTemplate.CreateBatchMessage(userName, action, events.Count, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, summary).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserDataSavedNotifier: Error sending batch notification");
        }
    }
}
