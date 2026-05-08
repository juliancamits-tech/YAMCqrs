using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Configuration;
using YAMCqrs.BackgroundWorker.CustomHealthCheck;
using YAMCqrs.BackgroundWorker.Implementation;

namespace YAMCqrs.BackgroundWorker.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core background worker services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cfg">Configuration settings for the background worker.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// - IWorkerStorage implementation (InMemoryWorkerStorage if not already registered)
    /// - Background worker configuration
    /// - CleanBackGroundWorker as a hosted service
    /// - Health check for monitoring worker status
    /// </remarks>
    public static void AddBackgroundWorkerCore(
        this IServiceCollection services, BackgroundWorkerConfiguration? cfg)
    {
        if (services.Count(s => s.ServiceType == typeof(IWorkerStorage)) == 1)
        {
            if (cfg == null)
                return;

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            logger?.LogWarning("AddBackgroundWorkerCore was called multiple times. Only first registration is used.");
            return;
        }

        cfg ??= new BackgroundWorkerConfiguration();

        //Register the configuration
        services.AddSingleton<IOptions<BackgroundWorkerConfiguration>>(new OptionsWrapper<BackgroundWorkerConfiguration>(cfg));

        if (!typeof(IWorkerStorage).IsAssignableFrom(cfg.WorkerStorageType))
        {
            throw new ArgumentException(
                $"Type {cfg.WorkerStorageType.Name} must implement IWorkerStorage",
                nameof(cfg));
        }

        // Add the store for the events
        services.AddSingleton(typeof(IWorkerStorage), cfg.WorkerStorageType);

        services.AddHostedService<CleanBackGroundWorker>();

        services.AddHealthChecks().AddCheck<HealthCheckReport>("Background Worker Health Check", tags: ["background", "worker"]);
        return;
    }
}

