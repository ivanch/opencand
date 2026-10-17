<#
.SYNOPSIS
    Builds and runs OpenCand locally (API or ETL mode).

.DESCRIPTION
    By default, builds and runs the API backend along with the UI frontend.
    Allows toggling to ETL mode to parse CSV data into PostgreSQL.
    Supports both native toolchain and containerized Docker execution.

.PARAMETER Mode
    Selects whether to run 'API' (default) or 'ETL'.

.PARAMETER ETL
    Convenience switch to run ETL mode. Equivalent to -Mode ETL.

.PARAMETER Docker
    Run services using Docker Compose.

.PARAMETER NoBuild
    Skip building before running.

.PARAMETER NoUI
    In native API mode, skip launching the frontend Vite server.

.EXAMPLE
    .\run.ps1
    Builds and runs the API and UI natively (API on http://localhost:5299, UI on http://localhost:5173).

.EXAMPLE
    .\run.ps1 -ETL
    Builds and runs the ETL process natively.

.EXAMPLE
    .\run.ps1 -Docker
    Runs the PostgreSQL, API, and UI stack inside Docker Compose.

.EXAMPLE
    .\run.ps1 -ETL -Docker
    Runs PostgreSQL and executes the ETL container inside Docker.
#>

[CmdletBinding()]
param(
    [ValidateSet("API", "ETL")]
    [string]$Mode = "API",

    [switch]$ETL,

    [switch]$Docker,

    [switch]$NoBuild,

    [switch]$NoUI,

    [string]$DatabaseHost = "localhost",
    [int]$DatabasePort = 5432
)

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot

if ($ETL) {
    $Mode = "ETL"
}

function Write-Step($title) {
    Write-Host "`n====> $title" -ForegroundColor Cyan
}

function Write-Success($msg) {
    Write-Host "[OK] $msg" -ForegroundColor Green
}

function Write-Warn($msg) {
    Write-Host "[WARN] $msg" -ForegroundColor Yellow
}

function Write-Err($msg) {
    Write-Host "[ERROR] $msg" -ForegroundColor Red
}

function Get-ComposeCommand {
    if (Get-Command "docker" -ErrorAction SilentlyContinue) {
        $null = docker compose version 2>&1
        if ($LASTEXITCODE -eq 0) {
            return "docker compose"
        }
    }
    if (Get-Command "docker-compose" -ErrorAction SilentlyContinue) {
        return "docker-compose"
    }
    return $null
}

function Test-DatabasePort($hostName, $port) {
    try {
        $client = New-Object System.Net.Sockets.TcpClient
        $iar = $client.BeginConnect($hostName, $port, $null, $null)
        $wait = $iar.AsyncWaitHandle.WaitOne(1000, $false)
        if ($wait) {
            $client.EndConnect($iar)
            $client.Close()
            return $true
        }
        $client.Close()
        return $false
    }
    catch {
        return $false
    }
}

Set-Location -Path $ScriptRoot

Write-Host "==================================================" -ForegroundColor Blue
Write-Host "             OpenCand Local Runner               " -ForegroundColor White
Write-Host "  Execution Mode: $Mode" -ForegroundColor Yellow
Write-Host "  Runtime:        $($(if ($Docker) { 'Docker Compose' } else { 'Native (dotnet / npm)' }))" -ForegroundColor Gray
Write-Host "==================================================" -ForegroundColor Blue

# ─── Build Step ──────────────────────────────────────────────────────────────
if (-not $NoBuild) {
    Write-Step "Building project dependencies"
    $buildTarget = if ($Mode -eq "ETL") { "ETL" } else { "All" }
    $buildArgs = @{
        Target = $buildTarget
    }
    if ($Docker) {
        $buildArgs["Docker"] = $true
    }
    & "$ScriptRoot\build.ps1" @buildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Build failed. Aborting run."
        exit $LASTEXITCODE
    }
}

# ─── Docker Execution ────────────────────────────────────────────────────────
if ($Docker) {
    $composeCmd = Get-ComposeCommand
    if (-not $composeCmd) {
        Write-Err "Docker Compose is required for -Docker mode."
        Write-Host "You can install it via: winget install Docker.DockerCompose" -ForegroundColor Yellow
        exit 1
    }

    if ($Mode -eq "API") {
        Write-Step "Starting OpenCand (Postgres + API + UI) via Docker Compose"
        Write-Host "Services starting:" -ForegroundColor Gray
        Write-Host "  - PostgreSQL: localhost:5432" -ForegroundColor Gray
        Write-Host "  - API:        http://localhost:5299 (Swagger: http://localhost:5299/swagger)" -ForegroundColor Gray
        Write-Host "  - UI:         http://localhost:3000" -ForegroundColor Gray
        Write-Host "Press Ctrl+C to stop services.`n" -ForegroundColor DarkGray
        Invoke-Expression "$composeCmd up"
    } else {
        Write-Step "Ensuring PostgreSQL is up for ETL in Docker"
        Invoke-Expression "$composeCmd up -d postgres"
        Write-Step "Running OpenCand ETL container"
        Invoke-Expression "$composeCmd --profile etl run --rm etl"
    }
    exit 0
}

# ─── Native Execution ────────────────────────────────────────────────────────
Write-Step "Checking PostgreSQL connection (${DatabaseHost}:${DatabasePort})"
$dbReady = Test-DatabasePort $DatabaseHost $DatabasePort
if ($dbReady) {
    Write-Success "PostgreSQL detected on ${DatabaseHost}:${DatabasePort}"
} else {
    Write-Warn "PostgreSQL is not responding on ${DatabaseHost}:${DatabasePort}."
    Write-Host "If you do not have PostgreSQL running locally, you can start one via Docker:" -ForegroundColor Yellow
    Write-Host "  docker run -d --name opencand-db -p 5432:5432 -e POSTGRES_USER=root -e POSTGRES_PASSWORD=root -e POSTGRES_DB=opencand postgres:16-alpine`n" -ForegroundColor Cyan
}

if ($Mode -eq "API") {
    $fotosDir = Join-Path $ScriptRoot "src/OpenCand.API/fotos_cand"
    if (-not (Test-Path $fotosDir)) {
        New-Item -ItemType Directory -Path $fotosDir -Force | Out-Null
    }

    if (-not $NoUI) {
        Write-Step "Launching UI Frontend (Vite dev server in separate window)"
        $uiPath = Join-Path $ScriptRoot "src/OpenCand.UI"
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$uiPath'; Write-Host 'OpenCand UI dev server starting...' -ForegroundColor Cyan; npm run dev"
        Write-Host "Frontend URL:  http://localhost:5173" -ForegroundColor Green
    }

    Write-Step "Starting OpenCand API (REST Backend)"
    Write-Host "API URL:       http://localhost:5299" -ForegroundColor Green
    Write-Host "Swagger UI:    http://localhost:5299/swagger" -ForegroundColor Green
    Write-Host "Press Ctrl+C to terminate API.`n" -ForegroundColor DarkGray

    dotnet run --project "$ScriptRoot/src/OpenCand.API/OpenCand.API.csproj" --launch-profile "http"
} elseif ($Mode -eq "ETL") {
    Write-Step "Starting OpenCand ETL (Data Importer)"
    Write-Host "Parsing CSV files into PostgreSQL..." -ForegroundColor Gray
    Write-Host "Press Ctrl+C to terminate ETL.`n" -ForegroundColor DarkGray

    dotnet run --project "$ScriptRoot/src/OpenCand.ETL/OpenCand.ETL.csproj"
}
