using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.PublishEvents.Domain;

/// <summary>
/// Represents a stored event with metadata
/// </summary>
public sealed class PublishStoredEvent
{
    /// <summary>
    /// Maximum number of retry attempts for failed events
    /// </summary>
    public static int MaxRetryCount = 3;

    /// <summary>
    /// Gets the unique identifier of the stored event
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the full type name of the event
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Gets the destination for the event (Internal or external system)
    /// </summary>
    public ServiceBusProvider EventDestination { get; private set; }

    /// <summary>
    /// Gets the routing key for the event (optional)
    /// </summary>
    public string? RoutingKey { get; private set; }

    /// <summary>
    /// Gets the JSON value of the event
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// Gets the timestamp when the event was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the current status of the event
    /// </summary>
    public EventStatus Status { get; private set; }

    /// <summary>
    /// Gets the number of retry attempts for this event
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Gets the error message if the event failed
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Creates a PublishStoredEvent from an event instance
    /// </summary>
    public PublishStoredEvent(PublishEvent @event)
    {
        Id = Guid.CreateVersion7();
        EventType = @event.GetType().FullName ?? throw new InvalidOperationException("Event type must have a full name");
        Value = JsonSerializerExtensions.SerializeWithConcreteType(@event);
        CreatedAt = DateTime.UtcNow;
        Status = EventStatus.Pending;
        EventDestination = @event.Destination();
    }

    /// <summary>
    /// Internal constructor for restoring from persistence (MongoDB, SQL, etc.)
    /// </summary>
    public PublishStoredEvent(
        Guid id,
        string eventType,
        ServiceBusProvider eventDestination,
        string? routingKey,
        string payload,
        DateTime createdAt,
        EventStatus status,
        int retryCount,
        string? error)
    {
        Id = id;
        EventType = eventType;
        EventDestination = eventDestination;
        RoutingKey = routingKey;
        Value = payload;
        CreatedAt = createdAt;
        Status = status;
        RetryCount = retryCount;
        Error = error;
    }

    /// <summary>
    /// Marks the event as successfully processed
    /// </summary>
    public void SetProcessed()
    {
        Status = EventStatus.Processed;
    }

    /// <summary>
    /// Marks the event as failed or retries it
    /// </summary>
    /// <param name="error">The error message describing the failure</param>
    public void SetFailed(string error)
    {
        if (RetryCount > PublishStoredEvent.MaxRetryCount)
        {
            Status = EventStatus.Failed;
            Error = error;
            return;
        }

        RetryCount++;
        Status = EventStatus.Pending;
    }
}
