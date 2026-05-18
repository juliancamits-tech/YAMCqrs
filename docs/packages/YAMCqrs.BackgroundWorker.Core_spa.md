# YAMCqrs.BackgroundWorker.Core

Una librería .NET para crear workers en segundo plano que procesan tareas por lotes con soporte para procesamiento paralelo, auditoría de ejecución y monitoreo de salud cumpliendo con lo definido en el [ADR 7](../adr/0007-backgroundservice.md)

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.BackgroundWorker.Core
```

## 📋 Descripción

Esta librería proporciona una infraestructura robusta para implementar servicios en segundo plano que procesan elementos en lotes. La librería ofrece:

- **Procesamiento por lotes controlado:** Procesa múltiples elementos de forma eficiente con soporte para paralelismo configurable
- **Auditoría de ejecución:** Registra automáticamente cada ejecución con métricas de éxito/fallo
- **Monitoreo de salud:** Health checks integrados para verificar el estado de los workers
- **Gestión de errores:** Umbral de error configurable para determinar el estado de la ejecución
- **Almacenamiento flexible:** Implementación en memoria incluida, con soporte para implementaciones personalizadas


## 🚀 Uso Rápido

Para registrar el motor de Background Workers en tu contenedor de dependencias:

```csharp
  builder.Services.AddBackgroundWorker(options =>
        {
            options.MinutesToKeepSuccesTask = 60;
            options.MinutesToKeepFailedTask = BackgroundWorkerConfiguration.DayToMinutes(7);
        });
