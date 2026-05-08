using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.CommandTest;

/// <summary>
/// Handler for <see cref="MyCommand"/>.
/// The class is internal and not directly referenced, but it will be instantiated by the generated DI registration.
/// </summary>
internal sealed class MyCommandHandler : ICommandHandler<MyCommand, string>
{
    public Task<Result<string>> HandleAsync(MyCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Ok(command.Name));
    }
}