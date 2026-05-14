namespace YAMCqrs.EventBus.Core;

/// <summary>
/// Represents the status of an event in the event store
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Unknown / uninitialized status (safe default value).
    /// </summary>
    None = 0,

    /// <summary>
    /// Event is pending processing
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Event is currently being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Event has been successfully processed
    /// </summary>
    Processed = 3,

    /// <summary>
    /// Event processing has failed
    /// </summary>
    Failed = 4
}

/// <summary>
/// Represents the supported service bus providers for event publishing
/// </summary>
public enum ServiceBusProvider
{
    /// <summary>
    /// No provider specified (safe default value). Should not be used for actual event publishing.
    /// </summary>
    None = 0,
    /// <summary>
    /// In-memory provider using for domain events. Suitable for production use with in memory infrastructure.
    /// </summary>
    InMemory = 1,
    /// <summary>
    /// Kafka provider for event publishing. Requires additional configuration and dependencies. Suitable for production use with Kafka infrastructure.
    /// </summary>
    Kafka = 2,
}