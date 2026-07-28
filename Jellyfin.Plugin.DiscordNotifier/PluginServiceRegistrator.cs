using System;
using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.DiscordNotifier.Notifiers;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using MediaBrowser.Controller.Events.Session;
using MediaBrowser.Controller.Events.Updates;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.DiscordNotifier;

/// <summary>
/// Registers Jellyfin services and injects them into notifiers.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <summary>
    /// Registers Jellyfin services and injects them into notifiers.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="applicationHost">The application host.</param>
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHttpClient(DiscordSender.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        serviceCollection.AddSingleton<DiscordSender>();

        // User consumers.
        serviceCollection.AddSingleton<IEventConsumer<UserCreatedEventArgs>, UserCreatedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<UserDeletedEventArgs>, UserDeletedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<UserLockedOutEventArgs>, UserLockedOutNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<UserUpdatedEventArgs>, UserUpdatedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<UserPasswordChangedEventArgs>, UserPasswordChangedNotifier>();

        // Security consumers.
        serviceCollection.AddSingleton<IEventConsumer<AuthenticationResultEventArgs>, AuthenticationSuccessNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<AuthenticationRequestEventArgs>, AuthenticationFailureNotifier>();

        // Session consumers.
        serviceCollection.AddSingleton<IEventConsumer<SessionStartedEventArgs>, SessionStartNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<SessionEndedEventArgs>, SessionEndedNotifier>();

        // Media consumers.
        serviceCollection.AddSingleton<IHostedService, ItemAddedNotifier>();
        serviceCollection.AddSingleton<IHostedService, ItemDeletedNotifier>();
        serviceCollection.AddSingleton<IHostedService, ItemUpdatedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PlaybackStartEventArgs>, PlaybackStartNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PlaybackStopEventArgs>, PlaybackStopNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PlaybackProgressEventArgs>, PlaybackProgressNotifier>();

        // Server consumers.
        serviceCollection.AddSingleton<IHostedService, ServerStartedNotifier>();
        serviceCollection.AddSingleton<IHostedService, TaskCompletedNotifier>();
        serviceCollection.AddSingleton<IHostedService, PendingRestartNotifier>();

        // Plugin consumers.
        serviceCollection.AddSingleton<IEventConsumer<PluginInstallingEventArgs>, PluginInstallingNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PluginInstalledEventArgs>, PluginInstalledNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PluginUpdatedEventArgs>, PluginUpdatedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PluginUninstalledEventArgs>, PluginUninstalledNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<InstallationFailedEventArgs>, PluginInstallationFailedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<PluginInstallationCancelledEventArgs>, PluginInstallationCancelledNotifier>();

        // Media extras.
        serviceCollection.AddSingleton<IHostedService, SubtitleDownloadFailureNotifier>();
        serviceCollection.AddSingleton<IHostedService, LyricDownloadFailureNotifier>();
        serviceCollection.AddSingleton<IHostedService, UserDataSavedNotifier>();
    }
}