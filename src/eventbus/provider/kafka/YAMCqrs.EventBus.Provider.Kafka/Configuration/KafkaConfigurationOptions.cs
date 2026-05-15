using Microsoft.Extensions.Configuration;

namespace YAMCqrs.EventBus.Provider.Kafka.Configuration;

/// <summary>
/// Configuration options for Kafka connection and consumer settings
/// </summary>
public class KafkaConfigurationOptions
{
    /// <summary>
    /// List of Kafka topics to subscribe to.
    /// Topics should follow naming convention: lowercase, object.action format (e.g., "users.created").
    /// </summary>
    internal string[] Topics { get; set; } = [];
    /// <summary>
    /// Gets or sets the connection string for the Kafka
    /// If Null or empty, it will be retrieved from configuration using the key "Kafka"
    /// If null or empty in configuration, an exception will be thrown.
    /// </summary>
    public required string ConnectionString { get; init; } = string.Empty;
    /// <summary>
    /// Application Name used by Kafka for logging
    /// </summary>
    public required string KafkaClientName { get; init; } = string.Empty;
    /// <summary>
    /// Group Name used by Kafka for agroup when reading topic
    /// </summary>
    public required string KafkaGroupName { get; init; } = string.Empty;
    /// <summary>
    /// Number of concurrent consumer tasks to process messages in parallel.
    /// Higher values increase throughput but require more resources.
    /// Default is 1 for sequential processing.
    /// </summary>
    public int MaxConcurrentConsumers { get; set; } = 1;

    public string GetConnectionString(IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(ConnectionString, nameof(ConnectionString));

        if (ConnectionString.StartsWith("cs_", StringComparison.InvariantCultureIgnoreCase))
        {
            var aux = ConnectionString["cs_".Length..];
            return cfg.GetConnectionString(aux) ?? throw new InvalidOperationException($"Connection string not found in configuration for key '{ConnectionString}'");
        }

        return ConnectionString;
    }
}

