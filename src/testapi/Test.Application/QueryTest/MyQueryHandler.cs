using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Queries;

namespace Test.Application.QueryTest;

/// <summary>
/// Handler for <see cref="MyQuery"/>.
/// </summary>
internal sealed class MyQueryHandler : IQueryHandler<MyQuery, string>
{
    public Task<Result<string>> HandleAsync(MyQuery command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Ok(command.Name));
    }
}