using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Session;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for PlaybackStart notification.
/// </summary>
public static class PlaybackStartTemplate
{
    /// <summary>
    /// Creates a Discord message when a playback session starts.
    /// </summary>
    /// <param name="eventArgs">The playback start event arguments.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(PlaybackStartEventArgs eventArgs, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var session = eventArgs.Session;
        var item = eventArgs.Item;
        var playMethod = session?.PlayState?.PlayMethod;
        var isTranscoding = playMethod == PlayMethod.Transcode;
        var positionTicks = session?.PlayState?.PositionTicks;
        var runtimeTicks = item?.RunTimeTicks;

        var userName = session?.UserName ?? "Unknown";
        var isResuming = positionTicks.HasValue && positionTicks.Value > 0;
        var action = isResuming ? "Playback Resumed" : "Playback Started";

        var authorName = BuildAuthorName(action, item);
        var authorUrl = item is not null ? $"{serverUrl}/web/index.html#!/details?id={item.Id}" : null;

        var overview = item?.Overview;
        var progressBar = PlaybackUtils.BuildProgressBar(positionTicks ?? 0, runtimeTicks);
        var description = BuildDescription(userName, overview, progressBar);

        var thumbnail = item is not null
            ? new DiscordEmbedMedia { Url = $"{serverUrl}/Items/{item.Id}/Images/Primary" }
            : null;

        if (!config.PlaybackDetailedMode)
        {
            return new DiscordWebhookPayload
            {
                Embeds =
                [
                    new DiscordEmbed
                    {
                        Author = new DiscordEmbedAuthor { Name = authorName, Url = authorUrl },
                        Description = description,
                        Color = 0x2ECC71,
                        Thumbnail = thumbnail,
                        Footer = DiscordEmbedFooter.FromConfig(config),
                        Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                    }
                ]
            };
        }

        var color = isTranscoding ? 0xE67E22 : 0x2ECC71;

        var fields = new List<DiscordEmbedField>
        {
            new() { Name = "Play Method", Value = playMethod?.ToString() ?? "Unknown", Inline = true },
            new() { Name = "Device", Value = session?.DeviceName ?? "Unknown", Inline = true },
            new() { Name = "Client", Value = session?.Client ?? "Unknown", Inline = true }
        };

        if (isTranscoding && session?.TranscodingInfo is { } info)
        {
            if (!string.IsNullOrEmpty(info.VideoCodec))
            {
                fields.Add(new DiscordEmbedField { Name = "Video Codec", Value = info.VideoCodec.ToUpperInvariant(), Inline = true });
            }

            if (!string.IsNullOrEmpty(info.AudioCodec))
            {
                fields.Add(new DiscordEmbedField { Name = "Audio Codec", Value = info.AudioCodec.ToUpperInvariant(), Inline = true });
            }

            if (info.TranscodeReasons != 0)
            {
                var reasonText = string.Join(", ", Array.ConvertAll(
                    info.TranscodeReasons.ToString().Split(','),
                    part => SplitCamelCase(part.Trim())));
                fields.Add(new DiscordEmbedField { Name = "Transcode Reasons", Value = reasonText, Inline = false });
            }
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Author = new DiscordEmbedAuthor { Name = authorName, Url = authorUrl },
                    Description = description,
                    Color = color,
                    Thumbnail = thumbnail,
                    Fields = fields.ToArray(),
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }

    private static string BuildAuthorName(string action, MediaBrowser.Controller.Entities.BaseItem? item)
    {
        if (item is null)
        {
            return action;
        }

        if (item is Episode episode)
        {
            var season = episode.ParentIndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            var ep = episode.IndexNumber?.ToString("00", CultureInfo.InvariantCulture) ?? "??";
            return $"{action} • {episode.SeriesName} S{season}E{ep} ~ {episode.Name}";
        }

        return item.ProductionYear.HasValue
            ? $"{action} • {item.Name} ({item.ProductionYear})"
            : $"{action} • {item.Name}";
    }

    private static string BuildDescription(string userName, string? overview, string progressBar)
    {
        var lines = new List<string> { $"**{userName}**" };

        if (!string.IsNullOrWhiteSpace(overview))
        {
            var truncated = overview.Length > 300 ? overview[..300] + "…" : overview;
            lines.Add($"> {truncated}");
        }

        if (!string.IsNullOrEmpty(progressBar))
        {
            lines.Add(progressBar);
        }

        return string.Join("\n\n", lines);
    }

    private static string SplitCamelCase(string value)
        => Regex.Replace(value, "([A-Z])", " $1").Trim();
}
