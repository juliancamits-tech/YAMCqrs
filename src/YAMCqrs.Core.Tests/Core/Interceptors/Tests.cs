using System;
using System.Collections.Generic;
using System.Text;
using Test.Application.Interceptors;
using YAMCqrs.Core.Abstractions.Commands;
using YAMCqrs.Core.Abstractions.Queries;

namespace YAMCqrs.Core.Tests.Core.Interceptors;

public class Tests
{
    private static ICommandInterceptor<TestCommand, string> CreateInterceptor()
    {
        var interceptor = new TestCommandInterceptor2<TestCommand, string>();
        return interceptor;
    }
    private static IQueryInterceptor<TestQuery, string> CreateInterceptor2()
    {
        var interceptor = new TestQueryInterceptor2<TestQuery, string>();
        return interceptor;
    }

    [Fact]
    public void CommandInterceptorBase_Default_Layer_Order()
    {
        var interceptor =new TestCommandInterceptor<TestCommand,string>();

        Assert.Equal(InterceptorLayer.Application, interceptor.Layer);
        Assert.Equal(100, interceptor.Order);
    }

    [Fact]
    public void CommandInterface_Default_Layer_Order()
    {
        var interceptor = CreateInterceptor();

        Assert.Equal(InterceptorLayer.Application, interceptor.Layer);
        Assert.Equal(100, interceptor.Order);
    }

    [Fact]
    public void QueryInterceptorBase_Default_Layer_Order()
    {
        var interceptor = new TestQueryInterceptor<TestQuery, string>();

        Assert.Equal(InterceptorLayer.Application, interceptor.Layer);
        Assert.Equal(100, interceptor.Order);
    }

    [Fact]
    public void QueryInterface_Default_Layer_Order()
    {
        var interceptor = CreateInterceptor2();

        Assert.Equal(InterceptorLayer.Application, interceptor.Layer);
        Assert.Equal(100, interceptor.Order);
    }
}