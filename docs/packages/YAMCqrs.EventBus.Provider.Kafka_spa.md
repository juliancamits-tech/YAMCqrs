# YAMCqrs.EventBus.Provider.Kafka

Este paquete proporciona una implementación de dispersion de Eventos para la libreria EventBus de YAMCqrs utilizando **Kafka**. Permite que los eventos de integración sean enviados y recibidos por este medio.

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.EventBus.Provider.Kafka
```

## 🚀 Uso Rápido

Para registrar la dispersion por Kafka en tu contenedor de dependencias:

```csharp
 builder.Services.AddEventBus(opt =>
        {
          //Configuracion de la libreria base de EventBus
        })
        .UseKafka(new YAMCqrs.EventBus.Provider.Kafka.Configuration.KafkaConfigurationOptions()
        {
            ConnectionString = "cs_Kafka",
            KafkaClientName = "TestApp",
            KafkaGroupName = "TestApp",
        })
        ;
```

> [!TIP]
> Al usar como ConnectionString "cs_Kafka" estamos diciendole a la libreria que dentro del array "ConnectionStrings" busque el valor real en la clave Kafka segun lo definido en el [ADR 13](../adr/0013-connection-strings.md)

## ⚙️ Configuración

- **ConnectionString**: String de conexion para Kafka.
- **KafkaClientName**: Identificado que usa Kafka para temas de log.
- **KafkaGroupName**: Identificador que usa Kafka para asignar el grupo compartido para leer mensajes del topic.
- **MaxConcurrentConsumers**: Cantidad de threads que van a estar escuchando mensajes desde Kafka.


## 🛠️ Detalles de Implementación

- **Persistencia**: Los mensajes se persisten y otro scope son los que los procesan segun lo defnido en el [ADR 6](../adr/0006-domain-event.md).
- **Mapeo**: La descarga de mensajes es agnostica a su contenido por lo cual un mensaje encolado en un "mal formato" moriria en la aplicacion y no generaria un loop en la cola del topico para no avanzar el puntero.
- **Reintentos en conexion**: La libreria de Kafka requiere que todos los topicos a escuchar existan para conectarse, en case que falte alguno se haran reintentos infinitos hasta que se logre la conexion (Queda evidencia en el log de los intentos fallidos).
- **SourceGeneration**: Este proyecto utiliza SourceGeneration para descubrir todos los Topics de los eventos a escuchar.

## 📋 Dependencias

- Confluent.Kafka
- El proyecto `YAMCqrs.EventBus.Core`


## Envio de eventos

Para enviar un evento hace falta que dicho evento herede KafkaPublishEvent y implemente las interfaces requeridas, luego se debe publicar utilizando la interface IEventPublisher

> [!IMPORTANT]
> Recordar que lo que se genera es la intencion de enviar el evento, pero el envio real se hace en un scope separado

### 💡 Ejemplo como enviar un evento

#### Definicion del evento a enviar

```csharp
internal sealed class MyKafkaPublishEvent : KafkaPublishEvent
{
    public const string TopicName = "kafka.event.test";

    public MyKafkaPublishEvent()
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

#### Envio del evento

```csharp
internal sealed class MyLogicCommandHandler(IEventPublisher eventPublisher) : ICommandHandler<MyLogicCommand, string>
{
    public async Task<Result<string>> HandleAsync(MyLogicCommand command, CancellationToken cancellationToken = default)
    {
        //Logica de negocios y demas cosas

        //Envio del evento
        await eventPublisher.PublishAsync(new MyKafkaPublishEvent(), cancellationToken);

        return Result<string>.Ok(command.Name);
    }
}
```

## Recepcion de eventos

Para recibir un evento hace falta que dicho evento herede KafkaSubscribeEvent y implemente le pase al constructor el nombre del topic a escuchar, es importante que la variable pasada sea un valor definido y no un valor que se calcule en ejecucion, luego se debe crear un CommandHandler respetando esta interface ICommandHandler<MyKafkaSubscribeEvent, bool>

> [!IMPORTANT]
> Recordar que en este momento el mensaje ya fue descargado desde Kafka y almacenado internamnete por lo cual el procesamiento del evento no impacta en el proceso de escucha de eventos

> [!IMPORTANT]
> KafkaSubscribeEvent recibe un string que es el nombre del topic que representa el mensaje, este valor debe ser un string literal o una referencia a una variable constante ya que esto es necesario para que el SourceGeneration pueda armar con exito la lista de topicos

## 💡 Ejemplo como recibir un evento

### Definicion del evento a enviar

```csharp
internal sealed class MyKafkaSubscribeEvent() : KafkaSubscribeEvent(MyKafkaPublishEvent.TopicName)
{
    public int Numerito { get; init; }
}
```

### Procesamiento del evento

```csharp
internal sealed partial class MyKafkaSubscribeEventHanlder(ILogger<MyKafkaSubscribeEventHanlder> logger) : ICommandHandler<MyKafkaSubscribeEvent, bool>
{
    public Task<Result<bool>> HandleAsync(MyKafkaSubscribeEvent command, CancellationToken cancellationToken = default)
    {
        this.LogReception(command.Numerito);
        return Task.FromResult(Result<bool>.Success(true));
    }

    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Se recibio el numero: {numerito} de KAFKA!")]
    private partial void LogReception(int numerito);
}
```

