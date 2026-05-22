using FluentAssertions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Core.Tests.Core.SubscribeStoredEventTests;

public class Tests
{
    private static SubscribeStoredEvent CreateEvent()
    {
        return new SubscribeStoredEvent(Guid.CreateVersion7(), "Test", "Test", null, "Test", ServiceBusProvider.None);
    }

    [Fact]
    public void Event_Succes()
    {
        var ev = CreateEvent();
        ev.SetProcessed();

        Assert.Equal(EventStatus.Processed, ev.Status);
    }

    [Fact]
    public void Event_Failed()
    {
        var ev = CreateEvent();
        var str = "My Error";

        for (int x = 0; x < InternalSharedStatics.SubscribeEventMaxRetryCount + 2; x++)
            ev.SetFailed(str);

        Assert.Equal(EventStatus.Failed, ev.Status);
        ev.RetryCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Event_ShouldKeepProcessing_ForRetry()
    {
        var ev = CreateEvent();
        var str = "My Error";

        ev.SetFailed(str);

        Assert.Equal(EventStatus.Pending, ev.Status);
        Assert.Equal(1, ev.RetryCount);
    }

    [Fact]
    public void Event_Constructor()
    {
        var id = Guid.CreateVersion7();
        var topic = "topic";
        var value = "vali";
        var storeage = "test";
        var date = DateTime.Now;
        var status = EventStatus.None;
        var retry = 10;
        var provider = ServiceBusProvider.None;

        var e = new SubscribeStoredEvent(id, topic, value, null, date, status, retry, string.Empty, storeage, provider);

        e.Id.Should().Be(id);
        e.Topic.Should().Be(topic);
        e.Value.Should().Be(value);
        e.Headers.Should().BeNull();
        e.ReceivedAt.Should().Be(date);
        e.RetryCount.Should().Be(retry);
        e.Error.Should().BeNullOrEmpty();
        e.SourceStorage.Should().Be(storeage);
        e.SourceEvent.Should().Be(provider);
    }
}
