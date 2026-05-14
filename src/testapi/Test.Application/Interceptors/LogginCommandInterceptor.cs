using Microsoft.Extensions.Logging;
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Interceptors;

/// <summary>
/// Example of a command interceptor that logs the execution of commands using source-generated logging methods.
/// </summary>
/// <typeparam name="TCommand">All Commands.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
/// <param name="logger">Logger for out the message.</param>
internal sealed partial class LogginCommandInterceptor<TCommand, TResult>(ILogger<LogginCommandInterceptor<TCommand, TResult>> logger) : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken)
    {
        this.LogEndCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    public Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken)
    {
        this.LogStartCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken)
    {
        this.LogErrorCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    #region Logger

    [LoggerMessage(Level = LogLevel.Information, Message = "Start Command {CommandName}")]
    private partial void LogStartCommand(string commandName);

    [LoggerMessage(Level = LogLevel.Information, Message = "End Command {CommandName}")]
    private partial void LogEndCommand(string commandName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error Command {CommandName}")]
    private partial void LogErrorCommand(string commandName);
    #endregion
}
