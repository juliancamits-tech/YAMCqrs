using YAMCqrs.EventBus.Core.PublishEvents.Domain;

namespace YAMCqrs.EventBus.Storage.MongoDb.PublishEvents;

/// <summary>
/// Provides centralized mapping between the Domain Event and the MongoDB Document.
/// </summary>
internal static class PublishEventMapper
{
    /// <summary>
    /// Maps a domain event to its MongoDB document representation.
    /// </summary>
    public static PublishStoredEventDocument ToDocument(this PublishStoredEvent domain)
    {
        return new PublishStoredEventDocument
        {
            Id = domain.Id,
            EventType = domain.EventType,
            EventDestination = domain.EventDestination,
            RoutingKey = domain.RoutingKey,
            Value = domain.Value,
            CreatedAt = domain.CreatedAt,
            Status = domain.Status,
            RetryCount = domain.RetryCount,
            Error = domain.Error
        };
    }

    /// <summary>
    /// Maps a MongoDB document back to the domain event entity.
    /// </summary>
    public static PublishStoredEvent ToDomain(this PublishStoredEventDocument document)
    {
        // Note: This assumes PublishStoredEvent has an accessible constructor or public setters.
        // If the domain class uses init-only properties, this object initializer remains valid.
        return new PublishStoredEvent
        (
            id: document.Id,
            eventType: document.EventType,
            eventDestination: document.EventDestination,
            routingKey: document.RoutingKey,
            payload: document.Value,
            createdAt: document.CreatedAt,
            status: document.Status,
            retryCount: document.RetryCount,
            error: document.Error
        );
    }
}