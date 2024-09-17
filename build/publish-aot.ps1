#! /usr/bin/env pwsh
#Requires -Version 7.0
#Requires -PSEdition Core

# This script is for locally testing AOT compilation

param (
    [string] $Configuration = "Release",
    [string] $RuntimeIdentifier,
    [switch] $GenerateAotMetadata = $false
)

$installSdk = Join-Path $PSScriptRoot "install-sdk.ps1"
& $installSdk

$dotnet = Join-Path "$env:DOTNET_INSTALL_DIR" "dotnet"

function DotNetPublish {
    param([string]$Project)

    $additionaArgs = @();
    
    if ($GenerateAotMetadata) {
        $additionaArgs += "/p:GenerateAotMetadata=true"
    }
    
    & $dotnet publish $Project -c Release -r $RuntimeIdentifier --self-contained $additionaArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$publishProjectPaths = @(
    "src/HwoodiwissSyncer/HwoodiwissSyncer.csproj"
)

& $dotnet workload restore

foreach ($publishProjectPath in $publishProjectPaths) {
    DotNetPublish $publishProjectPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
}
