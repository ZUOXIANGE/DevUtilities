#!/usr/bin/env pwsh
# Clean script for DevUtilities

param(
    [switch]$All = $false
)

$ProjectPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "src" | Join-Path -ChildPath "DevUtilities.csproj"
$ReleasesPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "tools" | Join-Path -ChildPath "releases"

Write-Host "Cleaning DevUtilities..." -ForegroundColor Green

# Clean build artifacts
Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
dotnet clean $ProjectPath

if ($All) {
    Write-Host "Cleaning all output directories..." -ForegroundColor Yellow
    
    # Remove bin and obj directories
    $BinPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "src" | Join-Path -ChildPath "bin"
    $ObjPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "src" | Join-Path -ChildPath "obj"
    
    if (Test-Path $BinPath) {
        Remove-Item $BinPath -Recurse -Force
        Write-Host "Removed: $BinPath" -ForegroundColor Gray
    }
    
    if (Test-Path $ObjPath) {
        Remove-Item $ObjPath -Recurse -Force
        Write-Host "Removed: $ObjPath" -ForegroundColor Gray
    }
    
    # Remove releases
    if (Test-Path $ReleasesPath) {
        Remove-Item $ReleasesPath -Recurse -Force
        Write-Host "Removed: $ReleasesPath" -ForegroundColor Gray
    }
}

Write-Host "Clean completed!" -ForegroundColor Green