using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Lyrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when lyric downloads fail.
/// Debounces failures over a 30-second window to send a single summary
/// instead of one webhook per failed item during a library scan.
/// </summary>
public class LyricDownloadFailureNotifier : IHostedService
{
    private const int IndividualCap = 3;
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(30);

    private readonly ILyricManager _lyricManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<LyricDownloadFailureNotifier> _logger;

    private readonly ConcurrentQueue<LyricDownloadFailureEventArgs> _queue = new();
    private DateTime _lastEnqueue = DateTime.MinValue;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="LyricDownloadFailureNotifier"/> class.
    /// </summary>
    /// <param name="lyricManager">The lyric manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public LyricDownloadFailureNotifier(ILyricManager lyricManager, DiscordSender sender, ILogger<LyricDownloadFailureNotifier> logger)
    {
        _lyricManager = lyricManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lyricManager.LyricDownloadFailure += OnLyricDownloadFailure;
        _cts = new CancellationTokenSource();
        _ = ProcessLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _lyricManager.LyricDownloadFailure -= OnLyricDownloadFailure;
        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();
        }
    }

    private void OnLyricDownloadFailure(object? sender, LyricDownloadFailureEventArgs e)
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
                    _logger.LogError(ex, "LyricDownloadFailureNotifier: Flush iteration failed");
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
        if (!config.EnablePlugin || !config.LyricDownloadFailureNotifier || !config.HasWebhookFor(NotifierEvent.LyricDownloadFailure))
        {
            while (_queue.TryDequeue(out _))
            {
            }

            return;
        }

        var events = new System.Collections.Generic.List<LyricDownloadFailureEventArgs>();
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
                    var message = LyricDownloadFailureTemplate.CreateMessage(e, config);
                    await _sender.SendAsync(NotifierEvent.LyricDownloadFailure, message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LyricDownloadFailureNotifier: Error sending notification");
                }
            }

            return;
        }

        // Large batch: send a single summary.
        try
        {
            _logger.LogInformation("LyricDownloadFailureNotifier: {Count} failures batched into summary", events.Count);

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
                        Title = $"Lyric Download Failed: {events.Count} items",
                        Description = itemNames,
                        Color = 0xE74C3C,
                        Footer = DiscordEmbedFooter.FromConfig(config),
                        Timestamp = DateTime.UtcNow.ToString("o")
                    }
                ]
            };

            await _sender.SendAsync(NotifierEvent.LyricDownloadFailure, summary).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LyricDownloadFailureNotifier: Error sending batch notification");
        }
    }
}
