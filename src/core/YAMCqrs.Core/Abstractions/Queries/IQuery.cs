namespace YAMCqrs.Core.Abstractions.Queries;

/// <summary>
/// Marker interface for queries that always return a Result as response
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
public interface IQuery<TResult>
{
}
