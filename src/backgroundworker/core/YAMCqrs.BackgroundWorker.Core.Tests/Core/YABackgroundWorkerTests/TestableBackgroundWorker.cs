using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.YABackgroundWorkerTests;

internal class TestableBackgroundWorker(
    IServiceProvider serviceProvider,
    ILogger<TestableBackgroundWorker> logger,
    Func<IServiceScope, CancellationToken, Task<bool>>? initialSetup = null,
    Action? finalCleanup = null,
    Func<IServiceScope, CancellationToken, Task<IEnumerable<string>?>>? getBatch = null,
    Func<string, IServiceScope, CancellationToken, Task<bool>>? processItem = null,
    Func<IServiceScope, CancellationToken, Task<PrevalidationResult>>? batchPrevalidation = null,
    Func<IServiceScope, WorkerExecution, CancellationToken, Task>? batchPostProcesing = null,
    int sleepInterval = 1,
    int parallelismDegree = 1,
    int errorThreshold = 10,
    bool skipEmptyResults = true) : YABackgroundWorker<string>(serviceProvider, logger)
{
    private readonly Func<IServiceScope, CancellationToken, Task<bool>> _initialSetup = initialSetup ?? ((scope, ct) => Task.FromResult(true));
    private readonly Action _finalCleanup = finalCleanup ?? (() => { });
    private readonly Func<IServiceScope, CancellationToken, Task<IEnumerable<string>?>> _getBatch = getBatch ?? ((scope, ct) => Task.FromResult<IEnumerable<string>?>(null));
    private readonly Func<string, IServiceScope, CancellationToken, Task<bool>> _processItem = processItem ?? ((item, scope, ct) => Task.FromResult(true));
    private readonly Func<IServiceScope, CancellationToken, Task<PrevalidationResult>> _batchPrevalidation = batchPrevalidation ?? ((scope, ct) => Task.FromResult(PrevalidationResult.Execute()));
    private readonly Func<IServiceScope, WorkerExecution, CancellationToken, Task> _batchPostProcesing = batchPostProcesing ?? ((scope, execution, ct) => Task.CompletedTask);
    private readonly int _sleepInterval = sleepInterval;
    private readonly int _parallelismDegree = parallelismDegree;
    private readonly int _errorThreshold = errorThreshold;
    private readonly bool _skipEmptyResults = skipEmptyResults;

    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
        => _initialSetup(serviceScope, stoppingToken);

    protected override void FinalCleanUp()
        => _finalCleanup();

    protected override bool SkipEmptyResults() => _skipEmptyResults;

    protected override Task<IEnumerable<string>?> GetBatchForProcessing(IServiceScope serviceScope, CancellationToken stoppingToken)
        => _getBatch(serviceScope, stoppingToken);

    protected override Task<bool> ProcessItemAsync(string item, IServiceScope serviceScope, Guid executionId, CancellationToken stoppingToken)
        => _processItem(item, serviceScope, stoppingToken);

    protected override Task<PrevalidationResult> BatchPrevalidation(IServiceScope serviceScope, CancellationToken stoppingToken)
        => _batchPrevalidation(serviceScope, stoppingToken);

    protected override Task BatchPostProcesing(IServiceScope serviceScope, WorkerExecution currentExecution, CancellationToken stoppingToken)
 => _batchPostProcesing(serviceScope, currentExecution, stoppingToken);

    protected override int SleepIntervalInSeconds() => _sleepInterval;

    protected override int ParallelismDegree() => _parallelismDegree;

    protected override int ErrorThresholdPercentage() => _errorThreshold;

    public int PErrorThresholdPercentage() => ErrorThresholdPercentage();
    public int PParallelismDegree() => ParallelismDegree();
    public int PSleepIntervalInSeconds() => SleepIntervalInSeconds();

    protected override Task BatchSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    
}