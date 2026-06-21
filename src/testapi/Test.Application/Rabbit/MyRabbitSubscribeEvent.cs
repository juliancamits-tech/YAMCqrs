using YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;

namespace Test.Application.Rabbit;

internal sealed class MyRabbitSubscribeEvent() : RabbitSubscribeEvent(MyRabbitPublishEvent.TopicName)
{
    public override string RoutingKey => MyRabbitPublishEvent.RoutingKeyName;

    public string RandomProduct { get; set; } = string.Empty;
}
