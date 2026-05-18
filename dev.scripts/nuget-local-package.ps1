# Fail fast
$ErrorActionPreference = "Stop"

# Rutas hardcodeadas a los proyectos a empaquetar (relativas a la raíz)
$projectsToPack = @(
    ".\src\backgroundworker\core\YAMCqrs.BackgroundWorker\YAMCqrs.BackgroundWorker.csproj" #Core BackgroundWorker
    ".\src\backgroundworker\storage\mongodb\YAMCqrs.BackgroundWorker.Storage.MongoDb\YAMCqrs.BackgroundWorker.Storage.MongoDb.csproj" #BackgroundWorker MongoDB Storage
    ".\src\backgroundworker\storage\mongodb\YAMCqrs.BackgroundWorker.Storage.MongoDb\YAMCqrs.BackgroundWorker.Storage.MongoDb.csproj" #BackgroundWorker MongoDB Storage
    ".\src\core\YAMCqrs.Core\YAMCqrs.Core.csproj" #Core
    ".\src\eventbus\core\YAMCqrs.EventBus.Core\YAMCqrs.EventBus.Core.csproj" #Core EventBus
    ".\src\eventbus\provider\kafka\YAMCqrs.EventBus.Provider.Kafka\YAMCqrs.EventBus.Provider.Kafka.csproj" #EventBus Kafka Provider
    ".\src\eventbus\storage\YAMCqrs.EventBus.Storage.MongoDb\YAMCqrs.EventBus.Storage.MongoDb.csproj" #EventBus MongoDB Storage
)

# Configuración común
$configuration = "Release"
$packageOutput = ".\nugets"

Remove-Item -Path $packageOutput -Recurse -Force   

# Asegurar que el directorio de salida exista
if (-not (Test-Path $packageOutput)) {
    New-Item -ItemType Directory -Path $packageOutput | Out-Null
}

foreach ($project in $projectsToPack) {

    Write-Host "Building $project..."

    dotnet build $project -c $configuration

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed for $project"
    }

    Write-Host "Packing $project..."

    dotnet pack $project `
        -c $configuration `
        --no-build `
        -o $packageOutput `
        /p:PackAnalyzers=true `
        /p:AnalyzerPackagePath="analyzers/dotnet/cs"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for $project"
    }
}

Write-Host "All packages packed successfully."
