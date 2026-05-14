using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Extensions;

namespace YAMCqrs.BackgroundWorker.Storage.MondgoDb;

public static class ServiceCollectionExtensions
{
    public static IBackgroundWorkerBuilder UseMongoDb(
        this IBackgroundWorkerBuilder builder,
        BackgroundWorkerMongoConfiguration configuration)
    {
        builder.Services.AddSingleton<IWorkerStorageMongoDbContext, WorkerStorageMongoDbContext>();
        builder.Services.AddSingleton<IWorkerStorage, MongoDbWorkerStorage>();

        // Register BackgroundWorkerMongoConfiguration as IOptions<T>
        builder.Services.AddSingleton<IOptions<BackgroundWorkerMongoConfiguration>>(
            new OptionsWrapper<BackgroundWorkerMongoConfiguration>(configuration));

        return builder;
    }
}