using YAMCqrs.ServiceBus.Core.PublishEvents.Domain;

namespace YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;

/// <summary>
/// Event storage abstraction (Outbox pattern)
/// </summary>
public interface IPublishEventStore
{
    /// <summary>
    /// Stores an event for later processing
    /// </summary>
    Task StoreAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent;

    /// <summary>
    /// Gets pending events to process
    /// </summary>
    Task<IEnumerable<PublishStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as processed
    /// </summary>
    Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as failed
    /// </summary>
    Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default);
}
