using System.Text.Json.Serialization;
using YAMCqrs.Core.Abstractions.Commands;

namespace YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions;

/// <summary>
/// Base class for commands that represent Service Bus messages.
/// These commands are dispatched by ServiceBusMessageHandler when a message is consumed.
/// Each derived class must specify the Service Bus topic it listens to.
/// </summary>
/// <remarks>
/// Constructor that forces derived classes to provide the topic
/// </remarks>
public abstract class SubscribeEvent(string topic) : ICommand<bool>
{
    public abstract ServiceBusProvider Provider { get; }

    /// <summary>
    /// topic where this command listens for messages
    /// Must be explicitly defined in each derived class via constructor
    /// </summary>
    [JsonIgnore]
    public string Topic { get; init; } = topic;

    /// <summary>
    /// Headers (optional)
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Timestamp when the message was received
    /// </summary>
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Command type name for logging and tracking
    /// </summary>
    [JsonIgnore]
    public string CommandType => GetType().FullName ?? GetType().Name;
}
