namespace YAMCqrs.ServiceBus.Core.SubscribeEvents.Domain;

public class SubscribeStoredEvent(
    Guid id,
    string topic,
    string value,
    Dictionary<string, string>? headers,
    string sourceStorage,
    ServiceBusProvider sourceEvent)
{
    public static int MaxRetryCount = 3;

    public Guid Id { get; set; } = id;
    public string Topic { get; set; } = topic;
    public string Value { get; set; } = value;
    public Dictionary<string, string>? Headers { get; set; } = headers;
    public DateTimeOffset ReceivedAt { get; set; } = DateTime.UtcNow;
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

