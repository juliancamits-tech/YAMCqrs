using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Provider.Kafka.Abstractions;
using YAMCqrs.EventBus.Provider.Kafka.Configuration;
using YAMCqrs.EventBus.Provider.Kafka.Implementation;
using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;

namespace YAMCqrs.EventBus.Provider.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IEventBusBuilder ExtendUseKafka(this IEventBusBuilder builder, KafkaConfigurationOptions options, string[] topics)
    {
        options.Topics = topics;
        builder.Services.AddSingleton<IOptions<KafkaConfigurationOptions>>(new OptionsWrapper<KafkaConfigurationOptions>(options));

        // FIRST: Direct registration with specific interface (creates the real instance)
        builder.Services.AddSingleton<IKafkaEventBusPublisher, KafkaPublisher>();

        // SECOND: Keyed registration using the specific interface (alias/reference only)
        builder.Services.AddKeyedSingleton<IEventBusPublisher>(ServiceBusProvider.Kafka,
            (sp, key) => sp.GetRequiredService<IKafkaEventBusPublisher>());

        // Register consumer as hosted service
        builder.Services.AddHostedService<KafkaSubscriber>();

        return builder;
    }
}
