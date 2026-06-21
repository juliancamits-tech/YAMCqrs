using Microsoft.Extensions.Logging;
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Rabbit;

/// <summary>
/// Handler that recibe a event from the service bus.
/// </summary>
internal sealed partial class RabbitSubscribeEventHanlder(ILogger<RabbitSubscribeEventHanlder> logger) : ICommandHandler<MyRabbitSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(MyRabbitSubscribeEvent command, CancellationToken cancellationToken = default)
    {
        this.LogReception(command.RandomProduct);
        return Task.FromResult(Result.Success(true));
    }

    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Se recibio el producto: {productName} de Rabbit!")]
    private partial void LogReception(string productName);
}
