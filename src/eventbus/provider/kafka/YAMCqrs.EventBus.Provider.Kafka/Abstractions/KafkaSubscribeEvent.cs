using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.EventBus.Provider.Kafka.Abstractions;

public class KafkaSubscribeEvent(string topic) : SubscribeEvent(topic)
{
    public override ServiceBusProvider Provider => ServiceBusProvider.Kafka;
}
