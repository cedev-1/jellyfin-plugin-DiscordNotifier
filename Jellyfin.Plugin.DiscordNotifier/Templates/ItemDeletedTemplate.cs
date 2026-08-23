using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for ItemDeleted notification.
/// </summary>
public static class ItemDeletedTemplate
{
    /// <summary>
    /// Creates a Discord message when a single library item is deleted.
    /// </summary>
    /// <param name="item">The item that was deleted.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(BaseItem item, PluginConfiguration config)
    {
        var typeLabel = item.GetType().Name switch
        {
            "Movie" => "Movie",
            "Episode" => "Episode",
            "Series" => "Series",
            "Audio" => "Track",
            "MusicAlbum" => "Album",
            "Book" => "Book",
            _ => item.GetType().Name
        };

        string titleText;
        if (item is Episode episode && !string.IsNullOrEmpty(episode.SeriesName))
        {
            var s = episode.ParentIndexNumber.GetValueOrDefault();
            var e = episode.IndexNumber.GetValueOrDefault();
            titleText = $"Deleted {typeLabel}: {DiscordText.Safe(episode.SeriesName)} — S{s:D2}E{e:D2} ~ {DiscordText.Safe(item.Name)}";
        }
        else
        {
            var name = DiscordText.Safe(item.Name);
            titleText = item.ProductionYear.HasValue
                ? $"Deleted {typeLabel}: {name} ({item.ProductionYear})"
                : $"Deleted {typeLabel}: {name}";
        }

        titleText = DiscordText.Truncate(titleText, 256);

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Color = 0xE74C3C,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }

    /// <summary>
    /// Creates a single Discord embed for a batch of deleted episodes from the same series.
    /// </summary>
    /// <param name="episodes">The list of deleted episodes (all from the same series).</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="seriesDeleted">Whether the series itself was also deleted.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateEpisodeGroupMessage(IReadOnlyList<Episode> episodes, PluginConfiguration config, bool seriesDeleted = false)
    {
        if (episodes.Count == 0)
        {
            return new DiscordWebhookPayload();
        }

        var first = episodes[0];
        var seriesName = DiscordText.Safe(first.SeriesName ?? first.Name);
        var year = first.ProductionYear;

        var label = seriesDeleted ? "Deleted Series" : "Deleted Episodes";
        var titleText = year.HasValue
            ? $"{label}: {seriesName} ({year})"
            : $"{label}: {seriesName}";
        titleText = DiscordText.Truncate(titleText, 256);

        var sb = new StringBuilder();
        var sorted = episodes
            .OrderBy(e => e.ParentIndexNumber ?? 0)
            .ThenBy(e => e.IndexNumber ?? 0)
            .ToList();

        const int MaxDescriptionLength = 3900;
        int epIndex = 0;

        foreach (var season in sorted.GroupBy(e => e.ParentIndexNumber))
        {
            var seasonLabel = season.Key.HasValue ? $"**Season {season.Key}**" : "**Specials**";
            sb.AppendLine(seasonLabel);

            foreach (var ep in season)
            {
                string epLine = ep.ParentIndexNumber.HasValue && ep.IndexNumber.HasValue
                    ? $"S{ep.ParentIndexNumber:D2}E{ep.IndexNumber:D2} — {DiscordText.Safe(ep.Name)}"
                    : DiscordText.Safe(ep.Name);

                if (sb.Length + epLine.Length + 4 > MaxDescriptionLength)
                {
                    int remaining = sorted.Count - epIndex;
                    sb.AppendLine(CultureInfo.InvariantCulture, $"*… and {remaining} more*");
                    goto doneBuilding;
                }

                sb.AppendLine(epLine);
                epIndex++;
            }

            sb.AppendLine();
        }

        doneBuilding:
        var description = sb.ToString().TrimEnd();

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Color = 0xE74C3C,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }

    /// <summary>
    /// Creates a single Discord embed for a batch of deleted tracks from the same album.
    /// </summary>
    /// <param name="tracks">The list of deleted tracks (all from the same album).</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateTrackGroupMessage(IReadOnlyList<Audio> tracks, PluginConfiguration config)
    {
        if (tracks.Count == 0)
        {
            return new DiscordWebhookPayload();
        }

        var first = tracks[0];
        var albumName = DiscordText.Safe(!string.IsNullOrEmpty(first.Album) ? first.Album : first.AlbumEntity?.Name ?? first.Name);
        var year = first.AlbumEntity?.ProductionYear ?? first.ProductionYear;

        var titleText = year.HasValue
            ? $"Deleted Album: {albumName} ({year})"
            : $"Deleted Album: {albumName}";
        titleText = DiscordText.Truncate(titleText, 256);

        var sb = new StringBuilder();

        var artist = first.AlbumArtists is { Count: > 0 } ? string.Join(", ", first.AlbumArtists) : null;
        if (!string.IsNullOrWhiteSpace(artist))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"**{DiscordText.Safe(artist)}**");
            sb.AppendLine();
        }

        var sorted = tracks
            .OrderBy(t => t.ParentIndexNumber ?? 0)
            .ThenBy(t => t.IndexNumber ?? 0)
            .ToList();

        const int MaxDescriptionLength = 3900;
        int trackIndex = 0;

        foreach (var track in sorted)
        {
            string trackLine = track.IndexNumber.HasValue
                ? $"{track.IndexNumber:D2}. {DiscordText.Safe(track.Name)}"
                : DiscordText.Safe(track.Name);

            if (sb.Length + trackLine.Length + 4 > MaxDescriptionLength)
            {
                int remaining = sorted.Count - trackIndex;
                sb.AppendLine(CultureInfo.InvariantCulture, $"*… and {remaining} more*");
                break;
            }

            sb.AppendLine(trackLine);
            trackIndex++;
        }

        var description = sb.ToString().TrimEnd();

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Color = 0xE74C3C,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
