# YAMCqrs.EventBus.Storage.MongoDb

[Documentacion en español](YAMCqrs.EventBus.Storage.MongoDb_spa.md)

Persistent storage implementation for `YAMCqrs.EventBus.Core` using **MongoDB**.

> [!IMPORTANT]
> This package DOES NOT use MongoDB as a messaging broker.
>
> MongoDB is used exclusively as a persistence mechanism for:
> - storing pending events
> - decoupling event processing
> - enabling execution in independent scopes
> - supporting retries
> - maintaining auditing and event tracking

Actual event delivery and consumption remain the responsibility of the configured messaging provider (Kafka, RabbitMQ, etc).

---

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.EventBus.Storage.MongoDb
```

---

## 🚀 Quick Start

Register MongoDB Storage in the dependency injection container:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Base EventBus configuration
})
.UseMongoDb(new EventBusStorageMongoConfiguration()
{
    ConnectionString = "cs_MongoDb",
    DatabaseName = "TestAppDb",
});
```

> [!TIP]
> Using `"cs_MongoDb"` as the ConnectionString instructs the library to automatically resolve the value from `ConnectionStrings:MongoDb` following [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuration

### EventBusStorageMongoConfiguration

- `ConnectionString`
  Connection string used to connect to MongoDB.

- `DatabaseName`
  Database name used for event persistence.

---

## 🛠️ Architectural Purpose

Decoupled persistence enables the EventBus to:

- process events outside the original scope
- decouple publishing and processing
- survive application restarts
- implement safe retries
- maintain full auditing
- reduce broker coupling

This enables patterns similar to:
- Outbox Pattern
- Inbox Pattern
- Event Auditing
- Reliable Messaging

---

## 📤 Publish Event Persistence

Outgoing events are stored before being delivered to the broker.

Collection used:
- `PublishEvents`

Stored information:
- event type
- destination
- serialized payload
- status
- retries
- timestamps

Example:

```json
{
  "_id": "019e3223-200b-7308-baba-7a99740491c6",
  "EventType": "Test.Application.Kafka.MyKafkaPublishEvent",
  "EventDestination": "Kafka",
  "Value": "{\"Numerito\":28,\"EventId\":\"019e3223-1ff7-755d-8dc1-f5b804e9cfb2\",\"Timestamp\":\"2026-05-16T18:53:43.2871219+00:00\"}",
  "CreatedAt": {
    "$date": "2026-05-16T18:53:43.334Z"
  },
  "Status": "Processed",
  "RetryCount": 0
}
```

---

## 📥 Subscribe Event Persistence

Incoming broker events are also persisted before processing.

Collection used:
- `SubscribeEvents`

Benefits:
- reprocessing
- auditing
- decoupling reception and execution
- resilience

Example:

```json
{
  "_id": "019e3223-667b-7edd-822d-f883b9d932f4",
  "Topic": "kafka.event.test",
  "Value": "{\"Numerito\":28,\"EventId\":\"019e3223-1ff7-755d-8dc1-f5b804e9cfb2\",\"Timestamp\":\"2026-05-16T18:53:43.2871219+00:00\"}",
  "Headers": {
    "EventType": "Test.Application.Kafka.MyKafkaPublishEvent",
    "EventId": "019e3223-1ff7-755d-8dc1-f5b804e9cfb2",
    "Timestamp": "2026-05-16T18:53:43.2871219+00:00"
  },
  "ReceivedAt": {
    "$date": "2026-05-16T18:54:01.339Z"
  },
  "Status": "Pending",
  "RetryCount": 0,
  "SourceStorage": "MongoDb",
  "SourceEvent": "Kafka"
}
```

---

## ⚡ Main Features

- MongoDB persistence
- Event auditing
- Retry support
- Outbox-like storage
- Inbox-like storage
- Reliable event processing
- Decoupled processing
- Independent scopes
- Asynchronous processing
- Distributed systems support
- Event tracking
- Broker-independent persistence

---

## 🛠️ Implementation Details

### Automatic indexes

Required indexes are automatically created during application startup.

---

### Automatic collection creation

The MongoDB instance must allow automatic collection creation.

---

### Broker-independent persistence

The storage layer is independent from the messaging broker used.

The storage implementation works with:
- Kafka
- RabbitMQ
- Azure Service Bus
- AWS SQS/SNS
- custom providers

---

## 📋 Dependencies

- `MongoDB.Driver`
- `YAMCqrs.EventBus.Core`

