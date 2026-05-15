using Microsoft.Extensions.Logging;
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

namespace Test.Application.Kafka;

/// <summary>
/// Handler that recibe a event from the service bus.
/// </summary>
internal sealed partial class MyKafkaSubscribeEventHanlder(ILogger<MyKafkaSubscribeEventHanlder> logger) : ICommandHandler<MyKafkaSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(MyKafkaSubscribeEvent command, CancellationToken cancellationToken = default)
    {
        this.LogReception(command.Numerito);
        return Task.FromResult(Result<bool>.Success(true));
    }

    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Se recibio el numero: {numerito} de KAFKA!")]
    private partial void LogReception(int numerito);
}