using YAMCqrs.Core.Abstractions.Queries;

namespace YAMCqrs.Core.Tests.Core.Interceptors;

/// <summary>
/// Example of a query interceptor that logs the execution of queries using source-generated logging methods.
/// </summary>
/// <typeparam name="TQuery">All Queries.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
internal sealed partial class TestQueryInterceptor<TQuery, TResult>() : QueryInterceptorBase<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public override Task OnAfterAsync(TQuery query, Result<TResult> result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task OnBeforeAsync(TQuery query, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task OnErrorAsync(TQuery query, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal sealed partial class TestQueryInterceptor2<TQuery, TResult>() : IQueryInterceptor<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public Task OnAfterAsync(TQuery query, Result<TResult> result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnBeforeAsync(TQuery query, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(TQuery query, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
