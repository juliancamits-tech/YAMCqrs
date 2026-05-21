namespace YAMCqrs.Core.Tests.Core.ResultTests;

public class Tests
{
    [Fact]
    public void Success()
    {
        var number = Random.Shared.Next();
        var r = Result.Success(number);

        Assert.True(r.IsSuccess);
        Assert.Empty(r.Error);
        Assert.Equal(number, r.Value);
    }

    [Fact]
    public void Ok()
    {
        var number = Random.Shared.Next();
        var r = Result.Ok(number);

        Assert.True(r.IsSuccess);
        Assert.Empty(r.Error);
        Assert.Equal(number, r.Value);
    }

    [Fact]
    public void FromTResult()
    {
        var number = Random.Shared.Next();
        var r = Result.FromTResult(number);

        Assert.True(r.IsSuccess);
        Assert.Empty(r.Error);
        Assert.Equal(number, r.Value);
    }

    [Fact]
    public void ToResult()
    {
        var number = Random.Shared.Next();
        var r = Result.ToResult(number);

        Assert.True(r.IsSuccess);
        Assert.Empty(r.Error);
        Assert.Equal(number, r.Value);
    }

    [Fact]
    public void Failure()
    {
        var msg = "Mi Error";
        var r = Result.Failure<object>(msg);

        Assert.True(r.IsFailure);
        Assert.NotEmpty(r.Error);
        Assert.Null(r.Value);
    }

    [Fact]
    public void Fail()
    {
        var msg = "Mi Error";
        var r = Result.Fail<object>(msg);

        Assert.True(r.IsFailure);
        Assert.NotEmpty(r.Error);
        Assert.Null(r.Value);
    }

    [Fact]
    public void FromString()
    {
        var msg = "Mi Error";
        var r = Result.FromString<object>(msg);

        Assert.True(r.IsFailure);
        Assert.NotEmpty(r.Error);
        Assert.Null(r.Value);
    }

}
