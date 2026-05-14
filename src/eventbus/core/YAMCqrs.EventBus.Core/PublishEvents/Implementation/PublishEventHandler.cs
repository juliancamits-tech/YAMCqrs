using Microsoft.Extensions.Logging;
using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;


namespace YAMCqrs.EventBus.Core.PublishEvents.Implementation;

internal sealed partial class PublishEventHandler<TEvent>(
    IEventBusPublisherFactory publisherFactory,
    ILogger<PublishEventHandler<TEvent>> logger) : IEventHandler<TEvent>
    where TEvent : PublishEvent
{
    private readonly ILogger<PublishEventHandler<TEvent>> _logger = logger;

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var publisher = publisherFactory.GetPublisher(@event.Destination());

            ArgumentNullException.ThrowIfNull(publisher, $"No publisher found for destination '{@event.Destination()}'");

            var serializedEvent = JsonSerializerExtensions.SerializeWithConcreteType(@event);

            var headers = @event.GetCustomHeaders();
            headers ??= new Dictionary<string, string>
            {
                ["EventType"] = @event.EventType,
                ["EventId"] = @event.EventId.ToString(),
                ["Timestamp"] = @event.Timestamp.ToString("O")
            };

            await publisher.PublishAsync(@event.Topic(), serializedEvent, headers, cancellationToken);

            LogEventPublished(@event.EventType, @event.Topic());
        }
        catch (Exception)
        {
            LogEventPublishFailed(@event.EventType, @event.Topic());
            throw;
        }
    }

    #region Logger Message
    [LoggerMessage(LogLevel.Information, "Event published: {EventType} to topic {Topic}")]
    private partial void LogEventPublished(string eventType, string topic);

    [LoggerMessage(LogLevel.Error, "Failed to publish event: {EventType} to topic {Topic}")]
    private partial void LogEventPublishFailed(string eventType, string topic);
    #endregion
}
