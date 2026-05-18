# YAMCqrs.EventBus.Provider.Kafka

[Documentacion en español](YAMCqrs.EventBus.Provider.Kafka_spa.md)

Kafka provider for the `YAMCqrs.EventBus` ecosystem.

This package adds support for publishing and consuming integration events using **Apache Kafka** as the messaging broker.

The implementation integrates with `YAMCqrs.EventBus.Core` and uses a decoupled architecture based on persistence, asynchronous processing, and internal background workers.

---

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.EventBus.Provider.Kafka
```

---

## 🚀 Quick Start

Register Kafka in the dependency injection container:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Base EventBus configuration
})
.UseKafka(new KafkaConfigurationOptions()
{
    ConnectionString = "cs_Kafka",
    KafkaClientName = "TestApp",
    KafkaGroupName = "TestApp",
});
```

> [!TIP]
> Using `"cs_Kafka"` as the ConnectionString instructs the library to resolve the actual value from `ConnectionStrings:Kafka` following [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuration

### KafkaConfigurationOptions

- `ConnectionString`
  Connection string used to connect to Kafka.

- `KafkaClientName`
  Identifier used by Kafka for logs and metrics.

- `KafkaGroupName`
  Consumer Group used for distributed topic consumption.

- `MaxConcurrentConsumers`
  Maximum number of concurrent consumers listening for messages.

---

## 🛠️ Main Features

### Decoupled persistence

Messages are first persisted and later processed in independent scopes.

Benefits:
- resilience
- asynchronous processing
- infrastructure decoupling
- controlled retries

---

### Safe consumption

Message downloads are content-agnostic.

If a message contains invalid payload data:
- the failure occurs inside the application
- Kafka infinite retry loops are avoided
- offsets can continue advancing

This prevents permanently blocking the topic.

---

### Automatic connection retries

Kafka requires all topics to exist before consumers can start.

If a topic is missing:
- the connection retries indefinitely
- failures are logged
- the system self-recovers once the topic exists

---

### Source Generation

The library uses Source Generators to:
- automatically discover topics
- register consumers
- avoid Reflection
- improve startup performance

---

## 📋 Dependencies

- `Confluent.Kafka`
- `YAMCqrs.EventBus.Core`

---

# 📤 Publishing Events

To publish an event:
1. Inherit from `KafkaPublishEvent`
2. Define the Topic
3. Use `IEventPublisher`

> [!IMPORTANT]
> Actual publishing happens in a separate scope.
> The handler only registers the publishing intent.

---

## 💡 Publish Event Example

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

## 💡 Publishing the Event

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

# 📥 Consuming Events

To consume events:
1. Inherit from `KafkaSubscribeEvent`
2. Define a constant topic
3. Implement `ICommandHandler<TEvent, bool>`

> [!IMPORTANT]
> The topic name must be:
> - a string literal
> - or a constant reference
>
> This is required for Source Generators to automatically discover topics.

---

## 💡 Consume Event Example

```csharp
internal sealed class MyKafkaSubscribeEvent()
    : KafkaSubscribeEvent(MyKafkaPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
```

---

## 💡 Event Processing Example

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

## ⚡ Architectural Features

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

