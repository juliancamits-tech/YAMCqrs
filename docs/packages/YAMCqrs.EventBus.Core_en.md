# YAMCqrs.EventBus.Core

[Documentacion en español](YAMCqrs.EventBus.Core_spa.md)

Core event management library for the YAMCqrs ecosystem.

The package is completely messaging-provider agnostic and provides a decoupled infrastructure for event publishing, persistence, processing, and subscription.

It also includes an `InMemory` implementation that enables local Domain Event simulation and allows evolving those events into real Integration Events following ADR 6 guidelines.

---

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.EventBus.Core
```

---

## 🚀 Quick Start

```csharp
builder.Services.AddEventBus(opt =>
{
    opt.ConcurrentWorkers = 1;
    opt.BatchSize = 100;
});
```

---

## ⚙️ Configuration

- `ConcurrentWorkers`
  Number of concurrent workers for parallel processing.

- `BatchSize`
  Maximum number of events processed per execution.

- `PollingIntervalSeconds`
  Polling interval used to fetch pending events.

- `ErrorThresholdPercentage`
  Maximum allowed error percentage before marking a batch as failed.

---

## 🛠️ Core Components

### IEventBusPublisher

Allows extending event publishing to:
- Kafka
- RabbitMQ
- Azure Service Bus
- AWS SQS/SNS
- Custom providers

The library includes an `InMemory` implementation for local Domain Events.

---

### IEventPublisher

Registers the intent to publish events while respecting the current transactional scope.

Benefits:
- prevents inconsistent persistence
- decouples business logic
- supports asynchronous processing

---

### IPublishEventStore

Responsible for storing pending events before publishing.

Includes:
- auditing
- tracking
- retries
- processing decoupling

> ⚠️ The InMemory implementation is not recommended for production.

---

### ISubscribeEventStore

Stores received events before processing.

Enables:
- reprocessing
- auditing
- decoupling reception and consumption

---

## 📡 Domain Events with InMemory

### Outgoing event

```csharp
internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
{
    public const string TopicName = "domain-event-topic";

    public int Numerito { get; init; }

    public override string Topic()
    {
        return TopicName;
    }
}
```

---

### Publish the event

```csharp
await eventPublisher.PublishAsync(
    new DomainEventPublishEvent(),
    cancellationToken);
```

---

### Incoming event

```csharp
internal sealed class DomainEventSubscribeEvent()
    : InMemorySuscribeEvent(DomainEventPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
```

---

### Process the event

```csharp
internal sealed class DomainEventSubscribeHanlder
    : ICommandHandler<DomainEventSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(
        DomainEventSubscribeEvent command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
```

---

## ⚡ Architectural Features

- Event-driven architecture
- Domain Events
- Integration Events
- Outbox-like processing
- Asynchronous processing
- Low coupling
- Extensible infrastructure
- Event auditing
- Retry support

---

## 📋 Dependencies

- `YAMCqrs.EventBus.Core`
- `YAMCqrs.BackgroundWorker`
