using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Configuration;
using YAMCqrs.BackgroundWorker.Domain;

namespace YAMCqrs.BackgroundWorker.Implementation;

/// <summary>
/// Background worker that periodically cleans old execution records from storage.
/// Runs every 5 minutes to remove outdated execution history.
/// </summary>
public class CleanBackGroundWorker(IServiceProvider serviceProvider) : YABackgroundWorker<int>(serviceProvider)
{
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
        var options = serviceScope.ServiceProvider.GetService<IOptionsSnapshot<BackgroundWorkerConfiguration>>();
        var optValue = options?.Value ?? new BackgroundWorkerConfiguration();

        var storage = serviceScope.ServiceProvider.GetRequiredService<IWorkerStorage>();
        await storage.CleanStorageAsync(optValue.GetDateForSuccessfulTaskCleanup(), optValue.GetDateForFailedTaskCleanup());

        return true;
    }

    /// <summary>
    /// Cleanup runs every 5 minutes (300 seconds).
    /// </summary>
    protected override int SleepIntervalInSeconds()
    {
        return 60 * 5;
    }
}