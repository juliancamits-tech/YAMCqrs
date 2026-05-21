namespace YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Marker interface for commands that always return a Result as response
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Interface used by SourceGeneration")]
public interface ICommand<TResult>
{
}
