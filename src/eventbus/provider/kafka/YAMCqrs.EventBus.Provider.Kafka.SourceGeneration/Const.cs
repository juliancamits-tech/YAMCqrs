namespace YAMCqrs.EventBus.Provider.Kafka.SourceGeneration;

internal static class Const
{
    public const string NamespaceName = "namespace YAMCqrs.EventBus.Provider.Kafka;";

    public static class InterfacesNames
    {
        public const string KafkaPublishEvent = "YAMCqrs.EventBus.Provider.Kafka.Abstractions.KafkaPublishEvent";
        public const string KafkaSubscribeEvent = "YAMCqrs.EventBus.Provider.Kafka.Abstractions.KafkaSubscribeEvent";
    }

    public static class Usings
    {
        public const string Options = "using YAMCqrs.EventBus.Provider.Kafka.Configuration;";
        public const string Extensions = "using YAMCqrs.EventBus.Provider.Kafka.Extensions;";
        public const string StaticServiceCollectionExtensions = "using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;";
    }
}
