namespace YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Handler for commands that return a Result<T> object
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="T">The type of value inside the Result</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}