# Changelog Parser and Generator

This document provides the parsing logic and F# code generation patterns for the update-changelog skill.

## Markdown Parser Logic

### Structure Recognition

```
# Changelog                          -> Title (ignore)
## [Version] - Date or ## [Unreleased] -> Version block
### Added/Changed/Fixed/Removed      -> Category header
- Item text                          -> Change item
[Version]: URL                       -> Link reference (footer)
```

### Regular Expression Patterns

**Version Header**: `^##\s+\[([^\]]+)\](?:\s+-\s+(.+))?$`
- Group 1: Version number/name
- Group 2: Date (optional, missing for Unreleased)

**Category Header**: `^###\s+(Added|Changed|Fixed|Removed|Deprecated|Security)$`
- Group 1: Category name

**List Item**: `^-\s+(.+)$`
- Group 1: Item text

**Link Reference**: `^\[([^\]]+)\]:\s+(.+)$`
- Group 1: Version reference
- Group 2: URL

## F# Code Generation Template

### Basic Structure

```fsharp
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
                            // VERSION BLOCKS GO HERE

                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // FOOTER LINKS
                        ]
                    ]
                ]
            }
        ]
    ]
```

### Version Block Template

For Unreleased (no date):
```fsharp
Html.div [
    prop.className "mb-8"
    prop.children [
        Html.h3 [
            prop.className "text-2xl font-bold text-primary mb-4"
            prop.text "[Unreleased]"
        ]
        // CATEGORIES GO HERE
    ]
]
```

For Released version (with date):
```fsharp
Html.div [
    prop.className "mb-8"
    prop.children [
        Html.div [
            prop.className "flex items-baseline gap-3 mb-4"
            prop.children [
                Html.h3 [
                    prop.className "text-2xl font-bold text-primary"
                    prop.text "[VERSION]"
                ]
                Html.span [
                    prop.className "text-text-secondary"
                    prop.text "- DATE"
                ]
            ]
        ]
        // CATEGORIES GO HERE
    ]
]
```

### Category Block Template

```fsharp
Html.div [
    prop.className "space-y-3"
    prop.children [
        Html.div [
            prop.children [
                Html.h4 [
                    prop.className "text-lg font-semibold text-text-primary mb-2"
                    prop.text "CATEGORY_NAME"
                ]
                Html.ul [
                    prop.className "list-disc list-inside text-text-secondary space-y-1"
                    prop.children [
                        // LIST ITEMS GO HERE
                    ]
                ]
            ]
        ]
    ]
]
```

### List Item Template

```fsharp
Html.li "ITEM_TEXT"
```

### Footer Template

```fsharp
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
```

## Generation Algorithm

1. **Read CHANGELOG.md** and split into lines
2. **Initialize state**:
   - Current version = null
   - Current category = null
   - Versions list = []
3. **Parse line by line**:
   - If version header: Create new version object
   - If category header: Set current category
   - If list item: Add to current category in current version
   - Skip other lines
4. **Generate F# code**:
   - Start with module header and imports
   - Generate header HTML
   - For each version: Generate version block
   - Between versions: Add HR separators (except last)
   - Add footer
5. **Write to Changelog.fs**

## Escape Rules

When generating F# string literals:
- Double quotes in markdown need to be escaped or use different syntax
- Use `prop.text` for simple strings
- Use `prop.children` with `Html.text` for complex content
- Preserve special characters in change descriptions

## Edge Cases

1. **Empty categories**: Skip rendering categories with no items
2. **No date**: Unreleased section has no date field
3. **Multiple versions**: Add HR separator between each version
4. **Special characters**: Escape properly in F# strings
5. **Links in text**: Parse markdown links and convert to `Html.a` elements if needed

## Example Transformation

**Markdown**:
```markdown
## [1.0.1] - 2025-11-05
### Added
- New feature X
- Another feature Y
### Fixed
- Bug fix Z
```

**Generated F#**:
```fsharp
Html.div [
    prop.className "mb-8"
    prop.children [
        Html.div [
            prop.className "flex items-baseline gap-3 mb-4"
            prop.children [
                Html.h3 [
                    prop.className "text-2xl font-bold text-primary"
                    prop.text "[1.0.1]"
                ]
                Html.span [
                    prop.className "text-text-secondary"
                    prop.text "- 2025-11-05"
                ]
            ]
        ]
        Html.div [
            prop.className "space-y-3"
            prop.children [
                Html.div [
                    prop.children [
                        Html.h4 [
                            prop.className "text-lg font-semibold text-text-primary mb-2"
                            prop.text "Added"
                        ]
                        Html.ul [
                            prop.className "list-disc list-inside text-text-secondary space-y-1"
                            prop.children [
                                Html.li "New feature X"
                                Html.li "Another feature Y"
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.children [
                        Html.h4 [
                            prop.className "text-lg font-semibold text-text-primary mb-2"
                            prop.text "Fixed"
                        ]
                        Html.ul [
                            prop.className "list-disc list-inside text-text-secondary space-y-1"
                            prop.children [
                                Html.li "Bug fix Z"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
]
```

This template and logic should be used by the update-changelog skill to automatically generate the Changelog.fs component from CHANGELOG.md.
