using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Domain;
using YAMCqrs.BackgroundWorker.Core.Implementation;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.YABackgroundWorkerTests;

public class Tests
{
    [Fact]
    public async Task ExecuteAsync_Should_Call_InitialSetup_Before_First_Execution()
    {
        // Arrange
        var initialSetupEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();
        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) =>
            {
                initialSetupEvent.Set();
                return Task.FromResult(true);
            },
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(null),
            sleepInterval: 1000
        );

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await worker.StartAsync(cts.Token);
        var wasInitialSetupCalled = initialSetupEvent.Wait(TimeSpan.FromSeconds(1), cts.Token);
        await worker.StopAsync(cts.Token);

        // Assert
        wasInitialSetupCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Stop_If_InitialSetup_Returns_False()
    {
        // Arrange
        var getBatchEvent = new ManualResetEventSlim(false);
        var initialSetupEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();
        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) =>
            {
                initialSetupEvent.Set();
                return Task.FromResult(false);
            },
            getBatch: (scope, ct) =>
            {
                getBatchEvent.Set();
                return Task.FromResult<IEnumerable<string>?>(["item1"]);
            }
        );

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await worker.StartAsync(cts.Token);
        initialSetupEvent.Wait(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken); // Give time for ExecuteAsync to return
        await worker.StopAsync(CancellationToken.None);

        // Assert
        getBatchEvent.IsSet.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Process_Batch_Items_Successfully()
    {
        // Arrange
        var processedItems = new List<string>();
        var processingCompleteEvent = new ManualResetEventSlim(false);
        var expectedCount = 3;
        var services = CreateServiceCollection();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["item1", "item2", "item3"]),
            processItem: (item, scope, ct) =>
            {
                lock (processedItems)
                {
                    processedItems.Add(item);
                    if (processedItems.Count == expectedCount)
                    {
                        processingCompleteEvent.Set();
                    }
                }
                return Task.FromResult(true);
            },
            sleepInterval: 10000
        );

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        // Act
        await worker.StartAsync(cts.Token);
        var processingCompleted = processingCompleteEvent.Wait(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        processingCompleted.Should().BeTrue();
        processedItems.Should().HaveCount(3);
        processedItems.Should().Contain(["item1", "item2", "item3"]);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Skip_Batch_When_Prevalidation_Fails()
    {
        // Arrange
        var processItemEvent = new ManualResetEventSlim(false);
        var prevalidationEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            batchPrevalidation: (scope, ct) =>
            {
                prevalidationEvent.Set();
                return Task.FromResult(PrevalidationResult.Skip("Service unavailable"));
            },
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["item1"]),
            processItem: (item, scope, ct) =>
            {
                processItemEvent.Set();
                return Task.FromResult(true);
            },
            sleepInterval: 10000
        );

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await worker.StartAsync(cts.Token);
        prevalidationEvent.Wait(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await Task.Delay(200, TestContext.Current.CancellationToken); // Give time to confirm processItem is NOT called
        await worker.StopAsync(CancellationToken.None);

        // Assert
        processItemEvent.IsSet.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Skip_When_Batch_Is_Empty()
    {
        // Arrange
        var processItemEvent = new ManualResetEventSlim(false);
        var getBatchEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            getBatch: (scope, ct) =>
            {
                getBatchEvent.Set();
                return Task.FromResult<IEnumerable<string>?>([]);
            },
            processItem: (item, scope, ct) =>
            {
                processItemEvent.Set();
                return Task.FromResult(true);
            },
            sleepInterval: 10000
        );

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await worker.StartAsync(cts.Token);
        getBatchEvent.Wait(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await Task.Delay(200, TestContext.Current.CancellationToken); // Give time to confirm processItem is NOT called
        await worker.StopAsync(CancellationToken.None);

        // Assert
        processItemEvent.IsSet.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Record_Success_And_Failed_Counts()
    {
        // Arrange
        var services = CreateServiceCollection();

        using var cts = new CancellationTokenSource();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["success1", "fail1", "success2"]),
            processItem: (item, scope, ct) =>
            {
                var result = item.StartsWith("success");
                return Task.FromResult(result);
            },
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                cts.Cancel();
                return Task.CompletedTask;
            },
            sleepInterval: 100,
            parallelismDegree: 1
        );

        // Act
        await worker.StartAsync(cts.Token);
        var processingCompleted = Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(10));

        // Assert
        processingCompleted.Should().BeTrue();
        var storage = services.Item1.GetRequiredService<IWorkerStorage>();
        var lastExecution = await storage.GetLastExecutionAsync(worker.GetType().FullName!, TestContext.Current.CancellationToken);
        lastExecution.Should().NotBeNull();
        lastExecution!.Success.Should().Be(2);
        lastExecution!.Failed.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Process_Items_In_Parallel()
    {
        // Arrange
        var services = CreateServiceCollection();
        var processingItems = new HashSet<string>();
        var maxConcurrent = 0;
        var lockObj = new object();
        var processingCompleteEvent = new ManualResetEventSlim(false);
        var processedCount = 0;
        var expectedCount = 4;

        using var cts = new CancellationTokenSource();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["item1", "item2", "item3", "item4"]),
            processItem: async (item, scope, ct) =>
            {
                lock (lockObj)
                {
                    processingItems.Add(item);
                    maxConcurrent = Math.Max(maxConcurrent, processingItems.Count);
                }
                await Task.Delay(100, TestContext.Current.CancellationToken); // Longer delay to ensure overlap
                lock (lockObj)
                {
                    processingItems.Remove(item);
                    if (Interlocked.Increment(ref processedCount) == expectedCount)
                    {
                        processingCompleteEvent.Set();
                    }
                }
                return true;
            },
            parallelismDegree: 3,
            sleepInterval: 10000
        );

        // Act
        await worker.StartAsync(cts.Token);
        var processingCompleted = processingCompleteEvent.Wait(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        processingCompleted.Should().BeTrue();
        maxConcurrent.Should().BeGreaterThan(1, "items should be processed in parallel");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exceptions_In_ProcessItem()
    {
        // Arrange
        var services = CreateServiceCollection();

        using var cts = new CancellationTokenSource();
        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["item1", "throw", "item2"]),
            processItem: (item, scope, ct) =>
            {
                if (item == "throw")
                    throw new InvalidOperationException("Test exception");
                return Task.FromResult(true);
            },
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                cts.Cancel();
                return Task.CompletedTask;
            },
            parallelismDegree: 1,
            sleepInterval: 10000
        );

        // Act
        await worker.StartAsync(cts.Token);
        var processingCompleted = Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(10));

        // Assert
        processingCompleted.Should().BeTrue();
        var storage = services.Item1.GetRequiredService<IWorkerStorage>();
        var lastExecution = await storage.GetLastExecutionAsync(worker.GetType().FullName!, TestContext.Current.CancellationToken);
        lastExecution.Should().NotBeNull();
        lastExecution!.Failed.Should().Be(1);
        lastExecution!.Success.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Call_FinalCleanUp_On_Stop()
    {
        // Arrange
        var cleanupEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();

        using var cts = new CancellationTokenSource();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) =>
            {
                // ✅ Cancelar inmediatamente después del setup
                cts.Cancel();
                return Task.FromResult(true);
            },
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(null),
            finalCleanup: () => cleanupEvent.Set(),
            sleepInterval: 100
        );

        // Act
        await worker.StartAsync(cts.Token);

        // ✅ Esperar a que el token esté cancelado
        cts.Token.WaitHandle.WaitOne();

        // ✅ Esperar a que FinalCleanUp se ejecute
        var wasCleanupCalled = cleanupEvent.Wait(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        await worker.StopAsync(CancellationToken.None);

        // Assert
        wasCleanupCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Store_Execution_Results()
    {
        // Arrange
        var services = CreateServiceCollection();
        

        // ✅ Token sin timeout, lo cancelaremos manualmente desde processItem
        using var cts = new CancellationTokenSource();
        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            initialSetup: (scope, ct) => Task.FromResult(true),
            batchPrevalidation: (scope, ct) => { return Task.FromResult(PrevalidationResult.Execute()); },
            getBatch: (scope, ct) => Task.FromResult<IEnumerable<string>?>(["item1"]),
            processItem: (item, scope, ct) => Task.FromResult(true),
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                cts.Cancel();
                return Task.CompletedTask;
            },
            sleepInterval: 100
        );

        // Act
        await worker.StartAsync(cts.Token);
        var isStoped = Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(10));

        // Assert
        isStoped.Should().BeTrue("the worker should have stopped after cancellation");
        var storage = services.Item1.GetRequiredService<IWorkerStorage>();
        var executions = await storage.GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);
        executions.Should().NotBeEmpty();
        executions.Should().Contain(e => e.WorkerName == worker.GetType().FullName);
    }

    [Fact]
    public void InternalErrorThresholdPercentage_Should_Clamp_Values()
    {
        // Arrange & Act
        var services = CreateServiceCollection();
        var workerNegative = new TestableBackgroundWorker(services.Item1, services.Item2, errorThreshold: -10);
        var workerOver100 = new TestableBackgroundWorker(services.Item1, services.Item2, errorThreshold: 150);
        var workerValid = new TestableBackgroundWorker(services.Item1, services.Item2, errorThreshold: 50);

        // Assert - We can verify by checking execution results after processing
        workerNegative.PErrorThresholdPercentage().Should().Be(-10);
        workerOver100.PErrorThresholdPercentage().Should().Be(150);
        workerValid.PErrorThresholdPercentage().Should().Be(50);
    }

    [Fact]
    public void InternalSleepInterval_Should_Be_At_Least_1_Second()
    {
        // Arrange & Act
        var services = CreateServiceCollection();
        var workerZero = new TestableBackgroundWorker(services.Item1, services.Item2, sleepInterval: 0);
        var workerNegative = new TestableBackgroundWorker(services.Item1, services.Item2, sleepInterval: -100);
        var workerValid = new TestableBackgroundWorker(services.Item1, services.Item2, sleepInterval: 5);

        // Assert
        workerZero.PSleepIntervalInSeconds().Should().Be(0);
        workerNegative.PSleepIntervalInSeconds().Should().Be(-100);
        workerValid.PSleepIntervalInSeconds().Should().Be(5);
    }

    [Fact]
    public void InternalParallelismDegree_Should_Be_At_Least_1()
    {
        // Arrange & Act
        var services = CreateServiceCollection();
        var workerZero = new TestableBackgroundWorker(services.Item1, services.Item2, parallelismDegree: 0);
        var workerNegative = new TestableBackgroundWorker(services.Item1, services.Item2, parallelismDegree: -5);
        var workerValid = new TestableBackgroundWorker(services.Item1, services.Item2, parallelismDegree: 10);

        // Assert
        workerZero.PParallelismDegree().Should().Be(0);
        workerNegative.PParallelismDegree().Should().Be(-5);
        workerValid.PParallelismDegree().Should().Be(10);
    }


    [Fact]
    public async Task ExecuteAsync_Should_Persist_Execution_When_Prevalidation_Returns_Skip()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var cts = new CancellationTokenSource();

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            batchPrevalidation: (scope, ct) =>
                Task.FromResult(PrevalidationResult.Skip("Servicio no disponible")),
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                // Cancelar luego del post-processing para asegurar que AddAsync ya ocurrió
                cts.Cancel();
                return Task.CompletedTask;
            },
            sleepInterval: 10000
        );

        // Act
        await worker.StartAsync(cts.Token);
        Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(5));

        // Assert
        var storage = services.Item1.GetRequiredService<IWorkerStorage>();
        var lastExecution = await storage.GetLastExecutionAsync(worker.GetType().FullName!, TestContext.Current.CancellationToken);
        lastExecution.Should().NotBeNull();
        lastExecution!.Status.Should().Be(WorkerExecution.ExecutionStatus.FailedPrevalidation);
        lastExecution!.Message.Should().Be("Servicio no disponible");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Call_PostProcessing_When_Prevalidation_Returns_Skip()
    {
        // Arrange
        var postProcessingEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            batchPrevalidation: (scope, ct) =>
                Task.FromResult(PrevalidationResult.Skip("Servicio no disponible")),
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                postProcessingEvent.Set();
                return Task.CompletedTask;
            },
            sleepInterval: 10000
        );

        // Act
        await worker.StartAsync(cts.Token);
        var wasPostProcessingCalled = postProcessingEvent.Wait(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        wasPostProcessingCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Persist_Execution_When_Prevalidation_Returns_SkipSilently()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var cts = new CancellationTokenSource();
        var iterationCount = 0;

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            batchPrevalidation: (scope, ct) =>
            {
                // Cancelar luego de la primera iteración para no loopar indefinidamente
                if (Interlocked.Increment(ref iterationCount) == 1)
                    cts.Cancel();
                return Task.FromResult(PrevalidationResult.SkipSilently());
            },
            sleepInterval: 10000
        );

        // Act
        await worker.StartAsync(cts.Token);
        Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(5));

        // Assert
        var storage = services.Item1.GetRequiredService<IWorkerStorage>();
        var lastExecution = await storage.GetLastExecutionAsync(worker.GetType().FullName!, TestContext.Current.CancellationToken);
        lastExecution.Should().BeNull("SkipSilently no debe dejar registro en storage");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Not_Call_PostProcessing_When_Prevalidation_Returns_SkipSilently()
    {
        // Arrange
        var postProcessingEvent = new ManualResetEventSlim(false);
        var services = CreateServiceCollection();
        using var cts = new CancellationTokenSource();
        var iterationCount = 0;

        var worker = new TestableBackgroundWorker(
            services.Item1, services.Item2,
            batchPrevalidation: (scope, ct) =>
            {
                if (Interlocked.Increment(ref iterationCount) == 1)
                    cts.Cancel();
                return Task.FromResult(PrevalidationResult.SkipSilently());
            },
            batchPostProcesing: (scope, workerExecution, ct) =>
            {
                postProcessingEvent.Set();
                return Task.CompletedTask;
            },
            sleepInterval: 10000

        );

        // Act
        await worker.StartAsync(cts.Token);
        Helper.WaitForWorkerCompletion(cts.Token, worker.ExecuteTask, TimeSpan.FromSeconds(5));
        await Task.Delay(200, TestContext.Current.CancellationToken); // Margen para confirmar que post-processing no se dispara
        await worker.StopAsync(CancellationToken.None);

        // Assert
        postProcessingEvent.IsSet.Should().BeFalse();
    }

    private static Tuple<ServiceProvider, ILogger<TestableBackgroundWorker>> CreateServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IWorkerStorage, InMemoryWorkerStorage>();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<TestableBackgroundWorker>>();
        ArgumentNullException.ThrowIfNull(logger);
        return Tuple.Create(provider, logger);
    }
}
