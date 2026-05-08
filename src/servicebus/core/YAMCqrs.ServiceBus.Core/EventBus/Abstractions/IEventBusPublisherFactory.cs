namespace YAMCqrs.ServiceBus.Core.EventBus.Abstractions;

/// <summary>
/// Factory for resolving event bus publishers by provider name
/// </summary>
internal interface IEventBusPublisherFactory
{
    /// <summary>
    /// Gets a publisher for the specified provider
    /// </summary>
    /// <param name="provider">Provider name (use ServiceBusProviders constants)</param>
    /// <returns>The event bus publisher instance</returns>
    IEventBusPublisher GetPublisher(ServiceBusProvider provider);
}
