namespace YAMCqrs.ServiceBus.Core.Configuration;

public class ServiceBusConfiguration
{
    public int ConcurrentWorkers { get; set; } = 4;
    public int BatchSize { get; set; } = 10;
    public int PollingIntervalSeconds { get; set; } = 5;
    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
    public int ErrorThresholdPercentage { get; set; } = 50;
}
