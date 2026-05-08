using YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.ServiceBus.Core.InMemoryBus;

public abstract class InMemoryPublishEvent : PublishEvent
{
    public override ServiceBusProvider Destination() => ServiceBusProvider.InMemory;

}