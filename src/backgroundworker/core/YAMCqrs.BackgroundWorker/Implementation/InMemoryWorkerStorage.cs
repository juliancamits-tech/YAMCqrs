using System.Collections.Concurrent;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Domain;

namespace YAMCqrs.BackgroundWorker.Implementation;

/// <summary>
/// In-memory implementation of IWorkerStorage for storing worker execution history.
/// Uses a thread-safe ConcurrentBag for storage without persistence.
/// </summary>
/// <remarks>
/// <para><strong>WARNING: NOT INTENDED FOR PRODUCTION USE</strong></para>
/// <para>This implementation stores data in memory only, which has critical limitations:</para>
/// <list type="bullet">
/// <item><description><strong>Data Loss:</strong> All execution history is lost when the application restarts.</description></item>
/// <item><description><strong>Incorrect Scheduling:</strong> Workers cannot track when they should next execute after a restart. 
/// A worker configured to run every hour will execute immediately on restart, even if only 40 minutes had passed.</description></item>
/// <item><description><strong>Inconsistent Health Checks:</strong> Health checks cannot report accurate status after restarts due to lost history.</description></item>
/// </list>
/// <para><strong>Recommended Use:</strong> Local development and testing only.</para>
/// <para><strong>For Production:</strong> Implement a persistent version of <see cref="IWorkerStorage"/> using a database or other durable storage.</para>
/// </remarks>
internal class InMemoryWorkerStorage : IWorkerStorage
{
    /// <summary>
    /// Thread-safe collection storing all worker executions in memory.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WorkerExecution>> _executions = new();

    /// <summary>
    /// Adds a worker execution record to the in-memory storage.
    /// </summary>
    /// <param name="execution">The execution record to store.</param>
    /// <returns>A completed task.</returns>
    public Task AddAsync(WorkerExecution execution)
    {
        var workerExecutions = _executions.GetOrAdd(execution.WorkerName, _ => new ConcurrentDictionary<Guid, WorkerExecution>());
        workerExecutions.TryAdd(execution.Id, execution);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes old execution records from storage based on their completion date and status.
    /// </summary>
    /// <param name="dateForCompleted">Cutoff date for successful executions. Records older than this will be removed.</param>
    /// <param name="dateForError">Cutoff date for failed executions. Records older than this will be removed.</param>
    /// <returns>A completed task.</returns>
    public Task CleanStorageAsync(DateTime dateForCompleted, DateTime dateForError)
    {
        foreach (var worker in _executions)
        {
            var keyToRemove = new List<Guid>();
            foreach (var execution in worker.Value)
            {
                if (execution.Value.ExecutionEndTime < dateForCompleted && execution.Value.IsSuccessful)
                {
                    keyToRemove.Add(execution.Key);
                }
                else if (execution.Value.ExecutionEndTime < dateForError && !execution.Value.IsSuccessful)
                {
                    keyToRemove.Add(execution.Key);
                }
            }

            foreach (var key in keyToRemove)
            {
                worker.Value.TryRemove(key, out _);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the most recent execution record for a specific worker.
    /// </summary>
    /// <param name="workerName">The name of the worker.</param>
    /// <returns>The last execution record, or null if none exists.</returns>
    public Task<WorkerExecution?> GetLastExecutionAsync(string workerName)
    {
        if (!_executions.TryGetValue(workerName, out var workerExecutions))
            return Task.FromResult<WorkerExecution?>(null);

        var execution = workerExecutions.OrderByDescending(x => x.Value.ExecutionStartTime).LastOrDefault();

        return Task.FromResult(execution.Value ?? null);
    }

    /// <summary>
    /// Retrieves the most recent execution record for each distinct worker.
    /// </summary>
    /// <returns>A list containing the last execution for each worker.</returns>
    public Task<List<WorkerExecution>> GetLastExecutionByWorkerAsync()
    {
        var result = new List<WorkerExecution>();

        foreach (var worker in _executions)
        {
            var lastExecution = worker.Value.OrderByDescending(e => e.Value.ExecutionStartTime).FirstOrDefault().Value;
            if (lastExecution != null)
            {
                result.Add(lastExecution);
            }
        }

        return Task.FromResult(result);
    }

}