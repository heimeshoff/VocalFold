#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Updates version numbers across the project files
.DESCRIPTION
    Updates version in package.json, installer.iss, and Directory.Build.props
.PARAMETER Version
    The version to set (e.g., "1.2.0")
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "Updating version to: $Version"

# Update package.json
$packageJsonPath = Join-Path $PSScriptRoot "..\package.json"
if (Test-Path $packageJsonPath) {
    Write-Host "Updating package.json..."
    $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
    $packageJson.version = $Version
    $packageJson | ConvertTo-Json -Depth 100 | Set-Content $packageJsonPath
    Write-Host "✅ Updated package.json"
} else {
    Write-Warning "package.json not found at: $packageJsonPath"
}

# Update installer.iss (update the default version in GetStringParam)
$installerPath = Join-Path $PSScriptRoot "..\installer.iss"
if (Test-Path $installerPath) {
    Write-Host "Updating installer.iss..."
    $installerContent = Get-Content $installerPath -Raw

    # Update the default version in GetStringParam("AppVersion", "x.x.x")
    $installerContent = $installerContent -replace '(GetStringParam\("AppVersion",\s*")[^"]+(")', "`${1}$Version`${2}"

    # Also update the hardcoded AppVersion if it exists
    $installerContent = $installerContent -replace '(#define\s+AppVersion\s+")[^"]+(")', "`${1}$Version`${2}"

    Set-Content $installerPath -Value $installerContent -NoNewline
    Write-Host "✅ Updated installer.iss"
} else {
    Write-Warning "installer.iss not found at: $installerPath"
}

# Update Directory.Build.props
$buildPropsPath = Join-Path $PSScriptRoot "..\Directory.Build.props"
if (Test-Path $buildPropsPath) {
    Write-Host "Updating Directory.Build.props..."
    $buildPropsContent = Get-Content $buildPropsPath -Raw

    # Update <Version>x.x.x</Version>
    $buildPropsContent = $buildPropsContent -replace '(<Version>)[^<]+(</Version>)', "`${1}$Version`${2}"

    Set-Content $buildPropsPath -Value $buildPropsContent -NoNewline
    Write-Host "✅ Updated Directory.Build.props"
} else {
    Write-Warning "Directory.Build.props not found at: $buildPropsPath"
}

Write-Host ""
Write-Host "✅ Version update complete: $Version"
