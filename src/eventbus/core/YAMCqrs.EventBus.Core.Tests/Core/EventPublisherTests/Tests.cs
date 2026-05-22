using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Transactions;
using YAMCqrs.EventBus.Core.PublishEvents.Implementation;
using YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.EventPublisherTests;

public class Tests
{
    private static Tuple<EventPublisher, InMemoryPublishEventStore> CreatePublisher()
    {
        var logger = Substitute.For<ILogger<InMemoryPublishEventStore>>();
        var memory = new InMemoryPublishEventStore(logger);
        var logger2 = Substitute.For<ILogger<EventPublisher>>();
        var ep = new EventPublisher(memory, logger2);

        return Tuple.Create(ep, memory);
    }

    [Fact]
    public async Task EventPublisher_Publish_Zero()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            scope.Complete();
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(0);
    }

    [Fact]
    public async Task EventPublisher_Publish_One()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            await ev.PublishAsync(new TestEvent(), TestContext.Current.CancellationToken);

            scope.Complete();
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(1);
    }

    [Fact]
    public async Task EventPublisher_Publish_Two()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            await ev.PublishManyAsync(TestContext.Current.CancellationToken, new TestEvent(), new TestEvent());

            scope.Complete();
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(2);
    }

    [Fact]
    public async Task EventPublisher_Publish_Two_Separated()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            await ev.PublishManyAsync(TestContext.Current.CancellationToken, new TestEvent());
            await ev.PublishManyAsync(TestContext.Current.CancellationToken, new TestEvent());

            scope.Complete();
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(2);
    }

    [Fact]
    public async Task EventPublisher_Publish_List()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            await ev.PublishManyAsync([new TestEvent(), new TestEvent()],TestContext.Current.CancellationToken);

            scope.Complete();
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(2);
    }

    [Fact]
    public async Task EventPublisher_Publish_Dispose()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        await ev.PublishAsync(new TestEvent(), TestContext.Current.CancellationToken);
        ev.Dispose();

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(1);
    }

    [Fact]
    public async Task EventPublisher_Publish_DisposeSafe()
    {
        try
        {
            var tuple = CreatePublisher();
            var ev = tuple.Item1;

            ev.Dispose();
            ev.Dispose();
            await ev.PublishAsync(new TestEvent(), TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            ex.Should().BeOfType<ObjectDisposedException>();
        }
    }

    [Fact]
    public async Task EventPublisher_Publish_RollBack()
    {
        var tuple = CreatePublisher();
        var ev = tuple.Item1;
        var memory = tuple.Item2;

        using (var scope = new TransactionScope())
        {
            await ev.PublishAsync(new TestEvent(), TestContext.Current.CancellationToken);
        }

        var re = await memory.GetPendingEventsAsync(100, TestContext.Current.CancellationToken);
        re.Should().NotBeNull();
        re.Should().HaveCount(0);
    }
}
