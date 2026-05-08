
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.Core.Abstractions.Queries;

namespace YAMCqrs.Core.Abstractions;
/// <summary>
/// Mediator for dispatching commands and queries to their respective handlers
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command with a specific result type
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the command execution</returns>
    Task<Result<T>> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries for a result
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The query result</returns>
    Task<Result<T>> QueryAsync<T>(IQuery<T> query, CancellationToken cancellationToken = default);
}
