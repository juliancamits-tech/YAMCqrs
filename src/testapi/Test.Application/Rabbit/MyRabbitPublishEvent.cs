using Bogus;
using YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;

namespace Test.Application.Rabbit;

internal sealed class MyRabbitPublishEvent : RabbitPublishEvent
{
    public const string TopicName = "my.rabbit.event";
    public const string RoutingKeyName = "my.route";

    public MyRabbitPublishEvent()
    {
        var f = new Faker();
        this.RandomProduct = f.Commerce.Product();
    }

    public string RandomProduct { get; init; }

    public override string RoutingKey => RoutingKeyName;

    public override void AddCustomHeaders(ref Dictionary<string, string> headers)
    {
        return;
    }

    public override string Topic()
    {
        return TopicName;
    }
}
