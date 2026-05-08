namespace YAMCqrs.ServiceBus.Core.SourceGeneration;

public static class Const
{
    public const string NamespaceName = "namespace YAMCqrs.ServiceBus.Core;";

    public static class InterfacesNames
    {
        public const string PublishEvent = "YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions.PublishEvent";
        public const string IEventHandler = "YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions.IEventHandler<TEvent>";
        public const string SubscribeEvent = "YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions.SubscribeEvent";
    }

    public static class Usings
    {
        public const string IEventDispatcher = "using YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;";
        public const string Extensions = "using YAMCqrs.ServiceBus.Core.Extensions;";
        public const string ITopicToCommand = "using YAMCqrs.ServiceBus.Core.SubscribeEvents.Abstractions;";
        public const string CoreExtensions = "using YAMCqrs.Core.Extensions;";

    }
}