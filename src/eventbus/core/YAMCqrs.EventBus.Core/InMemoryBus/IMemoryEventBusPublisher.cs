using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.InMemoryBus;

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