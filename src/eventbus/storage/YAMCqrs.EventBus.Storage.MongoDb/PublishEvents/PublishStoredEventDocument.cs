using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using YAMCqrs.EventBus.Core;

namespace YAMCqrs.EventBus.Storage.MongoDb.PublishEvents;

/// <summary>
/// MongoDB representation of the PublishStoredEvent domain entity.
/// </summary>
[BsonIgnoreExtraElements]
internal class PublishStoredEventDocument
{
    /// <summary>
    /// The unique identifier of the event, stored as a string in MongoDB for better compatibility.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    /// <summary>
    /// The full type name of the event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The source of the event.
    /// </summary>
    public string EventSource { get; set; } = string.Empty;

    /// <summary>
    /// The destination for the event (Internal or external system). Stored as string for flexibility.
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public ServiceBusProvider EventDestination { get; set; }

    /// <summary>
    /// The routing key for the event (optional).
    /// </summary>
    [BsonIgnoreIfNull]
    public string? RoutingKey { get; set; }

    /// <summary>
    /// The JSON value of the event.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the event was created.
    /// </summary>
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The current status of the event. Stored as string for flexibility.
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public EventStatus Status { get; set; }

    /// <summary>
    /// The number of retry attempts for this event.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// The error message if the event failed.
    /// </summary>
    [BsonIgnoreIfNull]
    public string? Error { get; set; }
}