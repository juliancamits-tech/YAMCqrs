using Microsoft.Extensions.DependencyInjection;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.EventBus.Implementation;
using YAMCqrs.EventBus.Core.InMemoryBus;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Implementation;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Implementation;

namespace YAMCqrs.EventBus.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public interface IEventBusBuilder
    {
        IServiceCollection Services { get; }
        EventBusConfiguration Configuration { get; }
    }

    public class EventBusBuilder(IServiceCollection services, EventBusConfiguration configuration) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
        public EventBusConfiguration Configuration => configuration;
    }

    /// <summary>
    /// Adds the CQRS services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IEventBusBuilder ExtendEventBusCqrs(this IServiceCollection services, EventBusConfiguration cfg)
    {
        services.AddSingleton<IPublishEventStore, InMemoryPublishEventStore>();
        services.AddSingleton<ISubscribeEventStore, InMemorySubscribeEventStore>();


        //Implementation of the InMemory Service bus used for "Domain Event"
        // FIRST: Direct registration with specific interface (creates the real instance)
        services.AddSingleton<IMemoryEventBusPublisher, InMemoryPublisher>();
        // SECOND: Keyed registration using the specific interface (alias/reference only)
        services.AddKeyedSingleton<IEventBusPublisher>(ServiceBusProvider.InMemory,
        (sp, key) => sp.GetRequiredService<IMemoryEventBusPublisher>());

        //Interface used for the Dev for publish message to the service bus.
        services.AddScoped<IEventPublisher, EventPublisher>();
        //Resolver for the IEventBusPublisher based on the ServiceBusProvider enum, this will be used by the EventPublisher to get the correct implementation of the IEventBusPublisher.
        services.AddSingleton<IEventBusPublisherFactory, EventBusPublisherFactory>();
        //Registering the event handlers for the publish events, this will be used by the EventPublisher to get the correct event handler for the event to be published.
        services.AddSingleton<IEventHandler<PublishEvent>, PublishEventHandler<PublishEvent>>();
        services.AddTransient(typeof(IEventHandler<>), typeof(PublishEventHandler<>));
        //Background Service that process the event to be published to the service bus.
        services.AddHostedService<PublishEventProcessorService>();
        //Background Service that process the event received from the service bus.
        services.AddHostedService<SubscribeEventProcessorService>();

        return new EventBusBuilder(services, cfg);
    }
}