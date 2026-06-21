namespace YAMCqrs.EventBus.Provider.RabbitMq.Configuration;

public class RabbitConfigurationOptions
{
    /// <summary>
    /// Connection string for connect to RabbitMq
    /// </summary>
    public required string ConnectionString { get; init; } = string.Empty;
    /// <summary>
    /// Name of the Queue shared with the group
    /// </summary>
    public required string QueueGroupName { get; init; } = string.Empty;

    internal List<Topics> Topics { get; init; } = [];
}

internal sealed record Topics(string Exchange, string RoutingKey);
