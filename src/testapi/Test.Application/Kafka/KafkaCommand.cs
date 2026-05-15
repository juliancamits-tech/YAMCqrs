using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Kafka;

/// <summary>
/// Kafka command.
/// </summary>
public sealed class KafkaCommand : ICommand<string>
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
