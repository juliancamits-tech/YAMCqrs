namespace YAMCqrs.EventBus.Core.SourceGeneration;

public static class Const
{
    public const string NamespaceName = "namespace YAMCqrs.EventBus.Core;";

    public static class InterfacesNames
    {
        public const string PublishEvent = "YAMCqrs.EventBus.Core.PublishEvents.Abstractions.PublishEvent";
        public const string IEventHandler = "YAMCqrs.EventBus.Core.PublishEvents.Abstractions.IEventHandler<TEvent>";
        public const string SubscribeEvent = "YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions.SubscribeEvent";
    }

    public static class Usings
    {
        public const string IEventDispatcher = "using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;";
        public const string Extensions = "using YAMCqrs.EventBus.Core.Extensions;";
        public const string ITopicToCommand = "using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;";
        public const string CoreExtensions = "using YAMCqrs.Core.Extensions;";
        public const string Configuration = "using YAMCqrs.EventBus.Core.Configuration;";
        public const string StaticServiceCollectionExtensions = "using static YAMCqrs.EventBus.Core.Extensions.ServiceCollectionExtensions;";
    }
}