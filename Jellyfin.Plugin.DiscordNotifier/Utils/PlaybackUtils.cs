using System;
using System.Globalization;

namespace Jellyfin.Plugin.DiscordNotifier.Utils;

/// <summary>
/// Shared helpers for playback notification formatting.
/// </summary>
public static class PlaybackUtils
{
    private const int BarLength = 20;

    /// <summary>
    /// Builds a Unicode progress bar string for a playback position.
    /// Example output: <c>`▓▓▓▓▓░░░░░░░░░░░░░░░` 25% (00:32:11 / 02:08:44)</c>.
    /// </summary>
    /// <param name="positionTicks">Current position in ticks.</param>
    /// <param name="runtimeTicks">Total duration in ticks.</param>
    /// <returns>A formatted progress string, or an empty string if data is unavailable.</returns>
    public static string BuildProgressBar(long? positionTicks, long? runtimeTicks)
    {
        if (!positionTicks.HasValue || !runtimeTicks.HasValue || runtimeTicks.Value <= 0)
        {
            return string.Empty;
        }

        var percent = (double)positionTicks.Value / runtimeTicks.Value;
        percent = Math.Clamp(percent, 0, 1);

        var filled = (int)(percent * BarLength);
        var bar = new string('▓', filled) + new string('░', BarLength - filled);

        return $"`{bar}` {percent:P0}  ({FormatDuration(positionTicks.Value)} / {FormatDuration(runtimeTicks.Value)})";
    }

    /// <summary>
    /// Formats a ticks value as a human-readable duration string.
    /// </summary>
    /// <param name="ticks">Duration in ticks.</param>
    /// <returns>Formatted duration (e.g. "1:23:45" or "23:45").</returns>
    public static string FormatDuration(long ticks)
    {
        var ts = TimeSpan.FromTicks(ticks);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : ts.ToString(@"m\:ss", CultureInfo.InvariantCulture);
    }
}
