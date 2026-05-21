using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace YAMCqrs.Core.Extensions;

[ExcludeFromCodeCoverage(Justification = "Its a wrapper for configure a Dictionary with limits")]
public static class ConcurrentDictionaryExtension
{
    public static ConcurrentDictionary<TKey, TValue> CreateNewDictionary<TKey, TValue>() where TKey : notnull
    {
        const int maxCapacity = 100;
        return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity: maxCapacity);
    }
}