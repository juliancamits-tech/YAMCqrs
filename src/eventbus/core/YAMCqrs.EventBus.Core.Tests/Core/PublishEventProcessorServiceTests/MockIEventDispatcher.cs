using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.Tests.Core.PublishStoredEventTests;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishEventProcessorServiceTests;

internal class MockIEventDispatcher(bool invalidDeserilizer, bool dispatchResult = true) : IEventDispatcher
{
    public object? DeserializeEvent(string eventType, string json)
    {
        if (invalidDeserilizer)
            return null;

        return new TestEvent();
    }

    public async Task<bool> DispatchAsync(string eventTypeName, object yamEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return dispatchResult;
    }

    public string? GetEventTypeName(string eventType)
    {
        return null;
    }
}
