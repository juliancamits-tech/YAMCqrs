using NSubstitute;
using YAMCqrs.EventBus.Core.InMemoryBus;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemoryPublisherTests;

public class Tests
{

    [Fact]
    public async Task PublishMessageInMemory()
    {
        var subs = Substitute.For<ISubscribeEventStore>();
        subs.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(), Arg.Any<ServiceBusProvider>(), Arg.Any<CancellationToken>()).Returns(true);
        var mem = new InMemoryPublisher(subs);

        await mem.PublishAsync("topic", "{}", null, TestContext.Current.CancellationToken);

        await subs.Received(1).StoreAsync("topic", "{}", null, ServiceBusProvider.InMemory, TestContext.Current.CancellationToken);
    }
}
