using FluentAssertions;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemoryPublishEventTests;

public class Tests
{
    [Fact]
    public void InMemoryPublishEvent_Provider()
    {
        var ev = new DomainEventPublishEvent();

        var des = ev.Destination();

        des.Should().Be(ServiceBusProvider.InMemory);
    }
}
