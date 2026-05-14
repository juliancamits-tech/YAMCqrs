using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.InMemoryBus;

public abstract class InMemorySuscribeEvent(string topic) : SubscribeEvent(topic)
{
    public override ServiceBusProvider Provider => ServiceBusProvider.InMemory;
}
