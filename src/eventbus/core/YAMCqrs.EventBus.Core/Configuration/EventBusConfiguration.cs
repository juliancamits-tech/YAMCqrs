using System.Diagnostics.CodeAnalysis;

namespace YAMCqrs.EventBus.Core.Configuration;

[ExcludeFromCodeCoverage(Justification = "Configuration class with no logic.")]
public class EventBusConfiguration
{
    public WorkerConfiguration PublishConfiguration { get; init; } = new WorkerConfiguration();
    public WorkerConfiguration SubscribeConfiguration { get; init; } = new WorkerConfiguration();
}

[ExcludeFromCodeCoverage(Justification = "Configuration class with no logic.")]
public class WorkerConfiguration()
{
    public int ConcurrentWorkers { get; set; } = 4;
    public int BatchSize { get; set; } = 10;
    public int PollingIntervalSeconds { get; set; } = 5;
    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
    public int ErrorThresholdPercentage { get; set; } = 50;
}
