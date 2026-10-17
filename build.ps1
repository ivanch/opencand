<#
.SYNOPSIS
    Builds the OpenCand projects (All, API, ETL, Core, or UI).

.DESCRIPTION
    Supports both native toolchain (dotnet CLI + npm) and containerized Docker builds.
    By default, performs a native build of all projects.

.PARAMETER Target
    The target to build. Allowed values: All, API, ETL, Core, UI. Default is All.

.PARAMETER Docker
    Build Docker images instead of native binaries.

.PARAMETER Configuration
    Build configuration for .NET projects (Debug or Release). Default is Release.

.PARAMETER Clean
    Clean previous build artifacts before building.

.EXAMPLE
    .\build.ps1
    Builds all projects natively in Release mode.

.EXAMPLE
    .\build.ps1 -Target API
    Builds only the API project natively.

.EXAMPLE
    .\build.ps1 -Docker
    Builds Docker images for all services.
#>

[CmdletBinding()]
param(
    [ValidateSet("All", "API", "ETL", "Core", "UI")]
    [string]$Target = "All",

    [switch]$Docker,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot

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

Set-Location -Path $ScriptRoot

Write-Host "==================================================" -ForegroundColor Blue
Write-Host "             OpenCand Build Pipeline             " -ForegroundColor White
Write-Host "  Target:        $Target" -ForegroundColor Gray
Write-Host "  Mode:          $($(if ($Docker) { 'Docker Container' } else { 'Native Toolchain' }))" -ForegroundColor Gray
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
Write-Host "==================================================" -ForegroundColor Blue

if ($Docker) {
    # ─── Docker Build Mode ──────────────────────────────────────────────────
    Write-Step "Checking Docker environment"
    if (-not (Get-Command "docker" -ErrorAction SilentlyContinue)) {
        Write-Err "Docker is not found in PATH. Please install Docker or run without -Docker."
        exit 1
    }

    $composeCmd = Get-ComposeCommand

    if ($composeCmd) {
        Write-Step "Building with $composeCmd"
        if ($Target -eq "All") {
            Invoke-Expression "$composeCmd --profile etl build"
        } else {
            $service = switch ($Target) {
                "API" { "api" }
                "ETL" { "etl" }
                "UI"  { "ui" }
                "Core" { "api" }
            }
            Invoke-Expression "$composeCmd build $service"
        }
    } else {
        Write-Warn "Docker Compose plugin not detected; falling back to direct 'docker build'."
        if ($Target -in @("All", "API", "Core")) {
            Write-Step "Building Docker Image: opencand-api"
            docker build -t opencand-api:latest -f src/OpenCand.API/Dockerfile .
        }
        if ($Target -in @("All", "ETL")) {
            Write-Step "Building Docker Image: opencand-etl"
            docker build -t opencand-etl:latest -f src/OpenCand.ETL/Dockerfile .
        }
        if ($Target -in @("All", "UI")) {
            Write-Step "Building Docker Image: opencand-ui"
            docker build -t opencand-ui:latest -f src/OpenCand.UI/Dockerfile src/OpenCand.UI
        }
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Err "Docker build failed."
        exit $LASTEXITCODE
    }

    Write-Success "Docker build completed successfully!"
    exit 0
}

# ─── Native Build Mode ───────────────────────────────────────────────────────
if ($Target -in @("All", "API", "ETL", "Core")) {
    Write-Step "Verifying .NET SDK"
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        Write-Err ".NET SDK is not found in PATH. Please install .NET 8.0 SDK."
        exit 1
    }

    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK $dotnetVersion" -ForegroundColor Gray

    if ($Clean) {
        Write-Step "Cleaning .NET projects"
        dotnet clean OpenCand.sln -c $Configuration
    }

    if ($Target -eq "All") {
        Write-Step "Building solution: OpenCand.sln ($Configuration)"
        dotnet build OpenCand.sln -c $Configuration
    } elseif ($Target -eq "API") {
        Write-Step "Building OpenCand.API ($Configuration)"
        dotnet build src/OpenCand.API/OpenCand.API.csproj -c $Configuration
    } elseif ($Target -eq "ETL") {
        Write-Step "Building OpenCand.ETL ($Configuration)"
        dotnet build src/OpenCand.ETL/OpenCand.ETL.csproj -c $Configuration
    } elseif ($Target -eq "Core") {
        Write-Step "Building OpenCand.Core ($Configuration)"
        dotnet build src/OpenCand.Core/OpenCand.Core.csproj -c $Configuration
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Err ".NET build failed."
        exit $LASTEXITCODE
    }
    Write-Success ".NET build succeeded!"
}

if ($Target -in @("All", "UI")) {
    Write-Step "Verifying Node.js and npm"
    if (-not (Get-Command "npm" -ErrorAction SilentlyContinue)) {
        Write-Err "npm is not found in PATH. Please install Node.js (v18+)."
        exit 1
    }

    $uiDir = Join-Path $ScriptRoot "src/OpenCand.UI"
    Push-Location $uiDir
    try {
        if ($Clean -and (Test-Path "dist")) {
            Write-Step "Removing UI dist directory"
            Remove-Item "dist" -Recurse -Force
        }

        if (-not (Test-Path "node_modules")) {
            Write-Step "Installing UI dependencies (npm ci)"
            npm ci
            if ($LASTEXITCODE -ne 0) {
                Write-Warn "npm ci failed; attempting npm install..."
                npm install
            }
        }

        Write-Step "Building UI (npm run build)"
        npm run build
        if ($LASTEXITCODE -ne 0) {
            Write-Err "UI build failed."
            exit $LASTEXITCODE
        }
        Write-Success "UI build succeeded!"
    }
    finally {
        Pop-Location
    }
}

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host "              All Builds Succeeded!              " -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
