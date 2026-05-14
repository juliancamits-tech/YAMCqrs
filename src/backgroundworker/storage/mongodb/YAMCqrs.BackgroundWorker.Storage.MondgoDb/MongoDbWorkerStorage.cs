using MongoDB.Bson;
using MongoDB.Driver;
using YAMCqrs.BackgroundWorker.Abstractions;
using YAMCqrs.BackgroundWorker.Domain;

namespace YAMCqrs.BackgroundWorker.Storage.MondgoDb;

internal class MongoDbWorkerStorage : IWorkerStorage
{
    private readonly IMongoCollection<WorkerExecutionDocument> _collection;

    /// <summary>
    /// Status values considered successful, mirroring <see cref="WorkerExecution.IsSuccessful"/>.
    /// </summary>
    private static readonly string[] SuccessfulStatuses =
    [
        WorkerExecution.ExecutionStatus.Success.ToString(),
        WorkerExecution.ExecutionStatus.NoItemsToProcess.ToString()
    ];

    public MongoDbWorkerStorage(IWorkerStorageMongoDbContext context)
    {
        _collection = context.Database.GetCollection<WorkerExecutionDocument>("WorkerExecutions");
        InitializeIndexes();
    }

    /// <summary>
    /// Creates the collection indexes on first instantiation.
    /// Safe to call multiple times — MongoDB skips creation if they already exist.
    /// </summary>
    private void InitializeIndexes()
    {
        _collection.Indexes.CreateMany(
        [
            new CreateIndexModel<WorkerExecutionDocument>(
                Builders<WorkerExecutionDocument>.IndexKeys.Ascending(e => e.WorkerName),
                new CreateIndexOptions { Name = "IX_WorkerExecutions_WorkerName" }
            ),
            new CreateIndexModel<WorkerExecutionDocument>(
                Builders<WorkerExecutionDocument>.IndexKeys
                    .Ascending(e => e.WorkerName)
                    .Descending(e => e.ExecutionStartTime),
                new CreateIndexOptions { Name = "IX_WorkerExecutions_WorkerName_ExecutionStartTime" }
            ),
            new CreateIndexModel<WorkerExecutionDocument>(
                Builders<WorkerExecutionDocument>.IndexKeys.Ascending(e => e.ExecutionEndTime),
                new CreateIndexOptions { Name = "IX_WorkerExecutions_ExecutionEndTime" }
            ),
            new CreateIndexModel<WorkerExecutionDocument>(
                Builders<WorkerExecutionDocument>.IndexKeys.Ascending(e => e.Status),
                new CreateIndexOptions { Name = "IX_WorkerExecutions_Status" }
            ),
        ]);
    }

    /// <inheritdoc/>
    public async Task AddAsync(WorkerExecution execution)
    {
        var document = WorkerExecutionDocument.FromDomain(execution);
        await _collection.InsertOneAsync(document);
    }

    /// <inheritdoc/>
    public async Task CleanStorageAsync(DateTime dateForCompleted, DateTime dateForError)
    {
        // Successful executions older than dateForCompleted
        var successFilter = Builders<WorkerExecutionDocument>.Filter.And(
            Builders<WorkerExecutionDocument>.Filter.Lt(e => e.ExecutionEndTime, dateForCompleted),
            Builders<WorkerExecutionDocument>.Filter.In(e => e.Status, SuccessfulStatuses)
        );

        // Failed executions older than dateForError
        var failedFilter = Builders<WorkerExecutionDocument>.Filter.And(
            Builders<WorkerExecutionDocument>.Filter.Lt(e => e.ExecutionEndTime, dateForError),
            Builders<WorkerExecutionDocument>.Filter.Nin(e => e.Status, SuccessfulStatuses)
        );

        await _collection.DeleteManyAsync(
            Builders<WorkerExecutionDocument>.Filter.Or(successFilter, failedFilter));
    }

    /// <inheritdoc/>
    public async Task<WorkerExecution?> GetLastExecutionAsync(string workerName)
    {
        var document = await _collection
            .Find(Builders<WorkerExecutionDocument>.Filter.Eq(e => e.WorkerName, workerName))
            .Sort(Builders<WorkerExecutionDocument>.Sort.Descending(e => e.ExecutionStartTime))
            .Limit(1)
            .FirstOrDefaultAsync();

        return document?.ToDomain();
    }

    /// <inheritdoc/>
    public async Task<List<WorkerExecution>> GetLastExecutionByWorkerAsync()
    {
        // $group replaces _id with WorkerName (string), which conflicts with WorkerExecutionDocument's
        // [BsonId] Guid field. The pipeline outputs BsonDocument and is mapped manually.
        var pipeline = PipelineDefinition<WorkerExecutionDocument, BsonDocument>.Create(
        [
            new BsonDocument("$sort", new BsonDocument("ExecutionStartTime", -1)),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"]                = "$WorkerName",
                ["DocId"]             = new BsonDocument("$first", "$_id"),
                ["WorkerName"]        = new BsonDocument("$first", "$WorkerName"),
                ["ExecutionStartTime"]= new BsonDocument("$first", "$ExecutionStartTime"),
                ["ExecutionEndTime"]  = new BsonDocument("$first", "$ExecutionEndTime"),
                ["Status"]            = new BsonDocument("$first", "$Status"),
                ["Success"]           = new BsonDocument("$first", "$Success"),
                ["Failed"]            = new BsonDocument("$first", "$Failed"),
                ["Message"]           = new BsonDocument("$first", "$Message"),
            })
        ]);

        var rawDocuments = await _collection.Aggregate(pipeline).ToListAsync();

        return [.. rawDocuments
            .Select(doc => new WorkerExecutionDocument
            {
                Id                 = Guid.Parse(doc["DocId"].AsString),
                WorkerName         = doc["WorkerName"].AsString,
                ExecutionStartTime = doc["ExecutionStartTime"].ToUniversalTime(),
                ExecutionEndTime   = doc["ExecutionEndTime"].ToUniversalTime(),
                Status             = doc["Status"].AsString,
                Success            = doc["Success"].AsInt32,
                Failed             = doc["Failed"].AsInt32,
                Message            = doc["Message"].AsString,
            }.ToDomain())];
    }
}
