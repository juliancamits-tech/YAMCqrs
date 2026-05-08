using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using YAMCqrs.Core.Extensions;
using YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;
using YAMCqrs.ServiceBus.Core.PublishEvents.Domain;

namespace YAMCqrs.ServiceBus.Core.PublishEvents.Implementation;

/// <summary>
/// In-memory implementation of IPublishEventStore (use for development/testing)
/// In production, replace with database implementation
/// </summary>
internal sealed partial class InMemoryEventStore(ILogger<InMemoryEventStore> logger) : IPublishEventStore
{
    private readonly ConcurrentDictionary<Guid, PublishStoredEvent> _events = ConcurrentDictionaryExtension.CreateNewDictionary<Guid, PublishStoredEvent>();
    private readonly ILogger<InMemoryEventStore> _logger = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "Event stored: {EventType} [{EventId}] → {Destination}{RoutingKey}")]
    private partial void EventStored(string eventType, Guid eventId, ServiceBusProvider destination, string routingKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Event marked as processed: [{EventId}]")]
    private partial void EventProcessed(Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Event marked as failed after {RetryCount} retries: [{EventId}] - {Error} (Removed for memory)")]
    private partial void EventErrorA(int retryCount, Guid eventId, string error);

    [LoggerMessage(Level = LogLevel.Error, Message = "Event retry {RetryCount}//{MaxRetry}: [{EventId}] - {Error}")]
    private partial void EventErrorB(int retryCount, int maxRetry, Guid eventId, string error);

    public Task StoreAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent
    {
        var payload = JsonSerializerExtensions.SerializeWithConcreteType(@event);

        var eventType = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name;

        var storedEvent = new PublishStoredEvent(@event, payload, eventType);

        _events.TryAdd(storedEvent.Id, storedEvent);
        EventStored(eventType, storedEvent.Id, storedEvent.EventDestination, storedEvent.RoutingKey ?? string.Empty);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<PublishStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var pending = _events.Values
            .Where(e => e.Status == EventStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IEnumerable<PublishStoredEvent>>(pending);
    }

    public Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        _events.TryRemove(eventId, out _);

        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default)
    {
        if (_events.TryGetValue(eventId, out var storedEvent))
        {
            storedEvent.SetFailed(error);

            if (storedEvent.Status == EventStatus.Failed)
            {
                EventErrorA(storedEvent.RetryCount, eventId, error);
                _events.TryRemove(eventId, out _);
            }
            else
            {
                EventErrorB(storedEvent.RetryCount, PublishStoredEvent.MaxRetryCount, eventId, error);
            }
        }

        return Task.CompletedTask;
    }
}
