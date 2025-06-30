using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Plugin.DiscordNotifier.Notifiers;
using Jellyfin.Plugin.DiscordNotifier.Configuration;

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
        // Register sender.
        serviceCollection.AddScoped<DiscordSender>();

        // Register PluginConfiguration
        serviceCollection.AddScoped<PluginConfiguration>();

        // Register application host
        serviceCollection.AddSingleton(applicationHost);

        // User consumers.
        serviceCollection.AddSingleton<IEventConsumer<UserCreatedEventArgs>, UserCreatedNotifier>();

        serviceCollection.AddScoped<IEventConsumer<UserDeletedEventArgs>, UserDeletedNotifier>();
    }
}