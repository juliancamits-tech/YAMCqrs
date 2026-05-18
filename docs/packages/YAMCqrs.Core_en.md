# YAMCqrs.Core

[Documentacion en español](YAMCqrs.Core_spa.md)

Core library of the YAMCqrs ecosystem built around the principles of **CQRS (Command Query Responsibility Segregation)** and the **Mediator pattern** as the orchestration mechanism.

This package provides a lightweight, high-performance abstraction layer for implementing Commands, Queries, Handlers, Interceptors, and dispatching pipelines in .NET applications.

---

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.Core
```

---

## 🚀 Quick Start

Register YAMCqrs in the dependency injection container:

```csharp
builder.Services.AddYAMCqrs();
```

Once registered, the framework automatically discovers and registers:
- Command Handlers
- Query Handlers
- Interceptors
- Dispatcher implementations

---

## 📋 Main Concepts

### ICommand & IQuery

YAMCqrs separates read and write operations using independent abstractions:

- `ICommand<TResult>` → Represents write operations or actions that modify state.
- `IQuery<TResult>` → Represents read-only operations.

This separation promotes:
- Cleaner architecture
- Explicit intent
- Better scalability
- Easier testing and maintenance

Example:

```csharp
public sealed class CreateUserCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
}
```

---

### ICommandHandler & IQueryHandler

Handlers contain the business logic associated with a specific Command or Query.

Example:

```csharp
internal sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Business logic here
        return Result<Guid>.Ok(Guid.NewGuid());
    }
}
```

---

### Result Pattern

The framework uses a `Result<T>` object instead of Exceptions for expected business validation failures.

Benefits:
- Avoids using Exceptions for control flow
- Improves readability
- Makes failures explicit
- Better performance under validation-heavy scenarios

Example:

```csharp
return Result<string>.Failure("User already exists");
```

---

### Interceptors

Interceptors allow adding cross-cutting concerns to the execution pipeline.

Common use cases:
- Logging
- Auditing
- Metrics
- Validation
- Tracing
- Security checks

Available abstractions:
- `ICommandInterceptor`
- `IQueryInterceptor`
- `CommandInterceptorBase`
- `QueryInterceptorBase`

> [!IMPORTANT]
> Interceptors require explicit `Layer` and `Order` definitions to ensure deterministic execution order.

Example:

```csharp
internal sealed class LoggingInterceptor<TCommand, TResult>
    : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnBeforeAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Executing {typeof(TCommand).Name}");
        return Task.CompletedTask;
    }
}
```

---

### IDispatcher

`IDispatcher` is the main entry point used to execute Commands and Queries.

Example:

```csharp
var result = await dispatcher.SendAsync(command);

if (result.IsFailure)
{
    return BadRequest(result.Error);
}

return Ok(result.Value);
```

---

## ⚡ Performance & Source Generation

> [!IMPORTANT]
> YAMCqrs.Core uses Source Generators to automatically generate:
>
> - Dependency Injection registrations
> - Dispatcher implementations
> - Handler mappings

This design avoids runtime Reflection and improves:
- Startup performance
- Execution performance
- AOT compatibility
- Native trimming support

---

## 🛠️ Implementation Details

### Features Included

- CQRS abstractions
- Mediator orchestration
- Dispatcher pipeline
- Interceptor pipeline
- Result pattern
- Source-generated DI registration
- Reflection-free dispatcher generation

### Architecture Goals

- High performance
- Low allocations
- Explicit pipelines
- Predictable execution
- Clean separation of concerns
- Minimal dependencies

---

## 📚 Recommended Reading

- [Basic Command Workflow Example](../examples/CommandWorkFlow_en.md)
- [ADR 8 — Result Object](../adr/0008-result-object.md)
- [ADR 10 — Pipeline Interceptors](../adr/0010-pipeline-interceptor.md)

---

## 📋 Dependencies

This package has **no external dependencies**.

Only the .NET runtime is required.
