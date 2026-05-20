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
        await storage.AddAsync(execution);
        var lastExecution = await storage.GetLastExecutionAsync("TestWorker");

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
        await storage.AddAsync(execution1);
        await storage.AddAsync(execution2);
        await storage.AddAsync(execution3);

        // Assert
        var worker1Last = await storage.GetLastExecutionAsync("Worker1");
        var worker2Last = await storage.GetLastExecutionAsync("Worker2");

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
        var result = await storage.GetLastExecutionAsync("NonExistentWorker");

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
        await storage.AddAsync(execution1);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution2);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        await storage.AddAsync(execution3);

        var result = await storage.GetLastExecutionAsync("TestWorker");

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

        await storage.AddAsync(worker1Exec1);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker1Exec1.EndExecution(10);

        await storage.AddAsync(worker2Exec1);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker2Exec1.EndExecution(10);

        await storage.AddAsync(worker1Exec2);
        await Task.Delay(10, TestContext.Current.CancellationToken);
        worker1Exec2.EndExecution(10);

        await storage.AddAsync(worker3Exec1);
        worker3Exec1.EndExecution(10);

        // Act
        var results = await storage.GetLastExecutionByWorkerAsync();

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
        var results = await storage.GetLastExecutionByWorkerAsync();

        // Assert
        results.Should().BeEmpty();
    }

}
