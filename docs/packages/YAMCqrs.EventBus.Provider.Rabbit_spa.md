# YAMCqrs.EventBus.Provider.RabbitMq

Proveedor RabbitMq para el ecosistema `YAMCqrs.EventBus`.

Este paquete agrega soporte para publicación y consumo de eventos de integración utilizando **RabbitMq** como broker de mensajería.

La implementación se integra con `YAMCqrs.EventBus.Core` y utiliza una arquitectura desacoplada basada en persistencia, procesamiento asíncrono y workers internos.

---

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Provider.RabbitMq
```

---

## 🚀 Uso Rápido

Registrar Rabbit en el contenedor de dependencias:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Configuración base del EventBus
})
 .UseRabbit(new YAMCqrs.EventBus.Provider.RabbitMq.Configuration.RabbitConfigurationOptions()
{
    ConnectionString = "cs_Rabbit",
    QueueGroupName = "TestApp",
});
```

> [!TIP]
> Utilizando `"cs_Rabbit"` como ConnectionString, la librería buscará el valor real dentro de `ConnectionStrings:Rabbit` siguiendo [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuración

### RabbitConfigurationOptions

- `ConnectionString`
  Connection string utilizada para conectarse a Rabbit.

- `QueueGroupName`
  Consumer Group utilizado para consumo distribuido de topics.

---

## 🛠️ Características Principales

### Persistencia desacoplada

Los mensajes primero son persistidos y luego procesados en scopes independientes.

Beneficios:
- resiliencia
- procesamiento asíncrono
- desacople de infraestructura
- retry controlado

---

### Consumo seguro

La descarga de mensajes es agnóstica al contenido.

Si un mensaje posee un formato inválido:
- el error ocurre dentro de la aplicación
- no se genera loop infinito en Rabbit
- el offset puede continuar avanzando

Esto evita bloquear permanentemente el topic.

---

### Reintentos automáticos de conexión

Rabbit requiere que todos los topics existan antes de iniciar el consumer.

Si algún topic no existe:
- la conexión seguirá reintentando indefinidamente
- los errores quedan registrados en logs
- el sistema se recupera automáticamente cuando el topic aparece

---

### Source Generation

La librería utiliza Source Generators para:
- descubrir topics automáticamente
- registrar consumers
- evitar Reflection
- mejorar startup performance

---

## 📋 Dependencias

- `RabbitMQ.Client`
- `YAMCqrs.EventBus.Core`

---

# 📤 Publicación de Eventos

Para publicar un evento:
1. Heredar de `RabbitPublishEvent`
2. Definir el Topic (Tambien llamado exchange)
3. Definir el RoutingKey
4. Utilizar `IEventPublisher`

> [!IMPORTANT]
> La publicación real ocurre en un scope separado.
> El handler solamente registra la intención de publicar.

---

## 💡 Ejemplo de Evento de Publicación

```csharp
internal sealed class MyRabbitPublishEvent  : RabbitPublishEvent
{
    public const string TopicName = "my.rabbit.event";
    public const string RoutingKeyName = "my.route";

    public override string RoutingKey => RoutingKeyName;

    public override void AddCustomHeaders(ref Dictionary<string, string> headers)
    {
        return;
    }

    public override string Topic()
    {
        return TopicName;
    }
}
```

---

## 💡 Publicar el Evento

```csharp
internal sealed class MyLogicCommandHandler(
    IEventPublisher eventPublisher)
    : ICommandHandler<MyLogicCommand, string>
{
    public async Task<Result<string>> HandleAsync(
        MyLogicCommand command,
        CancellationToken cancellationToken = default)
    {
        await eventPublisher.PublishAsync(
            new MyRabbitPublishEvent(),
            cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
```

---

# 📥 Consumo de Eventos

Para consumir eventos:
1. Heredar de `RabbitSubscribeEvent`
2. Definir un topic constante (Tambien llamado exchange)
3. Definir el RoutingKey constante
4. Implementar `ICommandHandler<TEvent, bool>`

> [!IMPORTANT]
> El nombre del topic debe ser:
> - un string literal
> - o una constante
>
> Esto es obligatorio para que el Source Generator pueda descubrir automáticamente los topics.

---

## 💡 Ejemplo de Evento de Consumo

```csharp
internal sealed class MyRabbitSubscribeEvent() : RabbitSubscribeEvent(MyRabbitPublishEvent.TopicName)
{
    public override string RoutingKey => MyRabbitPublishEvent.RoutingKeyName;

    public string RandomProduct { get; set; } = string.Empty;
}
```

---

## 💡 Procesamiento del Evento

```csharp
internal sealed partial class MyRabbitSubscribeEventHanlder
    : ICommandHandler<MyRabbitSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(
        MyRabbitSubscribeEvent command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
```

---

## ⚡ Características Arquitectónicas

- Rabbit integration
- Event-driven architecture
- Domain Events
- Integration Events
- Outbox-like processing
- Asynchronous consumers
- Retry support
- Low coupling
- Source-generated discovery
- Reflection-free startup
- Distributed messaging
- Background processing

