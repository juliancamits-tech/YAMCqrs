using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using YAMCqrs.EventBus.Provider.Kafka.Configuration;

namespace YAMCqrs.EventBus.Provider.Kafka.Helper;

internal static class KafkaConfigHelper
{
    /// <summary>
    /// Creates a producer configuration from the application settings
    /// </summary>
    /// <param name="cfg">Kafka configuration options</param>
    /// <returns>Producer configuration for Confluent.Kafka</returns>
    public static ProducerConfig CreateProduceConfig(KafkaConfigurationOptions cfg, IConfiguration configuration)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = cfg.GetConnectionString(configuration),
            // This name is for logs and metrics
            ClientId = cfg.KafkaClientName
        };
        return producerConfig;
    }

    /// <summary>
    /// Creates a consumer configuration from the application settings
    /// </summary>
    /// <param name="cfg">Kafka configuration options</param>
    /// <returns>Consumer configuration for Confluent.Kafka</returns>
    public static ConsumerConfig CreateConsumeConfig(KafkaConfigurationOptions cfg, IConfiguration configuration)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = cfg.GetConnectionString(configuration),
            // This name is for logs and metrics
            ClientId = cfg.KafkaClientName,
            // Consumer group id: all instances with the same groupId share work
            GroupId = cfg.KafkaGroupName,
            // Start from earliest if no committed offset is found
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Explicitly disable auto-commit to use manual commit strategy
            EnableAutoCommit = false
        };
        return consumerConfig;
    }
}
