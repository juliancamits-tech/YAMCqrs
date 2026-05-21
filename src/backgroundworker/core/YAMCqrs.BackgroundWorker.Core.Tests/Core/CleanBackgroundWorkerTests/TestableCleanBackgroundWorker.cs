using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.Implementation;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.CleanBackgroundWorkerTests;

public class TestableCleanBackgroundWorker(IServiceProvider serviceProvider, ILogger<CleanBackGroundWorker> logger, IOptions<BackgroundWorkerConfiguration> cfg) : CleanBackGroundWorker(serviceProvider, logger, cfg)
{

    // Expose protected methods for testing
    public new Task<bool> ProcessItemAsync(int item, IServiceScope scope, Guid executionId, CancellationToken token)
        => base.ProcessItemAsync(item, scope, executionId, token);

    public new Task<IEnumerable<int>?> GetBatchForProcessing(IServiceScope scope, CancellationToken token)
        => base.GetBatchForProcessing(scope, token);

    public new int SleepIntervalInSeconds() => base.SleepIntervalInSeconds();
}

