using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Provider.Kafka.Configuration;
using YAMCqrs.EventBus.Provider.Kafka.Helper;

namespace YAMCqrs.EventBus.Provider.Kafka.Implementation;

/// <summary>
/// Background service that consumes messages from Kafka topics
/// </summary>
internal sealed partial class KafkaSubscriber(
    ILogger<KafkaSubscriber> logger,
    IOptions<KafkaConfigurationOptions> configuration,
    ISubscribeEventStore messagePersister,
    IConfiguration theConfiguration,
    IConsumer<string, string>? consumer = null) : IHostedService
{
    private readonly ILogger<KafkaSubscriber> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly KafkaConfigurationOptions _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ISubscribeEventStore _messagePersister = messagePersister ?? throw new ArgumentNullException(nameof(messagePersister));
    private readonly IConsumer<string, string>? _externalConsumer = consumer; private readonly List<Task> _consumerTasks = [];
    private readonly IConfiguration _theConfiguration = theConfiguration;
    private CancellationTokenSource? _cancellationTokenSource;

    #region Logger Messages
    [LoggerMessage(LogLevel.Debug, "KafkaSubscriber starting with {Count} concurrent consumers")]
    private partial void LogStarting(int count);

    [LoggerMessage(LogLevel.Information, "KafkaServiceBusSubscriber started consuming topics: {Topics}")]
    private partial void LogStartConsuming(string topics);

    [LoggerMessage(LogLevel.Debug, "Received message: {Message} from topic: {Topic}")]
    private partial void LogReceivedMessage(string message, string topic);

    [LoggerMessage(LogLevel.Debug, "Message committed from topic: {Topic}, Partition: {Partition}, Offset: {Offset}")]
    private partial void LogMessageCommitted(string topic, Partition partition, Offset offset);

    [LoggerMessage(LogLevel.Error, "Error processing message from topic: {Topic}, Offset: {Offset}. Message will be retried. HandlerResponse: {HandlerResponse}")]
    private partial void LogErrorProcessingMessage(string topic, Offset offset, string handlerResponse);

    [LoggerMessage(LogLevel.Warning, "Some subscriptions don't exist, recreating task in 1min")]
    private partial void LogSubscriptionsDontExist();

    [LoggerMessage(LogLevel.Critical, "Error consuming messages from Kafka, Task is shutting down, ErrorMessage: {ErrorMessage}")]
    private partial void LogCriticalError(string errorMessage);

    [LoggerMessage(LogLevel.Warning, "KafkaSubscriber stopping...")]
    private partial void LogStopping();

    [LoggerMessage(LogLevel.Warning, "KafkaSubscriber stopped")]
    private partial void LogStopped();

    [LoggerMessage(LogLevel.Warning, "Consumer task cancelled")]
    private partial void LogConsumerTaskCancelled();

    [LoggerMessage(LogLevel.Error, "Kafka broker error: {ErrorReason}. Retrying...")]
    private partial void LogKafkaBrokerError(string errorReason);

    [LoggerMessage(LogLevel.Error, "Failed to commit offset for topic: {Topic}, Partition: {Partition}, Offset: {Offset}")]
    private partial void LogCommitFailed(string topic, Partition partition, Offset offset);

    [LoggerMessage(LogLevel.Debug, "Cleaned up {Count} completed consumer tasks")]
    private partial void LogTasksCleanedUp(int count);

    [LoggerMessage(LogLevel.Warning, "No topics configured for Kafka consumer. Shutting down Subscriber.")]
    private partial void LogNoTopicsConfigured();

    #endregion

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_configuration.Topics.Length == 0)
        {
            LogNoTopicsConfigured();
            return Task.CompletedTask;
        }

        LogStarting(_configuration.MaxConcurrentConsumers);

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Create consumer tasks
        for (int x = 0; x < _configuration.MaxConcurrentConsumers; x++)
        {
            CreateConsumerTask(delayBeforeStart: TimeSpan.Zero);
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        LogStopping();

        // Signal cancellation to consumer tasks
        _cancellationTokenSource?.Cancel();

        // Wait for all consumer tasks to complete
        if (_consumerTasks.Count > 0)
        {
            try
            {
                // Don't pass the external cancellation token to avoid race conditions
                // The tasks will complete once they observe the internal cancellation
                await Task.WhenAll(_consumerTasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected - consumer tasks were cancelled
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
            {
                // Expected - all tasks were cancelled
            }
        }

        _cancellationTokenSource?.Dispose();
        LogStopped();
    }

    /// <summary>
    /// Creates a new consumer task, optionally waiting before starting it, and cleans up completed tasks
    /// </summary>
    /// <param name="delayBeforeStart">Time to wait before starting the consumer. Use TimeSpan.Zero for immediate start.</param>
    private void CreateConsumerTask(TimeSpan delayBeforeStart)
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
            return;

        // Clean up completed tasks
        var completedCount = _consumerTasks.RemoveAll(t => t.IsCompleted);
        if (completedCount > 0)
        {
            LogTasksCleanedUp(completedCount);
        }

        // Create new task with optional delay
        var task = Task.Run(async () =>
        {
            if (delayBeforeStart > TimeSpan.Zero)
            {
                await Task.Delay(delayBeforeStart, _cancellationTokenSource.Token);
            }

            await ExecuteConsumer(_cancellationTokenSource.Token);
        }, _cancellationTokenSource.Token);

        _consumerTasks.Add(task);
    }

    public async Task ExecuteConsumer(CancellationToken cancellationToken)
    {
        IConsumer<string, string> consumer;
        bool ownsConsumer;

        // Use external consumer (for tests) or create a new one
        if (_externalConsumer != null)
        {
            consumer = _externalConsumer;
            ownsConsumer = false;
        }
        else
        {
            consumer = new ConsumerBuilder<string, string>(
                KafkaConfigHelper.CreateConsumeConfig(_configuration, _theConfiguration)).Build();
            ownsConsumer = true;
        }

        try
        {
            consumer.Subscribe(_configuration.Topics);

            if (_logger.IsEnabled(LogLevel.Information))
                LogStartConsuming(string.Join(",", _configuration.Topics));

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);
                LogReceivedMessage(consumeResult.Message.Value, consumeResult.Topic);

                try
                {
                    // Extract Kafka native headers
                    Dictionary<string, string>? headers = null;
                    if (consumeResult.Message.Headers != null && consumeResult.Message.Headers.Count > 0)
                    {
                        headers = [];
                        foreach (var header in consumeResult.Message.Headers)
                        {
                            headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes());
                        }
                    }

                    // No Key parameter - messages are independent
                    await _messagePersister.StoreAsync(
                        consumeResult.Topic,
                        consumeResult.Message.Value,
                        headers,
                        ServiceBusProvider.Kafka,
                        cancellationToken);

                    consumer.Commit(consumeResult);
                    LogMessageCommitted(consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                }
                catch (KafkaException)
                {
                    LogCommitFailed(consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                }
                catch (Exception ex)
                {
                    LogErrorProcessingMessage(consumeResult.Topic, consumeResult.Offset, ex.Message);
                }
            }
        }
        catch (ConsumeException)
        {
            LogSubscriptionsDontExist();
            CreateConsumerTask(TimeSpan.FromMinutes(1));
        }
        catch (KafkaException kex)
        {
            // Broker/connection error in Consume()
            LogKafkaBrokerError(kex.Error.Reason);
            CreateConsumerTask(TimeSpan.FromSeconds(30));
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            LogConsumerTaskCancelled();
        }
        catch (Exception ex)
        {
            LogCriticalError(ex.Message);
            throw;
        }
        finally
        {
            if (ownsConsumer)
            {
                consumer.Close();
                consumer.Dispose();
            }
        }
    }
}