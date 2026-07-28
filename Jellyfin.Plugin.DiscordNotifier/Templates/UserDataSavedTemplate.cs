using System;
using System.Globalization;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Template for UserDataSaved notification.
/// </summary>
public static class UserDataSavedTemplate
{
    /// <summary>
    /// Creates a Discord message when a user marks content as watched/unwatched or rates it.
    /// </summary>
    /// <param name="eventArgs">The user data save event arguments.</param>
    /// <param name="userName">The name of the user who triggered the event.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateMessage(UserDataSaveEventArgs eventArgs, string userName, PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        var item = eventArgs.Item;
        var userData = eventArgs.UserData;
        var safeUserName = DiscordText.Safe(userName);

        string title;
        string description;
        int color;

        if (eventArgs.SaveReason == UserDataSaveReason.TogglePlayed)
        {
            var action = userData.Played ? "marked as watched" : "marked as unwatched";
            title = DiscordText.Truncate(BuildItemTitle(item), 256);
            description = $"**{safeUserName}** {action} this.";
            color = userData.Played ? 0x2ECC71 : 0x95A5A6;
        }
        else
        {
            title = DiscordText.Truncate(BuildItemTitle(item), 256);
            var stars = userData.Rating.HasValue
                ? BuildStars(userData.Rating.Value)
                : "No rating";
            description = $"**{safeUserName}** rated this: {stars}";
            color = 0xF1C40F;
        }

        var thumbnail = new DiscordEmbedMedia
        {
            Url = $"{serverUrl}/Items/{item.Id}/Images/Primary"
        };

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = title,
                    Description = description,
                    Url = $"{serverUrl}/web/#/details?id={item.Id}",
                    Color = color,
                    Thumbnail = thumbnail,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }

    /// <summary>
    /// Creates a summary Discord message for a batch of user data changes.
    /// </summary>
    /// <param name="userName">The name of the user.</param>
    /// <param name="action">The action description (e.g. "marked as watched").</param>
    /// <param name="count">The number of items affected.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateBatchMessage(string userName, string action, int count, PluginConfiguration config)
    {
        var safeUserName = DiscordText.Safe(userName);
        var color = action.Contains("watched", StringComparison.OrdinalIgnoreCase) ? 0x2ECC71 : 0xF1C40F;

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = $"Batch Update: {count} items",
                    Description = $"**{safeUserName}** {action} **{count}** items.",
                    Color = color,
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                }
            ]
        };
    }

    private static string BuildItemTitle(BaseItem item)
    {
        if (item is Episode episode && !string.IsNullOrEmpty(episode.SeriesName))
        {
            var s = episode.ParentIndexNumber?.ToString("D2", CultureInfo.InvariantCulture) ?? "??";
            var e = episode.IndexNumber?.ToString("D2", CultureInfo.InvariantCulture) ?? "??";
            return $"{DiscordText.Safe(episode.SeriesName)} — S{s}E{e} ~ {DiscordText.Safe(episode.Name)}";
        }

        var name = DiscordText.Safe(item.Name);
        return item.ProductionYear.HasValue
            ? $"{name} ({item.ProductionYear})"
            : name;
    }

    private static string BuildStars(double rating)
    {
        var rounded = (int)Math.Round(rating);
        var filled = new string('\u2605', Math.Clamp(rounded, 0, 5));
        var empty = new string('\u2606', Math.Clamp(5 - rounded, 0, 5));
        return $"{filled}{empty} ({rating:0.#}/5)";
    }
}
