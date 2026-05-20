<# 
.SYNOPSIS
  Ejecuta tests con cobertura (XPlat Code Coverage) sobre el .sln de la carpeta actual
  y genera:
    - Reporte HTML (index.html)
    - Summary Markdown para GitHub (SummaryGithub.md)
    - Summary de texto (Summary.txt)

.DETALLES
  - Usa OpenCover por defecto: --collect:"XPlat Code Coverage;Format=opencover"
  - Busca automáticamente el primer *.runsettings bajo el directorio actual y lo pasa a dotnet test.
  - Limpia ejecuciones anteriores (TestResults y CoverageReport) para evitar que se mezclen reportes.
  - ReportGenerator: si no está, intenta instalar global; si falla, instala local con tool manifest.

.PARAMS
  -ResultsDir: Carpeta donde dotnet test deja resultados (default: TestResults)
  -ReportDir : Carpeta de salida de ReportGenerator (default: CoverageReport)
  -Clean     : Si es true, borra ResultsDir/ReportDir antes de ejecutar (default: true)
#>

[CmdletBinding()]
param(
    [string] $ResultsDir = "TestResults",
    [string] $ReportDir = "CoverageReport",
    [bool]   $Clean = $true
)

Set-Location -Path (Get-Location)
$ErrorActionPreference = "Stop"

function Write-Section([string]$text) {
    Write-Host "`n=== $text ===" -ForegroundColor Cyan
}

function Remove-PathIfExists([string]$path) {
    if (Test-Path -LiteralPath $path) {
        Write-Host "Borrando: $path" -ForegroundColor DarkYellow
        Remove-Item -LiteralPath $path -Recurse -Force -ErrorAction Stop
    }
    else {
        Write-Host "No existe (skip): $path" -ForegroundColor DarkGray
    }
}

function Get-ReportGeneratorCmd {
    # 1) ¿Está en PATH?
    $cmd = Get-Command reportgenerator -ErrorAction SilentlyContinue
    if ($cmd) {
        return "reportgenerator"
    }

    # 2) Ruta típica de tool global (Windows)
    $globalExe = Join-Path $HOME ".dotnet\tools\reportgenerator.exe"
    if (Test-Path $globalExe) {
        return $globalExe
    }

    # 3) ¿Tool local (manifest) ya instalada?
    if (Test-Path ".config/dotnet-tools.json") {
        $list = & dotnet tool list 2> $null
        if ($LASTEXITCODE -eq 0 -and ($list -match "dotnet-reportgenerator-globaltool")) {
            return "dotnet tool run reportgenerator"
        }
    }

    return $null
}

function Install-ReportGenerator {
    $existing = Get-ReportGeneratorCmd
    if ($existing) {
        Write-Host "ReportGenerator ya disponible: $existing" -ForegroundColor Green
        return $existing
    }

    Write-Section "Instalando ReportGenerator globalmente"
    & dotnet tool install -g dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -eq 0) {
        $afterGlobal = Get-ReportGeneratorCmd
        if ($afterGlobal) { return $afterGlobal }
    }
    else {
        # Si ya estaba instalado global, algunos SDK devuelven error; probamos update
        & dotnet tool update -g dotnet-reportgenerator-globaltool 2> $null | Out-Null
        $afterUpdate = Get-ReportGeneratorCmd
        if ($afterUpdate) { return $afterUpdate }
    }

    Write-Section "Instalación global no disponible. Probando instalación local (tool manifest)"
    if (-not (Test-Path ".config/dotnet-tools.json")) {
        & dotnet new tool-manifest
        if ($LASTEXITCODE -ne 0) {
            throw "No se pudo crear el tool manifest local."
        }
    }

    & dotnet tool install dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -ne 0) {
        # Puede fallar si ya estaba instalada: probamos update
        & dotnet tool update dotnet-reportgenerator-globaltool | Out-Null
    }

    if ($LASTEXITCODE -eq 0) {
        return "dotnet tool run reportgenerator"
    }

    throw "No se pudo instalar ReportGenerator (ni global ni local)."
}

