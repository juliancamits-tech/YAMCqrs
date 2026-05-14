using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace YAMCqrs.EventBus.Storage.MongoDb;

internal interface IEventBusStorageMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoClient Client { get; }
}


internal class EventBusStorageMongoDbContext : IEventBusStorageMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public EventBusStorageMongoDbContext(IOptions<EventBusStorageMongoConfiguration> options, IConfiguration configuration)
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
