using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Events.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Plugin.DiscordNotifier.Notifiers;

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
        serviceCollection.AddSingleton<DiscordSender>();

        serviceCollection.AddSingleton<IEventConsumer<UserCreatedEventArgs>, UserCreatedNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<UserDeletedEventArgs>, UserDeletedNotifier>();

        serviceCollection.AddSingleton<IEventConsumer<AuthenticationResultEventArgs>, AuthenticationSuccessNotifier>();
        serviceCollection.AddSingleton<IEventConsumer<AuthenticationRequestEventArgs>, AuthenticationFailureNotifier>();
    }
}