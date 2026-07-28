using System;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Jellyfin.Plugin.DiscordNotifier.Utils;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Updates;

namespace Jellyfin.Plugin.DiscordNotifier.Templates;

/// <summary>
/// Shared template for all plugin lifecycle notifications.
/// </summary>
public static class PluginEventTemplate
{
    /// <summary>
    /// Creates a Discord message when a plugin is installed.
    /// </summary>
    /// <param name="info">The installation info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateInstalledMessage(InstallationInfo info, PluginConfiguration config)
        => Build("Plugin Installed", $"**{DiscordText.Safe(info.Name)}** v{info.Version} has been installed.", 0x2ECC71, info.Id, info.Name, info.Version?.ToString(), info.Changelog, config);

    /// <summary>
    /// Creates a Discord message when a plugin is updated.
    /// </summary>
    /// <param name="info">The installation info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateUpdatedMessage(InstallationInfo info, PluginConfiguration config)
        => Build("Plugin Updated", $"**{DiscordText.Safe(info.Name)}** has been updated to v{info.Version}.", 0x3498DB, info.Id, info.Name, info.Version?.ToString(), info.Changelog, config);

    /// <summary>
    /// Creates a Discord message when a plugin installation starts.
    /// </summary>
    /// <param name="info">The installation info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateInstallingMessage(InstallationInfo info, PluginConfiguration config)
        => Build("Plugin Installing", $"Installing **{DiscordText.Safe(info.Name)}** v{info.Version}…", 0x3498DB, info.Id, info.Name, info.Version?.ToString(), null, config);

    /// <summary>
    /// Creates a Discord message when a plugin installation is cancelled.
    /// </summary>
    /// <param name="info">The installation info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateCancelledMessage(InstallationInfo info, PluginConfiguration config)
        => Build("Plugin Installation Cancelled", $"Installation of **{DiscordText.Safe(info.Name)}** v{info.Version} was cancelled.", 0x95A5A6, info.Id, info.Name, info.Version?.ToString(), null, config);

    /// <summary>
    /// Creates a Discord message when a plugin installation fails.
    /// </summary>
    /// <param name="info">The installation info.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateFailedMessage(InstallationInfo info, Exception? exception, PluginConfiguration config)
        => Build("Plugin Installation Failed", $"Failed to install **{DiscordText.Safe(info.Name)}** v{info.Version}.", 0xE74C3C, info.Id, info.Name, info.Version?.ToString(), exception?.Message, config);

    /// <summary>
    /// Creates a Discord message when a plugin is uninstalled.
    /// </summary>
    /// <param name="plugin">The plugin info.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <returns>The Discord webhook payload.</returns>
    public static DiscordWebhookPayload CreateUninstalledMessage(PluginInfo plugin, PluginConfiguration config)
        => Build("Plugin Uninstalled", $"**{DiscordText.Safe(plugin.Name)}** v{plugin.Version} has been uninstalled.", 0xE67E22, plugin.Id, plugin.Name, plugin.Version?.ToString(), null, config);

    private static DiscordWebhookPayload Build(
        string title,
        string description,
        int color,
        Guid pluginId,
        string pluginName,
        string? version,
        string? changelog,
        PluginConfiguration config)
    {
        string serverUrl = ServerUrlHelper.GetServerUrl(config);
        string? pluginUrl = pluginId != Guid.Empty
            ? $"{serverUrl}/web/#/dashboard/plugins/{pluginId:N}?name={Uri.EscapeDataString(pluginName)}"
            : null;

        var fields = new System.Collections.Generic.List<DiscordEmbedField>
        {
            new() { Name = "Plugin", Value = DiscordText.Safe(pluginName), Inline = true }
        };

        if (!string.IsNullOrWhiteSpace(version))
        {
            fields.Add(new DiscordEmbedField { Name = "Version", Value = version, Inline = true });
        }

        if (!string.IsNullOrWhiteSpace(changelog))
        {
            fields.Add(new DiscordEmbedField { Name = "Changelog", Value = DiscordText.Truncate(changelog, 1024), Inline = false });
        }

        return new DiscordWebhookPayload
        {
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = title,
                    Description = description,
                    Url = pluginUrl,
                    Color = color,
                    Fields = fields.ToArray(),
                    Footer = DiscordEmbedFooter.FromConfig(config),
                    Timestamp = DateTime.UtcNow.ToString("o")
                }
            ]
        };
    }
}
