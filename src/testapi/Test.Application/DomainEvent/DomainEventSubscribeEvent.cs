using YAMCqrs.EventBus.Core.InMemoryBus;

namespace Test.Application.DomainEvent
{
    internal sealed class DomainEventSubscribeEvent() : InMemorySuscribeEvent(DomainEventPublishEvent.TopicName)
    {
        public int Numerito { get; init; }
    }
}
