namespace YAMCqrs.EventBus.Core.EventBus.Abstractions;

/// <summary>
/// Interface for standaralized event bus publisher. This abstracts the underlying service bus implementation and allows for different providers (e.g., Azure Service Bus, RabbitMQ, etc.) to be used interchangeably.
/// </summary>
public interface IEventBusPublisher
{
    /// <summary>
    /// Publishes a message to ServiceBus with optional headers
    /// </summary>
    public Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken);
}
