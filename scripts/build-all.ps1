#!/usr/bin/env pwsh
# Cross-platform build script for DevUtilities

param(
    [string]$Configuration = "Release",
    [switch]$SelfContained = $false,
    [switch]$Clean = $false
)

$Runtimes = @("win-x64", "linux-x64", "osx-x64")
$BuildScript = Join-Path -Path $PSScriptRoot -ChildPath "build.ps1"

Write-Host "Starting cross-platform build..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Self-contained: $SelfContained" -ForegroundColor Yellow

foreach ($Runtime in $Runtimes) {
    Write-Host "`nBuilding for $Runtime..." -ForegroundColor Cyan
    
    if ($SelfContained) {
        & $BuildScript -Configuration $Configuration -Runtime $Runtime -SelfContained -Clean:$Clean
    } else {
        & $BuildScript -Configuration $Configuration -Runtime $Runtime -Clean:$Clean
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed for $Runtime"
        exit $LASTEXITCODE
    }
}

Write-Host "`nAll builds completed successfully!" -ForegroundColor Green