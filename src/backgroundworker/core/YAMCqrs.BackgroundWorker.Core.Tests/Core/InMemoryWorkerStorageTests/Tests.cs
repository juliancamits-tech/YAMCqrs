using FluentAssertions;
using YAMCqrs.BackgroundWorker.Core.Domain;
using YAMCqrs.BackgroundWorker.Core.Implementation;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.InMemoryWorkerStorageTests;

public class Tests
{
    [Fact]
    public async Task AddAsync_Should_Store_Execution()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();
        var execution = new WorkerExecution("TestWorker");

        // Act
        await storage.AddAsync(execution, TestContext.Current.CancellationToken);
        var lastExecution = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);

        // Assert
        lastExecution.Should().NotBeNull();
        lastExecution!.WorkerName.Should().Be("TestWorker");
        lastExecution.Id.Should().Be(execution.Id);
    }

    [Fact]
    public async Task AddAsync_Should_Store_Multiple_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();
        var execution1 = new WorkerExecution("Worker1");
        var execution2 = new WorkerExecution("Worker2");
        var execution3 = new WorkerExecution("Worker1");

        // Act
        await storage.AddAsync(execution1, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution2, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution3, TestContext.Current.CancellationToken);

        // Assert
        var worker1Last = await storage.GetLastExecutionAsync("Worker1", TestContext.Current.CancellationToken);
        var worker2Last = await storage.GetLastExecutionAsync("Worker2", TestContext.Current.CancellationToken);

        worker1Last.Should().NotBeNull();
        worker1Last!.WorkerName.Should().Be("Worker1");
        worker2Last.Should().NotBeNull();
        worker2Last!.Id.Should().Be(execution2.Id);
    }

    [Fact]
    public async Task GetLastExecutionAsync_Should_Return_Null_When_No_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        // Act
        var result = await storage.GetLastExecutionAsync("NonExistentWorker", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLastExecutionAsync_Should_Return_Most_Recent_Execution()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();
        var execution1 = new WorkerExecution("TestWorker");
        var execution2 = new WorkerExecution("TestWorker");
        var execution3 = new WorkerExecution("TestWorker");

        // Act
        await storage.AddAsync(execution1, TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution2, TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution3, TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.WorkerName.Should().Be("TestWorker");
    }

    [Fact]
    public async Task GetLastExecutionByWorkerAsync_Should_Return_Last_Execution_Per_Worker()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var worker1Exec1 = new WorkerExecution("Worker1");
        var worker1Exec2 = new WorkerExecution("Worker1");
        var worker2Exec1 = new WorkerExecution("Worker2");
        var worker3Exec1 = new WorkerExecution("Worker3");

        await storage.AddAsync(worker1Exec1, TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker1Exec1.EndExecution(10);

        await storage.AddAsync(worker2Exec1, TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker2Exec1.EndExecution(10);

        await storage.AddAsync(worker1Exec2, TestContext.Current.CancellationToken);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker1Exec2.EndExecution(10);

        await storage.AddAsync(worker3Exec1, TestContext.Current.CancellationToken);
        worker3Exec1.EndExecution(10);

        // Act
        var results = await storage.GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(e => e.WorkerName == "Worker1" && e.Id == worker1Exec2.Id);
        results.Should().Contain(e => e.WorkerName == "Worker2" && e.Id == worker2Exec1.Id);
        results.Should().Contain(e => e.WorkerName == "Worker3" && e.Id == worker3Exec1.Id);
    }

    [Fact]
    public async Task GetLastExecutionByWorkerAsync_Should_Return_Empty_List_When_No_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        // Act
        var results = await storage.GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Remove_Old_Completed_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var oldExecution = new WorkerExecution("TestWorker");
        oldExecution.IncrementSuccessCount();
        oldExecution.EndExecution(10);

        await storage.AddAsync(oldExecution, TestContext.Current.CancellationToken);

        // Act - Clean executions older than now (should remove the old one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(1),
            dateForError: DateTime.UtcNow.AddHours(1), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Remove_Old_Failed_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var oldExecution = new WorkerExecution("TestWorker");
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.EndExecution(10);

        await storage.AddAsync(oldExecution, TestContext.Current.CancellationToken);

        // Act - Clean executions older than now (should remove the old one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(1),
            dateForError: DateTime.UtcNow.AddHours(1), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Remove_Old_Both_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var oldExecution = new WorkerExecution("TestWorker");
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.EndExecution(10);

        var oldExecution2 = new WorkerExecution("TestWorker2");
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.EndExecution(10);

        await storage.AddAsync(oldExecution, TestContext.Current.CancellationToken);
        await storage.AddAsync(oldExecution2, TestContext.Current.CancellationToken);

        // Act - Clean executions older than now (should remove the old one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(5),
            dateForError: DateTime.UtcNow.AddHours(5), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);
        var result2 = await storage.GetLastExecutionAsync("TestWorker2", TestContext.Current.CancellationToken);

        result.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Remove_Old_Only_Completed_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var oldExecution = new WorkerExecution("FailedWorker");
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.EndExecution(10);

        var oldExecution2 = new WorkerExecution("CompletedWorker");
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.EndExecution(10);

        await storage.AddAsync(oldExecution, TestContext.Current.CancellationToken);
        await storage.AddAsync(oldExecution2, TestContext.Current.CancellationToken);

        // Act - Clean executions older than now (should remove the old one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(1),
            dateForError: DateTime.UtcNow.AddHours(-1), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("FailedWorker", TestContext.Current.CancellationToken);
        var result2 = await storage.GetLastExecutionAsync("CompletedWorker", TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Remove_Old_Only_Failed_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var oldExecution = new WorkerExecution("FailedWorker");
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.IncrementFailedCount();
        oldExecution.EndExecution(10);

        var oldExecution2 = new WorkerExecution("CompletedWorker");
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.IncrementSuccessCount();
        oldExecution2.EndExecution(10);

        await storage.AddAsync(oldExecution, TestContext.Current.CancellationToken);
        await storage.AddAsync(oldExecution2, TestContext.Current.CancellationToken);

        // Act - Clean executions older than now (should remove the old one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(-1),
            dateForError: DateTime.UtcNow.AddHours(1), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("FailedWorker", TestContext.Current.CancellationToken);
        var result2 = await storage.GetLastExecutionAsync("CompletedWorker", TestContext.Current.CancellationToken);


        result2.Should().NotBeNull();
        result.Should().BeNull();
    }


    [Fact]
    public async Task CleanStorageAsync_Should_Not_Remove_Recent_Executions()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var recentExecution = new WorkerExecution("TestWorker");
        recentExecution.IncrementSuccessCount();
        recentExecution.EndExecution(10);

        await storage.AddAsync(recentExecution, TestContext.Current.CancellationToken);

        // Act - Clean executions older than 1 hour ago (should keep the recent one)
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddHours(-1),
            dateForError: DateTime.UtcNow.AddHours(-1), TestContext.Current.CancellationToken);

        var result = await storage.GetLastExecutionAsync("TestWorker", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recentExecution.Id);
    }

    [Fact]
    public async Task CleanStorageAsync_Should_Handle_Different_Cutoff_Dates_For_Success_And_Errors()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();

        var successExecution = new WorkerExecution("SuccessWorker");
        successExecution.IncrementSuccessCount();
        successExecution.EndExecution(10);

        var failedExecution = new WorkerExecution("FailedWorker");
        failedExecution.IncrementFailedCount();
        failedExecution.EndExecution(0); // Will fail with any errors

        await storage.AddAsync(successExecution, TestContext.Current.CancellationToken);
        await storage.AddAsync(failedExecution, TestContext.Current.CancellationToken);

        // Act - Keep failed executions longer than successful ones
        await storage.CleanStorageAsync(
            dateForCompleted: DateTime.UtcNow.AddSeconds(1), // Clean old successful
            dateForError: DateTime.UtcNow.AddHours(-1), TestContext.Current.CancellationToken); // Keep recent errors


        var allExecutions = await storage.GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);
        allExecutions.Should().NotBeNull();
    }

    [Fact]
    public async Task Storage_Should_Be_ThreadSafe()
    {
        // Arrange
        var storage = new InMemoryWorkerStorage();
        var tasks = new List<Task>();
        const int taskCount = 100;

        // Act - Add many executions concurrently
        for (int i = 0; i < taskCount; i++)
        {
            var workerName = $"Worker{i % 10}"; // 10 different workers
            tasks.Add(Task.Run(async () =>
            {
                var execution = new WorkerExecution(workerName);
                await storage.AddAsync(execution, TestContext.Current.CancellationToken);
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);

        // Assert
        var lastExecutions = await storage.GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);
        lastExecutions.Should().HaveCount(10); // Should have 10 unique workers
    }

}
