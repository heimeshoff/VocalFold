# Update Changelog Skill

You are a specialized skill for updating the VocalFold project's changelog. Your job is to:

1. Help the user update the CHANGELOG.md file
2. Automatically regenerate the Changelog.fs component to reflect the changes

## Instructions

When this skill is invoked, follow these steps:

### Step 1: Understand the Update Request
Ask the user what changes they want to make to the changelog:
- Are they adding a new version release?
- Are they updating the Unreleased section?
- Are they adding specific changes (Added, Changed, Fixed, Removed)?

### Step 2: Update CHANGELOG.md
Update the `VocalFold.WebUI/src/CHANGELOG.md` file with the requested changes, following the Keep a Changelog format:
- Use semantic versioning for version numbers
- Include the date in YYYY-MM-DD format
- Organize changes into categories: Added, Changed, Fixed, Removed
- Update the comparison links at the bottom

### Step 3: Parse and Generate Changelog.fs
After updating CHANGELOG.md, automatically regenerate `VocalFold.WebUI/src/Components/Changelog.fs`:

1. Read the CHANGELOG.md file
2. Parse it to extract:
   - Versions and their dates
   - Change categories and their items
   - Links
3. Generate the F# Feliz code that renders this as HTML

The generated code should:
- Use the same styling as the current Changelog.fs (cards, colors, typography)
- Render each version as a section
- Show each change category (Added, Changed, Fixed, Removed) with its items
- Include the footer with links to Keep a Changelog and Semantic Versioning

### Step 4: Verify
After updating both files:
1. Show the user a summary of what was changed
2. Confirm both files are in sync

## Key Requirements

- **Keep the Changelog Format**: Follow [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format strictly
- **Semantic Versioning**: Use [Semantic Versioning](https://semver.org/spec/v2.0.0.html) for version numbers
- **Styling Consistency**: The Changelog.fs component must match the existing design system:
  - Use `Components.Card` module
  - Use Tailwind classes for styling
  - Use the color scheme: `text-primary`, `text-secondary`, `text-text-primary`, `text-text-secondary`
  - Use proper spacing and typography classes
- **Synchronization**: CHANGELOG.md is the source of truth, and Changelog.fs must accurately reflect its content

## Example Usage

**User**: "Add a new feature to the unreleased section: Dark mode support"

**Expected Action**:
1. Add "Dark mode support" to the "Added" section under [Unreleased] in CHANGELOG.md
2. Regenerate Changelog.fs to include this item in the rendered HTML

## Parser Logic

When parsing CHANGELOG.md:
- H2 headers (`##`) are version sections
- H3 headers (`###`) are change categories (Added, Changed, Fixed, Removed)
- List items (`-`) are individual changes
- Dates are in the format `[Version] - YYYY-MM-DD`
- The [Unreleased] section has no date

## F# Code Generation Guidelines

The Changelog.fs component structure should be:
```fsharp
module Components.Changelog

open Feliz
open Types
open Components.Card

let view (dispatch: Msg -> unit) =
    Html.div [
        // Header
        // Card containing all versions
        // Each version as a section with:
        //   - Version number and date
        //   - Change categories (Added, Changed, Fixed, Removed)
        //   - List of items per category
        // Footer with links
    ]
```

Use proper F# list syntax and Feliz HTML DSL. Generate code that is clean, readable, and maintainable.

## Important Notes

- Always maintain both files in sync
- Preserve the existing styling and structure
- If adding a new version, move items from Unreleased to the new version section
- Update the comparison links at the bottom of CHANGELOG.md when adding new versions
- The generated F# code must compile without errors
