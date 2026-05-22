using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;

internal class TestEvent : PublishEvent
{
    public override ServiceBusProvider Destination()
    {
        return ServiceBusProvider.None;
    }

    public override Dictionary<string, string>? GetCustomHeaders()
    {
        return null;
    }

    public override string Topic()
    {
        return "Topic";
    }
}
