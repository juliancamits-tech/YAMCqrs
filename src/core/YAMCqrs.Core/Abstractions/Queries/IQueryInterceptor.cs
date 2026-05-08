namespace YAMCqrs.Core.Abstractions.Queries;

/// <summary>
/// Interceptor for query execution pipeline with result
/// </summary>
public interface IQueryInterceptor<TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Execution layer. Determines when this interceptor runs relative to others.
    /// See <see cref="InterceptorLayer"/> for standard layers.
    /// Default: <see cref="InterceptorLayer.Application"/>
    /// </summary>
    InterceptorLayer Layer => InterceptorLayer.Application;

    /// <summary>
    /// Execution order within the layer. Lower values execute first.
    /// Use this to control order among interceptors in the same layer.
    /// Default: 100 (medium priority)
    /// </summary>
    int Order => 100;

    Task OnBeforeAsync(TQuery query, CancellationToken cancellationToken);
    Task OnAfterAsync(TQuery query, Result<TResult> result, CancellationToken cancellationToken);
    Task OnErrorAsync(TQuery query, Exception exception, CancellationToken cancellationToken);
}

public abstract class QueryInterceptorBase<TQuery, TResult> : IQueryInterceptor<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public virtual InterceptorLayer Layer => InterceptorLayer.Application;
    public virtual int Order => 100;

    public abstract Task OnBeforeAsync(TQuery query, CancellationToken cancellationToken);
    public abstract Task OnAfterAsync(TQuery query, Result<TResult> result, CancellationToken cancellationToken);
    public abstract Task OnErrorAsync(TQuery query, Exception exception, CancellationToken cancellationToken);
}