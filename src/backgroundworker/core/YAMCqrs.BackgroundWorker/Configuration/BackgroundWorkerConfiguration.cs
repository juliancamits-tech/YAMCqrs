using YAMCqrs.BackgroundWorker.Implementation;

namespace YAMCqrs.BackgroundWorker.Configuration;

/// <summary>
/// Represents the configuration settings for a background worker, including task retention durations and storage
/// options.
/// </summary>
/// <remarks>Use this class to specify how long completed or failed tasks are retained and to configure
/// the storage mechanism for background worker tasks. The default settings use in-memory storage and retain both
/// successful and failed tasks for 60 minutes.</remarks>
public class BackgroundWorkerConfiguration
{
    /// <summary>
    /// Gets or sets the type of storage to be used for worker executions. The default is InMemoryWorkerStorage, which stores data in memory and is suitable for development and testing. For production scenarios, consider implementing a custom storage solution that persists data to a database or other durable storage mechanism.
    /// </summary>
    public Type WorkerStorageType { get; set; } = typeof(InMemoryWorkerStorage);

    /// <summary>
    /// Gets or sets the number of minutes to retain information about successful tasks.
    /// </summary>
    public int MinutesToKeepSuccesTask { get; set; } = 60;
    /// <summary>
    /// Gets or sets the number of minutes to retain information about failed tasks.
    /// <remarks>Use static class BackGroundWorkerConfiguration.HourToMinutes and BackGroundWorkerConfiguration.DayToMinutes if you are lazy</remarks>
    /// </summary>
    public int MinutesToKeepFailedTask { get; set; } = 60;

    /// <summary>
    /// Convert Hours to Minutes
    /// </summary>

    public static int HourToMinutes(int hours)
    {
        return hours * 60;
    }
    /// <summary>
    /// Convert Days to Minutes
    /// </summary>
    public static int DayToMinutes(int days)
    {
        return HourToMinutes(days * 24);
    }

    internal DateTime GetDateForSuccessfulTaskCleanup()
    {
        return DateTime.UtcNow.AddMinutes(-MinutesToKeepSuccesTask);
    }
    internal DateTime GetDateForFailedTaskCleanup()
    {
        return DateTime.UtcNow.AddMinutes(-MinutesToKeepFailedTask);
    }
}
