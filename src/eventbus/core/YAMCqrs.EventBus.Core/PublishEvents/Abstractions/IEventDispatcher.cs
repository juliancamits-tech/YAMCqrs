namespace YAMCqrs.EventBus.Core.PublishEvents.Abstractions;


/// <summary>
/// Dispatcher for routing events to their handlers without reflection.
/// This interface is implemented by source-generated code.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches an event to all registered handlers.
    /// </summary>
    /// <param name="eventTypeName">Full type name of the event</param>
    /// <param name="event">The event instance to dispatch</param>
    /// <param name="serviceProvider">Service provider to resolve handlers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the event was dispatched successfully, false if no handler was found</returns>
    Task<bool> DispatchAsync(
        string eventTypeName,
        object @event,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes an event from JSON using the event type name (without reflection).
    /// </summary>
    /// <param name="eventType">Simple name or fully qualified name of the event</param>
    /// <param name="json">JSON representation of the event</param>
    /// <returns>Deserialized event object or null if type not found</returns>
    object? DeserializeEvent(string eventType, string json);

    /// <summary>
    /// Gets the simple event type name from a fully qualified name.
    /// </summary>
    /// <param name="eventType">Fully qualified event type name</param>
    /// <returns>Simple event type name or null if not found</returns>
    string? GetEventTypeName(string eventType);
}