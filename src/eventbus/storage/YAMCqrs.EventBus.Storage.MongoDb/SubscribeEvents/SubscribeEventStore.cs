using MongoDB.Driver;
using YAMCqrs.EventBus.Core;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Domain;

namespace YAMCqrs.EventBus.Storage.MongoDb.SubscribeEvents;

internal class SubscribeEventStore : ISubscribeEventStore
{
    private readonly IMongoCollection<SubscribeStoredEventDocument> _collection;

    public SubscribeEventStore(IEventBusStorageMongoDbContext context)
    {
        _collection = context.Database.GetCollection<SubscribeStoredEventDocument>("SubscribeEvents");

        // Definición de índices para optimizar la búsqueda de pendientes por orden de llegada
        var indexKeysDefinition = Builders<SubscribeStoredEventDocument>.IndexKeys
            .Ascending(e => e.Status)
            .Ascending(e => e.ReceivedAt);

        // El driver de Mongo maneja la creación de índices de forma idempotente (no hace nada si ya existen)
        _collection.Indexes.CreateOneAsync(new CreateIndexModel<SubscribeStoredEventDocument>(indexKeysDefinition));
    }

    public async Task<IEnumerable<SubscribeStoredEvent>> GetPendingEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SubscribeStoredEventDocument>.Filter.Eq(x => x.Status, EventStatus.Pending);
        var sort = Builders<SubscribeStoredEventDocument>.Sort.Ascending(x => x.ReceivedAt);

        var documents = await _collection.Find(filter)
            .Sort(sort)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);

        return documents.Select(doc => doc.ToDomain());
    }

    public async Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default)
    {
        // Filtramos por ID y aseguramos que el estado siga siendo el inicial (Pending).
        // Esto evita que si otro proceso ya lo tomó, nosotros lo sobreescribamos.
        var filter = Builders<SubscribeStoredEventDocument>.Filter.And(
            Builders<SubscribeStoredEventDocument>.Filter.Eq(x => x.Id, eventId),
            Builders<SubscribeStoredEventDocument>.Filter.Eq(x => x.Status, EventStatus.Pending)
        );

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
        var filter = Builders<SubscribeStoredEventDocument>.Filter.And(
            Builders<SubscribeStoredEventDocument>.Filter.Eq(x => x.Id, eventId),
            Builders<SubscribeStoredEventDocument>.Filter.Eq(x => x.Status, EventStatus.Pending)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document != null)
        {
            var domain = document.ToDomain();
            domain.SetProcessed();
            await _collection.ReplaceOneAsync(filter, domain.ToDocument(), cancellationToken: cancellationToken);
        }
    }

    public async Task<bool> StoreAsync(string topic, string value, Dictionary<string, string>? headers, ServiceBusProvider serviceBusProvider, CancellationToken cancellationToken)
    {
        // Creamos la entidad de dominio para asegurar que nace con el estado correcto (Pending, RetryCount 0, etc.)
        var domain = new SubscribeStoredEvent(
            id: Guid.CreateVersion7(),
            topic: topic,
            value: value,
            headers: headers,
            receivedAt: DateTime.UtcNow,
            status: EventStatus.Pending,
            retryCount: 0,
            error: null,
            sourceStorage: "MongoDb",
            sourceEvent: serviceBusProvider);

        await _collection.InsertOneAsync(domain.ToDocument(), cancellationToken: cancellationToken);
        return true;
    }
}
