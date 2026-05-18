# YAMCqrs.BackgroundWorker.Core

[Documentacion en español](YAMCqrs.BackgroundWorker.Core_spa.md)

A .NET library for creating background workers that process batch tasks with support for parallel processing, execution auditing, and health monitoring in compliance with what is defined in [ADR 7](../adr/0007-backgroundservice.md)

## ⚙️ Installation

```bash
dotnet add package YAMCqrs.BackgroundWorker.Core
```

## 📋 Description

This library provides a robust infrastructure for implementing background services that process items in batches. The library offers:

- **Controlled batch processing:** Efficiently processes multiple items with support for configurable parallelism
- **Execution auditing:** Automatically logs each execution with success/failure metrics
- **Health monitoring:** Integrated health checks to verify worker status
- **Error management:** Configurable error threshold to determine execution status
- **Flexible storage:** Includes an in-memory implementation with support for custom implementations

## 🚀 Quick Start

To register the Background Workers engine in your dependency container:

```csharp
builder.Services.AddBackgroundWorker(options =>
{
    options.MinutesToKeepSuccesTask = 60;
    options.MinutesToKeepFailedTask = BackgroundWorkerConfiguration.DayToMinutes(7);
});
```

> [!TIP]
> Here in the configuration we are specifying that successfully completed tasks should be deleted every 60 minutes, while tasks that ended with errors should be deleted after 7 days.

## 🛠️ Implementation Details

- **YABackgroundWorker:** Abstract class to standardize background task behavior
- **IWorkerStorage:** Storage for execution history. Includes an `InMemory` implementation (not recommended for production).
- **CleanBackGroundWorker:** Worker responsible for cleaning the storage
- **HealthCheckReport:** Automatic health check over worker execution results

## 📋 Dependencies

- Only official Microsoft packages.

## 💡 Basic Usage

### 1. Create a Custom Worker

Inherit from `YABackgroundWorker<TWorkItem>` and implement the abstract methods:

```csharp
public class MyWorker : YABackgroundWorker<MyItem>
{
    public MyWorker(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    // Initial setup before the worker starts
    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Your initialization code here
        // Return true if initialization was successful, false otherwise
        // For example, use this to validate feature flags for task activation
        return Task.FromResult(true);
    }

    // Get the batch of items to process
    protected override async Task<IEnumerable<MyItem>?> GetBatchForProcessing(
        IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Retrieve items from your data source
        return await GetItems();
    }

    // Process an individual item
    protected override async Task<bool> ProcessItemAsync(
        MyItem item, IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Your processing logic here
        // Return true if processed successfully, false otherwise
        return true;
    }

    // Validation before processing the batch
    protected override Task<PrevalidationResult> BatchPrevalidation(
        IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Check preconditions (e.g., external service availability)
        // The goal is to avoid executing a batch that could fail 100% due to a third-party issue.
        return Task.FromResult(PrevalidationResult.Execute());
    }

    // Final cleanup when the worker stops
    protected override void FinalCleanUp()
    {
        // Safely dispose resources or perform other actions when the Task (NOT THE BATCH) shuts down.
    }

    // Wait interval between executions (in seconds)
    protected override int SleepIntervalInSeconds() => 60;

    // Degree of parallelism (number of items processed simultaneously)
    protected override int ParallelismDegree() => 5;

    // Error threshold percentage (0-100)
    protected override int ErrorThresholdPercentage() => 10;

    // If no items are found when retrieving a batch, the execution is not logged in storage.
    protected override bool SkipEmptyResults() => true;
}
```

> [!WARNING]
> Although in theory the wait time between executions can be set to DAYS, depending on where and how the task is deployed, it may stop unexpectedly. It is recommended to use a shorter sleep interval and perform a deeper validation in `BatchPrevalidation` to decide whether execution should occur when intervals are very long.

### 2. Register the Worker

In your `Program.cs` or `Startup.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure the background worker core
builder.Services.AddBackgroundWorker(options =>
{
    // Base BackgroundWorker library configuration
});

// Register your custom worker traditionally.
builder.Services.AddHostedService<MyWorker>();

var app = builder.Build();
app.Run();
```

## ⚙️ Configuration

### BackgroundWorkerConfiguration

Configure how long executions are retained in storage:

```csharp
new BackgroundWorkerConfiguration
{
    // Time in minutes to keep successful tasks (default: 60)
    MinutesToKeepSuccesTask = 60,

    // Time in minutes to keep failed tasks (default: 60)
    MinutesToKeepFailedTask = BackgroundWorkerConfiguration.DayToMinutes(7)
}
```

Available helper methods:
- `BackgroundWorkerConfiguration.HourToMinutes(int hours)` - Converts hours to minutes
- `BackgroundWorkerConfiguration.DayToMinutes(int days)` - Converts days to minutes

### Worker Parameters

Each custom worker must configure:

