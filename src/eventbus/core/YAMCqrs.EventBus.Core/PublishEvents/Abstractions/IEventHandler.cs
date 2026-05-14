namespace YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

/// <summary>
/// Handler for processing events asynchronously
/// Is used by the source-generated dispatcher to route events to their handlers without reflection.
/// </summary>
/// <typeparam name="TEvent">The event type to handle</typeparam>
public interface IEventHandler<in TEvent> where TEvent : PublishEvent
{
    /// <summary>
    /// Handles the event asynchronously
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