try {
    if ($Clean) {
        Write-Section "Limpieza de ejecuciones anteriores"
        Remove-PathIfExists (Join-Path -Path (Get-Location) -ChildPath $ResultsDir)
        Remove-PathIfExists (Join-Path -Path (Get-Location) -ChildPath $ReportDir)
    }

    Write-Section "Buscando solución (.slnx) en la carpeta actual"
    $sln = Get-ChildItem -Path ./src -Filter *.slnx -File | Sort-Object Name | Select-Object -First 1
    if (-not $sln) {
        Write-Error "No se encontró ningún archivo .slnx en $(Get-Location). Abortando."
        exit 1
    }
    Write-Host "Se usará la solución: $($sln.Name)" -ForegroundColor Yellow

    $resultsPath = Join-Path -Path (Get-Location) -ChildPath $ResultsDir
    if (-not (Test-Path $resultsPath)) {
        New-Item -ItemType Directory -Path $resultsPath | Out-Null
    }

    Write-Section "Buscando *.runsettings (opcional)"
    $runsettings = Get-ChildItem -Path . -Recurse -Filter *.runsettings -File -ErrorAction SilentlyContinue |
    Sort-Object FullName |
    Select-Object -First 1

    if ($runsettings) {
        Write-Host "Runsettings encontrado: $($runsettings.FullName)" -ForegroundColor Yellow
    }
    else {
        Write-Host "No se encontró *.runsettings. Se ejecutará sin settings." -ForegroundColor DarkGray
    }

    Write-Section "Ejecutando tests con cobertura (OpenCover)"
    $testCmd = @(
        "test", $sln.FullName,
        "--collect", "XPlat Code Coverage;Format=opencover",
        "--results-directory", $resultsPath
    )

    if ($runsettings) {
        $testCmd += @("--settings", $runsettings.FullName)
    }

    & dotnet @testCmd
    $testsExit = $LASTEXITCODE

    if ($testsExit -ne 0) {
        Write-Error "Alguno(s) test(s) falló(aron). Revisá la salida anterior. Código: $testsExit"
        exit $testsExit
    }

    Write-Host "Todos los tests pasaron ✅" -ForegroundColor Green

    Write-Section "Buscando archivos de cobertura (OpenCover primero, fallback Cobertura)"
    $openCoverFiles = Get-ChildItem -Path $resultsPath -Recurse -File -Include "coverage.opencover.xml"
    $coberturaFiles = Get-ChildItem -Path $resultsPath -Recurse -File -Include "coverage.cobertura.xml", "coverage.xml"

    $coverageFiles = @()
    if ($openCoverFiles) { $coverageFiles += $openCoverFiles }
    if ($coberturaFiles) { $coverageFiles += $coberturaFiles }

    if (-not $coverageFiles -or $coverageFiles.Count -eq 0) {
        Write-Error "No se hallaron archivos de cobertura bajo $resultsPath."
        Write-Error "Esperados: coverage.opencover.xml (preferido) o coverage.cobertura.xml/coverage.xml (fallback)."
        exit 1
    }

    Write-Host ("Se encontraron {0} archivo(s) de cobertura:" -f $coverageFiles.Count) -ForegroundColor Green
    $coverageFiles | Sort-Object FullName | ForEach-Object { Write-Host " - $($_.FullName)" }

    $reportPath = Join-Path -Path (Get-Location) -ChildPath $ReportDir
    if (-not (Test-Path $reportPath)) {
        New-Item -ItemType Directory -Path $reportPath | Out-Null
    }

    Write-Section "Verificando/instalando ReportGenerator"
    $reportGenInvoker = Install-ReportGenerator

    Write-Section "Generando reportes (HTML + MarkdownSummaryGithub)"
    $reportsArg = @(
        "$resultsPath/**/coverage.opencover.xml",
        "$resultsPath/**/coverage.cobertura.xml",
        "$resultsPath/**/coverage.xml"
    ) -join ";"

    $rgArgs = @(
        "-reports:`"$reportsArg`"",
        "-targetdir:`"$reportPath`"",
        "-reporttypes:Html;TextSummary;MarkdownSummaryGithub"
    )

    if ($reportGenInvoker -eq "dotnet tool run reportgenerator") {
        & dotnet tool run reportgenerator @rgArgs
    }
    else {
        & $reportGenInvoker @rgArgs
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Error "ReportGenerator falló con código $LASTEXITCODE."
        exit $LASTEXITCODE
    }

    $summaryTxt = Join-Path $reportPath "Summary.txt"
    if (Test-Path $summaryTxt) {
        Write-Section "Resumen de cobertura (Summary.txt)"
        Get-Content $summaryTxt
    }

    $indexPath = Join-Path $reportPath "index.html"
    $summaryMd = Join-Path $reportPath "SummaryGithub.md"

    Write-Section "Reportes generados"
    if (Test-Path $indexPath) { Write-Host "HTML: $indexPath" -ForegroundColor Green } else { Write-Host "HTML no encontrado: $indexPath" -ForegroundColor DarkYellow }
    if (Test-Path $summaryMd) { Write-Host "MD  : $summaryMd" -ForegroundColor Green } else { Write-Host "MD no encontrado: $summaryMd" -ForegroundColor DarkYellow }

    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}