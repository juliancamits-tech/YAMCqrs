using YAMCqrs.BackgroundWorker.Domain;

namespace YAMCqrs.BackgroundWorker.Abstractions;

public interface IWorkerStorage
{
    /// <summary>
    /// Save the result of a worker execution
    /// </summary>
    public Task AddAsync(WorkerExecution execution);
    /// <summary>
    /// Get the last execution of a worker
    /// </summary>
    public Task<WorkerExecution?> GetLastExecutionAsync(string workerName);
    /// <summary>
    /// Clean storage of old worker executions
    /// </summary>
    public Task CleanStorageAsync(DateTime dateForCompleted, DateTime dateForError);
    /// <summary>
    /// Get the last execution of each worker
    /// </summary>
    /// <returns>List of the last execution for each workerName</returns>
    public Task<List<WorkerExecution>> GetLastExecutionByWorkerAsync();
}
