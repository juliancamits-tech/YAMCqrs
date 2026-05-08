using YAMCqrs.ServiceBus.Core.EventBus.Abstractions;
using YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.ServiceBus.Core.InMemoryBus;

public interface IMemoryEventBusPublisher : IEventBusPublisher
{
}

public class InMemoryPublisher(ISubscribeEventStore messagePersister) : IMemoryEventBusPublisher
{
    public Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        return messagePersister.StoreAsync(topic, jsonValue, headers, ServiceBusProvider.InMemory, cancellationToken);
    }
}