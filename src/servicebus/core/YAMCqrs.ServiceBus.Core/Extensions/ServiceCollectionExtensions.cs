using Microsoft.Extensions.DependencyInjection;
using YAMCqrs.BackgroundWorker.Extensions;
using YAMCqrs.ServiceBus.Core.EventBus.Abstractions;
using YAMCqrs.ServiceBus.Core.EventBus.Implementation;
using YAMCqrs.ServiceBus.Core.InMemoryBus;
using YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;
using YAMCqrs.ServiceBus.Core.PublishEvents.Implementation;
using YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.ServiceBus.Core.SubscribeEvents.Implementation;

namespace YAMCqrs.ServiceBus.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the CQRS services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static void ExtendServiceBusCqrs(this IServiceCollection services)
    {
        //TODO: Change this InMemoryImplementations to be configurable during the Program.cs configuration phase.
        services.AddSingleton<IPublishEventStore, InMemoryEventStore>();
        services.AddSingleton<ISubscribeEventStore, InMemoryMessagePersister>();


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

        services.AddBackgroundWorkerCore(null);
    }
}