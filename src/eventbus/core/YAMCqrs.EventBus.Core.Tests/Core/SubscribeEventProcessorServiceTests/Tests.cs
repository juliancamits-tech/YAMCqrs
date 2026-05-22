using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.Extensions;
using YAMCqrs.Core.Abstractions;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.Extensions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Implementation;
using YAMCqrs.EventBus.Core.Tests.Core.InMemorySuscribeEventTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.SubscribeEventProcessorServiceTests;

public class Tests
{
    [Fact]
    public async Task Run()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new EventBusConfiguration());
        service.AddSingleton<IDispatcher>(_ => new MockIDispatcher(false));

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var iTopic = Substitute.For<ITopicToCommand>();
        iTopic.TryDeserializerTopic(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<SubscribeEvent?>()).Returns(callInfo =>
        {
            callInfo[2] = (SubscribeEvent?)(new DomainEventSubscribeEvent());
            return true;
        });


        var eventStore = provider.GetRequiredService<ISubscribeEventStore>();
        Assert.NotNull(eventStore);
        await eventStore.StoreAsync("topic", "{}", null, ServiceBusProvider.InMemory, TestContext.Current.CancellationToken);

        var ps = new SubscribeEventProcessorService(provider, log, cfg, iTopic);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ps.StartAsync(cts.Token);
    }


    [Fact]
    public async Task EmptyResult()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new EventBusConfiguration());
        service.AddSingleton<IDispatcher>(_ => new MockIDispatcher(false));

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var iTopic = Substitute.For<ITopicToCommand>();
        iTopic.TryDeserializerTopic(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<SubscribeEvent?>()).Returns(callInfo =>
        {
            callInfo[2] = (SubscribeEvent?)(new DomainEventSubscribeEvent());
            return true;
        });

        var ps = new SubscribeEventProcessorService(provider, log, cfg, iTopic);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ps.StartAsync(cts.Token);
    }


    [Fact]
    public async Task Missing_Dispatcher()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new EventBusConfiguration());
        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var iTopic = Substitute.For<ITopicToCommand>();
        iTopic.TryDeserializerTopic(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<SubscribeEvent?>()).Returns(callInfo =>
        {
            callInfo[2] = (SubscribeEvent?)(new DomainEventSubscribeEvent());
            return true;
        });


        var eventStore = provider.GetRequiredService<ISubscribeEventStore>();
        Assert.NotNull(eventStore);
        await eventStore.StoreAsync("topic", "{}", null, ServiceBusProvider.InMemory, TestContext.Current.CancellationToken);

        var ps = new SubscribeEventProcessorService(provider, log, cfg, iTopic);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ps.StartAsync(cts.Token);
    }

    [Fact]
    public async Task ITopicToCommand_Empty()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();
        service.ExtendEventBusCqrs(new EventBusConfiguration());
        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        var iTopic = Substitute.For<ITopicToCommand>();


        var eventStore = provider.GetRequiredService<ISubscribeEventStore>();
        Assert.NotNull(eventStore);
        await eventStore.StoreAsync("topic", "{}", null, ServiceBusProvider.InMemory, TestContext.Current.CancellationToken);

        var ps = new SubscribeEventProcessorService(provider, log, cfg, iTopic);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        await ps.StartAsync(cts.Token);
    }

    [Fact]
    public void Missing_ITopicToCommand()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = Options.Create(new EventBusConfiguration());

        Assert.Throws<ArgumentNullException>(() =>
        new SubscribeEventProcessorService(provider, log, cfg, null!)
        );
    }

    [Fact]
    public void Missing_Configuration()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddBackgroundWorker();

        var provider = service.BuildServiceProvider();
        var log = provider.GetRequiredService<ILogger<SubscribeEventProcessorService>>();
        var cfg = Options.Create(new EventBusConfiguration());
        var iTopic = Substitute.For<ITopicToCommand>();
        Assert.Throws<NullReferenceException>(() =>
        new SubscribeEventProcessorService(provider, log, null!, iTopic)
        );
    }

}
