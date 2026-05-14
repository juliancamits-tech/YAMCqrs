using MongoDB.Driver;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.PublishEvents.Abstractions;
using YAMCqrs.EventBus.Core.PublishEvents.Domain;
using YAMCqrs.EventBus.Storage.MongoDb;

namespace YAMCqrs.EventBus.Storage.MongoDb.PublishEvents;

internal class PublishEventStore : IPublishEventStore
{
    private readonly IMongoCollection<PublishStoredEventDocument> _collection;

    public PublishEventStore(IEventBusStorageMongoDbContext context)
    {
        _collection = context.Database.GetCollection<PublishStoredEventDocument>("PublishEvents");

        // Índice compuesto para optimizar el worker de salida (Outbox pattern)
        var indexKeysDefinition = Builders<PublishStoredEventDocument>.IndexKeys
            .Ascending(e => e.Status)
            .Ascending(e => e.CreatedAt);

        // Idempotente: No crea nada si ya existe
        _collection.Indexes.CreateOne(new CreateIndexModel<PublishStoredEventDocument>(indexKeysDefinition));
    }

    public async Task<IEnumerable<PublishStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PublishStoredEventDocument>.Filter.Eq(x => x.Status, EventStatus.Pending);
        var sort = Builders<PublishStoredEventDocument>.Sort.Ascending(x => x.CreatedAt);

        var documents = await _collection.Find(filter)
            .Sort(sort)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);

        return documents.Select(doc => doc.ToDomain());
    }

    public async Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PublishStoredEventDocument>.Filter.Eq(x => x.Id, eventId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document != null)
        {
            var domain = document.ToDomain();
            domain.SetFailed(error);
            await _collection.ReplaceOneAsync(filter, domain.ToDocument(), cancellationToken: cancellationToken);
        }
    }

    public async Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PublishStoredEventDocument>.Filter.Eq(x => x.Id, eventId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document != null)
        {
            var domain = document.ToDomain();
            domain.SetProcessed();
            await _collection.ReplaceOneAsync(filter, domain.ToDocument(), cancellationToken: cancellationToken);
        }
    }

    public async Task StoreAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : PublishEvent
    {
        var domain = new PublishStoredEvent(@event);

        await _collection.InsertOneAsync(domain.ToDocument(), cancellationToken: cancellationToken);
    }
}
