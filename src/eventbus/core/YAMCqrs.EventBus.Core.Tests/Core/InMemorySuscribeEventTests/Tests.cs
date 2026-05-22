using FluentAssertions;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemorySuscribeEventTests;

public class Tests
{
    [Fact]
    public void InMemoryPublishEvent_Provider()
    {
        var ev = new DomainEventSubscribeEvent();

        var des = ev.Provider;

        des.Should().Be(ServiceBusProvider.InMemory);
    }
}
