using Microsoft.Extensions.Logging;
using YAMCqrs.Core;
using YAMCqrs.Core.Abstractions.Queries;

namespace Test.Application.Interceptors;

/// <summary>
/// Example of a query interceptor that logs the execution of queries using source-generated logging methods.
/// </summary>
/// <typeparam name="TQuery">All Queries.</typeparam>
/// <typeparam name="TResult">With all Results.</typeparam>
/// <param name="logger">Logger for out the message.</param>
internal sealed partial class LogginQueryInterceptor<TQuery, TResult>(ILogger<LogginQueryInterceptor<TQuery, TResult>> logger) : QueryInterceptorBase<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public override InterceptorLayer Layer => InterceptorLayer.Logging;

    public override int Order => 1;

    public override Task OnAfterAsync(TQuery query, Result<TResult> result, CancellationToken cancellationToken)
    {
        this.LogEndQuery(query.GetType().Name);
        return Task.CompletedTask;
    }

    public override Task OnBeforeAsync(TQuery query, CancellationToken cancellationToken)
    {
        this.LogStartQuery(query.GetType().Name);
        return Task.CompletedTask;
    }

    public override Task OnErrorAsync(TQuery query, Exception exception, CancellationToken cancellationToken)
    {
        this.LogErrorQuery(query.GetType().Name);
        return Task.CompletedTask;
    }

    #region Logger
    [LoggerMessage(Level = LogLevel.Information, Message = "Start Query {QueryName}")]
    private partial void LogStartQuery(string queryName);

    [LoggerMessage(Level = LogLevel.Information, Message = "End Query {QueryName}")]
    private partial void LogEndQuery(string queryName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error Query {QueryName}")]
    private partial void LogErrorQuery(string queryName);
    #endregion
}
