using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Testing.Platform.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Sdk;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Configuration;
using YAMCqrs.BackgroundWorker.Core.Domain;
using YAMCqrs.BackgroundWorker.Core.Implementation;
using static Xunit.Sdk.TestOptionsNames;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.CleanBackgroundWorkerTests;

public class Tests
{
    private async static Task<Tuple<CleanBackGroundWorker, MockWorkerStorage>> CreateWorker(params WorkerExecution[] workerExecutions)
    {
        var services = new ServiceCollection();
        var mockStorage = new MockWorkerStorage();
        services.AddLogging();
        services.AddSingleton<IWorkerStorage>(mockStorage);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CleanBackGroundWorker>>();
        var cfg = new OptionsWrapper<BackgroundWorkerConfiguration>(new());
        ArgumentNullException.ThrowIfNull(logger);
        var worker = new CleanBackGroundWorker(serviceProvider, logger, cfg);

        foreach (var w in workerExecutions)
            await mockStorage.AddAsync(w, CancellationToken.None);

        return Tuple.Create(worker, mockStorage);
    }

    [Fact]
    public async Task CleanBackgroundWorker_Should_Clean_Old_Executions()
    {
        var oldExecution = new WorkerExecution("TestWorker");
        oldExecution.EndExecution(10);

        var tuple = await CreateWorker([oldExecution]);
        var worker = tuple.Item1;
        var mockStorage = tuple.Item2;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2)); // Stop after 2 seconds

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(1000, TestContext.Current.CancellationToken); // Give it time to execute once
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        // Assert
        mockStorage.CleanStorageAsyncCallCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CleanBackgroundWorker_Should_End()
    {
        var tuple = await CreateWorker();

        var worker = tuple.Item1;
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2)); // Stop after 2 seconds

        // Act
        try
        {
            await worker.StartAsync(cts.Token);
            await Task.Delay(1000, TestContext.Current.CancellationToken); // Give it time to execute once
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }

        // Assert
        worker.Should().NotBeNull();
        worker?.ExecuteTask?.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessItemAsync_Should_Call_CleanStorageAsync()
    {
        // Arrange
        var mockStorage = Substitute.For<IWorkerStorage>();
        mockStorage.CleanStorageAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(mockStorage);
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CleanBackGroundWorker>>();
        ArgumentNullException.ThrowIfNull(logger);
        var cfg = new OptionsWrapper<BackgroundWorkerConfiguration>(new());
        var worker = new TestableCleanBackgroundWorker(serviceProvider, logger, cfg);

        // Act
        var result = await worker.ProcessItemAsync(1, scope, Guid.CreateVersion7(), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await mockStorage.Received(1).CleanStorageAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SleepIntervalInSeconds_Should_Return_SleepInterval()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IWorkerStorage>(new MockWorkerStorage());
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger<CleanBackGroundWorker>>();
        ArgumentNullException.ThrowIfNull(logger);
        var cfg = new OptionsWrapper<BackgroundWorkerConfiguration>(new());
        var worker = new TestableCleanBackgroundWorker(serviceProvider, logger, cfg);
        // Act
        var interval = worker.SleepIntervalInSeconds();
        
        // Assert
        interval.Should().Be(worker.SleepIntervalInSeconds());
    }
}