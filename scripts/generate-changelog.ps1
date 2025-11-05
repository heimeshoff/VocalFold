#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates CHANGELOG.md from git commits using conventional commit format
.DESCRIPTION
    Parses git commits between the current tag and previous tag, categorizes them
    into Added, Changed, Fixed, etc., and updates CHANGELOG.md
.PARAMETER Version
    The version being released (e.g., "1.2.0")
.PARAMETER PreviousTag
    Optional. The previous git tag to compare against. If not provided, will find the latest tag automatically.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$PreviousTag = ""
)

$ErrorActionPreference = "Stop"

# Determine the changelog file path
$changelogPath = Join-Path $PSScriptRoot "..\VocalFold.WebUI\src\CHANGELOG.md"

# Get the previous tag if not provided
if ([string]::IsNullOrEmpty($PreviousTag)) {
    $tags = git tag --sort=-v:refname
    if ($tags) {
        $PreviousTag = $tags | Select-Object -First 1
        Write-Host "Found previous tag: $PreviousTag"
    } else {
        Write-Host "No previous tag found. Using all commits."
        $PreviousTag = ""
    }
}

# Get commits since previous tag or all commits if no previous tag
$commitRange = if ([string]::IsNullOrEmpty($PreviousTag)) { "HEAD" } else { "$PreviousTag..HEAD" }
Write-Host "Getting commits from range: $commitRange"

$commits = git log $commitRange --pretty=format:"%s|||%b|||%H" --no-merges

if (-not $commits) {
    Write-Host "No commits found in range $commitRange"
    exit 0
}

# Parse commits into categories
$added = @()
$changed = @()
$fixed = @()
$removed = @()
$security = @()
$deprecated = @()
$other = @()

foreach ($commitLine in $commits) {
    if ([string]::IsNullOrWhiteSpace($commitLine)) { continue }

    $parts = $commitLine -split '\|\|\|', 3
    $subject = $parts[0].Trim()
    $body = if ($parts.Length -gt 1) { $parts[1].Trim() } else { "" }
    $hash = if ($parts.Length -gt 2) { $parts[2].Trim() } else { "" }

    # Parse conventional commit format
    if ($subject -match '^(feat|feature)(\(.+?\))?:\s*(.+)$') {
        $added += $matches[3].Trim()
    }
    elseif ($subject -match '^fix(\(.+?\))?:\s*(.+)$') {
        $fixed += $matches[2].Trim()
    }
    elseif ($subject -match '^(chore|refactor|perf|style)(\(.+?\))?:\s*(.+)$') {
        $changed += $matches[3].Trim()
    }
    elseif ($subject -match '^docs(\(.+?\))?:\s*(.+)$') {
        $changed += $matches[2].Trim()
    }
    elseif ($subject -match '^(remove|delete)(\(.+?\))?:\s*(.+)$') {
        $removed += $matches[3].Trim()
    }
    elseif ($subject -match '^security(\(.+?\))?:\s*(.+)$') {
        $security += $matches[2].Trim()
    }
    elseif ($subject -match '^deprecate(\(.+?\))?:\s*(.+)$') {
        $deprecated += $matches[2].Trim()
    }
    else {
        # If no conventional commit prefix, add to "Changed" category
        $changed += $subject
    }
}

# Build the new changelog section
$date = Get-Date -Format "yyyy-MM-dd"
$newSection = @()
$newSection += "## [$Version] - $date"
$newSection += ""

if ($added.Count -gt 0) {
    $newSection += "### Added"
    foreach ($item in $added) {
        $newSection += "- $item"
    }
    $newSection += ""
}

if ($changed.Count -gt 0) {
    $newSection += "### Changed"
    foreach ($item in $changed) {
        $newSection += "- $item"
    }
    $newSection += ""
}

if ($deprecated.Count -gt 0) {
    $newSection += "### Deprecated"
    foreach ($item in $deprecated) {
        $newSection += "- $item"
    }
    $newSection += ""
}

if ($removed.Count -gt 0) {
    $newSection += "### Removed"
    foreach ($item in $removed) {
        $newSection += "- $item"
    }
    $newSection += ""
}

if ($fixed.Count -gt 0) {
    $newSection += "### Fixed"
    foreach ($item in $fixed) {
        $newSection += "- $item"
    }
    $newSection += ""
}

if ($security.Count -gt 0) {
    $newSection += "### Security"
    foreach ($item in $security) {
        $newSection += "- $item"
    }
    $newSection += ""
}

# Read existing changelog
if (Test-Path $changelogPath) {
    $existingContent = Get-Content $changelogPath -Raw

    # Find where to insert the new section (after # Changelog header)
    if ($existingContent -match '(?s)(# Changelog\s*\r?\n\r?\n)(.*)') {
        $header = $matches[1]
        $rest = $matches[2]

        # Combine: header + new section + rest
        $newContent = $header + ($newSection -join "`n") + "`n`n" + $rest
    } else {
        # No header found, create one
        $newContent = "# Changelog`n`n" + ($newSection -join "`n") + "`n`n" + $existingContent
    }
} else {
    # Create new changelog file
    $newContent = "# Changelog`n`n" + ($newSection -join "`n") + "`n"
}

# Write the updated changelog
Set-Content -Path $changelogPath -Value $newContent -NoNewline
Write-Host "✅ Changelog updated at: $changelogPath"
Write-Host ""
Write-Host "New section:"
Write-Host "============"
$newSection | ForEach-Object { Write-Host $_ }
