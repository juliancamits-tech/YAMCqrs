using MongoDB.Bson;
using MongoDB.Driver;
using YAMCqrs.BackgroundWorker.Core.Abstractions;
using YAMCqrs.BackgroundWorker.Core.Domain;

namespace YAMCqrs.BackgroundWorker.Storage.MongoDb;

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
            // 1. Optimizado: Este ya cubre las búsquedas de solo WorkerName, eliminamos el individual.
            new CreateIndexModel<WorkerExecutionDocument>(
            Builders<WorkerExecutionDocument>.IndexKeys
                .Ascending(e => e.WorkerName)
                .Descending(e => e.ExecutionStartTime),
            new CreateIndexOptions { Name = "IX_WorkerExecutions_WorkerName_ExecutionStartTime" }
        ),
        
        // 2. Para el borrado por fecha de fin
        new CreateIndexModel<WorkerExecutionDocument>(
            Builders<WorkerExecutionDocument>.IndexKeys.Ascending(e => e.ExecutionEndTime),
            new CreateIndexOptions { Name = "IX_WorkerExecutions_ExecutionEndTime" }
        ),
        
        // 3. Para búsquedas generales por estado
        new CreateIndexModel<WorkerExecutionDocument>(
            Builders<WorkerExecutionDocument>.IndexKeys.Ascending(e => e.Status),
            new CreateIndexOptions { Name = "IX_WorkerExecutions_Status" }
        ),

        // 4. NUEVO: Clave para que el Aggregate que busca el más nuevo sea instantáneo (Index Scan + No Sort)
        new CreateIndexModel<WorkerExecutionDocument>(
            Builders<WorkerExecutionDocument>.IndexKeys
                .Ascending(e => e.Status)
                .Descending(e => e.ExecutionStartTime),
            new CreateIndexOptions { Name = "IX_WorkerExecutions_Status_ExecutionStartTime" }
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

    /// <inheritdoc/>
    public async Task CleanStorageAsync(DateTime dateForCompleted, DateTime dateForError)
    {
        // 1. Identificar los IDs de los documentos más nuevos que NO debemos borrar
        var idsToKeep = await GetLatestExecutionIdsAsync();

        // 2. Filtro base para Executions Exitosas viejas
        var successFilter = Builders<WorkerExecutionDocument>.Filter.And(
            Builders<WorkerExecutionDocument>.Filter.Lt(e => e.ExecutionEndTime, dateForCompleted),
            Builders<WorkerExecutionDocument>.Filter.In(e => e.Status, SuccessfulStatuses)
        );

        // 3. Filtro base para Executions Fallidas viejas
        var failedFilter = Builders<WorkerExecutionDocument>.Filter.And(
            Builders<WorkerExecutionDocument>.Filter.Lt(e => e.ExecutionEndTime, dateForError),
            Builders<WorkerExecutionDocument>.Filter.Nin(e => e.Status, SuccessfulStatuses)
        );

        // 4. Combinar filtros con un OR (Tu lógica original)
        var combinedFilter = Builders<WorkerExecutionDocument>.Filter.Or(successFilter, failedFilter);

        // 5. CRUCIAL: Excluir los IDs más nuevos de la eliminación
        if (idsToKeep.Count > 0)
        {
            var excludeKeepersFilter = Builders<WorkerExecutionDocument>.Filter.Nin(e => e.Id, idsToKeep);
            combinedFilter = Builders<WorkerExecutionDocument>.Filter.And(combinedFilter, excludeKeepersFilter);
        }

        // 6. Ejecutar el borrado seguro
        await _collection.DeleteManyAsync(combinedFilter);
    }

    private async Task<List<Guid>> GetLatestExecutionIdsAsync()
    {
        // Separamos en dos agregaciones: una para el set de exitosos y otra para el de fallidos
        // (Asumiendo que por "cada set" te refieres al set de Exitosos y al set de Fallidos)

        var successLatest = await _collection.Aggregate()
            .Match(Builders<WorkerExecutionDocument>.Filter.In(e => e.Status, SuccessfulStatuses))
            .Sort(Builders<WorkerExecutionDocument>.Sort.Descending(e => e.ExecutionStartTime))
            .Limit(1)
            .Project(e => e.Id)
            .FirstOrDefaultAsync();

        var failedLatest = await _collection.Aggregate()
            .Match(Builders<WorkerExecutionDocument>.Filter.Nin(e => e.Status, SuccessfulStatuses))
            .Sort(Builders<WorkerExecutionDocument>.Sort.Descending(e => e.ExecutionStartTime))
            .Limit(1)
            .Project(e => e.Id)
            .FirstOrDefaultAsync();

        var ids = new List<Guid>();
        if (successLatest != default) ids.Add(successLatest);
        if (failedLatest != default) ids.Add(failedLatest);

        return ids;
    }
}
