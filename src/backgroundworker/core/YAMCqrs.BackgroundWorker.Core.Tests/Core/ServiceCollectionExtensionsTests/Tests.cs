using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.CustomHealthCheck;
using YAMCqrs.BackgroundWorker.Core.Extensions;
using YAMCqrs.BackgroundWorker.Core.Implementation;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.ServiceCollectionExtensionsTests;

public class Tests
{
    [Fact]
    public void AddBackgroundWorkerCore_Should_Register_WithDefaultConfig()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks();
        services.AddBackgroundWorker();
        var provider = services.BuildServiceProvider();

        
        var storage = provider.GetRequiredService<IWorkerStorage>();
        Assert.IsType<InMemoryWorkerStorage>(storage);

        var background = provider.GetServices<IHostedService>().OfType<CleanBackGroundWorker>().FirstOrDefault();
        Assert.NotNull(background);

        var cfg = provider.GetRequiredService<IOptions<BackgroundWorkerConfiguration>>();
        var defaultCfg = new BackgroundWorkerConfiguration();
        Assert.Equal(cfg.Value.MinutesToKeepSuccesTask, defaultCfg.MinutesToKeepSuccesTask);
        Assert.Equal(cfg.Value.MinutesToKeepFailedTask, defaultCfg.MinutesToKeepFailedTask);

        var hc = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        Assert.True(hc?.Value.Registrations.Any(x => x.Name == "Background Worker Health Check"));
    }

    [Fact]
    public void AddBackgroundWorkerCore_Should_Register_WithCustomConfig()
    {
        var defaultCfg = new BackgroundWorkerConfiguration(Random.Shared.Next(1,100), Random.Shared.Next(1, 100));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBackgroundWorker(options =>
        {
            options.MinutesToKeepSuccesTask = defaultCfg.MinutesToKeepSuccesTask;
            options.MinutesToKeepFailedTask = defaultCfg.MinutesToKeepFailedTask;
        });
        var provider = services.BuildServiceProvider();

        var cfg = provider.GetRequiredService<IOptions<BackgroundWorkerConfiguration>>();
        
        Assert.Equal(cfg.Value.MinutesToKeepSuccesTask, defaultCfg.MinutesToKeepSuccesTask);
        Assert.Equal(cfg.Value.MinutesToKeepFailedTask, defaultCfg.MinutesToKeepFailedTask);
    }

}
