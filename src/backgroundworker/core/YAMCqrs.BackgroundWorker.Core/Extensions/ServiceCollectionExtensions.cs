using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.CustomHealthCheck;
using YAMCqrs.BackgroundWorker.Core.Implementation;

namespace YAMCqrs.BackgroundWorker.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Entry point for registering background worker services using a fluent API.
    /// </summary>
    public static IBackgroundWorkerBuilder AddBackgroundWorker(
        this IServiceCollection services,
        Action<BackgroundWorkerConfiguration>? configure = null)
    {
        var config = new BackgroundWorkerConfiguration();
        configure?.Invoke(config);

        return services.AddBackgroundWorkerCore(config);
    }

    /// <summary>
    /// Registers the core background worker services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cfg">Configuration settings for the background worker.</param>
    /// <returns>A builder for further configuration.</returns>
    /// <remarks>
    /// This method registers:
    /// - IWorkerStorage implementation
    /// - Background worker configuration
    /// - CleanBackGroundWorker as a hosted service
    /// - Health check for monitoring worker status
    /// </remarks>
    internal static IBackgroundWorkerBuilder AddBackgroundWorkerCore(
        this IServiceCollection services, BackgroundWorkerConfiguration cfg)
    {
        // Register the configuration
        services.AddSingleton<IOptions<BackgroundWorkerConfiguration>>(new OptionsWrapper<BackgroundWorkerConfiguration>(cfg));
        services.AddSingleton<IWorkerStorage, InMemoryWorkerStorage>();
        services.AddHostedService<CleanBackGroundWorker>();
        services.AddHealthChecks().AddCheck<HealthCheckReport>("Background Worker Health Check", tags: ["background", "worker"]);
        return new BackgroundWorkerBuilder(services, cfg);
    }
}

public interface IBackgroundWorkerBuilder
{
    IServiceCollection Services { get; }
    BackgroundWorkerConfiguration Configuration { get; }
}

internal class BackgroundWorkerBuilder(IServiceCollection services, BackgroundWorkerConfiguration configuration) : IBackgroundWorkerBuilder
{
    public IServiceCollection Services => services;
    public BackgroundWorkerConfiguration Configuration => configuration;
}
