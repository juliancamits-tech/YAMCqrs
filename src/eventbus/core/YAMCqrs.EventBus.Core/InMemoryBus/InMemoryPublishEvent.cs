using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.InMemoryBus;

public abstract class InMemoryPublishEvent : PublishEvent
{
    public override ServiceBusProvider Destination() => ServiceBusProvider.InMemory;

}