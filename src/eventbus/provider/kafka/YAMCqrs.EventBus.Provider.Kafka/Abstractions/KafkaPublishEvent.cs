using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.EventBus.Provider.Kafka.Abstractions;

public abstract class KafkaPublishEvent : PublishEvent
{
    public override ServiceBusProvider Destination() => ServiceBusProvider.Kafka;
}
