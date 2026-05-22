using System.Security.Cryptography;
using YAMCqrs.EventBus.Core.InMemoryBus;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemoryPublishEventTests
{
    internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
    {
        public override Dictionary<string, string>? GetCustomHeaders()
        {
            return null;
        }

        public override string Topic()
        {
            return "Topic";
        }
    }
}
