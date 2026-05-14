namespace YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;

public interface ITopicToCommand
{
    /// <summary>
    /// Attempts to deserialize a JSON message to its corresponding ServiceBusSubscribeEvent type based on topic.
    /// </summary>
    /// <returns>true if deserialization succeeded; otherwise, false.</returns>
    bool TryDeserializerTopic(string topic, string jsonMessage, out SubscribeEvent? command);

    /// <summary>
    /// Return is the internal list are empty or not
    /// </summary>
    /// <returns>bool</returns>
    bool IsEmpty();
}