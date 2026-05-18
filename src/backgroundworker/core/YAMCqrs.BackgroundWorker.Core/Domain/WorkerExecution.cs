using System.Reflection;

namespace YAMCqrs.BackgroundWorker.Core.Domain;

/// <summary>
/// Represents a single execution of a background worker, tracking its status, timing, and results.
/// </summary>
public class WorkerExecution(string workerName)
{
    /// <summary>
    /// Unique identifier for this execution instance.
    /// </summary>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    /// Name of the worker that performed this execution.
    /// </summary>
    public string WorkerName { get; set; } = workerName;

    /// <summary>
    /// UTC timestamp when the execution started.
    /// </summary>
    public DateTime ExecutionStartTime { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the execution ended.
    /// </summary>
    public DateTime ExecutionEndTime { get; private set; }

    /// <summary>
    /// Current status of the execution.
    /// </summary>
    public ExecutionStatus Status { get; private set; } = ExecutionStatus.Null;

    /// <summary>
    /// Indicates whether the execution completed successfully.
    /// </summary>
    public bool IsSuccessful => Status == ExecutionStatus.Success || Status == ExecutionStatus.NoItemsToProcess;

    private int _success;
    private int _failed;

    /// <summary>
    /// Number of items successfully processed.
    /// </summary>
    public int Success => _success;

    /// <summary>
    /// Number of items that failed to process.
    /// </summary>
    public int Failed => _failed;

    /// <summary>
    /// Additional information about the execution status.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public string Owner { get; private set; } = GetOwner();

    /// <summary>
    /// Atomically increments the count of successfully processed items.
    /// Thread-safe for parallel processing.
    /// </summary>
    public void IncrementSuccessCount()
    {
        Interlocked.Increment(ref _success);
    }

    /// <summary>
    /// Atomically increments the count of failed items.
    /// Thread-safe for parallel processing.
    /// </summary>
    public void IncrementFailedCount()
    {
        Interlocked.Increment(ref _failed);
    }

    /// <summary>
    /// Marks the execution as ended and determines the final status based on the error threshold.
    /// </summary>
    /// <param name="errorThresholdPercentage">Maximum acceptable error rate percentage (0-100).</param>
    public void EndExecution(int errorThresholdPercentage)
    {
        ExecutionEndTime = DateTime.UtcNow;

        if (this.Status == ExecutionStatus.Null)
            ShouldMarkAsFaulted(errorThresholdPercentage);
    }


    /// <summary>
    /// Marks the execution as skipped due to failed pre-validation.
    /// </summary>
    /// <param name="message">Reason for skipping the execution.</param>
    public void PrevalidationSkip(string message)
    {
        this.Status = ExecutionStatus.FailedPrevalidation;
        this.Message = message;
    }

    /// <summary>
    /// Determines whether the execution should be marked as failed based on the error threshold.
    /// </summary>
    /// <param name="errorThresholdPercentage">Maximum acceptable error rate percentage.</param>
    private void ShouldMarkAsFaulted(int errorThresholdPercentage)
    {
        var total = Success + Failed;

        if (total == 0)
        {
            this.Status = ExecutionStatus.NoItemsToProcess;
            this.Message = "No items to process.";
            return;
        }

        var errorPercentage = (Failed * 100) / total;
        if (errorPercentage >= errorThresholdPercentage)
        {
            this.Status = ExecutionStatus.Failed;
            this.Message = $"Execution failed with {errorPercentage}% errors, which meets or exceeds the threshold of {errorThresholdPercentage}%.";
            return;
        }

        this.Status = ExecutionStatus.Success;
    }

    /// <summary>
    /// Delays execution until the specified interval has elapsed since the previous execution.
    /// </summary>
    /// <remarks>If the specified interval has already elapsed, the method returns immediately without
    /// waiting. The delay is based on the time since the last execution, as determined by the internal execution
    /// end time.</remarks>
    /// <param name="sleepIntervalInSeconds">The number of seconds to wait after the previous execution before proceeding. Must be a non-negative value.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delay operation.</param>
    /// <returns>A task that represents the asynchronous delay operation.</returns>
    public async Task DelayUntilNextExecution(int sleepIntervalInSeconds, CancellationToken cancellationToken)
    {
        var executionDuration = ExecutionEndTime - ExecutionStartTime;
        var delayDuration = TimeSpan.FromSeconds(sleepIntervalInSeconds) - executionDuration;

        if (delayDuration > TimeSpan.Zero)
        {
            await Task.Delay(delayDuration, cancellationToken);
        }
    }

    /// <summary>
    /// Represents the possible states of a worker execution.
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// Initial state before execution completes.
        /// </summary>
        Null,

        /// <summary>
        /// Execution completed successfully within error threshold.
        /// </summary>
        Success,

        /// <summary>
        /// Execution failed due to exceeding error threshold.
        /// </summary>
        Failed,

        /// <summary>
        /// Execution was skipped due to failed pre-validation checks.
        /// </summary>
        FailedPrevalidation,

        /// <summary>
        /// Execution completed but no items were available to process.
        /// </summary>
        NoItemsToProcess
    }

    /// <summary>
    /// Calculates the total execution duration in seconds.
    /// </summary>
    /// <returns>Duration in seconds as an integer.</returns>
    public int GetDurationInSeconds()
    {
        return (int)(ExecutionEndTime - ExecutionStartTime).TotalSeconds;
    }

    #region Constructors

    public WorkerExecution(Guid id, string workerName, DateTime executionStartTime, DateTime executionEndTime, ExecutionStatus status, int success, int failed, string message) : this(workerName)
    {
        Id = id;
        ExecutionStartTime = executionStartTime;
        ExecutionEndTime = executionEndTime;
        Status = status;
        _success = success;
        _failed = failed;
        Message = message;
    }
    #endregion

    private static string OwnerResult = string.Empty;
    private static string GetOwner()
    {
        if (string.IsNullOrEmpty(OwnerResult))
        {
            // Get the entry assembly (the main application, not the NuGet package or caller).
            var assembly = Assembly.GetEntryAssembly();
            // Avoid using GetExecutingAssembly() since it will return the NuGet package assembly name.
            // Avoid using GetCallingAssembly() since it may return a caller assembly, 
            // which can be problematic if one NuGet package calls another.
            ArgumentNullException.ThrowIfNull(assembly);

            // Retrieve the AssemblyTitleAttribute which contains the application name.
            var titleAttribute =
                (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))!;

            // Default to an empty string if no title is found.
            var serviceName = titleAttribute?.Title ?? "";

            // Format the name based on the specified mode.
            return OwnerResult = serviceName;
        }

        return OwnerResult;
    }

}
