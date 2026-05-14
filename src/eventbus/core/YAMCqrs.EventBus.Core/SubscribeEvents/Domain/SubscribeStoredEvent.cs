namespace YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

public class SubscribeStoredEvent(
    Guid id,
    string topic,
    string value,
    Dictionary<string, string>? headers,
    string sourceStorage,
    ServiceBusProvider sourceEvent)
{
    public static int MaxRetryCount = 3;

    public SubscribeStoredEvent(Guid id,
        string topic,
        string value,
        Dictionary<string, string>? headers,
        DateTime receivedAt,
        EventStatus status,
        int retryCount,
        string? error,
        string sourceStorage,
        ServiceBusProvider sourceEvent
        ) : this(id, topic, value, headers, sourceStorage, sourceEvent)
    {
        ReceivedAt = receivedAt;
        Status = status;
        RetryCount = retryCount;
        Error = error;
    }

    public Guid Id { get; init; } = id;
    public string Topic { get; init; } = topic;
    public string Value { get; init; } = value;
    public Dictionary<string, string>? Headers { get; init; } = headers;
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
    public EventStatus Status { get; private set; } = EventStatus.Pending;
    public int RetryCount { get; set; }
    public string? Error { get; set; }

    public string SourceStorage { get; set; } = sourceStorage;

    public ServiceBusProvider SourceEvent { get; set; } = sourceEvent;

    public void SetProcessed()
    {
        Status = EventStatus.Processed;
    }

    public void SetFailed(string error)
    {
        if (RetryCount > SubscribeStoredEvent.MaxRetryCount)
        {
            Status = EventStatus.Failed;
            Error = error;
            return;
        }

        RetryCount++;
        Status = EventStatus.Pending;
    }
}

