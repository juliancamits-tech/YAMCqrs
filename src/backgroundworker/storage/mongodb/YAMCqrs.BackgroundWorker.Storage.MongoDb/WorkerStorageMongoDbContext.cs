using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace YAMCqrs.BackgroundWorker.Storage.MongoDb;

internal interface IWorkerStorageMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoClient Client { get; }
}

internal class WorkerStorageMongoDbContext : IWorkerStorageMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public WorkerStorageMongoDbContext(IOptions<BackgroundWorkerMongoConfiguration> options, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(options.Value, nameof(options));

        var clientSettings = MongoClientSettings.FromConnectionString(options.Value.GetConnectionString(configuration));

        // Configuración de resiliencia
        clientSettings.RetryWrites = true;
        clientSettings.RetryReads = true;

        this._client = new MongoClient(clientSettings);
        this._database = _client.GetDatabase(options.Value.GetDatabaseName());
    }

    public IMongoDatabase Database => _database;
    public IMongoClient Client => _client;
}