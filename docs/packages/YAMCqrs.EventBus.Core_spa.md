# YAMCqrs.EventBus.Core

Librería core para el manejo de eventos dentro del ecosistema YAMCqrs.

El paquete es completamente agnóstico al proveedor de mensajería y provee una infraestructura desacoplada para publicación, persistencia, procesamiento y suscripción de eventos.

Incluye además una implementación `InMemory` que permite simular Domain Events localmente y facilita evolucionar esos eventos hacia Integration Events reales siguiendo los lineamientos del [ADR 6](../adr/0006-domain-event.md).

---

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Core
```

---

## 🚀 Uso Rápido

```csharp
builder.Services.AddEventBus(opt =>
{
    opt.ConcurrentWorkers = 1;
    opt.BatchSize = 100;
});
```

---

## ⚙️ Configuración

- `ConcurrentWorkers`
  Cantidad de workers concurrentes para procesamiento paralelo.

- `BatchSize`
  Cantidad máxima de eventos procesados por ejecución.

- `PollingIntervalSeconds`
  Intervalo de polling para buscar eventos pendientes.

- `ErrorThresholdPercentage`
  Porcentaje máximo de errores permitido antes de marcar un batch como fallido.

---

## 🛠️ Componentes Principales

### IEventBusPublisher

Permite extender la publicación de eventos a:
- Kafka
- RabbitMQ
- Azure Service Bus
- AWS SQS/SNS
- Proveedores custom

La librería incluye una implementación `InMemory` para Domain Events locales.

---

### IEventPublisher

Registra la intención de publicar eventos respetando el scope transaccional actual.

Beneficios:
- evita persistencia inconsistente
- desacopla lógica de negocio
- soporta procesamiento asíncrono

---

### IPublishEventStore

Responsable de almacenar eventos pendientes de publicación.

Incluye:
- auditoría
- tracking
- retries
- desacople de procesamiento

> ⚠️ La implementación InMemory no se recomienda para producción.

---

### ISubscribeEventStore

Almacena eventos recibidos antes de procesarlos.

Permite:
- reprocesamiento
- auditoría
- desacople entre recepción y consumo

---

## 📡 Domain Events con InMemory

### Evento de salida

```csharp
internal sealed class DomainEventPublishEvent : InMemoryPublishEvent
{
    public const string TopicName = "domain-event-topic";

    public int Numerito { get; init; }

    public override string Topic()
    {
        return TopicName;
    }
}
```

---

### Publicar el evento

```csharp
await eventPublisher.PublishAsync(
    new DomainEventPublishEvent(),
    cancellationToken);
```

---

### Evento de recepción

```csharp
internal sealed class DomainEventSubscribeEvent()
    : InMemorySuscribeEvent(DomainEventPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
```

---

### Procesar el evento

```csharp
internal sealed class DomainEventSubscribeHanlder
    : ICommandHandler<DomainEventSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(
        DomainEventSubscribeEvent command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
```

---

## ⚡ Características Arquitectónicas

- Event-driven architecture
- Domain Events
- Integration Events
- Outbox-like processing
- Procesamiento asíncrono
- Bajo acoplamiento
- Infraestructura extensible
- Auditoría de eventos
- Retry support

---

## 📋 Dependencias

- `YAMCqrs.EventBus.Core`
- `YAMCqrs.BackgroundWorker`
