#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
using System;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.DiscordNotifier.Configuration;

namespace Jellyfin.Plugin.DiscordNotifier.Models;

/// <summary>
/// Represents a Discord webhook payload.
/// </summary>
public sealed class DiscordWebhookPayload
{
    /// <summary>Gets the plain text content outside the embed.</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets the list of embeds.</summary>
    [JsonPropertyName("embeds")]
    public DiscordEmbed[] Embeds { get; init; } = [];
}

/// <summary>
/// Represents a single Discord embed.
/// </summary>
public sealed class DiscordEmbed
{
    /// <summary>Gets the embed title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the embed description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets the URL that the title links to.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the embed color (decimal integer).</summary>
    [JsonPropertyName("color")]
    public int Color { get; init; }

    /// <summary>Gets the list of fields.</summary>
    [JsonPropertyName("fields")]
    public DiscordEmbedField[]? Fields { get; init; }

    /// <summary>Gets the footer.</summary>
    [JsonPropertyName("footer")]
    public DiscordEmbedFooter? Footer { get; init; }

    /// <summary>Gets the ISO 8601 timestamp.</summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; init; }
}

/// <summary>
/// Represents a field inside a Discord embed.
/// </summary>
public sealed class DiscordEmbedField
{
    /// <summary>Gets the field name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the field value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the field is displayed inline.</summary>
    [JsonPropertyName("inline")]
    public bool Inline { get; init; }
}

/// <summary>
/// Represents the footer of a Discord embed.
/// </summary>
public sealed class DiscordEmbedFooter
{
    private const string JellyfinIconUrl = "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fstatic0.xdaimages.com%2Fwordpress%2Fwp-content%2Fuploads%2F2024%2F02%2Fjellyfin-logo.png%3Fq%3D70%26fit%3Dcontain%26w%3D420%26dpr%3D1&f=1&nofb=1&ipt=403502d7a0586532bfb11842ad227f3212229625ccbf001cd1f60dc0abf41465";

    /// <summary>Gets the footer text.</summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    /// <summary>Gets the footer icon URL.</summary>
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; init; }

    /// <summary>
    /// Creates a footer using the server name from configuration.
    /// </summary>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>A <see cref="DiscordEmbedFooter"/> with the appropriate text and icon.</returns>
    public static DiscordEmbedFooter FromConfig(PluginConfiguration config)
    {
        var text = string.IsNullOrWhiteSpace(config.ServerName)
            ? "Jellyfin Discord Notifier"
            : $"{config.ServerName} • Jellyfin";

        return new DiscordEmbedFooter { Text = text, IconUrl = JellyfinIconUrl };
    }
}
