using System;
using System.Collections.Generic;
using System.Text;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.PrevalidationResultTests;

public class Tests
{
    [Fact]
    public void PrevalidationResult_Skip()
    {
        var msg = "mensaje random";
        var r = PrevalidationResult.Skip(msg);

        Assert.Equal(msg, r.Message);
        Assert.Equal(PrevalidationOutcome.Skip, r.Outcome);
    }

    [Fact]
    public void PrevalidationResult_SkipSilently()
    {
        var r = PrevalidationResult.SkipSilently();

        Assert.Null(r.Message);
        Assert.Equal(PrevalidationOutcome.SkipSilently, r.Outcome);
    }

    [Fact]
    public void PrevalidationResult_Execute()
    {
        var r = PrevalidationResult.Execute();

        Assert.Null(r.Message);
        Assert.Equal(PrevalidationOutcome.Execute, r.Outcome);
    }

    [Fact]
    public void PrevalidationResult_Valid()
    {
        var r = PrevalidationResult.Valid();

        Assert.Null(r.Message);
        Assert.Equal(PrevalidationOutcome.Execute, r.Outcome);
    }
}
