using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;

public abstract class RabbitSubscribeEvent(string topic) : SubscribeEvent(topic)
{
    public override ServiceBusProvider Provider => ServiceBusProvider.RabbitMq;
    public abstract string RoutingKey { get; }
}
