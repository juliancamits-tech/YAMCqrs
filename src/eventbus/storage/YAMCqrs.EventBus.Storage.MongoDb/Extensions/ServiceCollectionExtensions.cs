using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Storage.MongoDb.PublishEvents;
using YAMCqrs.EventBus.Storage.MongoDb.SubscribeEvents;
using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;

namespace YAMCqrs.EventBus.Storage.MongoDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IEventBusBuilder UseMongoDb(this IEventBusBuilder builder, EventBusStorageMongoConfiguration configuration)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IPublishEventStore,PublishEventStore>());
        builder.Services.Replace(ServiceDescriptor.Singleton<ISubscribeEventStore, SubscribeEventStore>());

        builder.Services.AddSingleton<IEventBusStorageMongoDbContext, EventBusStorageMongoDbContext>();
        builder.Services.AddSingleton<IOptions<EventBusStorageMongoConfiguration>>(
            new OptionsWrapper<EventBusStorageMongoConfiguration>(configuration));

        return builder;
    }
}
