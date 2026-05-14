using System.Text.Json.Serialization;

namespace YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

/// <summary>
/// Abstract class for make Events that will be published to the service bus, it contains common properties and methods for all events.
/// This class is designed to be inherited by specific event implementations, which will define the actual event data and behavior.
/// This class is not designed to be used directly, but rather as a base class for all publishable events in the system.
/// You don't use this class directly for create a especifict event, you use this class for set the standar that you need for the broker that you are using, for example, if you are using Kafka, you can set the topic and the headers that you need for Kafka in this class, and then all your events will inherit that standar.
/// </summary>
public abstract class PublishEvent
{
    /// <summary>
    /// Event unique identifier (for idempotency checks)
    /// </summary>
    public Guid EventId { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Event type name for deserialization
    /// </summary>
    [JsonIgnore]
    public string EventType => GetType().FullName ?? throw new InvalidOperationException("Event type must have a full name");

    /// <summary>
    /// Destination for the event (topic, queue, exchange, etc.)
    /// The interpretation depends on the infrastructure (Kafka, RabbitMQ, etc.)
    /// </summary>
    public abstract ServiceBusProvider Destination();
    /// <summary>
    /// Topic where this event will be published
    /// Must be explicitly defined in each derived class
    /// </summary>
    public abstract string Topic();

    /// <summary>
    /// Custom Header for the event, if no custom header is implemented and returned null, we use default headers that are
    /// ["EventType"] = this.EventType,
    /// ["EventId"] = this.EventId.ToString(),
    /// ["Timestamp"] = this.Timestamp.ToString("O")
    /// </summary>
    public abstract Dictionary<string, string>? GetCustomHeaders();
}
