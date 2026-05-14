using System.Security.Cryptography;
using YAMCqrs.EventBus.Core.InMemoryBus;

namespace Test.Application.DomainEvent
{
    internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
    {
        public const string TopicName = "domain-event-topic";

        public DomainEventPublishEvent()
        {
            this.Numerito = RandomNumberGenerator.GetInt32(500);
        }

        public int Numerito { get; init; }

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
