using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.EventBus.Core.InMemoryBus;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemorySubscribeEventStoreTests;

public class Tests
{
    [Fact]
    public async Task InMemorySubscribeEventStore_StoreEvent()
    {
        var store = new InMemorySubscribeEventStore();
        var topic = "topic";
        var value = "value";
        var provider = ServiceBusProvider.None;
        var r = await store.StoreAsync(topic,value,null,provider,TestContext.Current.CancellationToken);
        Assert.True(r);

        var ev = await store.GetPendingEventsAsync(100,TestContext.Current.CancellationToken);
        ev.Should().HaveCount(1);

        var first = ev.FirstOrDefault();

        first.Should().NotBeNull();

        first.Topic.Should().Be(topic);
        first.Value.Should().Be(value);
    }

    [Fact]
    public async Task InMemorySubscribeEventStore_StartEmpty()
    {
        var store = new InMemorySubscribeEventStore();
        var ev = await store.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        ev.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task InMemorySubscribeEventStore_RemoveFailedd()
    {
        var store = new InMemorySubscribeEventStore();
        var topic = "topic";
        var value = "value";
        var provider = ServiceBusProvider.None;
        var r = await store.StoreAsync(topic, value, null, provider, TestContext.Current.CancellationToken);
        Assert.True(r);

        var ev = await store.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        ev.Should().HaveCount(1);

        var first = ev.FirstOrDefault();
        first.Should().NotBeNull();

        await store.MarkAsFailedAsync(first, TestContext.Current.CancellationToken);

        ev = await store.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        ev.Should().BeNullOrEmpty();
    }


    [Fact]
    public async Task InMemorySubscribeEventStore_RemoveCompleted()
    {
        var store = new InMemorySubscribeEventStore();
        var topic = "topic";
        var value = "value";
        var provider = ServiceBusProvider.None;
        var r = await store.StoreAsync(topic, value, null, provider, TestContext.Current.CancellationToken);
        Assert.True(r);

        var ev = await store.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        ev.Should().HaveCount(1);

        var first = ev.FirstOrDefault();
        first.Should().NotBeNull();

        await store.MarkAsProcessedAsync(first, TestContext.Current.CancellationToken);

        ev = await store.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        ev.Should().BeNullOrEmpty();
    }
}
