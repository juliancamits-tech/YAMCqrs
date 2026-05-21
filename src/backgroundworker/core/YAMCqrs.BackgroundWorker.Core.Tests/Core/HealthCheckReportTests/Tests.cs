using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.CustomHealthCheck;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.HealthCheckReportTests;

public class Tests
{
    private static IWorkerStorage GetSubstitute(List<WorkerExecution> executions)
    {
        var mockStorage = Substitute.For<IWorkerStorage>();
        mockStorage.GetLastExecutionByWorkerAsync(Arg.Any<CancellationToken>()).Returns(executions);

        return mockStorage;
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Return_Healthy_When_All_Workers_Successful()
    {
        // Arrange
        var executions = new List<WorkerExecution>
        {
            CreateExecution("Worker1", isSuccessful: true),
            CreateExecution("Worker2", isSuccessful: true),
            CreateExecution("Worker3", isSuccessful: true)
        };

        var healthCheck = new HealthCheckReport(GetSubstitute(executions));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("Healthy: 3");
        result.Description.Should().Contain("Unhealthy: 0");
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Return_Unhealthy_When_All_Workers_Failed()
    {
        // Arrange
        var executions = new List<WorkerExecution>
        {
            CreateExecution("Worker1", isSuccessful: false),
            CreateExecution("Worker2", isSuccessful: false)
        };

        var healthCheck = new HealthCheckReport(GetSubstitute(executions));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Healthy: 0");
        result.Description.Should().Contain("Unhealthy: 2");
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Return_Degraded_When_Some_Workers_Failed()
    {
        // Arrange
        var executions = new List<WorkerExecution>
        {
            CreateExecution("Worker1", isSuccessful: true),
            CreateExecution("Worker2", isSuccessful: false),
            CreateExecution("Worker3", isSuccessful: true)
        };

        var healthCheck = new HealthCheckReport(GetSubstitute(executions));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Healthy: 2");
        result.Description.Should().Contain("Unhealthy: 1");
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Handle_Empty_Worker_List()
    {
        // Arrange

        var healthCheck = new HealthCheckReport(GetSubstitute([]));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Healthy: 0");
        result.Description.Should().Contain("Unhealthy: 0");
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Include_All_Workers_In_Details()
    {
        // Arrange
        var executions = new List<WorkerExecution>
        {
            CreateExecution("Worker1", isSuccessful: true),
            CreateExecution("Worker2", isSuccessful: false)
        };

        var mockStorate = GetSubstitute(executions);
        var healthCheck = new HealthCheckReport(mockStorate);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Description.Should().NotBeNullOrEmpty();
        await mockStorate.Received(1).GetLastExecutionByWorkerAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(10, 0, HealthStatus.Healthy)] //Todo sano
    [InlineData(0, 10, HealthStatus.Unhealthy)] //Todo roto
    [InlineData(5, 10, HealthStatus.Unhealthy)] //Mas roto que sano
    [InlineData(10, 5, HealthStatus.Degraded)] //Mas sano que roto
    public async Task CheckHealthAsync_Should_Determine_Correct_Status_Based_On_Counts(
        int healthyCount, int unhealthyCount, HealthStatus expectedStatus)
    {
        // Arrange
        var executions = new List<WorkerExecution>();

        for (int i = 0; i < healthyCount; i++)
            executions.Add(CreateExecution($"HealthyWorker{i}", isSuccessful: true));

        for (int i = 0; i < unhealthyCount; i++)
            executions.Add(CreateExecution($"UnhealthyWorker{i}", isSuccessful: false));

        var healthCheck = new HealthCheckReport(GetSubstitute(executions));
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(expectedStatus);
    }

    // Helper method to create test WorkerExecution instances
    private static WorkerExecution CreateExecution(string workerName, bool isSuccessful)
    {
        var execution = new WorkerExecution(workerName);

        if (isSuccessful)
        {
            execution.IncrementSuccessCount();
            execution.IncrementSuccessCount();
            execution.IncrementSuccessCount();
        }
        else
        {
            execution.IncrementFailedCount();
            execution.IncrementFailedCount();
            execution.IncrementFailedCount();
        }

        execution.EndExecution(errorThresholdPercentage: 10);
        return execution;
    }
}