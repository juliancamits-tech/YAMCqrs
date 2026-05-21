using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.BackgroundWorker.Core.Domain;
using static YAMCqrs.BackgroundWorker.Core.Domain.WorkerExecution;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.WorkerExecutionTests;

public class Tests
{
    [Fact]
    public void WorkerExecution_FullConstructor()
    {
        var id = Guid.CreateVersion7();
        var name = "Pepito";
        var time = DateTime.Now;
        var time2 = DateTime.UtcNow;
        var status = ExecutionStatus.Null;
        var number = Random.Shared.Next();
        var number2 = Random.Shared.Next();
        var worker = new WorkerExecution(id, name, time, time2, status, number, number2, string.Empty);

        Assert.Equal(id, worker.Id);
        Assert.Equal(name, worker.WorkerName);
        Assert.Equal(time, worker.ExecutionStartTime);
        Assert.Equal(time2, worker.ExecutionEndTime);
        Assert.Equal(status, worker.Status);
        Assert.Equal(number, worker.Success);
        Assert.Equal(number2, worker.Failed);
        Assert.Equal(string.Empty, worker.Message);
    }

    [Fact]
    public async Task Should_Throw_When_Cancelled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var execution = new WorkerExecution
        (
            Guid.CreateVersion7(), "oeo",
            DateTime.UtcNow,
            DateTime.UtcNow,
            ExecutionStatus.Null, 10, 10, string.Empty
        );

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            execution.DelayUntilNextExecution(10, cts.Token));
    }

}
