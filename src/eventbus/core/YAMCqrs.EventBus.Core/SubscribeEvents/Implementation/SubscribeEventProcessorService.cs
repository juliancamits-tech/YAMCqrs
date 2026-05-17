using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Domain;
using YAMCqrs.Core.Abstractions;
using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Core.Configuration;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Core.SubscribeEvents.Implementation;

internal partial class SubscribeEventProcessorService(
    IServiceProvider serviceProvider,
    ILogger<SubscribeEventProcessorService> logger,
    IOptions<EventBusConfiguration> config,
    ITopicToCommand topicToCommand) : YABackgroundWorker<SubscribeStoredEvent>(serviceProvider)
{
    private readonly ILogger<SubscribeEventProcessorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly EventBusConfiguration _config = config.Value ?? new EventBusConfiguration(); //TODO: delete the new a have a DI inyection.
    private readonly ITopicToCommand _topicToCommand = topicToCommand ?? throw new ArgumentNullException(nameof(topicToCommand));
    private int _currentBatchSize = 0;

    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        return Task.FromResult(!_topicToCommand.IsEmpty());
    }

    protected override void FinalCleanUp()
    {
        return;
    }

    protected override async Task<IEnumerable<SubscribeStoredEvent>?> GetBatchForProcessing(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        var processor = serviceScope.ServiceProvider.GetRequiredService<ISubscribeEventStore>();

        var pendingMessages = await processor.GetPendingEventsAsync(_config.BatchSize, stoppingToken);

        _currentBatchSize = pendingMessages.Count();

        return pendingMessages;
    }

    protected override async Task<bool> ProcessItemAsync(SubscribeStoredEvent item, IServiceScope serviceScope, Guid workerId, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        try
        {
            LogProcessingMessage(item.Id, item.Topic);

            if (!_topicToCommand.TryDeserializerTopic(item.Topic, item.Value, out var command))
            {
                LogNoCommandForTopic(item.Topic, item.Id);
                item.SetFailed($"No command type registered for topic: {item.Topic}");
                return false;
            }

            var dispatcher = serviceScope.ServiceProvider.GetRequiredService<IDispatcher>();
            var r = await dispatcher.SendAsync(command!, stoppingToken);

            if (r.IsSuccess)
            {
                item.SetProcessed();
                LogMessageProcessed(item.Id);
            }

            return r.IsSuccess;
        }
        catch (Exception ex)
        {
            LogMessageFailed(ex, item.Id, item.RetryCount);

            item.SetFailed(ex.Message);
            return false;
        }
    }

    protected override Task<PrevalidationResult> BatchPrevalidation(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        LogProcessingMessages(_currentBatchSize, _config.ConcurrentWorkers);
        return Task.FromResult(PrevalidationResult.Valid());
    }

    protected override int SleepIntervalInSeconds()
    {
        return _config.PollingIntervalSeconds;
    }

    protected override int ParallelismDegree()
    {
        return _config.GetConcurrentWorkers();
    }

    protected override int ErrorThresholdPercentage()
    {
        return _config.ErrorThresholdPercentage;
    }


    /// <summary>
    /// Reconstructs a Command instance from the persisted message JSON data
    /// </summary>
    private SubscribeEvent? ReconstructCommand(
        Type commandType,
        string value,
        Guid messageId)
    {
        try
        {
            // Get the generic method DeserializeWithConcreteType<T>
            var method = typeof(JsonSerializerExtensions)
                .GetMethod(nameof(JsonSerializerExtensions.DeserializeWithConcreteType))!
                .MakeGenericMethod(commandType);

            // Invoke the method with the dynamic type
            var command = method.Invoke(null, [value]) as SubscribeEvent;

            return command;
        }
        catch (Exception ex)
        {
            LogFailedToDeserialize(ex, messageId, commandType.FullName ?? commandType.Name);
            return null;
        }
    }


    /// <summary>
    /// Gets the correlation ID from the current activity or generates a new one
    /// </summary>
    private static string? GetCorrelationId()
    {
        return System.Diagnostics.Activity.Current?.TraceId.ToString();
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

    #region Logger Messages

    [LoggerMessage(LogLevel.Information, "Processing {Count} pending messages with max degree of parallelism: {MaxDegreeOfParallelism}")]
    private partial void LogProcessingMessages(int count, int maxDegreeOfParallelism);

    [LoggerMessage(LogLevel.Debug, "Processing message {MessageId} from topic {Topic}")]
    private partial void LogProcessingMessage(Guid messageId, string topic);

    [LoggerMessage(LogLevel.Debug, "Message {MessageId} processed successfully")]
    private partial void LogMessageProcessed(Guid messageId);

    [LoggerMessage(LogLevel.Warning, "Failed to process message {MessageId}. Retry count: {RetryCount}")]
    private partial void LogMessageFailed(Exception ex, Guid messageId, int retryCount);

    [LoggerMessage(LogLevel.Critical, "Failed to send alert for message {MessageId}")]
    private partial void LogAlertFailure(Exception ex, Guid messageId);

    [LoggerMessage(LogLevel.Warning, "No command type registered for topic {Topic}. Message {MessageId} will be marked as failed.")]
    private partial void LogNoCommandForTopic(string topic, Guid messageId);

    [LoggerMessage(LogLevel.Error, "Failed to deserialize message {MessageId} to command type {TypeName}")]
    private partial void LogFailedToDeserialize(Exception ex, Guid messageId, string typeName);
    #endregion
}
