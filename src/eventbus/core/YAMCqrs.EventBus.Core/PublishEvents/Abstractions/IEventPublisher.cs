namespace YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

/// <summary>
/// Public interface for staging events for publishing. This is the main entry point for clients to publish events.
/// </summary>
public interface IEventPublisher : IDisposable
{
    /// <summary>
    /// Stages an event for publishing.
    /// If an ambient transaction exists, the event is committed when the transaction completes.
    /// Otherwise, it's committed immediately.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent;

    /// <summary>
    /// Stages multiple events for publishing.
    /// Do internal implementation of PublishAsync
    /// </summary>
    Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent;

    /// <summary>
    /// Stages multiple events for publishing with params syntax.
    /// Do internal implementation of PublishAsync
    /// </summary>
    Task PublishManyAsync(CancellationToken cancellationToken = default, params PublishEvent[] events);
}