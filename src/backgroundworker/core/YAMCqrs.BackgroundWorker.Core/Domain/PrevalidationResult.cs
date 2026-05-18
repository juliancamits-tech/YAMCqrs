namespace YAMCqrs.BackgroundWorker.Core.Domain;

/// <summary>
/// Represents the result of the pre-validation phase of a batch,
/// indicating whether the worker should execute, skip with logging, or skip silently.
/// </summary>
public sealed record PrevalidationResult
{
    /// <summary>
    /// Result of the pre-validation.
    /// </summary>
    public PrevalidationOutcome Outcome { get; }

    /// <summary>
    /// Descriptive message for the reason of the skip. Only applies when <see cref="Outcome"/> is <see cref="PrevalidationOutcome.Skip"/>.
    /// </summary>
    public string? Message { get; }

    private PrevalidationResult(PrevalidationOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    /// <summary>
    /// The batch should be executed normally.
    /// </summary>
    public static PrevalidationResult Execute() => new(PrevalidationOutcome.Execute, null);
    /// <summary>
    /// Alias for <see cref="Execute"/>
    /// </summary>
    /// <returns></returns>
    public static PrevalidationResult Valid() => Execute();

    /// <summary>
    /// The batch should be skipped. The execution is logged in storage with the provided reason.
    /// Use when the skip is relevant for auditing or monitoring (e.g., external service unavailable).
    /// </summary>
    /// <param name="message">Reason for the skip. It is persisted in <see cref="WorkerExecution"/>.</param>
    public static PrevalidationResult Skip(string message) => new(PrevalidationOutcome.Skip, message);

    /// <summary>
    /// The batch should be skipped without leaving a record in storage.
    /// Use when the skip is expected and recurrent (e.g., "only execute on Mondays") and
    /// logging it would generate unnecessary noise in the history.
    /// </summary>
    public static PrevalidationResult SkipSilently() => new(PrevalidationOutcome.SkipSilently, null);
}

/// <summary>
/// Possible results of the pre-validation phase of a batch.
/// </summary>
public enum PrevalidationOutcome
{
    /// <summary>
    /// Default value. Should not be explicitly returned.
    /// </summary>
    None = 0,

    /// <summary>
    /// The batch should be executed.
    /// </summary>
    Execute = 1,

    /// <summary>
    /// The batch should be skipped and the execution is logged in storage.
    /// </summary>
    Skip = 2,

    /// <summary>
    /// The batch should be skipped without leaving a record in storage.
    /// </summary>
    SkipSilently = 3,
}
