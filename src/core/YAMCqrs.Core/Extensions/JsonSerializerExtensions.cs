using System.Text.Json;

namespace YAMCqrs.Core.Extensions;

public static class JsonSerializerExtensions
{
    public static JsonSerializerOptions JsonSerializerOptions = new()
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
