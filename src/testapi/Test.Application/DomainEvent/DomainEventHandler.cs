using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.ServiceBus.Core.PublishEvents.Abstractions;

namespace Test.Application.DomainEvent;

internal sealed class DomainEventHandler(IEventPublisher eventPublisher) : ICommandHandler<DomainEventCommand, string>
{
    public async Task<Result<string>> HandleAsync(DomainEventCommand command, CancellationToken cancellationToken = default)
    {
        await eventPublisher.PublishAsync(new DomainEventPublishEvent(), cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
