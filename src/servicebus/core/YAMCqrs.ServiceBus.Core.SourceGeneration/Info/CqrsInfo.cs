using System.Collections.Immutable;

namespace YAMCqrs.ServiceBus.Core.SourceGeneration.Info;

internal sealed class CqrsInfo(
    string fullTypeName,
    string @namespace,
    ImmutableArray<string> eventHandlers,
    ImmutableArray<string> integrationEvents,
    ImmutableArray<string> topics,
    bool isGenericDefinition)
{
    public string FullTypeName { get; } = fullTypeName;
    public string Namespace { get; } = @namespace;
    public ImmutableArray<string> EventHandlers { get; } = eventHandlers;
    public ImmutableArray<string> IntegrationEvents { get; } = integrationEvents;
    public ImmutableArray<string> Topics { get; } = topics;
    public bool IsGenericDefinition { get; } = isGenericDefinition;

    public bool IsIncomplete() =>
        string.IsNullOrEmpty(FullTypeName) || string.IsNullOrEmpty(Namespace);
}
