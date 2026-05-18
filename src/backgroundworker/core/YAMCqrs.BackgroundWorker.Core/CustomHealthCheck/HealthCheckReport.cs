using Microsoft.Extensions.Diagnostics.HealthChecks;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Core.CustomHealthCheck;

/// <summary>
/// Health check implementation that reports on the status of all background workers.
/// Checks the last execution of each worker to determine overall health.
/// </summary>
/// <remarks>
/// <para><strong>What Health Checks Monitor:</strong></para>
/// <para>Health checks evaluate the result of the last <strong>processing execution</strong>, which includes:</para>
/// <list type="bullet">
/// <item><description><see cref="YABackgroundWorker{TWorkItem}.BatchPrevalidation"/> - Pre-validation before processing</description></item>
/// <item><description><see cref="YABackgroundWorker{TWorkItem}.GetBatchForProcessing"/> - Batch retrieval</description></item>
/// <item><description><see cref="YABackgroundWorker{TWorkItem}.ProcessItemAsync"/> - Individual item processing</description></item>
/// <item><description><see cref="YABackgroundWorker{TWorkItem}.BatchPostProcesing"/> - Post-processing operations</description></item>
/// </list>
/// <para><strong>Important:</strong> Health checks do NOT monitor <see cref="YABackgroundWorker{TWorkItem}.InitialSetupAsync"/>. 
/// If InitialSetupAsync fails and the worker doesn't start, the health check will report the result of the previous execution 
/// (if one exists in storage). Workers without any execution history will not appear in health check results.</para>
/// </remarks>
internal class HealthCheckReport(IWorkerStorage workerStorage) : IHealthCheck
{
    /// <summary>
    /// Performs the health check by examining the last execution of each worker.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - Healthy if all workers succeeded
    /// - Degraded if some workers failed
    /// - Unhealthy if no healthy workers exist
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lastExecutions = await workerStorage.GetLastExecutionByWorkerAsync();

        var countHealthy = 0;
        var countUnhealthy = 0;
        var dictDetails = new Dictionary<string, Report>();
        foreach (var execution in lastExecutions)
        {
            if (execution.IsSuccessful)
            {
                countHealthy++;
            }
            else
            {
                countUnhealthy++;
            }
            dictDetails.Add(execution.WorkerName, new Report(execution));
        }

        HealthStatus status = HealthStatus.Healthy;
        if (countHealthy == 0 || countUnhealthy > countHealthy)
        {
            status = HealthStatus.Unhealthy;
        }
        else if (countUnhealthy > 0)
        {
            status = HealthStatus.Degraded;
        }

        return new HealthCheckResult(
            status: status,
            description: $"Healthy: {countHealthy} - Unhealthy: {countUnhealthy}"
        );
    }


    /// <summary>
    /// Represents detailed health information for a single worker execution.
    /// </summary>
    /// <remarks>
    /// Creates a health report from a worker execution.
    /// </remarks>
    /// <param name="execution">The worker execution to report on.</param>
    internal sealed class Report(WorkerExecution execution)
    {
        /// <summary>
        /// Health status of the worker execution.
        /// </summary>
        public HealthStatus Status { get; set; } = execution.IsSuccessful ? HealthStatus.Healthy : HealthStatus.Unhealthy;

        /// <summary>
        /// Reason or message associated with the execution status.
        /// </summary>
        public string? Reason { get; set; } = execution.Message;

        /// <summary>
        /// Duration of the execution in seconds.
        /// </summary>
        public int DurationInSeconds { get; set; } = execution.GetDurationInSeconds();

        /// <summary>
        /// UTC timestamp when the execution started.
        /// </summary>
        public DateTime StartTimeUtc { get; set; } = execution.ExecutionStartTime;
    }
}