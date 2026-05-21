using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Interceptors;

/// <summary>
/// Example of a command interceptor that logs the execution of commands using source-generated logging methods.
/// </summary>
/// <typeparam name="TCommand">All Commands.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
internal sealed partial class TestCommandInterceptor<TCommand, TResult>() : CommandInterceptorBase<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public override Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

internal sealed partial class TestCommandInterceptor2<TCommand, TResult>() : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}