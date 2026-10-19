using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a scheduled task completes.
/// Subscribes to ITaskManager.TaskCompleted (plain .NET event) because
/// Jellyfin does not publish TaskCompletionEventArgs through IEventManager.
/// </summary>
public class TaskCompletedNotifier : IHostedService
{
    private readonly ITaskManager _taskManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<TaskCompletedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCompletedNotifier"/> class.
    /// </summary>
    /// <param name="taskManager">The task manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public TaskCompletedNotifier(ITaskManager taskManager, DiscordSender sender, ILogger<TaskCompletedNotifier> logger)
    {
        _taskManager = taskManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _taskManager.TaskCompleted += OnTaskCompleted;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _taskManager.TaskCompleted -= OnTaskCompleted;
        return Task.CompletedTask;
    }

    private void OnTaskCompleted(object? sender, TaskCompletionEventArgs e)
    {
        _ = HandleAsync(e);
    }

    private async Task HandleAsync(TaskCompletionEventArgs eventArgs)
    {
        try
        {
            var config = Plugin.Config;
            if (!config.EnablePlugin || !config.TaskCompletedNotifier || !config.HasWebhookFor(NotifierEvent.TaskCompleted))
            {
                return;
            }

            // When "only on failure" is enabled, skip Completed and Cancelled tasks.
            if (config.TaskCompletedOnlyOnFailure &&
                eventArgs.Result.Status is TaskCompletionStatus.Completed or TaskCompletionStatus.Cancelled)
            {
                return;
            }

            _logger.LogInformation(
                "TaskCompletedNotifier: {TaskName} finished with status {Status}",
                eventArgs.Task.Name,
                eventArgs.Result.Status);

            var message = TaskCompletedTemplate.CreateMessage(eventArgs, config);
            await _sender.SendAsync(NotifierEvent.TaskCompleted, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TaskCompletedNotifier: Error sending notification");
        }
    }
}
