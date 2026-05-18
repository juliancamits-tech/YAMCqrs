using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YAMCqrs.BackgroundWorker.Core.Domain;


namespace YAMCqrs.BackgroundWorker.Core.Abstractions;

/// <summary>
/// Base class for creating background workers that process items in batches with parallelism support.
/// </summary>
/// <typeparam name="TWorkItem">Class that is going to be processed by the worker.</typeparam>
/// <param name="serviceProvider">IServiceProvider</param>
public abstract partial class YABackgroundWorker<TWorkItem>(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IWorkerStorage _workerStorage = serviceProvider.GetRequiredService<IWorkerStorage>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    //This is used by the partial LoggerMessage methods, for that some IDE's might show it as unused.
    private readonly ILogger<TWorkItem> _logger = serviceProvider.GetRequiredService<ILogger<TWorkItem>>();
    #region Logger Messages
    [LoggerMessage(LogLevel.Warning, "Background worker for {WorkerName} is closed because InitialSetupAsync return false")]
    private partial void LogWorkerFailStartUp(string workerName);

    [LoggerMessage(LogLevel.Information, "Starting background worker for {WorkerName}")]
    private partial void LogStartingWorker(string workerName);

    [LoggerMessage(LogLevel.Debug, "Sleeping")]
    private partial void LogSleeping();

    [LoggerMessage(LogLevel.Debug, "Batch Size: {batchSize}")]
    private partial void LogBatchSize(int batchSize);
    #endregion

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Perform initial setup in a separate scope
        await using (var initialScope = _serviceScopeFactory.CreateAsyncScope())
        {
            if (!await InitialSetupAsync(initialScope, stoppingToken))
            {
                LogWorkerFailStartUp(GetWorkerName());
                return;
            }
        }

        LogStartingWorker(GetWorkerName());
        // Wait until it's time for the first execution based on the last execution time
        await FirstTimeSleep(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var workerExecution = new WorkerExecution(GetWorkerName());
            var silentSkip = false;

            try
            {
                // Each phase runs in its own scope intentionally.
                //
                // A scope defines the lifetime boundary of all Scoped services resolved within it.
                // Any Scoped service that holds internal state (open resources, cached data, counters,
                // connection state, etc.) will carry that state for as long as the scope lives.
                //
                // Sharing a single scope across phases would silently couple them through
                // whatever internal state those services accumulate, in ways the derived class
                // cannot predict or control. A fresh scope per phase ensures each phase starts
                // with a clean, predictable set of resources — regardless of what services are used.
                //
                // using(...){} is used instead of using var to ensure each scope is disposed
                // immediately after its phase completes, releasing resources as early as possible.

                // Phase 1: Pre-validation — determines whether the batch should run.
                PrevalidationResult prevalidationResult;
                using (var prevalidationScope = _serviceScopeFactory.CreateScope())
                {
                    prevalidationResult = await BatchPrevalidation(prevalidationScope, stoppingToken);
                }

                if (prevalidationResult.Outcome == PrevalidationOutcome.SkipSilently)
                {
                    silentSkip = true;
                    continue;
                }

                if (prevalidationResult.Outcome == PrevalidationOutcome.Skip)
                {
                    workerExecution.PrevalidationSkip(prevalidationResult.Message ?? string.Empty);
                    continue;
                }

                // Phase 2: Setup — prepares shared context before the batch is fetched.
                using (var setupScope = _serviceScopeFactory.CreateScope())
                {
                    await BatchSetupAsync(setupScope, stoppingToken);
                }

                // Phase 3: Fetch — retrieves the batch of items to process.
                IEnumerable<TWorkItem>? batch;
                using (var fetchScope = _serviceScopeFactory.CreateScope())
                {
                    batch = await GetBatchForProcessing(fetchScope, stoppingToken);
                }

                LogBatchSize(batch?.Count() ?? 0);
                if (batch == null || !batch.Any())
                {
                    continue;
                }

                // Phase 4: Process — each item runs in its own scope.
                // This is mandatory: items are processed in parallel and Scoped services
                // are not thread-safe by design.
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = InternalParallelismDegree(),
                    CancellationToken = stoppingToken
                };

                await Parallel.ForEachAsync(batch, parallelOptions, async (item, ct) =>
                {
                    try
                    {
                        using var itemScope = _serviceScopeFactory.CreateScope();
                        var success = await ProcessItemAsync(item, itemScope, workerExecution.Id, stoppingToken);

                        if (success)
                            workerExecution.IncrementSuccessCount();
                        else
                            workerExecution.IncrementFailedCount();
                    }
                    catch
                    {
                        // Any exception is counted as a failure
                        workerExecution.IncrementFailedCount();
                    }
                });
            }
            //There is not catch for the main processing loop, because any exception should be treated as a failure and counted in the error threshold percentage. If we catch the exception here, we might be hiding critical errors that should be fixed.
            finally
            {
                workerExecution.EndExecution(InternalErrorThresholdPercentage());

                if (!silentSkip)
                {
                    // Phase 5: Post-processing — runs after results are finalized.
                    using (var postScope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            await BatchPostProcesing(postScope, workerExecution, stoppingToken);
                        }
                        catch
                        {
                            // Logging is responsibility of the derived class
                        }
                    }

                    if (SaveEmptyResult(workerExecution))
                        await _workerStorage.AddAsync(workerExecution);
                }

                LogSleeping();
                await workerExecution.DelayUntilNextExecution(InternalSleepIntervalInSeconds(), stoppingToken);
            }
        }

        FinalCleanUp();
    }

    /// <summary>
    /// Delays the first execution based on when the last execution completed.
    /// Ensures the worker respects the sleep interval even after restarts.
    /// </summary>
    private async Task FirstTimeSleep(CancellationToken cancellationToken)
    {
        var lastExecution = await _workerStorage.GetLastExecutionAsync(this.GetType().FullName ?? this.GetType().Name);
        if (lastExecution != null)
        {
            LogSleeping();
            await lastExecution.DelayUntilNextExecution(InternalSleepIntervalInSeconds(), cancellationToken);
        }
    }

    protected string GetWorkerName()
    {
        return this.GetType().FullName ?? this.GetType().Name;
    }

    /// <summary>
    /// Returns the canonical name used to identify a worker in storage,
    /// without requiring an instance of the worker.
    /// </summary>
    /// <typeparam name="TWorker">The concrete worker type.</typeparam>
    /// <example>
    /// </example>
    public static string GetWorkerName<TWorker>() where TWorker : YABackgroundWorker<TWorkItem>
        => typeof(TWorker).FullName ?? typeof(TWorker).Name;

    #region Abstract Methods - Level Worker
    /// <summary>
    ///     Excute any initial setup required before the main worker logic runs. Must be implemented by derived classes.
    /// </summary>
    /// <param name="stoppingToken">Token to signal cancellation.</param>
    /// <returns>if false the background worker will not start.</returns>
    protected abstract Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken);

    /// <summary>
    /// Clean up resorurces when the worker is stopping.
    /// </summary>
    protected abstract void FinalCleanUp();
    #endregion

    #region Abstract Methods - Batch Worker
    /// <summary>
    /// Get the batch of items to be processed
    /// </summary>
    protected abstract Task<IEnumerable<TWorkItem>?> GetBatchForProcessing(IServiceScope serviceScope, CancellationToken stoppingToken);

    /// <summary>
    /// Process a single item from the batch.
    /// </summary>
    /// <param name="item">The work item to process.</param>
    /// <param name="serviceScope">Scoped service provider for this item.</param>
    /// <param name="executionId">
    ///     Correlation ID for the current batch execution. Can be used to associate
    ///     processed records with this execution for traceability purposes.
    ///     <para>
    ///     <strong>Warning:</strong> Do NOT create a foreign key from your own tables
    ///     pointing to the <c>WorkerExecutions</c> table using this ID.
    ///     The cleanup worker (<see cref="CleanBackGroundWorker"/>) periodically deletes
    ///     old execution records, and any FK constraint will cause those deletes to fail.
    ///     Store the ID as a plain <see cref="Guid"/> column (no FK) for correlation only.
    ///     </para>
    /// </param>
    /// <param name="stoppingToken">Token to signal cancellation.</param>
    /// <returns>
    ///     <c>true</c> if the item was processed successfully; <c>false</c> otherwise.
    ///     Exceptions are treated as failures.
    /// </returns>
    protected abstract Task<bool> ProcessItemAsync(
        TWorkItem item,
        IServiceScope serviceScope,
        Guid executionId,
        CancellationToken stoppingToken);

    /// <summary>
    /// Pre-validación previa al procesamiento del batch, para decidir si debe ejecutarse o saltearse.
    /// </summary>
    /// <param name="serviceScope">Scope de servicios aislado para esta fase.</param>
    /// <param name="stoppingToken">Token de cancelación.</param>
    /// <returns>
    /// Un <see cref="PrevalidationResult"/> que indica la acción a tomar:
    /// <list type="bullet">
    ///   <item><see cref="PrevalidationResult.Execute"/> — el batch se ejecuta normalmente.</item>
    ///   <item><see cref="PrevalidationResult.Skip(string)"/> — el batch se saltea y la ejecución queda registrada en storage con el motivo. Usar cuando el skip es relevante para auditoría (ej: servicio externo no disponible).</item>
    ///   <item><see cref="PrevalidationResult.SkipSilently"/> — el batch se saltea sin dejar registro. Usar cuando el skip es esperado y recurrente (ej: "solo ejecutar los lunes") para evitar ruido en el historial.</item>
    /// </list>
    /// </returns>
    protected abstract Task<PrevalidationResult> BatchPrevalidation(IServiceScope serviceScope, CancellationToken stoppingToken);

    /// <summary>
    /// Prepares the environment before the batch is retrieved and processed.
    /// Use this step for one-time operations shared across all batch items,
    /// such as truncating staging tables, creating directory structures,
    /// or fetching shared context data.
    /// </summary>
    protected abstract Task BatchSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken);

    /// <summary>
    /// Performs post-processing operations after the entire batch has been processed.
    /// </summary>
    /// <param name="currentExecution">The current execution context of the worker. You can get here information about the current batch execution.</param>
    /// <remarks>Override this method to implement custom batch post-processing logic. The operation should
    /// honor the cancellation token to support graceful shutdown.</remarks>
    protected abstract Task BatchPostProcesing(IServiceScope serviceScope, WorkerExecution currentExecution, CancellationToken stoppingToken);
    #endregion

    #region Configuration Properties

    /// <summary>
    /// The interval in seconds between each execution of the worker.
    /// The worker will sleep for this interval after completing its processing before starting the next cycle.
    /// The sleep interval is reduced by the time taken to process the batch.
    /// </summary>
    protected abstract int SleepIntervalInSeconds();

    /// <summary>
    /// Maximum number of concurrent operations that can be executed in parallel.
    /// </summary>
    /// <remarks>A higher value may improve throughput for workloads that can be parallelized, but may also
    /// increase resource usage. Setting this property to 1 disables parallelism, causing operations to execute
    /// sequentially.</remarks>
    protected abstract int ParallelismDegree();

    /// <summary>
    /// The maximum acceptable error rate percentage for a batch execution.
    /// If the percentage of failed items exceeds this threshold, the entire batch is considered failed.
    /// </summary>
    /// <remarks>
    /// Value should be between 0 and 100. For example, a value of 10 means that if more than 10% 
    /// of items fail, the batch execution will be marked as faulted.
    /// </remarks>
    /// <returns>The error threshold percentage (0-100).</returns>
    protected abstract int ErrorThresholdPercentage();
    /// <summary>
    /// Set if the batch should be audit when NoItemsToProcess
    /// True = The batch is NOT saved
    /// False = The batch is saved
    /// </summary>
    /// <returns>True/False</returns>
    protected abstract bool SkipEmptyResults();

    private bool SaveEmptyResult(WorkerExecution workerExecution)
    {
        if (workerExecution.Status != WorkerExecution.ExecutionStatus.NoItemsToProcess)
            return true;

        return !SkipEmptyResults();
    }

    /// <summary>
    /// Internal method to ensure error threshold is within valid range (0-100).
    /// </summary>
    private int InternalErrorThresholdPercentage()
    {
        var value = ErrorThresholdPercentage();
        return value < 0 ? 0 : value > 100 ? 100 : value;
    }

    /// <summary>
    /// Internal method to ensure sleep interval is at least 1 second.
    /// </summary>
    private int InternalSleepIntervalInSeconds()
    {
        var value = SleepIntervalInSeconds();
        return value <= 0 ? 1 : value;
    }

    /// <summary>
    /// Internal method to ensure parallelism degree is at least 1.
    /// </summary>
    private int InternalParallelismDegree()
    {
        var value = ParallelismDegree();
        return value <= 0 ? 1 : value;
    }

    #endregion
}