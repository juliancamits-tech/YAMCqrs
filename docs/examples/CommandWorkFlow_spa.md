# Command Workflow

Este documento explica como implementar un ICommand, tambien aplica para IQuery con muy simples cambios.

## Creacion del ICommand

Creamos el ICommand que va a ser nuestra clase iniciadora con los parametros que hagan falta.
Es importante que herede la interface ICommand y como T pongamos el tipo de dato que esperamos de respuesta que puede ser un primitivo o una clase.

```csharp
using YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Represents a command with a name that returns a string result when executed.
/// </summary>
public class CreatePersonCommand : ICommand<string>
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
```

## Creacion del Handler

Creamos una clase que va a ser la responsable de procesar la logica de negocio asociada al comando.
Es importante que herede de ICommandHandler donde el primer valor es el comando que procesa y el segundo valor es el tipo de dato que devuelve.
Es importante entender que para NO usar Exceptions para reglas de negocio el Handler devuelve un Result para informar como termino el proceso.
Para mas informacion del Result ver el [ADR 8](../adr/0008-result-object.md)

```csharp
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Handler for <see cref="CreatePersonCommand"/>.
/// The class is internal and not directly referenced, but it will be instantiated by the generated DI registration.
/// </summary>
internal sealed class CreatePersonCommandHandler(...Posibles parametros sacados del DI) : ICommandHandler<CreatePersonCommand, string>
{
    public Task<Result<string>> HandleAsync(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        // Logica de negocio
        return Task.FromResult(Result<string>.Ok(command.Name));
    }
}
```

## Creacion de un Interceptor

Supongamos que tenemos un requerimiento de auditoria extrema donde todo commando debe logear su Request y Response para esto podemos crear un Interceptor.
Los interceptores cuentan con dos interfaces distintas una Interface que es ICommandInterceptor y una clase abstracta CommandInterceptorBase.
Se puede usar cualquiera de las dos pero hay que tener encuenta que en base a lo definido en el [ADR 10](../adr/0010-pipeline-interceptor.md) es necesario definir la capa de ejecucion del Interceptor y su orden para que los mismo sea implicitos y no como otras librerias donde se ejecutan por el orden en que fueron inyectados.

> [!IMPORTANT]
> Definir las propiedades Layer y Order.

```csharp
/// <summary>
/// Example of a command interceptor that logs the execution of commands using source-generated logging methods.
/// </summary>
/// <typeparam name="TCommand">All Commands.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
/// <param name="logger">Logger for out the message.</param>
internal sealed partial class LogginCommandInterceptor<TCommand, TResult>(ILogger<LogginCommandInterceptor<TCommand, TResult>> logger) : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken)
    {
        this.LogEndCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    public Task OnBeforeAsync(TCommand command, CancellationToken cancellationToken)
    {
        this.LogStartCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(TCommand command, Exception exception, CancellationToken cancellationToken)
    {
        this.LogErrorCommand(command.GetType().Name);
        return Task.CompletedTask;
    }

    #region Logger

    [LoggerMessage(Level = LogLevel.Information, Message = "Start Command {CommandName}")]
    private partial void LogStartCommand(string commandName);

    [LoggerMessage(Level = LogLevel.Information, Message = "End Command {CommandName}")]
    private partial void LogEndCommand(string commandName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error Command {CommandName}")]
    private partial void LogErrorCommand(string commandName);
    #endregion
}

```

## Ejecucion del Command

Para ejecutar el comando simplemente hay que utilizar el IDispatcher.

```csharp
[Route("api/[controller]")]
[ApiController]
public sealed class MyController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> CreatePerson(CreatePersonCommand command)
    {
        var result = await dispatcher.SendAsync(command, CancellationToken.None);

        if (result.IsFailure)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }
}
```