```

> [!TIP]
> Aca en la configuracion estamos diciendo que las Tareas completadas con exito se borren cada 60 minutos pero las tarreras cuyo resultado fue de error se borran cada 7 dias.

## 🛠️ Detalles de Implementación

- **YABackgroundWorker** : Clase abstracta para estandarizar el comportamiento de las tareas en segundo plano
- **IWorkerStorage:** Almacenamiento para el historial de ejecuciones. Incluye una implementación `InMemory` (no recomendada para producción).
- **CleanBackGroundWorker:** Worker que se encarga de la limpieza del Storage
- **HealthCheckReport:** HealthCheck automatico sobre los resultado de las ejecuciones de los Workers

## 📋 Dependencias

- Solamente paquetes oficiales de Microsoft.

## 💡 Uso Básico

### 1. Crear un Worker Personalizado

Hereda de `YABackgroundWorker<TWorkItem>` e implementa los métodos abstractos:

```csharp
public class MiWorker : YABackgroundWorker<MiElemento>
{
    public MiWorker(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    // Configuración inicial antes de que el worker comience
    protected override Task<bool> InitialSetupAsync(IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Tu código de inicialización aquí
        // Retorna true si la inicialización fue exitosa, false en caso contrario
        // Usar por ejemplo para validar flags de activacion de la tarea
        return Task.FromResult(true);
    }

    // Obtener el lote de elementos a procesar
    protected override async Task<IEnumerable<MiElemento>?> GetBatchForProcessing(
        IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Obtener elementos de tu fuente de datos
        return await ObtenerElementos();
    }

    // Procesar un elemento individual
    protected override async Task<bool> ProcessItemAsync(
        MiElemento item, IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Tu lógica de procesamiento aquí
        // Retorna true si se procesó con éxito, false en caso contrario
        return true;
    }

    // Validación previa antes de procesar el lote
    protected override Task<PrevalidationResult> BatchPrevalidation(
        IServiceScope serviceScope, CancellationToken stoppingToken)
    {
        // Verificar condiciones previas (ej: servicio externo disponible)
        // La idea es evitar ejecutar un Batch que puede ser 100% error debido a un problema de un tercero.
        return Task.FromResult(PrevalidationResult.Execute());
    }

    // Limpieza final cuando el worker se detiene
    protected override void FinalCleanUp()
    {
        // Dispose seguro de elementos o otro accionar cuando la Tarea (NO EL BATCH) se apague.
    }

    // Intervalo de espera entre ejecuciones (en segundos)
    protected override int SleepIntervalInSeconds() => 60;

    // Grado de paralelismo (número de elementos procesados simultáneamente)
    protected override int ParallelismDegree() => 5;

    // Umbral de error porcentual (0-100)
    protected override int ErrorThresholdPercentage() => 10;

    // En el caso que al buscar un batch para procesar no se obtiene elementos, entonces no se logean en el storage la ejecucion.
    protected override bool SkipEmptyResults() => true;
}
```

> [!WARNING]
> Aunque en teoria se puede poner como tiempo de espera entre ejecuciones DIAS dependiendo donde y como este deployado la tarea puede morir. Es recomendable poner un sleep menor y en BatchPrevalidation hace una validacion mas profunda de si se debe ejecutar o no cuando el tiempo entre ejecuciones es muy largo.

### 2. Registrar el Worker

En tu `Program.cs` o `Startup.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configurar el núcleo del background worker
 builder.Services.AddBackgroundWorker(options =>
        {
          //Configuracion de la libreria base de BackgroundWorker
        });
// Registrar tu worker personalizado de forma tradicional.
builder.Services.AddHostedService<MiWorker>();

var app = builder.Build();
app.Run();
```

## ⚙️ Configuración

### BackgroundWorkerConfiguration

Configura cuánto tiempo se retienen las ejecuciones en el almacenamiento:

```csharp
new BackgroundWorkerConfiguration
{
    // Tiempo en minutos para mantener tareas exitosas (default: 60)
    MinutesToKeepSuccesTask = 60,
    
    // Tiempo en minutos para mantener tareas fallidas (default: 60)
    MinutesToKeepFailedTask = BackgroundWorkerConfiguration.DayToMinutes(7)
}
```

Métodos helper disponibles:
- `BackgroundWorkerConfiguration.HourToMinutes(int hours)` - Convierte horas a minutos
- `BackgroundWorkerConfiguration.DayToMinutes(int days)` - Convierte días a minutos

### Parámetros del Worker

Cada worker personalizado debe configurar:

- **SleepIntervalInSeconds:** Tiempo de espera entre ejecuciones (el tiempo de procesamiento se descuenta automáticamente)
- **ParallelismDegree:** Número máximo de elementos procesados en paralelo (1 = secuencial)
- **ErrorThresholdPercentage:** Porcentaje máximo de errores permitido antes de marcar la ejecución como fallida (0-100)
- **SkipEmptyResults:** Setea si cuando el batch esta vacio si se debe guardar o no en la auditoria.

## 🏥 Health Checks

La librería incluye health checks automáticos para monitorear el estado de tus workers:

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("background")
});
```

El health check reporta:
- **Healthy:** Todos los workers se ejecutaron exitosamente
- **Degraded:** Algunos workers fallaron
- **Unhealthy:** Ningún worker está saludable

### ⚠️ Importante: Qué Monitorean los Health Checks

Los health checks **NO monitorean el estado actual del worker**, sino **el resultado de su última ejecución de procesamiento**. Esto significa que evalúan el resultado de las siguientes operaciones:

- `BatchPrevalidation()` - Validación previa del lote
- `GetBatchForProcessing()` - Obtención del lote de elementos
- `ProcessItemAsync()` - Procesamiento de cada elemento
- `BatchPostProcesing()` - Post-procesamiento del lote

**Nota sobre InitialSetupAsync():** Si `InitialSetupAsync()` retorna `false` y el worker no inicia, el health check mostrará el resultado de una ejecución anterior si existe en el almacenamiento. Si no existe ninguna ejecución anterior, el worker no aparecerá en el health check hasta que tenga al menos una ejecución completada.

## 📊 Almacenamiento de Ejecuciones

### Implementación en Memoria (Default)

Por defecto, se usa `InMemoryWorkerStorage` que almacena las ejecuciones en memoria.

> **⚠️ ADVERTENCIA: NO USAR EN PRODUCCIÓN**
> 
> La implementación `InMemoryWorkerStorage` **NO está diseñada para entornos de producción** por las siguientes razones:
> 
> 1. **Pérdida de historial:** Al reiniciar la aplicación, se pierde todo el historial de ejecuciones almacenado en memoria.
> 
> 2. **Intervalos de ejecución incorrectos:** El worker utiliza el historial para calcular cuándo debe ejecutarse nuevamente. Sin persistencia:
>    - Un worker configurado para ejecutarse cada 1 hora se reiniciará inmediatamente después de un reinicio del servicio
>    - Se pierde el tiempo restante calculado (ej: si faltaban 40 minutos para la próxima ejecución, se ejecutará inmediatamente)
>    - Esto puede causar ejecuciones duplicadas o sobrecarga del sistema
> 
> 3. **Health checks inconsistentes:** Sin historial persistente, los health checks no pueden reportar el estado real entre reinicios.
>
> **Recomendación:** Para producción, implemente una versión persistente de `IWorkerStorage` usando una base de datos.

**Uso recomendado:** Solo para desarrollo local y pruebas.

## 🔍 WorkerExecution

Cada ejecución del worker se registra con la siguiente información:

```csharp
public class WorkerExecution
{
    public Guid Id { get; }                        // ID único
    public string WorkerName { get; }              // Nombre del worker
    public DateTime ExecutionStartTime { get; }    // Hora de inicio (UTC)
    public DateTime ExecutionEndTime { get; }      // Hora de fin (UTC)
    public ExecutionStatus Status { get; }         // Estado de la ejecución
    public int Success { get; }                    // Elementos procesados exitosamente
    public int Failed { get; }                     // Elementos fallidos
    public string Message { get; }                 // Mensaje adicional
    public bool IsSuccessful { get; }              // Si la ejecución fue exitosa
}
```

Estados posibles:
- `Null`: Estado inicial
- `Success`: Completado exitosamente
- `Failed`: Falló por exceder el umbral de error
- `FailedPrevalidation`: Saltado por fallar la pre-validación (equivale a `PrevalidationResult.Skip(...)`)
- `NoItemsToProcess`: Completado pero sin elementos para procesar

## 🎯 Características Avanzadas

### Pre-validación de Lotes

Implementa `BatchPrevalidation` para verificar condiciones antes de procesar. El resultado determina si el batch se ejecuta, se saltea con registro o se saltea de forma silenciosa:

```csharp
protected override async Task<PrevalidationResult> BatchPrevalidation(
    IServiceScope serviceScope, CancellationToken stoppingToken)
{
    var servicio = serviceScope.ServiceProvider.GetRequiredService<MiServicio>();

    if (!await servicio.EstaDisponible())
    {
        // Skip registrado en storage: útil cuando el skip es anómalo o relevante para auditoría
        return PrevalidationResult.Skip("Servicio externo no disponible");
    }

    if (!EsLunes())
    {
        // Skip silencioso: no deja registro en storage.
        // Ideal para condiciones esperadas y recurrentes que generarían ruido innecesario
        return PrevalidationResult.SkipSilently();
    }

    return PrevalidationResult.Execute();
}
```

| Resultado | Registra en storage | Cuándo usarlo |
|-----------|--------------------|-----------------|
| `PrevalidationResult.Execute()` | ✅ Sí | El batch debe procesarse normalmente |
| `PrevalidationResult.Skip(mensaje)` | ✅ Sí | El skip es anómalo o relevante para auditoría (ej: servicio caído) |
| `PrevalidationResult.SkipSilently()` | ❌ No | El skip es esperado y recurrente (ej: "solo los lunes") |

### Procesamiento Paralelo

Configura el grado de paralelismo según tus necesidades:

```csharp
protected override int ParallelismDegree()
{
    // 1 = Secuencial
    // >1 = Procesamiento paralelo
    return Environment.ProcessorCount; // Usa todos los núcleos disponibles
}
```

### Limpieza Automática

El `CleanBackGroundWorker` se ejecuta cada 1 hora para limpiar ejecuciones antiguas según tu configuración.
