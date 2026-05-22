using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions;
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.Core.Abstractions.Queries;

namespace YAMCqrs.EventBus.Core.Tests.Core.SubscribeEventProcessorServiceTests;

internal class MockIDispatcher(bool negativeResult) : IDispatcher
{
    public Task<Result<T>> QueryAsync<T>(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (negativeResult)
            return Task.FromResult(Result.Fail<T>("err"));

        return Task.FromResult(Result.ToResult((T)(object)true));
    }

    public Task<Result<T>> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default)
    {
        if (negativeResult)
            return Task.FromResult(Result.Fail<T>("err"));

        return Task.FromResult(Result.ToResult((T)(object)true));
    }
}
