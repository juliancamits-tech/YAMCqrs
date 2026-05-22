using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.Implementation;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.EventBus.Implementation;
using YAMCqrs.EventBus.Core.Extensions;
using YAMCqrs.EventBus.Core.InMemoryBus;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Implementation;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Implementation;
using YAMCqrs.EventBus.Core.Tests.Core.EventBusPublisherFactoryTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.ServiceCollectionExtensionsTests;


public class Tests
{
    [Fact]
    public void ServiceCollection_AddExtraServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var cfgDef = new EventBusConfiguration();
        services.ExtendEventBusCqrs(cfgDef);

        var provider = services.BuildServiceProvider();

        var ipes = provider.GetRequiredService<IPublishEventStore>();
        ipes.Should().NotBeNull().And.BeOfType<InMemoryPublishEventStore>();

        var iseb = provider.GetRequiredService<ISubscribeEventStore>();
        iseb.Should().NotBeNull().And.BeOfType<InMemorySubscribeEventStore>();

        var imebp = provider.GetRequiredService<IMemoryEventBusPublisher>();
        imebp.Should().NotBeNull().And.BeOfType<InMemoryPublisher>();
        /*
        imebp = provider.GetRequiredKeyedService<IMemoryEventBusPublisher>(ServiceBusProvider.InMemory);
        imebp.Should().NotBeNull().And.BeOfType<InMemoryPublisher>();
        */
        var ep = provider.GetRequiredService<IEventPublisher>();
        ep.Should().NotBeNull().And.BeOfType<EventPublisher>();

        var epf = provider.GetRequiredService<IEventBusPublisherFactory>();
        epf.Should().NotBeNull().And.BeOfType<EventBusPublisherFactory>();

        var hand = provider.GetRequiredService<IYamHandler<PublishEvent>>();
        hand.Should().NotBeNull().And.BeOfType<PublishEventHandler<PublishEvent>>();

        /*
        var background = provider.GetServices<IHostedService>().OfType<PublishEventProcessorService>().FirstOrDefault();
        Assert.NotNull(background);

        var background2 = provider.GetServices<IHostedService>().OfType<SubscribeEventProcessorService>().FirstOrDefault();
        Assert.NotNull(background);
        */
        var cfgOpt = provider.GetRequiredService<IOptions<EventBusConfiguration>>();
        cfgOpt.Value.Should().NotBeNull();
        var cfg = cfgOpt.Value;
        cfg.PublishConfiguration.Should().NotBeNull();
        cfg.PublishConfiguration.ConcurrentWorkers.Should().Be(cfgDef.PublishConfiguration.ConcurrentWorkers);
        cfg.PublishConfiguration.BatchSize.Should().Be(cfgDef.PublishConfiguration.BatchSize);
        cfg.PublishConfiguration.PollingInterval.Should().Be(cfgDef.PublishConfiguration.PollingInterval);
        cfg.PublishConfiguration.ErrorThresholdPercentage.Should().Be(cfgDef.PublishConfiguration.ErrorThresholdPercentage);
        cfg.SubscribeConfiguration.Should().NotBeNull();
        cfg.SubscribeConfiguration.ConcurrentWorkers.Should().Be(cfgDef.SubscribeConfiguration.ConcurrentWorkers);
        cfg.SubscribeConfiguration.BatchSize.Should().Be(cfgDef.SubscribeConfiguration.BatchSize);
        cfg.SubscribeConfiguration.PollingInterval.Should().Be(cfgDef.SubscribeConfiguration.PollingInterval);
        cfg.SubscribeConfiguration.ErrorThresholdPercentage.Should().Be(cfgDef.SubscribeConfiguration.ErrorThresholdPercentage);




    }
}
