using System;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.DiscordNotifier.Utils;

/// <summary>
/// Helpers for sanitizing text before embedding it in a Discord message.
/// </summary>
public static partial class DiscordText
{
    /// <summary>
    /// Escapes Discord markdown characters, breaks @everyone/@here mentions,
    /// and truncates to the given maximum length.
    /// Use on every externally-sourced string (usernames, device names, item names, error messages).
    /// </summary>
    /// <param name="value">The raw string.</param>
    /// <param name="maxLength">Maximum allowed length (default 256 for titles).</param>
    /// <returns>A safe, truncated string. Returns <c>"Unknown"</c> for null/whitespace input.</returns>
    public static string Safe(string? value, int maxLength = 256)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        // Escape markdown special characters.
        var escaped = MarkdownChars().Replace(value, @"\$1");

        // Break @everyone / @here by inserting a zero-width space.
        escaped = escaped.Replace("@everyone", "@\u200beveryone", StringComparison.OrdinalIgnoreCase);
        escaped = escaped.Replace("@here", "@\u200bhere", StringComparison.OrdinalIgnoreCase);

        if (escaped.Length > maxLength)
        {
            escaped = string.Concat(escaped.AsSpan(0, maxLength - 1), "…");
        }

        return escaped;
    }

    /// <summary>
    /// Truncates a string to the specified length without markdown escaping.
    /// Use for values that are already known-safe (e.g. enum labels, generated text).
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum allowed length.</param>
    /// <returns>The truncated string, or <c>"Unknown"</c> for null/whitespace.</returns>
    public static string Truncate(string? value, int maxLength = 256)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        return value.Length > maxLength
            ? string.Concat(value.AsSpan(0, maxLength - 1), "…")
            : value;
    }

    [GeneratedRegex(@"([\\`*_~|>\[\]()@])")]
    private static partial Regex MarkdownChars();
}
