using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Storage.MongoDb.SubscribeEvents;

internal static class SubscribeEventMapper
{
    public static SubscribeStoredEventDocument ToDocument(this SubscribeStoredEvent @event)
    {
        return new SubscribeStoredEventDocument()
        {
            Id = @event.Id,
            Topic = @event.Topic,
            Value = @event.Value,
            Headers = @event.Headers,
            ReceivedAt = @event.ReceivedAt,
            Status = @event.Status,
            RetryCount = @event.RetryCount,
            Error = @event.Error,
            SourceStorage = "MongoDb",
            SourceEvent = @event.SourceEvent
        };
    }
    public static SubscribeStoredEvent ToDomain(this SubscribeStoredEventDocument document)
    {
        var @event = new SubscribeStoredEvent
            (
            document.Id,
            document.Topic,
            document.Value,
            document.Headers,
            document.ReceivedAt,
            document.Status,
            document.RetryCount,
            document.Error,
            document.SourceStorage,
            document.SourceEvent
            );

        return @event;
    }


}
