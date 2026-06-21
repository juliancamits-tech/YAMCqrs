using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Rabbit;

/// <summary>
/// Rabbit command.
/// </summary>
public class RabbitCommand : ICommand<string>
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}