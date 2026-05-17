namespace YAMCqrs.EventBus.Core.Configuration;

public class EventBusConfiguration
{
    public int ConcurrentWorkers { get; set; } = 4;
    public int BatchSize { get; set; } = 10;
    public int PollingIntervalSeconds { get; set; } = 5;
    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
    public int ErrorThresholdPercentage { get; set; } = 50;

    public int GetConcurrentWorkers()
    {
        return (int)Math.Truncate((double)ConcurrentWorkers / 2);
    }
}
