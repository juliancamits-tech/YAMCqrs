using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.EventBus.Core.PublishEvents.Domain;
using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;

public class Tests
{
    private static PublishStoredEvent CreateEvent()
    {
        return new PublishStoredEvent(new TestEvent());
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

        for (int x = 0; x < InternalSharedStatics.PublishEventMaxRetryCount + 2; x++)
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
        var eventType = "topic";
        var value = "vali";
        var date = DateTime.Now;
        var status = EventStatus.None;
        var retry = 10;
        var provider = ServiceBusProvider.None;

        var e = new PublishStoredEvent(id, eventType, provider, value, date, status, retry, string.Empty);

        e.Id.Should().Be(id);
        e.EventType.Should().Be(eventType);
        e.EventDestination.Should().Be(provider);
        e.Value.Should().Be(value);
        e.CreatedAt.Should().Be(date);
        e.Status.Should().Be(status);
        e.RetryCount.Should().Be(retry);
        e.Error.Should().BeNullOrEmpty();
    }
}