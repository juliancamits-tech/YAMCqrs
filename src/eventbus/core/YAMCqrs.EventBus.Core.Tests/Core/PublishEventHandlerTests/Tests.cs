using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Implementation;
using YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishEventHandlerTests;

public class Tests
{
    [Fact]
    public async Task Run()
    {
        ILogger<PublishEventHandler<TestEvent>> logger = NullLogger<PublishEventHandler<TestEvent>>.Instance;
        var publisherFactory = Substitute.For<IEventBusPublisherFactory>();
        publisherFactory.GetPublisher(Arg.Any<ServiceBusProvider>()).Returns(new MockIEventBusPublisher());

      
        var handler = new PublishEventHandler<TestEvent>(publisherFactory, logger);

        await handler.HandleAsync(new(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Run_Ex()
    {
        ILogger<PublishEventHandler<TestEvent>> logger = NullLogger<PublishEventHandler<TestEvent>>.Instance;
        var publisherFactory = Substitute.For<IEventBusPublisherFactory>();
        publisherFactory.GetPublisher(Arg.Any<ServiceBusProvider>()).Returns(new MockIEventBusPublisherEx());


        var handler = new PublishEventHandler<TestEvent>(publisherFactory, logger);

        await Assert.ThrowsAsync<Exception>(() => handler.HandleAsync(new(), TestContext.Current.CancellationToken));
    }
}
