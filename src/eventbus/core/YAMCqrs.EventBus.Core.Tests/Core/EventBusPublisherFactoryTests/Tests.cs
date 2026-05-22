using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.EventBus.Implementation;

namespace YAMCqrs.EventBus.Core.Tests.Core.EventBusPublisherFactoryTests;

public class Tests
{
    [Fact]
    public void PublisherFactory_ReturnCorrectService()
    {
        var service = new ServiceCollection();
        var inMemory = Substitute.For<IEventBusPublisher>();
        var kafka = Substitute.For<IEventBusPublisher>();
        
        service.AddKeyedSingleton<IEventBusPublisher>(ServiceBusProvider.InMemory, inMemory);
        service.AddKeyedSingleton<IEventBusPublisher>(ServiceBusProvider.Kafka, kafka);
        
        var provider = service.BuildServiceProvider();
        var factory = new EventBusPublisherFactory(provider);

        var publisher =  factory.GetPublisher(ServiceBusProvider.InMemory);
        Assert.Equal(inMemory, publisher);

        publisher = factory.GetPublisher(ServiceBusProvider.Kafka);
        Assert.Equal(kafka, publisher);
    }
}
