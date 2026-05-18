# YAMCqrs.BackgroundWorker.Storage.MongoDb

[Documentacion en español](YAMCqrs.BackgroundWorker.Storage.MondgoDb_spa.md)

This package provides a persistent storage implementation for the [YAMCqrs BackgroundWorker](YAMCqrs.BackgroundWorker.Core_en.md) project using **MongoDB**. It allows background task execution history to be stored for auditing purposes and health check calculations.

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.BackgroundWorker.Storage.MondgoDb
```

## 🚀 Quick Start

To register MongoDB storage in your dependency container:

```csharp
builder.Services.AddBackgroundWorker(options =>
{
    // Configuration for the core BackgroundWorker library.
    // For more details, see its corresponding documentation.
})
.UseMongoDb(new BackgroundWorkerMongoConfiguration
{
    ConnectionString = "cs_MongoDb",
    DatabaseName = "TestAppDb",
});
```

> [!TIP]
> By using `"cs_MongoDb"` as the ConnectionString, we are telling the library to look up the actual value inside the `"ConnectionStrings"` section using the `MongoDb` key, as defined in [ADR 13](../adr/0013-connection-strings.md)

## ⚙️ Configuration

- **ConnectionString:** Connection string used to connect to MongoDB.
- **DatabaseName:** Name of the database to use.

## 🛠️ Implementation Details

- **Indexes:** Automatically created.
- **Collections:** The MongoDB instance must allow automatic collection creation.

## 📋 Dependencies

- MongoDB.Driver
- `YAMCqrs.BackgroundWorker` project

## 💡 Example DB Document

Events are persisted in the `WorkerExecutions` collection with the following structure:

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
