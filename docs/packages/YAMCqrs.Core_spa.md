# YAMCqrs.Core

Librería CORE del ecosistema YAMCqrs construida sobre los principios de **CQRS (Command Query Responsibility Segregation)** y el patrón **Mediator** como mecanismo de orquestación.

Este paquete provee una capa liviana y de alto rendimiento para implementar Commands, Queries, Handlers, Interceptors y pipelines de ejecución en aplicaciones .NET.

---

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.Core
```

---

## 🚀 Uso Rápido

Registrar YAMCqrs en el contenedor de dependencias:

```csharp
builder.Services.AddYAMCqrs();
```

Una vez registrado, el framework descubre y registra automáticamente:
- Command Handlers
- Query Handlers
- Interceptors
- Implementaciones del Dispatcher

---

## 📋 Conceptos Principales

### ICommand & IQuery

YAMCqrs separa las operaciones de lectura y escritura utilizando abstracciones independientes:

- `ICommand<TResult>` → Representa operaciones de escritura o acciones que modifican estado.
- `IQuery<TResult>` → Representa operaciones de solo lectura.

Esta separación favorece:
- Arquitectura más limpia
- Intención explícita
- Mejor escalabilidad
- Mayor facilidad de testing y mantenimiento

Ejemplo:

```csharp
public sealed class CreateUserCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
}
```

---

### ICommandHandler & IQueryHandler

Los Handlers contienen la lógica de negocio asociada a un Command o Query específico.

Ejemplo:

```csharp
internal sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Lógica de negocio
        return Result<Guid>.Ok(Guid.NewGuid());
    }
}
```

---

### Result Pattern

El framework utiliza un objeto `Result<T>` en lugar de Exceptions para errores esperados de negocio.

Beneficios:
- Evita utilizar Exceptions como control de flujo
- Mejora la legibilidad
- Hace explícitos los errores
- Mejor rendimiento en escenarios con muchas validaciones

Ejemplo:

```csharp
return Result<string>.Failure("El usuario ya existe");
```

---

### Interceptors

Los Interceptors permiten agregar lógica transversal al pipeline de ejecución.

Casos de uso comunes:
- Logging
- Auditoría
- Métricas
- Validaciones
- Tracing
- Seguridad

Abstracciones disponibles:
- `ICommandInterceptor`
- `IQueryInterceptor`
- `CommandInterceptorBase`
- `QueryInterceptorBase`

> [!IMPORTANT]
> Los Interceptors requieren definir explícitamente `Layer` y `Order` para garantizar un orden de ejecución determinístico.

Ejemplo:

```csharp
internal sealed class LoggingInterceptor<TCommand, TResult>
    : ICommandInterceptor<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task OnBeforeAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ejecutando {typeof(TCommand).Name}");
        return Task.CompletedTask;
    }
}
```

---

### IDispatcher

`IDispatcher` es el punto de entrada principal utilizado para ejecutar Commands y Queries.

Ejemplo:

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
> YAMCqrs.Core utiliza Source Generators para generar automáticamente:
>
> - Registros de Dependency Injection
> - Implementaciones del Dispatcher
> - Mapeos de Handlers

Este diseño evita Reflection en runtime y mejora:
- Startup performance
- Performance de ejecución
- Compatibilidad con AOT
- Soporte para Native Trimming

---

## 🛠️ Detalles de Implementación

### Features Incluidas

- Abstracciones CQRS
- Orquestación Mediator
- Pipeline de Dispatcher
- Pipeline de Interceptors
- Result pattern
- Registro DI generado automáticamente
- Dispatcher sin Reflection

### Objetivos Arquitectónicos

- Alto rendimiento
- Bajas allocations
- Pipelines explícitos
- Ejecución predecible
- Separación clara de responsabilidades
- Dependencias mínimas

---

## 📚 Lecturas Recomendadas

- [Ejemplo básico de Command Workflow](../examples/CommandWorkFlow_spa.md)
- [ADR 8 — Result Object](../adr/0008-result-object.md)
- [ADR 10 — Pipeline Interceptors](../adr/0010-pipeline-interceptor.md)

---

## 📋 Dependencias

Este paquete no tiene dependencias externas.

Solamente requiere el runtime de .NET.
