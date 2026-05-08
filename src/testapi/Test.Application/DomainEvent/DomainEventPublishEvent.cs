using YAMCqrs.ServiceBus.Core.InMemoryBus;

namespace Test.Application.DomainEvent
{
    internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
    {
        public const string TopicName = "domain-event-topic";

        public override Dictionary<string, string>? GetCustomHeaders()
        {
            return null;
        }

        public override string Topic()
        {
            return TopicName;
        }
    }
}
