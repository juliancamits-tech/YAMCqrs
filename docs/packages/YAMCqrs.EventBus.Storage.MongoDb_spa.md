# YAMCqrs.EventBus.Storage.MongoDb

Este paquete proporciona una implementación de almacenamiento persistente para el EventBus de YAMCqrs utilizando **MongoDB**. Permite que los eventos de integración y dominio se guarden antes de ser procesados (esto aplica tanto a los mensajes de entrada como de salida).

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Storage.MongoDb
```

## 🚀 Uso Rápido

Para registrar el almacenamiento de Mongo en tu contenedor de dependencias:

```csharp
builder.Services.AddEventBus(opt =>
        {
          //Configuracion de la libreria base de EventBus
        })
        .UseMongoDb(new YAMCqrs.EventBus.Storage.MongoDb.EventBusStorageMongoConfiguration()
        {
            ConnectionString = "cs_MongoDb",
            DatabaseName = "TestAppDb",
        })
```

> [!TIP]
> Al usar como ConnectionString "cs_MongoDb" estamos diciendole a la libreria que dentro del array "ConnectionStrings" busque el valor real en la clave MongoDb segun lo definido en el [ADR 13](../adr/0013-connection-strings.md)

## ⚙️ Configuración
- **ConnectionString**: ConnectionString para conectarse a Mongo.
- **DatabaseName**: Nombre de la BD a utilizar.

## 🛠️ Detalles de Implementación

- **Indices:** Se crean automáticamente.
- **Tablas:** La instancia de mongo tiene que estar habilitada para crear las Colecciones automaticamente

## 📋 Dependencias

- MongoDB.Driver
- El proyecto `YAMCqrs.EventBus.Core`

## 💡 Ejemplo de Documento en DB

Los eventos se persisten en la colección `PublishEvents` con la siguiente estructura:

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


Los eventos se persisten en la colección `SubscribeEvents` con la siguiente estructura:

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
```json