using System.Collections.Immutable;

namespace YAMCqrs.EventBus.Core.SourceGeneration.Info;


internal sealed class EventHandlerInfo(
    string fullTypeName,
    string @namespace,
    ImmutableArray<string> eventTypes)
{
    public string FullTypeName { get; } = fullTypeName;
    public string Namespace { get; } = @namespace;
    public ImmutableArray<string> EventTypes { get; } = eventTypes;
}