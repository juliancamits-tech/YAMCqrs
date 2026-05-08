using System.Collections.Immutable;

namespace YAMCqrs.Core.SourceGeneration.Info;

internal sealed class CqrsInfo(
    string fullTypeName,
    string @namespace,
    ImmutableArray<(string CommandType, string ResultType)> commandHandlers,
    ImmutableArray<(string QueryType, string ResultType)> queryHandlers,
    ImmutableArray<(string CommandType, string ResultType)> commandInterceptors,
    ImmutableArray<(string QueryType, string ResultType)> queryInterceptors,
    ImmutableArray<string> eventHandlers,
    ImmutableArray<string> integrationEvents,
    bool isGenericDefinition)
{
    public string FullTypeName { get; } = fullTypeName;
    public string Namespace { get; } = @namespace;
    public ImmutableArray<(string CommandType, string ResultType)> CommandHandlers { get; } = commandHandlers;
    public ImmutableArray<(string QueryType, string ResultType)> QueryHandlers { get; } = queryHandlers;
    public ImmutableArray<(string CommandType, string ResultType)> CommandInterceptors { get; } = commandInterceptors;
    public ImmutableArray<(string QueryType, string ResultType)> QueryInterceptors { get; } = queryInterceptors;
    public ImmutableArray<string> EventHandlers { get; } = eventHandlers;
    public ImmutableArray<string> IntegrationEvents { get; } = integrationEvents;
    public bool IsGenericDefinition { get; } = isGenericDefinition;

    public bool IsIncomplete() =>
        string.IsNullOrEmpty(FullTypeName) || string.IsNullOrEmpty(Namespace);
}