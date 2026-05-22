using YAMCqrs.EventBus.Core.InMemoryBus;

namespace YAMCqrs.EventBus.Core.Tests.Core.InMemorySuscribeEventTests;

internal sealed class DomainEventSubscribeEvent() : InMemorySuscribeEvent("Topic")
{
}
