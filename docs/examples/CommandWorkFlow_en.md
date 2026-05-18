# Command Workflow

This document explains how to implement an `ICommand`. The same approach also applies to `IQuery` with only minor changes.

## Creating the ICommand

Create the `ICommand` that will act as the initiating class with the required parameters.
It is important that it implements the `ICommand` interface and that the generic type parameter (`T`) specifies the expected response type, which can be either a primitive type or a class.

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

## Creating the Handler

Create a class responsible for processing the business logic associated with the command.
It is important that it implements `ICommandHandler`, where the first generic argument is the command being processed and the second argument is the return type.

It is also important to understand that, in order to avoid using Exceptions for business rules, the Handler returns a `Result` object to indicate how the process ended.
For more information about `Result`, see [ADR 8](../adr/0008-result-object.md)

```csharp
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Commands;

/// <summary>
/// Handler for <see cref="CreatePersonCommand"/>.
/// The class is internal and not directly referenced, but it will be instantiated by the generated DI registration.
/// </summary>
internal sealed class CreatePersonCommandHandler(...Possible parameters resolved from DI) : ICommandHandler<CreatePersonCommand, string>
{
    public Task<Result<string>> HandleAsync(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        // Business logic
        return Task.FromResult(Result<string>.Ok(command.Name));
    }
}
```

## Creating an Interceptor

Suppose we have an extreme auditing requirement where every command must log its Request and Response. To achieve this, we can create an Interceptor.

Interceptors provide two different approaches:
- The `ICommandInterceptor` interface
- The `CommandInterceptorBase` abstract class

Either approach can be used, but keep in mind that based on what is defined in [ADR 10](../adr/0010-pipeline-interceptor.md), it is necessary to define the interceptor execution layer and order so that execution is explicit and deterministic, unlike other libraries where execution depends on the injection order.

> [!IMPORTANT]
> Define the `Layer` and `Order` properties.

```csharp
/// <summary>
/// Example of a command interceptor that logs the execution of commands using source-generated logging methods.
/// </summary>
/// <typeparam name="TCommand">All Commands.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
/// <param name="logger">Logger for output messages.</param>
internal sealed partial class LoggingCommandInterceptor<TCommand, TResult>(ILogger<LoggingCommandInterceptor<TCommand, TResult>> logger) : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnAfterAsync(TCommand command, Result<TResult> result, CancellationToken cancellationToken)
    {
        this.LogEndCommand(typeof(TCommand).Name);
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

## Executing the Command

To execute the command, simply use the `IDispatcher`.

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
