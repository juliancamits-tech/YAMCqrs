using YAMCqrs.ServiceBus.Core.InMemoryBus;

namespace Test.Application.DomainEvent
{
    internal sealed class DomainEventSubscribeEvent() : InMemorySuscribeEvent(DomainEventPublishEvent.TopicName)
    {
    }
}
