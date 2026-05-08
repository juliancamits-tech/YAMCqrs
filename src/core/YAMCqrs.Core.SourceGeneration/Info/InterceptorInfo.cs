using System.Collections.Immutable;

namespace YAMCqrs.Core.SourceGeneration.Info;

internal class InterceptorInfo(
    string fullTypeName,
    string @namespace,
    ImmutableArray<(string CommandType, string ResultType)> commandTypes,
    ImmutableArray<(string QueryType, string ResultType)> queryTypes,
    bool isGenericDefinition)
{
    public string FullTypeName { get; } = fullTypeName;
    public string Namespace { get; } = @namespace;
    public ImmutableArray<(string CommandType, string ResultType)> CommandTypes { get; } = commandTypes;
    public ImmutableArray<(string QueryType, string ResultType)> QueryTypes { get; } = queryTypes;
    public bool IsGenericDefinition { get; } = isGenericDefinition;

    public string FullTypeNameWithoutGenerics
    {
        get
        {
            var index = FullTypeName.IndexOf('<');
            return index >= 0 ? FullTypeName.Substring(0, index) : FullTypeName;
        }
    }

    public bool IsIncomplete()
    {
        return CommandTypes.IsDefaultOrEmpty && QueryTypes.IsDefaultOrEmpty;
    }
}
