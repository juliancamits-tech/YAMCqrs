using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.EventBus.Abstractions;
using YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;
using YAMCqrs.EventBus.Provider.RabbitMq.Configuration;
using YAMCqrs.EventBus.Provider.RabbitMq.Implementation;
using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;

namespace YAMCqrs.EventBus.Provider.RabbitMq.Extensions;

public static class ServiceCollectionExtensions
{
    public static IEventBusBuilder ExtendUseRabbitMq(this IEventBusBuilder builder, RabbitConfigurationOptions options, string[] topics)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(topics);

        //options.Topics = topics;
        foreach (var t in topics)
        {
            var items = t.Split("||");
            options.Topics.Add(new Topics(items[0], items[1]));
        }

        builder.Services.AddSingleton<IOptions<RabbitConfigurationOptions>>(new OptionsWrapper<RabbitConfigurationOptions>(options));

        // FIRST: Direct registration with specific interface (creates the real instance)
        builder.Services.AddSingleton<IRabbitEventBusPublisher, RabbitPublisher>();

        // SECOND: Keyed registration using the specific interface (alias/reference only)
        builder.Services.AddKeyedSingleton<IEventBusPublisher>(ServiceBusProvider.RabbitMq,
            (sp, key) => sp.GetRequiredService<IRabbitEventBusPublisher>());

        // Register consumer as hosted service
        builder.Services.AddHostedService<RabbitSubscriber>();

        return builder;
    }

}
