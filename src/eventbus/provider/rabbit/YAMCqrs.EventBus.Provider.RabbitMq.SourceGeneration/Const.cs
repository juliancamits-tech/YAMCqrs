namespace YAMCqrs.EventBus.Provider.RabbitMq.SourceGeneration;

internal static class Const
{
    public const string NamespaceName = "namespace YAMCqrs.EventBus.Provider.RabbitMq;";

    public static class InterfacesNames
    {
        public const string RabbitPublishEvent = "YAMCqrs.EventBus.Provider.RabbitMq.Abstractions.RabbitPublishEvent";
        public const string RabbitSubscribeEvent = "YAMCqrs.EventBus.Provider.RabbitMq.Abstractions.RabbitSubscribeEvent";
    }

    public static class Usings
    {
        public const string Options = "using YAMCqrs.EventBus.Provider.RabbitMq.Configuration;";
        public const string Extensions = "using YAMCqrs.EventBus.Provider.RabbitMq.Extensions;";
        public const string StaticServiceCollectionExtensions = "using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;";
    }
}
