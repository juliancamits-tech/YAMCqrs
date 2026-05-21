using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.Implementation;

/// <summary>
/// Background worker that periodically cleans old execution records from storage.
/// Runs every 5 minutes to remove outdated execution history.
/// </summary>
public class CleanBackGroundWorker(IServiceProvider serviceProvider, ILogger<CleanBackGroundWorker> logger, IOptions<BackgroundWorkerConfiguration> options) : YABackgroundWorker<int>(serviceProvider, logger)
{
    private readonly BackgroundWorkerConfiguration _configuration = options.Value;

    protected override Task BatchPostProcesing(IServiceScope serviceScope, WorkerExecution currentExecution, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// La pre-validación del worker de limpieza siempre aprueba la ejecución.
    /// </summary>
    protected override Task<PrevalidationResult> BatchPrevalidation(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.FromResult(PrevalidationResult.Execute());
    }

    protected override Task BatchSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Error threshold set to 100% - cleanup operations are always considered successful.
    /// </summary>
    protected override int ErrorThresholdPercentage()
    {
        return 100;
    }

    /// <summary>
    /// No cleanup required when the worker stops.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Nothing to test")]
    protected override void FinalCleanUp()
    {
        return;
    }

    /// <summary>
    /// Returns a single dummy item to trigger the cleanup process.
    /// </summary>
    protected override Task<IEnumerable<int>?> GetBatchForProcessing(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.FromResult<IEnumerable<int>?>([1]);
    }

    /// <summary>
    /// Initial setup always succeeds for the cleanup worker.
    /// </summary>
    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Cleanup runs sequentially without parallelism.
    /// </summary>
    protected override int ParallelismDegree()
    {
        return 1;
    }

    /// <summary>
    /// Processes the cleanup by removing old execution records from storage.
    /// Removes records older than 10 minutes for both successful and failed executions.
    /// </summary>
    protected override async Task<bool> ProcessItemAsync(int item, IServiceScope serviceScope, Guid executionId, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(serviceScope);

        var storage = serviceScope.ServiceProvider.GetRequiredService<IWorkerStorage>();
        await storage.CleanStorageAsync(_configuration.GetDateForSuccessfulTaskCleanup(), _configuration.GetDateForFailedTaskCleanup(), stoppingToken);

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Can't have empty Result, this functions never is called.")]
    protected override bool SkipEmptyResults()
    {
        return true;
    }

    /// <summary>
    /// Cleanup runs every 1 hour (3600 seconds).
    /// </summary>
    protected override int SleepIntervalInSeconds()
    {
        return 1 * 60 * 60;
    }
}