using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.CleanBackgroundWorkerTests;

public class MockWorkerStorage : IWorkerStorage
{
    private readonly List<WorkerExecution> _executions = [];
    public int CleanStorageAsyncCallCount { get; private set; }

    public Task AddAsync(WorkerExecution execution, CancellationToken cancellationToken)
    {
        _executions.Add(execution);
        return Task.CompletedTask;
    }

    public Task CleanStorageAsync(DateTime dateForCompleted, DateTime dateForError, CancellationToken cancellationToken)
    {
        CleanStorageAsyncCallCount++;
        return Task.CompletedTask;
    }

    public Task<WorkerExecution?> GetLastExecutionAsync(string workerName, CancellationToken cancellationToken)
        => Task.FromResult<WorkerExecution?>(null);

    public Task<List<WorkerExecution>> GetLastExecutionByWorkerAsync(CancellationToken cancellationToken)
        => Task.FromResult(new List<WorkerExecution>());
}