- **SleepIntervalInSeconds:** Wait time between executions (processing time is automatically discounted)
- **ParallelismDegree:** Maximum number of items processed in parallel (1 = sequential)
- **ErrorThresholdPercentage:** Maximum percentage of allowed errors before marking execution as failed (0-100)
- **SkipEmptyResults:** Determines whether empty batches should be stored in the audit log.

## 🏥 Health Checks

The library includes automatic health checks to monitor your workers:

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("background")
});
```

The health check reports:
- **Healthy:** All workers executed successfully
- **Degraded:** Some workers failed
- **Unhealthy:** No workers are healthy

### ⚠️ Important: What Health Checks Monitor

Health checks **DO NOT monitor the current state of the worker**, but rather **the result of its latest processing execution**. This means they evaluate the result of the following operations:

- `BatchPrevalidation()` - Batch pre-validation
- `GetBatchForProcessing()` - Retrieval of the item batch
- `ProcessItemAsync()` - Processing of each item
- `BatchPostProcesing()` - Batch post-processing

**Note about InitialSetupAsync():** If `InitialSetupAsync()` returns `false` and the worker does not start, the health check will display the result of a previous execution if one exists in storage. If no previous execution exists, the worker will not appear in the health check until it has completed at least one execution.

## 📊 Execution Storage

### In-Memory Implementation (Default)

By default, `InMemoryWorkerStorage` is used to store executions in memory.

> **⚠️ WARNING: DO NOT USE IN PRODUCTION**
>
> The `InMemoryWorkerStorage` implementation is **NOT designed for production environments** for the following reasons:
>
> 1. **Loss of history:** Restarting the application causes all execution history stored in memory to be lost.
>
> 2. **Incorrect execution intervals:** The worker uses history to calculate when it should execute again. Without persistence:
>    - A worker configured to run every 1 hour will restart immediately after a service restart
>    - Remaining calculated time is lost (e.g., if 40 minutes remained until the next execution, it will execute immediately)
>    - This may cause duplicate executions or system overload
>
> 3. **Inconsistent health checks:** Without persistent history, health checks cannot report the actual state between restarts.
>
> **Recommendation:** For production, implement a persistent version of `IWorkerStorage` using a database.

**Recommended use:** Only for local development and testing.

## 🔍 WorkerExecution

Each worker execution is logged with the following information:

```csharp
public class WorkerExecution
{
    public Guid Id { get; }                        // Unique ID
    public string WorkerName { get; }              // Worker name
    public DateTime ExecutionStartTime { get; }    // Start time (UTC)
    public DateTime ExecutionEndTime { get; }      // End time (UTC)
    public ExecutionStatus Status { get; }         // Execution status
    public int Success { get; }                    // Successfully processed items
    public int Failed { get; }                     // Failed items
    public string Message { get; }                 // Additional message
    public bool IsSuccessful { get; }              // Whether execution was successful
}
```

Possible statuses:
- `Null`: Initial state
- `Success`: Successfully completed
- `Failed`: Failed due to exceeding the error threshold
- `FailedPrevalidation`: Skipped due to failed pre-validation (equivalent to `PrevalidationResult.Skip(...)`)
- `NoItemsToProcess`: Completed but with no items to process

## 🎯 Advanced Features

### Batch Pre-validation

Implement `BatchPrevalidation` to verify conditions before processing. The result determines whether the batch executes, is skipped with logging, or skipped silently:

```csharp
protected override async Task<PrevalidationResult> BatchPrevalidation(
    IServiceScope serviceScope, CancellationToken stoppingToken)
{
    var service = serviceScope.ServiceProvider.GetRequiredService<MyService>();

    if (!await service.IsAvailable())
    {
        // Logged skip in storage: useful when the skip is abnormal or relevant for auditing
        return PrevalidationResult.Skip("External service unavailable");
    }

    if (!IsMonday())
    {
        // Silent skip: no record in storage.
        // Ideal for expected and recurring conditions that would otherwise generate unnecessary noise
        return PrevalidationResult.SkipSilently();
    }

    return PrevalidationResult.Execute();
}
```

| Result | Logged in storage | When to use |
|-----------|--------------------|-----------------|
| `PrevalidationResult.Execute()` | ✅ Yes | The batch should process normally |
| `PrevalidationResult.Skip(message)` | ✅ Yes | The skip is abnormal or relevant for auditing (e.g., external service down) |
| `PrevalidationResult.SkipSilently()` | ❌ No | The skip is expected and recurring (e.g., "only on Mondays") |

### Parallel Processing

Configure the degree of parallelism according to your needs:

```csharp
protected override int ParallelismDegree()
{
    // 1 = Sequential
    // >1 = Parallel processing
    return Environment.ProcessorCount; // Uses all available cores
}
```

### Automatic Cleanup

The `CleanBackGroundWorker` runs every 1 hour to clean old executions according to your configuration.
