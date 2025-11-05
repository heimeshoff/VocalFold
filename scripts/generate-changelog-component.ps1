#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates Changelog.fs F# component from CHANGELOG.md
.DESCRIPTION
    Parses CHANGELOG.md and generates a Feliz/React component with the same structure as the existing Changelog.fs
#>

$ErrorActionPreference = "Stop"

# File paths
$changelogMdPath = Join-Path $PSScriptRoot "..\VocalFold.WebUI\src\CHANGELOG.md"
$changelogFsPath = Join-Path $PSScriptRoot "..\VocalFold.WebUI\src\Components\Changelog.fs"

if (-not (Test-Path $changelogMdPath)) {
    Write-Error "CHANGELOG.md not found at: $changelogMdPath"
    exit 1
}

# Read and parse CHANGELOG.md
$content = Get-Content $changelogMdPath -Raw

# Parse versions
$versionPattern = '(?s)##\s+\[([^\]]+)\]\s*-\s*(\d{4}-\d{2}-\d{2})\s*(.*?)(?=##\s+\[|$)'
$matches = [regex]::Matches($content, $versionPattern)

if ($matches.Count -eq 0) {
    Write-Warning "No version sections found in CHANGELOG.md"
    exit 1
}

Write-Host "Found $($matches.Count) version(s) in CHANGELOG.md"

# Function to generate F# code for a list item
function Get-ListItemCode {
    param([string]$text)
    $escaped = $text.Replace('"', '\"').Trim()
    return "                                                            Html.li `"$escaped`""
}

# Function to generate F# code for a category section
function Get-CategoryCode {
    param(
        [string]$categoryName,
        [string[]]$items
    )

    if ($items.Count -eq 0) { return "" }

    $itemsCode = $items | ForEach-Object { Get-ListItemCode $_ }
    $itemsJoined = $itemsCode -join "`n"

    return @"
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "$categoryName"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
$itemsJoined
                                                        ]
                                                    ]
                                                ]
                                            ]
"@
}

# Generate F# code for all versions
$versionSections = @()

foreach ($match in $matches) {
    $version = $match.Groups[1].Value
    $date = $match.Groups[2].Value
    $body = $match.Groups[3].Value

    Write-Host "Processing version: $version ($date)"

    # Parse categories
    $categories = @{
        "Added" = @()
        "Changed" = @()
        "Deprecated" = @()
        "Removed" = @()
        "Fixed" = @()
        "Security" = @()
    }

    # Parse category sections
    $categoryPattern = '(?s)###\s+(\w+)\s*(.*?)(?=###|\z)'
    $categoryMatches = [regex]::Matches($body, $categoryPattern)

    foreach ($catMatch in $categoryMatches) {
        $categoryName = $catMatch.Groups[1].Value
        $categoryBody = $catMatch.Groups[2].Value

        # Parse list items (lines starting with -)
        $items = $categoryBody -split '\n' | Where-Object { $_ -match '^\s*-\s+(.+)$' } | ForEach-Object {
            if ($_ -match '^\s*-\s+(.+)$') {
                $matches[0].Groups[1].Value
            }
        }

        if ($categories.ContainsKey($categoryName)) {
            $categories[$categoryName] = $items
        }
    }

    # Generate category sections
    $categorySections = @()
    foreach ($catName in @("Added", "Changed", "Deprecated", "Removed", "Fixed", "Security")) {
        $catCode = Get-CategoryCode -categoryName $catName -items $categories[$catName]
        if ($catCode) {
            $categorySections += $catCode
        }
    }

    $categoriesJoined = $categorySections -join "`n"

    # Generate version section
    $versionCode = @"
                            // Version $version
                            Html.div [
                                prop.className "mb-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-baseline gap-3 mb-4"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-2xl font-bold text-primary"
                                                prop.text "[$version]"
                                            ]
                                            Html.span [
                                                prop.className "text-text-secondary"
                                                prop.text "- $date"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
$categoriesJoined
                                        ]
                                    ]
                                ]
                            ]
"@

    $versionSections += $versionCode
}

# Join all versions with horizontal rules
$versionsWithSeparators = @()
for ($i = 0; $i -lt $versionSections.Count; $i++) {
    $versionsWithSeparators += $versionSections[$i]

    if ($i -lt $versionSections.Count - 1) {
        $versionsWithSeparators += @"

                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]
"@
    }
}

$allVersionsCode = $versionsWithSeparators -join "`n`n"

# Generate complete F# file
$fsCode = @"
module Components.Changelog

open Feliz
open Types
open Components.Card

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6 max-w-4xl mx-auto"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "Changelog"
            ]

            card {
                Title = ""
                ClassName = Some "bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20"
                Children = [
                    Html.div [
                        prop.className "prose prose-invert max-w-none"
                        prop.children [
$allVersionsCode

                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // Footer info
                            Html.div [
                                prop.className "text-sm text-text-secondary/70 space-y-2"
                                prop.children [
                                    Html.p [
                                        prop.children [
                                            Html.text "The format is based on "
                                            Html.a [
                                                prop.href "https://keepachangelog.com/en/1.1.0/"
                                                prop.target "_blank"
                                                prop.rel "noopener noreferrer"
                                                prop.className "text-primary hover:text-primary-600 transition-colors underline"
                                                prop.text "Keep a Changelog"
                                            ]
                                            Html.text ", and this project adheres to "
                                            Html.a [
                                                prop.href "https://semver.org/spec/v2.0.0.html"
                                                prop.target "_blank"
                                                prop.rel "noopener noreferrer"
                                                prop.className "text-primary hover:text-primary-600 transition-colors underline"
                                                prop.text "Semantic Versioning"
                                            ]
                                            Html.text "."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            }
        ]
    ]
"@

# Write the F# file
Set-Content -Path $changelogFsPath -Value $fsCode -NoNewline
Write-Host "✅ Changelog.fs generated successfully at: $changelogFsPath"
