using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;

namespace Test.Application.Rabbit;

internal sealed class RabbitCommandHandler(IEventPublisher eventPublisher) : ICommandHandler<RabbitCommand, string>
{
    public async Task<Result<string>> HandleAsync(RabbitCommand command, CancellationToken cancellationToken = default)
    {
        await eventPublisher.PublishAsync(new MyRabbitPublishEvent(), cancellationToken);

        return Result.Ok(command.Name);
    }
}
