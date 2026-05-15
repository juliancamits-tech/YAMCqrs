using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAMCqrs.EventBus.Provider.Kafka.Abstractions;
using YAMCqrs.EventBus.Provider.Kafka.Configuration;
using YAMCqrs.EventBus.Provider.Kafka.Helper;

namespace YAMCqrs.EventBus.Provider.Kafka.Implementation;

/// <summary>
/// Implements event publishing to Kafka message broker
/// </summary>
internal partial class KafkaPublisher : IKafkaEventBusPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly bool _ownsProducer; // Track if we should dispose
    private readonly ILogger<KafkaPublisher> _logger;
    /// <summary>
    /// Initializes a new instance of the KafkaPublisher class
    /// </summary>
    /// <param name="configuration">Kafka configuration options</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="producer">Optional external producer for testing purposes</param>
    public KafkaPublisher(
        IOptions<KafkaConfigurationOptions> configuration,
        ILogger<KafkaPublisher> logger,
        IConfiguration theConfiguration,
        IProducer<string, string>? producer = null
        )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.Value);

        _logger = logger;

        if (producer is not null)
        {
            _producer = producer;
            _ownsProducer = false; // External producer, don't dispose
        }
        else
        {
            var config = KafkaConfigHelper.CreateProduceConfig(configuration.Value, theConfiguration);
            _producer = new ProducerBuilder<string, string>(config).Build();
            _ownsProducer = true; // We created it, we dispose it
        }
    }

    /// <summary>
    /// Publishes a message to a Kafka topic with optional headers
    /// </summary>
    /// <param name="topic">The Kafka topic to publish to</param>
    /// <param name="jsonValue">The message body in JSON format</param>
    /// <param name="headers">Optional message headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

        if (string.IsNullOrWhiteSpace(jsonValue))
            throw new ArgumentException("JSON value cannot be null or empty.", nameof(jsonValue));

        var header = new MessageMetadata
        {
            Headers = []
        };
        if (headers != null)
        {
            foreach (var h in headers)
            {
                header.Headers.Add(h.Key, System.Text.Encoding.UTF8.GetBytes(h.Value));
            }
        }

        var message = new Message<string, string>
        {
            Key = null!, // Always null for round-robin distribution
            Value = jsonValue,
            Headers = header.Headers
        };

        var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);

        if (deliveryResult.Status != PersistenceStatus.Persisted)
            throw new InvalidOperationException($"Failed to publish message to topic '{topic}'. Status: {deliveryResult.Status}");

        LogMessagePublished(topic);
    }

    /// <summary>
    /// Disposes the Kafka producer if it was created internally
    /// </summary>
    public void Dispose()
    {
        if (_ownsProducer)
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }

    #region Logger Message

    [LoggerMessage(LogLevel.Debug, "Message published to Kafka topic '{Topic}'")]
    private partial void LogMessagePublished(string topic);
    #endregion

}
