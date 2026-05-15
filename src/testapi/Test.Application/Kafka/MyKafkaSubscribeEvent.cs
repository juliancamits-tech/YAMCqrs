using YAMCqrs.EventBus.Provider.Kafka.Abstractions;

namespace Test.Application.Kafka;

internal sealed class MyKafkaSubscribeEvent() : KafkaSubscribeEvent(MyKafkaPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
