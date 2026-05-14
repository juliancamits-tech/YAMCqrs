using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YAMCqrs.BackgroundWorker.Extensions;
using YAMCqrs.BackgroundWorker.Storage.MondgoDb;
using YAMCqrs.Core;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.Extensions;
using YAMCqrs.EventBus.Storage.MongoDb.Extensions;

namespace Test.Application.Extensions;

/// <summary>
/// Provides extension methods for configuring application services.
/// </summary>
public static class ServicesCollectionExtension
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddBackgroundWorker(options =>
        {
            options.MinutesToKeepSuccesTask = 30;
            options.MinutesToKeepFailedTask = 30;
        })
        .UseMongoDb(new BackgroundWorkerMongoConfiguration
        {
            ConnectionString = "cs_MongoDb",
            DatabaseName = "TestAppDb",
        });

        services.AddCqrs();

        services.AddEventBus(opt =>
        {
            opt.ConcurrentWorkers = 1;
            opt.BatchSize = 100;
        })
        .UseMongoDb(new YAMCqrs.EventBus.Storage.MongoDb.EventBusStorageMongoConfiguration()
        {
            ConnectionString = "cs_MongoDb",
            DatabaseName = "TestAppDb",
        });
    }
}