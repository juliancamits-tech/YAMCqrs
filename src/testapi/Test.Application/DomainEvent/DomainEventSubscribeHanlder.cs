using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.DomainEvent;

/// <summary>
/// Handler that recibe a event from the service bus.
/// </summary>
internal sealed partial class DomainEventSubscribeHanlder() : ICommandHandler<DomainEventSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(DomainEventSubscribeEvent command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}