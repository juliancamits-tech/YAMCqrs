using System.Collections.Concurrent;
using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Core.InMemoryBus;

internal sealed class InMemorySubscribeEventStore : ISubscribeEventStore
{
    private readonly ConcurrentDictionary<Guid, SubscribeStoredEvent> _messages = ConcurrentDictionaryExtension.CreateNewDictionary<Guid, SubscribeStoredEvent>();

    public Task<bool> StoreAsync(string topic, string value, Dictionary<string, string>? headers, ServiceBusProvider serviceBusProvider, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var message = new SubscribeStoredEvent(
            id: Guid.NewGuid(),
            topic: topic,
            value: value,
            headers: headers,
            sourceStorage: "InMemory",
            sourceEvent: serviceBusProvider
        );

        _messages.TryAdd(message.Id, message);

        return Task.FromResult(true);
    }

    public Task<IEnumerable<SubscribeStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pending = _messages.Values
            .Where(m => m.Status == EventStatus.Pending)
            .OrderBy(m => m.ReceivedAt)
            .Take(batchSize)
            .ToArray();

        return Task.FromResult<IEnumerable<SubscribeStoredEvent>>(pending);
    }


    public Task MarkAsProcessedAsync(SubscribeStoredEvent storedEvent, CancellationToken cancellationToken = default)
    {
        _messages.TryRemove(storedEvent.Id, out var _);

        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(SubscribeStoredEvent storedEvent, CancellationToken cancellationToken = default)
    {
        if (storedEvent.Status == EventStatus.Failed)
            _messages.TryRemove(storedEvent.Id, out var _);

        return Task.CompletedTask;
    }
}
