using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace Test.Application.Kafka;

internal sealed class KafkaCommandHandler(IEventPublisher eventPublisher) : ICommandHandler<KafkaCommand, string>
{
    public async Task<Result<string>> HandleAsync(KafkaCommand command, CancellationToken cancellationToken = default)
    {
        await eventPublisher.PublishAsync(new MyKafkaPublishEvent(), cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
