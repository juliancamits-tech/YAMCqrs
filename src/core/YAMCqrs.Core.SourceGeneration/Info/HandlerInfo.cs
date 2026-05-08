using System;
using System.Collections.Immutable;
using System.Linq;

namespace YAMCqrs.Core.SourceGeneration.Info;

internal sealed class HandlerInfo(
    string fullTypeName,
    string @namespace,
    ImmutableArray<(string CommandType, string ResultType)> commandTypes,
    ImmutableArray<(string QueryType, string ResultType)> queryTypes) : IEquatable<HandlerInfo>
{
    public string FullTypeName { get; } = fullTypeName;
    public string Namespace { get; } = @namespace;
    public ImmutableArray<(string CommandType, string ResultType)> CommandTypes { get; } = commandTypes;
    public ImmutableArray<(string QueryType, string ResultType)> QueryTypes { get; } = queryTypes;

    public bool Equals(HandlerInfo other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FullTypeName == other.FullTypeName &&
               Namespace == other.Namespace &&
               CommandTypes.SequenceEqual(other.CommandTypes) &&
               QueryTypes.SequenceEqual(other.QueryTypes);
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is HandlerInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (FullTypeName != null ? FullTypeName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ CommandTypes.GetHashCode();
            hashCode = (hashCode * 397) ^ QueryTypes.GetHashCode();
            return hashCode;
        }
    }

    public override string ToString()
    {
        return $"HandlerInfo {{ FullTypeName = {FullTypeName}, Namespace = {Namespace}, CommandTypes = [{CommandTypes.Length}], QueryTypes = [{QueryTypes.Length}] }}";
    }

    public bool IsIncomplete()
    {
        return string.IsNullOrEmpty(FullTypeName) ||
               string.IsNullOrEmpty(Namespace) ||
               (CommandTypes.IsDefaultOrEmpty &&
               QueryTypes.IsDefaultOrEmpty) ||
               (CommandTypes.Length == 0 &&
               QueryTypes.Length == 0);
    }
}
