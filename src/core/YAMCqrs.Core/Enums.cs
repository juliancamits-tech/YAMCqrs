namespace YAMCqrs.Core;

/// <summary>
/// Defines the execution layers for CQRS interceptors.
/// Lower values execute first in OnBefore, last in OnAfter (like a stack).
/// </summary>
public enum InterceptorLayer
{
    /// <summary>
    /// None / unassigned layer (safe default value).
    /// </summary>
    None = 0,

    /// <summary>
    /// Security layer: Authentication, Authorization, Identity
    /// Executes first to ensure user context is established
    /// </summary>
    Security = 1,

    /// <summary>
    /// Validation layer: Input validation, business rule validation
    /// Executes after security to validate user input
    /// </summary>
    Validation = 2,

    /// <summary>
    /// Logging layer: Audit trails, diagnostics, telemetry
    /// Executes after validation to log valid operations
    /// </summary>
    Logging = 3,

    /// <summary>
    /// Performance layer: Caching, throttling, rate limiting
    /// Executes after logging to optimize performance
    /// </summary>
    Performance = 4,

    /// <summary>
    /// Transaction layer: Unit of Work, database transactions
    /// Executes close to the handler to manage transactions
    /// </summary>
    Transaction = 5,

    /// <summary>
    /// Resilience layer: Retry policies, circuit breakers, timeouts
    /// Executes close to the handler to handle failures
    /// </summary>
    Resilience = 6,

    /// <summary>
    /// Application layer: Default layer for custom interceptors
    /// </summary>
    Application = 10
}