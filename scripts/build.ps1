#!/usr/bin/env pwsh
# Build script for DevUtilities

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "",
    [switch]$SelfContained = $false,
    [switch]$Clean = $false
)

$ProjectPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "src" | Join-Path -ChildPath "DevUtilities.csproj"
$OutputPath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath "tools" | Join-Path -ChildPath "releases"

Write-Host "Building DevUtilities..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow

if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean $ProjectPath --configuration $Configuration
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore $ProjectPath

# Build the project
if ($Runtime) {
    Write-Host "Building for runtime: $Runtime" -ForegroundColor Yellow
    $OutputDir = Join-Path -Path $OutputPath -ChildPath $Runtime
    
    if ($SelfContained) {
        dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained true -o $OutputDir
    } else {
        dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained false -o $OutputDir
    }
} else {
    Write-Host "Building for current platform..." -ForegroundColor Yellow
    dotnet build $ProjectPath --configuration $Configuration
}

Write-Host "Build completed!" -ForegroundColor Green