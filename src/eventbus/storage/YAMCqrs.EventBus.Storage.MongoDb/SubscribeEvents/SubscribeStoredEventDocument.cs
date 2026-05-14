using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using YAMCqrs.EventBus.Core;

namespace YAMCqrs.EventBus.Storage.MongoDb.SubscribeEvents;


/// <summary>
/// MongoDB representation of the SubscribeStoredEvent domain entity.
/// </summary>
[BsonIgnoreExtraElements]
internal class SubscribeStoredEventDocument
{
    /// <summary>
    /// The unique identifier of the event, stored as a string in MongoDB for better compatibility.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Topic { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    [BsonIgnoreIfNull]
    public Dictionary<string, string>? Headers { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Stored as string for flexibility.
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public EventStatus Status { get; set; }

    public int RetryCount { get; set; }

    [BsonIgnoreIfNull]
    public string? Error { get; set; }

    public string SourceStorage { get; set; } = string.Empty;

    /// <summary>
    /// The destination for the event (Internal or external system). Stored as string for flexibility.
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public ServiceBusProvider SourceEvent { get; set; }
}
