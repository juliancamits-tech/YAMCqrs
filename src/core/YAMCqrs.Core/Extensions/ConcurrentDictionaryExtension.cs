using System.Collections.Concurrent;

namespace YAMCqrs.Core.Extensions;

public static class ConcurrentDictionaryExtension
{
    public static ConcurrentDictionary<TKey, TValue> CreateNewDictionary<TKey, TValue>() where TKey : notnull
    {
        const int maxCapacity = 100;
        return new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity: maxCapacity);
    }
}