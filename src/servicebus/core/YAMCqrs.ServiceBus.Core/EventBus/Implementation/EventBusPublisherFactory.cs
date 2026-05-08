using Microsoft.Extensions.DependencyInjection;
using YAMCqrs.ServiceBus.Core.EventBus.Abstractions;

namespace YAMCqrs.ServiceBus.Core.EventBus.Implementation;

/// <summary>
/// Factory for creating event bus publisher instances based on provider type
/// </summary>
internal sealed class EventBusPublisherFactory(IServiceProvider serviceProvider) : IEventBusPublisherFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IEventBusPublisher GetPublisher(ServiceBusProvider provider)
    {
        var publisher = _serviceProvider.GetRequiredKeyedService<IEventBusPublisher>(provider);

        return publisher;
    }
}