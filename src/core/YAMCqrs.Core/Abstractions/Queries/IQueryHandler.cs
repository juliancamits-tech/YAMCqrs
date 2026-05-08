namespace YAMCqrs.Core.Abstractions.Queries;

/// <summary>
/// Handler for queries that return a Result<T> object
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="T">The type of value inside the Result</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}