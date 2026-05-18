using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Extensions;

namespace YAMCqrs.BackgroundWorker.Storage.MongoDb;

public static class ServiceCollectionExtensions
{
    public static IBackgroundWorkerBuilder UseMongoDb(
        this IBackgroundWorkerBuilder builder,
        BackgroundWorkerMongoConfiguration configuration)
    {
        builder.Services.AddSingleton<IWorkerStorageMongoDbContext, WorkerStorageMongoDbContext>();
        builder.Services.Replace(ServiceDescriptor.Singleton<IWorkerStorage, MongoDbWorkerStorage>());
        // Register BackgroundWorkerMongoConfiguration as IOptions<T>
        builder.Services.AddSingleton<IOptions<BackgroundWorkerMongoConfiguration>>(
            new OptionsWrapper<BackgroundWorkerMongoConfiguration>(configuration));
        return builder;
    }
}