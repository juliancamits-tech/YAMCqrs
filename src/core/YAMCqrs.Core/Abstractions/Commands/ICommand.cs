namespace YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Marker interface for commands that always return a Result as response
/// </summary>
public interface ICommand<TResult>
{
}
