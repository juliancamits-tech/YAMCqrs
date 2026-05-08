using YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.ServiceBus.Core.InMemoryBus;

public abstract class InMemorySuscribeEvent(string topic) : SubscribeEvent(topic)
{
    public override ServiceBusProvider Provider => ServiceBusProvider.InMemory;
}
