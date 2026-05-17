# YAMCqrs.EventBus.Core

Este paquete proporciona el Core para el manejo de eventos, siendo agnostico a cualquier bus de eventos.
Cuenta con una implementacion de un bus de evento en memoria para simular los eventos de dominio facilitando que luego el evento se pueda convertir en un evento de integracion.

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Core
```

## 🚀 Uso Rápido
Para registrar la dispersion por Kafka en tu contenedor de dependencias:

```csharp
 builder.Services.AddEventBus(opt =>
        {
            opt.ConcurrentWorkers = 1;
            opt.BatchSize = 100;
        })
        ;
```

## ⚙️ Configuración

- **ConcurrentWorkers**: Cantidad de threads que van a ejecutar en pararelo los eventos (Se usa la mitad para enviar y la mitad para escuchar).
- **BatchSize**: Tamaño del batch en memoria que se va a traer para procesar en cada ejecucion.
- **PollingIntervalSeconds**: Cada cuando tiempo se van a buscar nuevos eventos para disparar.
- **ErrorThresholdPercentage**: Porcentaje de error que debe tener un batch para considerar que fallo.

## 🛠️ Detalles de Implementación

- **IEventBusPublisher** : Permite extender la publicacion de los eventos a multiples servicios de mensajeria. por defecto la libreria incluye una version InMemory para simular eventos de dominio.
- **IEventPublisher** : Guardar los deseos de enviar un evento y los procesa en un scope aparte, tiene en cuenta la transaccion del scope para no guardar los eventos si el scope termina con error.
- **IPublishEventStore** : Guarda los eventos para ser despachados en un scope aparte y brinda tambien auditoria la libreria incluye por defecto una implementacion en InMemory pero no se recomienda utilizar para produccion.
- **PublishEvent** : Clase base para los eventos de salida. es recomendable que cada proveedor cree su propia clase base hererando esta clase.
- **ISubscribeEventStore** : Guarda los eventos recibidos y los procesa en un scope aparte, tiene que ser usado por las librerias de extension para almacenar los mensajes recibidos y luego sean procesados en un scope aparte.
- **SubscribeEvent** : Clase base para los eventos de entrada. es recomendable que cada proveedor cree su propia clase base hererando esta clase.

## 📋 Dependencias

- El proyecto `YAMCqrs.EventBus.Core`
- El proyecto `YAMCqrs.BackgroundWorker`

## Eventos de dominio

Se pasa a explicar como se puede usar la libreria core para el envio y procesamiento de eventos de dominio usando su bus InMemory incluido.

### Evento de salida

Para enviar un evento de dominio creamos el evento y hacemos que herede InMemoryPublishEvent y implementamos lo que nos exige la interface

```csharp
    internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
    {
        public const string TopicName = "domain-event-topic";

        public DomainEventPublishEvent()
        {
            this.Numerito = RandomNumberGenerator.GetInt32(500);
        }

        public int Numerito { get; init; }

        public override Dictionary<string, string>? GetCustomHeaders()
        {
            return null;
        }

        public override string Topic()
        {
            return TopicName;
        }
    }
```

### Enviar el evento

Para enviar el evento simplemente hay que usar el servicio de IEventPublisher.

```csharp
internal sealed class MyLogicCommandHandler(IEventPublisher eventPublisher) : ICommandHandler<MyLogicCommand, string>
{
    public async Task<Result<string>> HandleAsync(MyLogicCommand command, CancellationToken cancellationToken = default)
    {
        await eventPublisher.PublishAsync(new DomainEventPublishEvent(), cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}

```

### Evento de recepcion

Debemos crear la clase hermana que va a recepcionar el evento

```csharp
    internal sealed class DomainEventSubscribeEvent() : InMemorySuscribeEvent(DomainEventPublishEvent.TopicName)
    {
        public int Numerito { get; init; }
    }
```

### Procesar el evento

Debemos crear el Handler que va a procesar el evento de dominio.

```csharp
internal sealed partial class DomainEventSubscribeHanlder(ILogger<DomainEventSubscribeHanlder> logger) : ICommandHandler<DomainEventSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(DomainEventSubscribeEvent command, CancellationToken cancellationToken = default)
    {
        this.LogReception(command.Numerito);
        return Task.FromResult(Result<bool>.Success(true));
    }

    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Se recibio el numero: {numerito}")]
    private partial void LogReception(int numerito);
}
```