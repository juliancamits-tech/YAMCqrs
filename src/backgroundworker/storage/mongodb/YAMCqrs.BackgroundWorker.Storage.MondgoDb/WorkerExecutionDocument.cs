using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using YAMCqrs.BackgroundWorker.Domain;

namespace YAMCqrs.BackgroundWorker.Storage.MondgoDb;

internal class WorkerExecutionDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; }

    public string WorkerName { get; init; } = string.Empty;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ExecutionStartTime { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ExecutionEndTime { get; set; }

    /// <summary>
    /// Current status stored as string for readability in queries.
    /// Converted to/from <see cref="WorkerExecution.ExecutionStatus"/> via <see cref="ToDomain"/>.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public int Success { get; set; }
    public int Failed { get; set; }
    public string Message { get; set; } = string.Empty;

    public WorkerExecution ToDomain()
    {
        return new WorkerExecution(
            id: Id,
            workerName: WorkerName,
            executionStartTime: ExecutionStartTime,
            executionEndTime: ExecutionEndTime,
            status: Enum.Parse<WorkerExecution.ExecutionStatus>(Status),
            success: Success,
            failed: Failed,
            message: Message
        );
    }

    public static WorkerExecutionDocument FromDomain(WorkerExecution execution)
    {
        return new WorkerExecutionDocument
        {
            Id = execution.Id,
            WorkerName = execution.WorkerName,
            ExecutionStartTime = execution.ExecutionStartTime,
            ExecutionEndTime = execution.ExecutionEndTime,
            Status = execution.Status.ToString(),
            Success = execution.Success,
            Failed = execution.Failed,
            Message = execution.Message
        };
    }
}