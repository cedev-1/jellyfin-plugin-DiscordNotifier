using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Subtitles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when subtitle downloads fail.
/// Debounces failures over a 30-second window to send a single summary
/// instead of one webhook per failed item during a library scan.
/// </summary>
public class SubtitleDownloadFailureNotifier : IHostedService
{
    private const int IndividualCap = 3;
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(30);

    private readonly ISubtitleManager _subtitleManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<SubtitleDownloadFailureNotifier> _logger;

    private readonly ConcurrentQueue<SubtitleDownloadFailureEventArgs> _queue = new();
    private DateTime _lastEnqueue = DateTime.MinValue;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleDownloadFailureNotifier"/> class.
    /// </summary>
    /// <param name="subtitleManager">The subtitle manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public SubtitleDownloadFailureNotifier(ISubtitleManager subtitleManager, DiscordSender sender, ILogger<SubtitleDownloadFailureNotifier> logger)
    {
        _subtitleManager = subtitleManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subtitleManager.SubtitleDownloadFailure += OnSubtitleDownloadFailure;
        _cts = new CancellationTokenSource();
        _ = ProcessLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _subtitleManager.SubtitleDownloadFailure -= OnSubtitleDownloadFailure;
        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();
        }
    }

    private void OnSubtitleDownloadFailure(object? sender, SubtitleDownloadFailureEventArgs e)
    {
        if (e.Item is null)
        {
            return;
        }

        _queue.Enqueue(e);
        _lastEnqueue = DateTime.UtcNow;
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (_queue.IsEmpty || DateTime.UtcNow - _lastEnqueue < DebounceWindow)
                    {
                        continue;
                    }

                    await FlushAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SubtitleDownloadFailureNotifier: Flush iteration failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task FlushAsync()
    {
        var config = Plugin.Config;
        if (!config.EnablePlugin || !config.SubtitleDownloadFailureNotifier || !config.HasWebhookFor(NotifierEvent.SubtitleDownloadFailure))
        {
            while (_queue.TryDequeue(out _))
            {
            }

            return;
        }

        var events = new System.Collections.Generic.List<SubtitleDownloadFailureEventArgs>();
        while (_queue.TryDequeue(out var e))
        {
            events.Add(e);
        }

        if (events.Count == 0)
        {
            return;
        }

        // Small batch: send individual messages.
        if (events.Count <= IndividualCap)
        {
            foreach (var e in events)
            {
                try
                {
                    var message = SubtitleDownloadFailureTemplate.CreateMessage(e, config);
                    await _sender.SendAsync(NotifierEvent.SubtitleDownloadFailure, message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SubtitleDownloadFailureNotifier: Error sending notification");
                }
            }

            return;
        }

        // Large batch: send a single summary.
        try
        {
            _logger.LogInformation("SubtitleDownloadFailureNotifier: {Count} failures batched into summary", events.Count);

            var itemNames = string.Join("\n", events.Take(10).Select(e => $"• {DiscordText.Safe(e.Item.Name)}"));
            if (events.Count > 10)
            {
                itemNames += $"\n*… and {events.Count - 10} more*";
            }

            var summary = new DiscordWebhookPayload
            {
                Embeds =
                [
                    new DiscordEmbed
                    {
                        Title = $"Subtitle Download Failed: {events.Count} items",
                        Description = itemNames,
                        Color = 0xE74C3C,
                        Footer = DiscordEmbedFooter.FromConfig(config),
                        Timestamp = DateTime.UtcNow.ToString("o")
                    }
                ]
            };

            await _sender.SendAsync(NotifierEvent.SubtitleDownloadFailure, summary).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubtitleDownloadFailureNotifier: Error sending batch notification");
        }
    }
}
