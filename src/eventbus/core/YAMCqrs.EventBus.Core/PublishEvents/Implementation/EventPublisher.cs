using Microsoft.Extensions.Logging;
using System.Transactions;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace YAMCqrs.EventBus.Core.PublishEvents.Implementation;

/// <summary>
/// Scoped event publisher that participates in ambient transactions.
/// Events are staged in-memory and committed when the transaction completes.
/// </summary>
internal sealed partial class EventPublisher(IPublishEventStore eventStore, ILogger<EventPublisher> logger) : IEventPublisher, IDisposable
{

    #region Logger Messages

    [LoggerMessage(LogLevel.Debug, "Staging event: {EventType}")]
    private partial void LogStagingEvent(string eventType);

    [LoggerMessage(LogLevel.Debug, "Staged {Count} events")]
    private partial void LogStagedEventsCount(int count);

    [LoggerMessage(LogLevel.Debug, "Enlisting in ambient transaction")]
    private partial void LogEnlistingInAmbientTransaction();

    [LoggerMessage(LogLevel.Information, "Committing staged events immediately (no ambient transaction]")]
    private partial void LogCommittingEventsImmediately();

    [LoggerMessage(LogLevel.Information, "Committing {Count} staged events to event store")]
    private partial void LogCommittingEventsToEventStore(int count);

    [LoggerMessage(LogLevel.Warning, "Disposing EventPublisher with {Count} staged events without transaction, committing immediately")]
    private partial void LogDisposingWithStagedEvents(int count);

    [LoggerMessage(LogLevel.Warning, "Transaction aborted, discarding {Count} staged events")]
    private partial void LogTransactionAborted(int count);

    [LoggerMessage(LogLevel.Error, "Failed to persist staged events to event store")]
    private partial void LogFailedToPersistEvents();

    #endregion

    private readonly List<PublishEvent> _stagedEvents = [];
    private bool _isEnlisted = false;
    private bool _isDisposed = false;

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent
    {
        ThrowIfDisposed();

        LogStagingEvent(typeof(TEvent).Name);

        // Stage event in memory
        _stagedEvents.Add(@event);

        // Enlist in ambient transaction (if exists and not already enlisted)
        EnlistInAmbientTransaction();

        return Task.CompletedTask;
    }

    public Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : PublishEvent
    {
        ThrowIfDisposed();

        foreach (var @event in events)
        {
            _stagedEvents.Add(@event);
        }

        LogStagedEventsCount(_stagedEvents.Count);

        EnlistInAmbientTransaction();

        return Task.CompletedTask;
    }

    public Task PublishManyAsync(CancellationToken cancellationToken = default, params PublishEvent[] events)
    {
        ThrowIfDisposed();

        foreach (var @event in events)
        {
            _stagedEvents.Add(@event);
        }

        LogStagedEventsCount(_stagedEvents.Count);

        EnlistInAmbientTransaction();

        return Task.CompletedTask;
    }

    private void EnlistInAmbientTransaction()
    {
        if (_isEnlisted || _stagedEvents.Count == 0)
            return;

        var currentTransaction = Transaction.Current;

        if (currentTransaction is not null)
        {
            LogEnlistingInAmbientTransaction();

            // Subscribe to transaction completion
            currentTransaction.TransactionCompleted += OnTransactionCompleted;
            _isEnlisted = true;
        }
        else
        {
            // No ambient transaction - commit immediately (backward compatibility)
            LogCommittingEventsImmediately();
            CommitEventsSync();
        }
    }

    private void OnTransactionCompleted(object? sender, TransactionEventArgs e)
    {
        if (e.Transaction?.TransactionInformation.Status == TransactionStatus.Committed)
        {
            LogCommittingEventsToEventStore(_stagedEvents.Count);
            CommitEventsSync();
        }
        else
        {
            LogTransactionAborted(_stagedEvents.Count);
            _stagedEvents.Clear();
        }
    }

    private void CommitEventsSync()
    {
        if (_stagedEvents.Count == 0)
            return;

        try
        {
            // Note: TransactionCompleted is synchronous, so we use sync-over-async here
            // Alternative: Queue events to background channel for async processing
            foreach (var @event in _stagedEvents)
            {
                eventStore.StoreAsync(@event, CancellationToken.None)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }

            LogCommittingEventsToEventStore(_stagedEvents.Count);
        }
        catch (Exception)
        {
            LogFailedToPersistEvents();
            // Don't throw - transaction is already completed
        }
        finally
        {
            _stagedEvents.Clear();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        // If there are staged events and we're NOT enlisted in a transaction,
        // it means no transaction scope was used - commit immediately
        if (_stagedEvents.Count > 0 && !_isEnlisted)
        {
            LogDisposingWithStagedEvents(_stagedEvents.Count);
            CommitEventsSync();
        }

        _stagedEvents.Clear();
        _isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(EventPublisher));
    }


}
