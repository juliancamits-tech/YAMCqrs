# YAMCqrs.EventBus.Storage.MongoDb

Implementación de almacenamiento persistente para `YAMCqrs.EventBus.Core` utilizando **MongoDB**.

> [!IMPORTANT]
> Este paquete NO utiliza MongoDB como broker de mensajería.
>
> MongoDB se utiliza exclusivamente como mecanismo de persistencia para:
> - almacenar eventos pendientes
> - desacoplar procesamiento
> - permitir ejecución en scopes independientes
> - soportar retries
> - mantener auditoría y tracking de eventos

El procesamiento real de eventos continúa siendo responsabilidad del provider de mensajería configurado (Kafka, RabbitMQ, etc).

---

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Storage.MongoDb
```

---

## 🚀 Uso Rápido

Registrar MongoDB Storage en el contenedor de dependencias:

```csharp
builder.Services.AddEventBus(opt =>
{
    // Configuración base del EventBus
})
.UseMongoDb(new EventBusStorageMongoConfiguration()
{
    ConnectionString = "cs_MongoDb",
    DatabaseName = "TestAppDb",
});
```

> [!TIP]
> Utilizando `"cs_MongoDb"` como ConnectionString, la librería resolverá automáticamente el valor desde `ConnectionStrings:MongoDb` siguiendo [ADR 13](../adr/0013-connection-strings.md).

---

## ⚙️ Configuración

### EventBusStorageMongoConfiguration

- `ConnectionString`
  Connection string utilizada para conectarse a MongoDB.

- `DatabaseName`
  Nombre de la base de datos utilizada para persistencia de eventos.

---

## 🛠️ Objetivo Arquitectónico

La persistencia desacoplada permite que el EventBus:

- procese eventos fuera del scope original
- desacople publicación y procesamiento
- sobreviva reinicios de aplicación
- implemente retries seguros
- mantenga auditoría completa
- reduzca acoplamiento con el broker

Esto permite implementar patrones similares a:
- Outbox Pattern
- Inbox Pattern
- Event Auditing
- Reliable Messaging

---

## 📤 Persistencia de Publish Events

Los eventos salientes se almacenan antes de enviarse al broker.

Colección utilizada:
- `PublishEvents`

Información almacenada:
- tipo de evento
- destino
- payload serializado
- estado
- retries
- timestamps

Ejemplo:

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

## 📥 Persistencia de Subscribe Events

Los eventos recibidos desde el broker también son persistidos antes de procesarse.

Colección utilizada:
- `SubscribeEvents`

Beneficios:
- reprocesamiento
- auditoría
- desacople de recepción y ejecución
- resiliencia

Ejemplo:

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

## ⚡ Características Principales

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

## 🛠️ Detalles de Implementación

### Índices automáticos

Los índices necesarios son creados automáticamente al iniciar la aplicación.

---

### Creación automática de colecciones

La instancia de MongoDB debe permitir creación automática de colecciones.

---

### Persistencia desacoplada del broker

La librería no depende del broker específico utilizado.

El storage funciona independientemente de:
- Kafka
- RabbitMQ
- Azure Service Bus
- AWS SQS/SNS
- providers custom

---

## 📋 Dependencias

- `MongoDB.Driver`
- `YAMCqrs.EventBus.Core`

