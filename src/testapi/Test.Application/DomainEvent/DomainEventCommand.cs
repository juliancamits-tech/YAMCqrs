using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.DomainEvent;

/// <summary>
/// Represents a command with a name that returns a string result when executed.
/// </summary>
public class DomainEventCommand : ICommand<string>
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
