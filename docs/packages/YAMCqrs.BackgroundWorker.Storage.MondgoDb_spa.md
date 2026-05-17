# YAMCqrs.BackgroundWorker.Storage.MondgoDb

Este paquete proporciona una implementación de almacenamiento persistente para el BackgroundWorker de YAMCqrs utilizando **MongoDB**. Permite que el historial de ejecuciones de las tareas en segundo planos sean almacenados para auditoria y calcular el healthcheck de los mismos.

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.BackgroundWorker.Storage.MondgoDb
```

## 🚀 Uso Rápido

Para registrar el almacenamiento de Mongo en tu contenedor de dependencias:

```csharp
  builder.Services.AddBackgroundWorker(options =>
        {
          //Configuracion de la libreria base de BackgroundWorker
        })
        .UseMongoDb(new BackgroundWorkerMongoConfiguration
        {
            ConnectionString = "cs_MongoDb",
            DatabaseName = "TestAppDb",
        });
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
- El proyecto `YAMCqrs.BackgroundWorker`

## 💡 Ejemplo de Documento en DB

Los eventos se persisten en la colección `WorkerExecutions` con la siguiente estructura:

```json
{
  "_id": "019e3215-a171-7ff2-880a-14597e93cde0",
  "WorkerName": "YAMCqrs.BackgroundWorker.Implementation.CleanBackGroundWorker",
  "ExecutionStartTime": {
    "$date": "2026-05-16T18:38:58.929Z"
  },
  "ExecutionEndTime": {
    "$date": "2026-05-16T18:38:59.163Z"
  },
  "Status": "Success",
  "Success": 1,
  "Failed": 0,
  "Message": ""
}
```