# YAMCqrs.EventBus.Provider.Kafka

Proveedor Kafka para el ecosistema `YAMCqrs.EventBus`.

Este paquete agrega soporte para publicación y consumo de eventos de integración utilizando **Apache Kafka** como broker de mensajería.

La implementación se integra con `YAMCqrs.EventBus.Core` y utiliza una arquitectura desacoplada basada en persistencia, procesamiento asíncrono y workers internos.

---

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Provider.Kafka
```

---

## 🚀 Uso Rápido

Registrar Kafka en el contenedor de dependencias:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Configuración base del EventBus
})
.UseKafka(new KafkaConfigurationOptions()
{
    ConnectionString = "cs_Kafka",
    KafkaClientName = "TestApp",
    KafkaGroupName = "TestApp",
});
```

> [!TIP]
> Utilizando `"cs_Kafka"` como ConnectionString, la librería buscará el valor real dentro de `ConnectionStrings:Kafka` siguiendo [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuración

### KafkaConfigurationOptions

- `ConnectionString`
  Connection string utilizada para conectarse a Kafka.

- `KafkaClientName`
  Nombre identificador utilizado por Kafka para logs y métricas.

- `KafkaGroupName`
  Consumer Group utilizado para consumo distribuido de topics.

- `MaxConcurrentConsumers`
  Cantidad máxima de consumers concurrentes escuchando mensajes.

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
- no se genera loop infinito en Kafka
- el offset puede continuar avanzando

Esto evita bloquear permanentemente el topic.

---

### Reintentos automáticos de conexión

Kafka requiere que todos los topics existan antes de iniciar el consumer.

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

- `Confluent.Kafka`
- `YAMCqrs.EventBus.Core`

---

# 📤 Publicación de Eventos

Para publicar un evento:
1. Heredar de `KafkaPublishEvent`
2. Definir el Topic
3. Utilizar `IEventPublisher`

> [!IMPORTANT]
> La publicación real ocurre en un scope separado.
> El handler solamente registra la intención de publicar.

---

## 💡 Ejemplo de Evento de Publicación

```csharp
internal sealed class MyKafkaPublishEvent : KafkaPublishEvent
{
    public const string TopicName = "kafka.event.test";

    public int Numerito { get; init; }

    public override Dictionary<string, string>? GetCustomHeaders()
    {
        return null;
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
            new MyKafkaPublishEvent(),
            cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
```

---

# 📥 Consumo de Eventos

Para consumir eventos:
1. Heredar de `KafkaSubscribeEvent`
2. Definir un topic constante
3. Implementar `ICommandHandler<TEvent, bool>`

> [!IMPORTANT]
> El nombre del topic debe ser:
> - un string literal
> - o una constante
>
> Esto es obligatorio para que el Source Generator pueda descubrir automáticamente los topics.

---

## 💡 Ejemplo de Evento de Consumo

```csharp
internal sealed class MyKafkaSubscribeEvent()
    : KafkaSubscribeEvent(MyKafkaPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
```

---

## 💡 Procesamiento del Evento

```csharp
internal sealed partial class MyKafkaSubscribeEventHanlder
    : ICommandHandler<MyKafkaSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(
        MyKafkaSubscribeEvent command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
```

---

## ⚡ Características Arquitectónicas

- Kafka integration
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

