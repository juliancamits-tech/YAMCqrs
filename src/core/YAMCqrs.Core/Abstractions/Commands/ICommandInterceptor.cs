namespace YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Interceptor for command execution pipeline with result
/// </summary>
public interface ICommandInterceptor<TCommand, TResult> where TCommand : ICommand<TResult>
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

    Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken);
    Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken);
    Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken);
}


public abstract class CommandInterceptorBase<TCommand, TResult> : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public virtual InterceptorLayer Layer => InterceptorLayer.Application;
    public virtual int Order => 100;

    public abstract Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken);
    public abstract Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken);
    public abstract Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken);
}