# YAMCqrs.EventBus.Provider.RabbitMq

[Documentacion en español](YAMCqrs.EventBus.Provider.Rabbit_spa.md)

RabbitMq provider for the `YAMCqrs.EventBus` ecosystem.

This package adds support for publishing and consuming integration events using **RabbitMq** as the messaging broker.

The implementation integrates with `YAMCqrs.EventBus.Core` and uses a decoupled architecture based on persistence, asynchronous processing, and internal workers.

---

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.EventBus.Provider.RabbitMq
```

---

## 🚀 Quick Start

Register Rabbit in the dependency container:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Base EventBus configuration
})
 .UseRabbit(new YAMCqrs.EventBus.Provider.RabbitMq.Configuration.RabbitConfigurationOptions()
{
    ConnectionString = "cs_Rabbit",
    QueueGroupName = "TestApp",
});
```

> [!TIP]
> Using "cs_Rabbit" as ConnectionString, the library will look up the real value in `ConnectionStrings:Rabbit` following [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuration

### RabbitConfigurationOptions

- `ConnectionString`
  Connection string used to connect to Rabbit.

- `QueueGroupName`
  Consumer group used for distributed consumption of topics.

---

## 🛠️ Main Features

### Decoupled persistence

Messages are first persisted and then processed in independent scopes.

Benefits:
- resilience
- asynchronous processing
- infrastructure decoupling
- controlled retry

---

### Safe consumption

Message fetching is content-agnostic.

If a message has an invalid format:
- the error occurs inside the application
- no infinite loop is created in Rabbit
- the offset can continue moving forward

This prevents permanently blocking the topic.

---

### Automatic reconnection retries

Rabbit requires all topics to exist before starting the consumer.

If any topic does not exist:
- the connection will keep retrying indefinitely
- errors are logged
- the system recovers automatically when the topic appears

---

### Source Generation

The library uses Source Generators to:
- discover topics automatically
- register consumers
- avoid Reflection
- improve startup performance

---

## 📋 Dependencies

- `RabbitMQ.Client`
- `YAMCqrs.EventBus.Core`

---

# 📤 Publishing Events

To publish an event:
1. Inherit from `RabbitPublishEvent`
2. Define the Topic (also called exchange)
3. Define the RoutingKey
4. Use `IEventPublisher`

> [!IMPORTANT]
> The actual publish happens in a separate scope.
> The handler only records the intent to publish.

---

## 💡 Example Publish Event

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
            new MyRabbitPublishEvent(),
            cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
```

---

# 📥 Consuming Events

To consume events:
1. Inherit from `RabbitSubscribeEvent`
2. Define a constant topic (also called exchange)
3. Define a constant RoutingKey
4. Implement `ICommandHandler<TEvent, bool>`

> [!IMPORTANT]
> The topic name must be:
> - a string literal
> - or a constant
>
> This is required so the Source Generator can automatically discover topics.

---

## 💡 Example Subscribe Event

```csharp
internal sealed class MyRabbitSubscribeEvent() : RabbitSubscribeEvent(MyRabbitPublishEvent.TopicName)
{
    public override string RoutingKey => MyRabbitPublishEvent.RoutingKeyName;

    public string RandomProduct { get; set; } = string.Empty;
}
```

---

## 💡 Event Processing

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

## ⚡ Architectural Highlights

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
