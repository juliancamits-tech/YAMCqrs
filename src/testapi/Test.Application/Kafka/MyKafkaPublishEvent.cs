using System.Security.Cryptography;
using YAMCqrs.EventBus.Provider.Kafka.Abstractions;

namespace Test.Application.Kafka;

internal sealed class MyKafkaPublishEvent : KafkaPublishEvent
{
    public const string TopicName = "kafka.event.test";

    public MyKafkaPublishEvent()
    {
        this.Numerito = RandomNumberGenerator.GetInt32(500);
    }

    public int Numerito { get; init; }

    public override Dictionary<string, string>? GetCustomHeaders()
    {
        return null;
    }

    public override string Topic()
    {
        return TopicName;
    }
}
