
using YAMCqrs.EventBus.Core.EventBus.Abstractions;

namespace YAMCqrs.EventBus.Core.Tests.Core.PublishEventHandlerTests
{
    internal class MockIEventBusPublisher : IEventBusPublisher
    {
        public async Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            return;
        }
    }

    internal class MockIEventBusPublisherEx : IEventBusPublisher
    {
        public async Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            throw new Exception();
        }
    }
}
