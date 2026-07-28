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
/// Template for ItemAdded notification.
/// </summary>
public static class ItemAddedTemplate
{
    /// <summary>
    /// Creates a Discord message when a new item is added to the library.
    /// </summary>
    /// <param name="item">The library item that was added.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(BaseItem item, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var typeLabel = item.GetType().Name switch
        {
            "Movie" => "Movie",
            "Episode" => "Episode",
            "Series" => "Series",
            "Audio" => "Track",
            "MusicAlbum" => "Album",
            _ => item.GetType().Name
        };

        var name = DiscordText.Safe(item.Name);
        var titleText = item.ProductionYear.HasValue
            ? $"New {typeLabel}: {name} ({item.ProductionYear})"
            : $"New {typeLabel}: {name}";
        titleText = DiscordText.Truncate(titleText, 256);

        string? description = null;

        if (item is Episode episode && !string.IsNullOrEmpty(episode.SeriesName))
        {
            description = $"**{DiscordText.Safe(episode.SeriesName)}**";
            if (episode.ParentIndexNumber.HasValue && episode.IndexNumber.HasValue)
            {
                description += $" — S{episode.ParentIndexNumber:D2}E{episode.IndexNumber:D2}";
            }
        }

        if (!string.IsNullOrWhiteSpace(item.Overview))
        {
            var overview = DiscordText.Truncate(item.Overview, 300);
            description = description is null ? overview : $"{description}\n\n{overview}";
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Url = $"{serverUrl}/web/#/details?id={item.Id}",
                    Color = 0x9B59B6,
                    Thumbnail = new DiscordEmbedMedia
                    {
                        Url = $"{serverUrl}/Items/{item.Id}/Images/Primary"
                    },
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }

    /// <summary>
    /// Creates a single Discord embed for a batch of episodes from the same series.
    /// </summary>
    /// <param name="episodes">The list of episodes (all from the same series).</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="isNewSeries">Whether the series itself was added alongside these episodes (brand-new series).</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateEpisodeGroupMessage(IReadOnlyList<Episode> episodes, PluginConfiguration config, bool isNewSeries = false)
    {
        if (episodes.Count == 0)
        {
            return new DiscordWebhookPayload();
        }

        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var first = episodes[0];
        var seriesName = DiscordText.Safe(first.SeriesName ?? first.Name);
        var seriesId = first.SeriesId;
        var year = first.ProductionYear;

        var label = isNewSeries ? "New Series" : "New Episodes";
        var titleText = year.HasValue
            ? $"{label}: {seriesName} ({year})"
            : $"{label}: {seriesName}";
        titleText = DiscordText.Truncate(titleText, 256);

        // Build description: episodes grouped by season.
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

        // Link and thumbnail: prefer the series item if available.
        var linkId = seriesId != Guid.Empty ? seriesId : first.Id;
        var thumbnailId = seriesId != Guid.Empty ? seriesId : first.Id;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Url = $"{serverUrl}/web/#/details?id={linkId}",
                    Color = 0x9B59B6,
                    Thumbnail = new DiscordEmbedMedia
                    {
                        Url = $"{serverUrl}/Items/{thumbnailId}/Images/Primary"
                    },
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }

    /// <summary>
    /// Creates a single Discord embed for a batch of tracks from the same album.
    /// </summary>
    /// <param name="tracks">The list of tracks (all from the same album).</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateTrackGroupMessage(IReadOnlyList<Audio> tracks, PluginConfiguration config)
    {
        if (tracks.Count == 0)
        {
            return new DiscordWebhookPayload();
        }

        string serverUrl = ServerUrlHelper.GetServerUrl(config);

        var first = tracks[0];
        var album = first.AlbumEntity;
        var albumName = DiscordText.Safe(!string.IsNullOrEmpty(first.Album) ? first.Album : album?.Name ?? first.Name);
        var albumId = album?.Id ?? Guid.Empty;
        var year = album?.ProductionYear ?? first.ProductionYear;

        var titleText = year.HasValue
            ? $"New Album: {albumName} ({year})"
            : $"New Album: {albumName}";
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

        var linkId = albumId != Guid.Empty ? albumId : first.Id;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = titleText,
                    Description = description,
                    Url = $"{serverUrl}/web/#/details?id={linkId}",
                    Color = 0x9B59B6,
                    Thumbnail = new DiscordEmbedMedia
                    {
                        Url = $"{serverUrl}/Items/{linkId}/Images/Primary"
                    },
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
