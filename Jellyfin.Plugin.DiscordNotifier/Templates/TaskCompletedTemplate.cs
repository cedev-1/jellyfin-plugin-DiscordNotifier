using System;
using System.Globalization;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for TaskCompleted notification.
/// </summary>
public static class TaskCompletedTemplate
{
    /// <summary>
    /// Creates a Discord message when a scheduled task completes.
    /// </summary>
    /// <param name="eventArgs">The task completion event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(TaskCompletionEventArgs eventArgs, PluginConfiguration config)
    {
        var result = eventArgs.Result;
        var task = eventArgs.Task;

        var status = result.Status;
        var color = status switch
        {
            TaskCompletionStatus.Completed => 0x2ECC71,
            TaskCompletionStatus.Failed => 0xE74C3C,
            TaskCompletionStatus.Cancelled => 0x95A5A6,
            TaskCompletionStatus.Aborted => 0xE67E22,
            _ => 0x95A5A6
        };

        var statusLabel = status switch
        {
            TaskCompletionStatus.Completed => "Completed",
            TaskCompletionStatus.Failed => "Failed",
            TaskCompletionStatus.Cancelled => "Cancelled",
            TaskCompletionStatus.Aborted => "Aborted",
            _ => status.ToString()
        };

        var duration = result.EndTimeUtc - result.StartTimeUtc;
        var durationText = duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);

        var titleText = DiscordText.Truncate($"Task {statusLabel}: {task.Name}", 256);

        var fields = new System.Collections.Generic.List<DiscordEmbedField>
        {
            new() { Name = "Status", Value = statusLabel, Inline = true },
            new() { Name = "Duration", Value = durationText, Inline = true },
            new() { Name = "Category", Value = DiscordText.Truncate(task.Category, 256), Inline = true }
        };

        if (status == TaskCompletionStatus.Failed && !string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            fields.Add(new DiscordEmbedField { Name = "Error", Value = DiscordText.Truncate(result.ErrorMessage, 1024), Inline = false });
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = string.IsNullOrWhiteSpace(task.Description) ? null : DiscordText.Truncate(task.Description, 4096),
                    Color = color,
                    Fields = fields.ToArray(),
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }
}
