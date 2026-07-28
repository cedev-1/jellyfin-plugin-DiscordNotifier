using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Templates;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Notifiers;

/// <summary>
/// Notifier that sends a Discord message when a library item's metadata is updated.
/// Suppresses updates on recently-created items (to avoid duplicating ItemAdded
/// notifications) and during library scans (which trigger bulk metadata refreshes).
/// </summary>
public class ItemUpdatedNotifier : IHostedService
{
    private static readonly TimeSpan PostAddSuppression = TimeSpan.FromMinutes(10);

    private readonly ILibraryManager _libraryManager;
    private readonly DiscordSender _sender;
    private readonly ILogger<ItemUpdatedNotifier> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemUpdatedNotifier"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="sender">The Discord sender service.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemUpdatedNotifier(ILibraryManager libraryManager, DiscordSender sender, ILogger<ItemUpdatedNotifier> logger)
    {
        _libraryManager = libraryManager;
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemUpdated += OnItemUpdated;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _libraryManager.ItemUpdated -= OnItemUpdated;
        return Task.CompletedTask;
    }

    private void OnItemUpdated(object? sender, ItemChangeEventArgs e)
    {
        _ = ProcessItemAsync(e.Item);
    }

    private async Task ProcessItemAsync(BaseItem item)
    {
        try
        {
            var config = Plugin.Config;
            if (!config.EnablePlugin || !config.ItemUpdatedNotifier || string.IsNullOrWhiteSpace(config.WebhookUrl))
            {
                return;
            }

            // Allow Series and MusicAlbum through (they inherit from Folder).
            if (item.IsVirtualItem || (item is Folder && item is not Series && item is not MusicAlbum))
            {
                return;
            }

            var typeName = item.GetType().Name;
            if (typeName is not ("Movie" or "Series" or "MusicAlbum" or "Book"))
            {
                return;
            }

            // Suppress during library scans — every item gets a metadata refresh,
            // producing hundreds of notifications for zero new information.
            if (_libraryManager.IsScanRunning)
            {
                return;
            }

            // Suppress items created less than 10 minutes ago to avoid echoing
            // the ItemAdded notification that was just sent.
            if (item.DateCreated != default && DateTime.UtcNow - item.DateCreated < PostAddSuppression)
            {
                return;
            }

            _logger.LogInformation("ItemUpdatedNotifier: {ItemName} ({ItemType}) metadata updated", item.Name, typeName);

            var message = ItemUpdatedTemplate.CreateMessage(item, config);
            await _sender.SendPostToWebhook(config.WebhookUrl, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ItemUpdatedNotifier: Error processing notification for {ItemName}", item?.Name);
        }
    }
}
