using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Provider.RabbitMq.Configuration;

namespace YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;

public abstract class RabbitPublishEvent : PublishEvent
{
    public override ServiceBusProvider Destination() => ServiceBusProvider.RabbitMq;
    public abstract string RoutingKey { get; }

    public override Dictionary<string, string>? GetCustomHeaders()
    {
        var headers = new Dictionary<string, string>();

        headers.TryAdd(ConfigurationConst.RouteRoutingKey, RoutingKey);

        AddCustomHeaders(ref headers);

        return headers;
    }

    public abstract void AddCustomHeaders(ref Dictionary<string, string> headers);
}
