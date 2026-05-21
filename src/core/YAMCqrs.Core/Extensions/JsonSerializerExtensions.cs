using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace YAMCqrs.Core.Extensions;

[ExcludeFromCodeCoverage(Justification = "Its a wrapper for good use of System.Text.Json")]
public static class JsonSerializerExtensions
{
    public readonly static JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializa un objeto usando su tipo concreto en runtime
    /// </summary>
    public static string SerializeWithConcreteType<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, obj?.GetType() ?? typeof(T), JsonSerializerOptions);
    }

    public static T? DeserializeWithConcreteType<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }
}
