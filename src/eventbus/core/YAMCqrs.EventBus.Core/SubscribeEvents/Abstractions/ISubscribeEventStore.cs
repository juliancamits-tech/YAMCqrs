using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

public interface ISubscribeEventStore
{
    /// <summary>
    /// Persists a message to storage for later processing.
    /// </summary>
    /// <param name="topic">The topic from which the message was received.</param>
    /// <param name="value">The message value (JSON payload).</param>
    /// <param name="headers">Headers (optional).</param>
    /// <param name="serviceBusProvider">The service bus provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">
    /// Thrown when the message cannot be persisted (e.g., database full, storage unavailable).
    /// This exception prevents the commit from executing.
    /// </exception>
    public Task<bool> StoreAsync(
        string topic,
        string value,
        Dictionary<string, string>? headers,
        ServiceBusProvider serviceBusProvider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets pending events to process
    /// </summary>
    Task<IEnumerable<SubscribeStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as processed
    /// </summary>
    Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an event as failed
    /// </summary>
    Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default);
}