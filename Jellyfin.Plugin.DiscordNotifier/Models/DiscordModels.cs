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
    /// <summary>Gets or sets the webhook bot display name override.</summary>
    [JsonPropertyName("username")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Username { get; set; }

    /// <summary>Gets or sets the webhook bot avatar URL override.</summary>
    [JsonPropertyName("avatar_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvatarUrl { get; set; }

    /// <summary>Gets the plain text content outside the embed.</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets the list of embeds.</summary>
    [JsonPropertyName("embeds")]
    public DiscordEmbed[] Embeds { get; init; } = [];

    /// <summary>Gets the allowed mentions configuration. Restricts all pings by default.</summary>
    [JsonPropertyName("allowed_mentions")]
    public DiscordAllowedMentions AllowedMentions { get; init; } = new();
}

/// <summary>
/// Controls which mentions are parsed in a Discord webhook message.
/// Default: no mentions are parsed (blocks @everyone, @here, role and user pings).
/// </summary>
public sealed class DiscordAllowedMentions
{
    /// <summary>Gets the list of mention types to parse (empty = none).</summary>
    [JsonPropertyName("parse")]
    public string[] Parse { get; init; } = [];
}

/// <summary>
/// Represents the author section of a Discord embed.
/// </summary>
public sealed class DiscordEmbedAuthor
{
    /// <summary>Gets the author name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the URL that the author name links to.</summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }
}

/// <summary>
/// Represents a single Discord embed.
/// </summary>
public sealed class DiscordEmbed
{
    /// <summary>Gets the author displayed above the title.</summary>
    [JsonPropertyName("author")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordEmbedAuthor? Author { get; init; }

    /// <summary>Gets the embed title.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    /// <summary>Gets the embed description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>Gets the URL that the title links to.</summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>Gets the embed color (decimal integer).</summary>
    [JsonPropertyName("color")]
    public int Color { get; init; }

    /// <summary>Gets the list of fields.</summary>
    [JsonPropertyName("fields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordEmbedField[]? Fields { get; init; }

    /// <summary>Gets the thumbnail image displayed to the right of the embed.</summary>
    [JsonPropertyName("thumbnail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordEmbedMedia? Thumbnail { get; init; }

    /// <summary>Gets the footer.</summary>
    [JsonPropertyName("footer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordEmbedFooter? Footer { get; init; }

    /// <summary>Gets the ISO 8601 timestamp.</summary>
    [JsonPropertyName("timestamp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Timestamp { get; init; }
}

/// <summary>
/// Represents a media object (image or thumbnail) inside a Discord embed.
/// </summary>
public sealed class DiscordEmbedMedia
{
    /// <summary>Gets the media URL.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
