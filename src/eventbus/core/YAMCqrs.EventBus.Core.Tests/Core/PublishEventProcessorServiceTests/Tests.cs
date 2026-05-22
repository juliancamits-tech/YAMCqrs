using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YAMCqrs.BackgroundWorker.Core.Extensions;
using YAMCqrs.Core.Abstractions;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.Extensions;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Implementation;
using YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;
using YAMCqrs.EventBus.Core.Tests.Core.SubscribeEventProcessorServiceTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishEventProcessorServiceTests;

public class Tests
{
    [Fact]
    public async Task Run()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());
        service.AddSingleton<IEventDispatcher>(_ => new MockIEventDispatcher(false));
        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();
        var eventDispatcher = provider.GetRequiredService<IEventDispatcher>();

        await eventStore.StoreAsync(new TestEvent(), TestContext.Current.CancellationToken);

        var ps = new PublishEventProcessorService(log, eventDispatcher, eventStore, cfg, provider);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(20));
        await ps.StartAsync(cts.Token);
    }

    [Fact]
    public async Task Invalid_DeserializeEvent()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());
        service.AddSingleton<IEventDispatcher>(_ => new MockIEventDispatcher(true));
        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();
        var eventDispatcher = provider.GetRequiredService<IEventDispatcher>();

        await eventStore.StoreAsync(new TestEvent(), TestContext.Current.CancellationToken);

        var ps = new PublishEventProcessorService(log, eventDispatcher, eventStore, cfg, provider);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(20));
        await ps.StartAsync(cts.Token);
    }


    [Fact]
    public async Task Invalid_Dispatcher()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());
        service.AddSingleton<IEventDispatcher>(_ => new MockIEventDispatcher(false,false));
        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();
        var eventDispatcher = provider.GetRequiredService<IEventDispatcher>();

        await eventStore.StoreAsync(new TestEvent(), TestContext.Current.CancellationToken);

        var ps = new PublishEventProcessorService(log, eventDispatcher, eventStore, cfg, provider);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(20));
        await ps.StartAsync(cts.Token);
    }

    [Fact]
    public async Task Missing_Logger()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());

        var provider = service.BuildServiceProvider();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();
        var eventDispatcher = Substitute.For<IEventDispatcher>();


        Assert.Throws<ArgumentNullException>(() =>
        new PublishEventProcessorService(null!, eventDispatcher, eventStore, cfg, provider)
        );
    }

    [Fact]
    public async Task Missing_Dispatcher()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();

        Assert.Throws<ArgumentNullException>(() =>
        new PublishEventProcessorService(log, null!, eventStore, cfg, provider)
        );
    }

    [Fact]
    public async Task Missing_PublishEventStore()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var eventDispatcher = Substitute.For<IEventDispatcher>();


        Assert.Throws<ArgumentNullException>(() =>
        new PublishEventProcessorService(null!, eventDispatcher, null!, cfg, provider)
        );
    }

    [Fact]
    public async Task Missing_Configuration()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new());

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<PublishEventProcessorService>>();
        var eventStore = provider.GetRequiredService<IPublishEventStore>();
        var eventDispatcher = Substitute.For<IEventDispatcher>();

        Assert.Throws<ArgumentNullException>(() =>
        new PublishEventProcessorService(null!, eventDispatcher, eventStore, null!, provider)
        );
    }
}
