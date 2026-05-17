using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Domain;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Domain;

namespace YAMCqrs.EventBus.Core.PublishEvents.Implementation;

/// <summary>
/// Background service that processes pending events with configurable parallelism
/// </summary>
internal partial class PublishEventProcessorService(
    ILogger<PublishEventProcessorService> logger,
    IEventDispatcher eventDispatcher,
    IPublishEventStore eventStore,
    IOptions<EventBusConfiguration> configuration,
    IServiceProvider serviceProvider)
    : YABackgroundWorker<PublishStoredEvent>(serviceProvider)
{
    private const int ErrorThresholdPercen = 50;

    private readonly ILogger<PublishEventProcessorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    private readonly IPublishEventStore _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    private readonly EventBusConfiguration _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));

    #region Logger Messages
    // ========== High-Performance LoggerMessage Delegates ==========

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Event Processor Service started with {Workers} workers, batch size: {BatchSize}, polling: {Interval}")]
    private static partial void LogServiceStarted(
        ILogger logger,
        int workers,
        int batchSize,
        TimeSpan interval);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Event Processor Service stopped")]
    private static partial void LogServiceStopped(ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error fetching pending events")]
    private static partial void LogErrorFetchingEvents(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Fetched {Count} pending events")]
    private static partial void LogEventsFetched(ILogger logger, int count);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Worker #{WorkerId} started")]
    private static partial void LogWorkerStarted(ILogger logger, int workerId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Worker #{WorkerId} stopped")]
    private static partial void LogWorkerStopped(ILogger logger, int workerId);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Worker #{WorkerId} processing: {EventType} [{Id}]")]
    private static partial void LogProcessingEvent(
        ILogger logger,
        int workerId,
        string eventType,
        Guid id);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Failed to deserialize event {EventId} of type {EventType}")]
    private static partial void LogDeserializationFailed(
        ILogger logger,
        Guid eventId,
        string eventType);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Information,
        Message = "Worker #{WorkerId} completed: {EventType} [{Id}]")]
    private static partial void LogEventCompleted(
        ILogger logger,
        int workerId,
        string eventType,
        Guid id);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Worker #{WorkerId} no handler for: {EventType}")]
    private static partial void LogNoHandlerFound(
        ILogger logger,
        int workerId,
        string eventType);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Worker #{WorkerId} error processing event [{Id}]")]
    private static partial void LogErrorProcessingEvent(
        ILogger logger,
        Exception exception,
        int workerId,
        Guid id);
    #endregion

    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        LogServiceStarted(_logger, _configuration.ConcurrentWorkers, _configuration.BatchSize, _configuration.PollingInterval);
        return Task.FromResult(true);
    }

    protected override async Task<IEnumerable<PublishStoredEvent>?> GetBatchForProcessing(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        var pendingEvents = await _eventStore.GetPendingEventsAsync(
           _configuration.BatchSize,
           stoppingToken);

        return pendingEvents;
    }

    protected override async Task<bool> ProcessItemAsync(PublishStoredEvent item, IServiceScope serviceScope, Guid workerId, CancellationToken stoppingToken)
    {
        try
        {
            var @event = _eventDispatcher.DeserializeEvent(item.EventType, item.Value);

            if (@event is null)
            {
                LogDeserializationFailed(_logger, item.Id, item.EventType);

                await _eventStore.MarkAsFailedAsync(
                    item.Id,
                    $"Event type not found: {item.EventType}",
                    stoppingToken);
                return false;
            }

            var dispatched = await _eventDispatcher.DispatchAsync(
                item.EventType,
                @event,
                serviceScope.ServiceProvider,
                stoppingToken);

            if (dispatched)
            {
                await _eventStore.MarkAsProcessedAsync(item.Id, stoppingToken);
                return true;
            }
            else
            {
                await _eventStore.MarkAsFailedAsync(item.Id, "Missing Handler", stoppingToken);
                return false;
            }
        }
        catch (Exception ex)
        {
            await _eventStore.MarkAsFailedAsync(item.Id, ex.Message, stoppingToken);
            return false;
        }
    }

    protected override int SleepIntervalInSeconds()
    {
        return _configuration.PollingIntervalSeconds;
    }

    protected override int ParallelismDegree()
    {
        return _configuration.GetConcurrentWorkers();
    }

    protected override void FinalCleanUp()
    {
        return;
    }

    protected override Task<PrevalidationResult> BatchPrevalidation(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.FromResult(PrevalidationResult.Valid());
    }

    protected override int ErrorThresholdPercentage()
    {
        return ErrorThresholdPercen;
    }


    protected override Task BatchSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    protected override Task BatchPostProcesing(IServiceScope serviceScope, WorkerExecution currentExecution, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    protected override bool SkipEmptyResults()
    {
        return true;
    }
}
