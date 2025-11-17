module Components.KeywordManager

open Feliz
open Types
open Components.Card
open Components.Button

// ============================================================================
// Keyword Modal Component
// ============================================================================

[<ReactComponent>]
let private KeywordModal (keyword: KeywordReplacement option) (index: int option) (categories: KeywordCategory list) (dispatch: Msg -> unit) (onClose: unit -> unit) =
    let keywordInput, setKeywordInput = React.useState(keyword |> Option.map (fun k -> k.Keyword) |> Option.defaultValue "")
    let replacementInput, setReplacementInput = React.useState(keyword |> Option.map (fun k -> k.Replacement) |> Option.defaultValue "")
    let selectedCategory, setSelectedCategory = React.useState(keyword |> Option.bind (fun k -> k.Category) |> Option.defaultValue "Uncategorized")

    let isValid = not (System.String.IsNullOrWhiteSpace(keywordInput))

    let handleSave () =
        if isValid then
            let newKeyword = {
                Keyword = keywordInput.Trim()
                Replacement = replacementInput
                Category = if selectedCategory = "Uncategorized" then None else Some selectedCategory
                UsageCount = keyword |> Option.bind (fun k -> k.UsageCount)  // Preserve usage count
            }
            dispatch (SaveKeyword newKeyword)
            onClose()

    Html.div [
        prop.className "fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50"
        prop.onClick (fun e ->
            if e.target = e.currentTarget then
                onClose()
        )
        prop.children [
            Html.div [
                prop.className "bg-background-card rounded-lg shadow-2xl w-full max-w-2xl mx-4 animate-slide-up"
                prop.onClick (fun e -> e.stopPropagation())
                prop.children [
                    // Header
                    Html.div [
                        prop.className "flex items-center justify-between p-6 border-b border-white/10"
                        prop.children [
                            Html.h3 [
                                prop.className "text-2xl font-bold text-text-primary"
                                prop.text (if index.IsSome then "Edit Keyword" else "Add Keyword")
                            ]
                            Html.button [
                                prop.className "text-text-secondary hover:text-text-primary transition-colors text-2xl"
                                prop.text "×"
                                prop.onClick (fun _ -> onClose())
                            ]
                        ]
                    ]

                    // Body
                    Html.div [
                        prop.className "p-6 space-y-4"
                        prop.children [
                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-primary mb-2"
                                    prop.text "Keyword (what you say)"
                                ]
                                Html.input [
                                    prop.type'.text
                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                                    prop.placeholder "e.g., 'comma', 'German email footer'"
                                    prop.value keywordInput
                                    prop.onChange setKeywordInput
                                    prop.autoFocus true
                                ]
                            ]

                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-primary mb-2"
                                    prop.text "Replacement (what to type)"
                                ]
                                Html.textarea [
                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors font-mono"
                                    prop.placeholder "e.g., ',', 'Best regards,\\nJohn Doe'"
                                    prop.value replacementInput
                                    prop.onChange setReplacementInput
                                    prop.rows 4
                                ]
                            ]

                            // Category Dropdown
                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-primary mb-2"
                                    prop.text "Category"
                                ]
                                Html.select [
                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                                    prop.value selectedCategory
                                    prop.onChange setSelectedCategory
                                    prop.children (
                                        categories
                                        |> List.map (fun cat ->
                                            Html.option [
                                                prop.value cat.Name
                                                prop.text cat.Name
                                            ]
                                        )
                                    )
                                ]
                            ]

                            Html.div [
                                prop.className "bg-primary/10 border border-primary/30 rounded p-3"
                                prop.children [
                                    Html.p [
                                        prop.className "text-sm text-text-secondary"
                                        prop.text "Tip: Keywords are matched case-insensitively and as whole phrases. Use \\n for newlines in replacement text."
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Footer
                    Html.div [
                        prop.className "flex items-center justify-end space-x-3 p-6 border-t border-white/10"
                        prop.children [
                            Html.button [
                                prop.className "px-6 py-2 bg-transparent hover:bg-white/10 text-text-primary rounded-lg font-medium transition-all"
                                prop.text "Cancel"
                                prop.onClick (fun _ -> onClose())
                            ]
                            let saveButtonClass =
                                sprintf "px-6 py-2 rounded-lg font-medium transition-all %s"
                                    (if isValid then "bg-primary hover:bg-primary-600 text-white" else "bg-gray-500 text-gray-300 cursor-not-allowed")
                            Html.button [
                                prop.className saveButtonClass
                                prop.text "Save"
                                prop.disabled (not isValid)
                                prop.onClick (fun _ -> handleSave())
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ============================================================================
// Category Modal Component
// ============================================================================

// Predefined color palette
let private categoryColors = [
    "#25abfe" // Blue
    "#ff8b00" // Orange
    "#00d4aa" // Teal
    "#ff4757" // Red
    "#a55eea" // Purple
    "#26de81" // Green
    "#fd79a8" // Pink
    "#f39c12" // Yellow
    "#95afc0" // Gray
]

[<ReactComponent>]
let private CategoryModal (category: KeywordCategory option) (existingCategories: KeywordCategory list) (dispatch: Msg -> unit) (onClose: unit -> unit) =
    let nameInput, setNameInput = React.useState(category |> Option.map (fun c -> c.Name) |> Option.defaultValue "")
    let selectedColor, setSelectedColor = React.useState(category |> Option.bind (fun c -> c.Color) |> Option.defaultValue categoryColors.[0])

    let originalName = category |> Option.map (fun c -> c.Name)

    // Validation
    let isDuplicateName =
        existingCategories
        |> List.exists (fun c ->
            c.Name.ToLower() = nameInput.Trim().ToLower() &&
            (originalName.IsNone || originalName.Value.ToLower() <> nameInput.Trim().ToLower())
        )

    let isValid =
        not (System.String.IsNullOrWhiteSpace(nameInput)) &&
        not isDuplicateName &&
        nameInput.Trim() <> "Uncategorized"

    let validationMessage =
        if System.String.IsNullOrWhiteSpace(nameInput) then
            Some "Category name cannot be empty"
        elif isDuplicateName then
            Some "A category with this name already exists"
        elif nameInput.Trim() = "Uncategorized" then
            Some "Cannot use 'Uncategorized' as a category name"
        else
            None

    let handleSave () =
        if isValid then
            let newCategory = {
                Name = nameInput.Trim()
                IsExpanded = true
                Color = Some selectedColor
            }
            dispatch (SaveCategory newCategory)
            onClose()

    Html.div [
        prop.className "fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50"
        prop.onClick (fun e ->
            if e.target = e.currentTarget then
                onClose()
        )
        prop.children [
            Html.div [
                prop.className "bg-background-card rounded-lg shadow-2xl w-full max-w-lg mx-4 animate-slide-up"
                prop.onClick (fun e -> e.stopPropagation())
                prop.children [
                    // Header
                    Html.div [
                        prop.className "flex items-center justify-between p-6 border-b border-white/10"
                        prop.children [
                            Html.h3 [
                                prop.className "text-2xl font-bold text-text-primary"
                                prop.text (if category.IsSome then "Edit Category" else "Create Category")
                            ]
                            Html.button [
                                prop.className "text-text-secondary hover:text-text-primary transition-colors text-2xl"
                                prop.text "×"
                                prop.onClick (fun _ -> onClose())
                            ]
                        ]
                    ]

                    // Body
                    Html.div [
                        prop.className "p-6 space-y-6"
                        prop.children [
                            // Category Name
                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-primary mb-2"
                                    prop.text "Category Name"
                                ]
                                Html.input [
                                    prop.type'.text
                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                                    prop.placeholder "e.g., 'Punctuation', 'Email Templates'"
                                    prop.value nameInput
                                    prop.onChange setNameInput
                                    prop.autoFocus true
                                ]

                                // Validation message
                                match validationMessage with
                                | Some msg ->
                                    Html.p [
                                        prop.className "text-red-500 text-sm mt-2"
                                        prop.text msg
                                    ]
                                | None -> Html.none
                            ]

                            // Color Picker
                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-primary mb-3"
                                    prop.text "Category Color"
                                ]
                                Html.div [
                                    prop.className "grid grid-cols-5 gap-3"
                                    prop.children (
                                        categoryColors
                                        |> List.map (fun color ->
                                            Html.button [
                                                prop.key color
                                                prop.className (sprintf "w-12 h-12 rounded-lg transition-all %s"
                                                    (if color = selectedColor then "ring-2 ring-white ring-offset-2 ring-offset-background-card scale-110" else "hover:scale-105"))
                                                prop.style [ style.backgroundColor color ]
                                                prop.onClick (fun _ -> setSelectedColor color)
                                            ]
                                        )
                                    )
                                ]
                            ]

                            // Preview
                            Html.div [
                                prop.className "bg-primary/10 border border-primary/30 rounded-lg p-4"
                                prop.children [
                                    Html.p [
                                        prop.className "text-sm text-text-secondary mb-2"
                                        prop.text "Preview:"
                                    ]
                                    Html.div [
                                        prop.className "flex items-center space-x-2"
                                        prop.children [
                                            Html.div [
                                                prop.className "w-3 h-3 rounded-full"
                                                prop.style [ style.backgroundColor selectedColor ]
                                            ]
                                            Html.span [
                                                prop.className "text-text-primary font-semibold"
                                                prop.text (if System.String.IsNullOrWhiteSpace(nameInput) then "Category Name" else nameInput)
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Footer
                    Html.div [
                        prop.className "flex items-center justify-end space-x-3 p-6 border-t border-white/10"
                        prop.children [
                            Html.button [
                                prop.className "px-6 py-2 bg-transparent hover:bg-white/10 text-text-primary rounded-lg font-medium transition-all"
                                prop.text "Cancel"
                                prop.onClick (fun _ -> onClose())
                            ]
                            let saveButtonClass =
                                sprintf "px-6 py-2 rounded-lg font-medium transition-all %s"
                                    (if isValid then "bg-primary hover:bg-primary-600 text-white" else "bg-gray-500 text-gray-300 cursor-not-allowed")
                            Html.button [
                                prop.className saveButtonClass
                                prop.text "Save"
                                prop.disabled (not isValid)
                                prop.onClick (fun _ -> handleSave())
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ============================================================================
// Built-in Commands Section
// ============================================================================

let private commandsSection () =
    Html.div [
        prop.className "bg-background-card rounded-lg overflow-hidden border border-primary/30"
        prop.children [
            Html.div [
                prop.className "bg-primary/5 px-6 py-3 border-b border-primary/30"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-between"
                        prop.children [
                            Html.div [
                                Html.h3 [
                                    prop.className "text-lg font-bold text-text-primary"
                                    prop.text "Built-in Commands"
                                ]
                                Html.p [
                                    prop.className "text-text-secondary text-sm mt-1"
                                    prop.text "These commands are built into VocalFold and cannot be edited or deleted"
                                ]
                            ]
                            Html.span [
                                prop.className "px-3 py-1 bg-primary/20 text-primary text-xs font-semibold rounded-full"
                                prop.text "SYSTEM"
                            ]
                        ]
                    ]
                ]
            ]
            Html.table [
                prop.className "w-full"
                prop.children [
                    Html.thead [
                        prop.className "bg-background-sidebar"
                        prop.children [
                            Html.tr [
                                Html.th [
                                    prop.className "text-left px-6 py-3 text-sm font-semibold text-text-primary"
                                    prop.text "Command"
                                ]
                                Html.th [
                                    prop.className "text-left px-6 py-3 text-sm font-semibold text-text-primary"
                                    prop.text "Description"
                                ]
                            ]
                        ]
                    ]
                    Html.tbody [
                        prop.children [
                            Html.tr [
                                prop.className "border-t border-white/10"
                                prop.children [
                                    Html.td [
                                        prop.className "px-6 py-4"
                                        prop.children [
                                            Html.span [
                                                prop.className "font-mono text-primary font-medium"
                                                prop.text "open settings"
                                            ]
                                        ]
                                    ]
                                    Html.td [
                                        prop.className "px-6 py-4"
                                        prop.children [
                                            Html.span [
                                                prop.className "text-text-secondary text-sm"
                                                prop.text "Opens the VocalFold settings page"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.tr [
                                prop.className "border-t border-white/10"
                                prop.children [
                                    Html.td [
                                        prop.className "px-6 py-4"
                                        prop.children [
                                            Html.span [
                                                prop.className "font-mono text-primary font-medium"
                                                prop.text "repeat message"
                                            ]
                                        ]
                                    ]
                                    Html.td [
                                        prop.className "px-6 py-4"
                                        prop.children [
                                            Html.span [
                                                prop.className "text-text-secondary text-sm"
                                                prop.text "Types the last transcribed message again. Useful for repeating text without re-recording."
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ============================================================================
// Category Accordion Component
// ============================================================================

// Helper to get keyword indices by category
let private getKeywordsByCategory (keywords: KeywordReplacement list) (categoryName: string) =
    keywords
    |> List.mapi (fun i kw -> (i, kw))
    |> List.filter (fun (_, kw) ->
        match kw.Category with
        | Some cat -> cat = categoryName
        | None -> categoryName = "Uncategorized"
    )

// Category header component
let private categoryHeader (category: KeywordCategory) (keywordCount: int) (isExpanded: bool) (isDragOver: bool) (dispatch: Msg -> unit) (onDragOver: string -> Browser.Types.DragEvent -> unit) (onDragLeave: unit -> unit) (onDrop: string -> Browser.Types.DragEvent -> unit) =
    Html.div [
        prop.className (sprintf "bg-background-sidebar border border-white/10 rounded-lg p-4 hover:bg-white/5 transition-all cursor-pointer %s" (if isDragOver then "ring-2 ring-primary bg-primary/10" else ""))
        prop.onClick (fun _ -> dispatch (ToggleCategory category.Name))
        prop.onDragOver (fun e -> onDragOver category.Name e)
        prop.onDragLeave (fun _ -> onDragLeave())
        prop.onDrop (fun e -> onDrop category.Name e)
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    Html.div [
                        prop.className "flex items-center space-x-3"
                        prop.children [
                            // Expand/Collapse indicator
                            Html.span [
                                prop.className (sprintf "text-text-secondary text-xl transition-transform %s" (if isExpanded then "rotate-90" else ""))
                                prop.text "▶"
                            ]

                            // Color indicator
                            match category.Color with
                            | Some color ->
                                Html.div [
                                    prop.className "w-3 h-3 rounded-full"
                                    prop.style [ style.backgroundColor color ]
                                ]
                            | None -> Html.none

                            // Category name and count
                            Html.div [
                                Html.h3 [
                                    prop.className "text-lg font-semibold text-text-primary"
                                    prop.text category.Name
                                ]
                                Html.p [
                                    prop.className "text-sm text-text-secondary"
                                    prop.text (sprintf "%d keyword%s" keywordCount (if keywordCount = 1 then "" else "s"))
                                ]
                            ]
                        ]
                    ]

                    // Action buttons
                    Html.div [
                        prop.className "flex space-x-2"
                        prop.onClick (fun e -> e.stopPropagation()) // Prevent toggle when clicking buttons
                        prop.children [
                            if category.Name <> "Uncategorized" then
                                Html.button [
                                    prop.className "px-3 py-1 bg-primary/20 hover:bg-primary/30 text-primary rounded font-medium text-sm transition-all"
                                    prop.text "Edit"
                                    prop.onClick (fun _ -> dispatch (EditCategory category.Name))
                                ]
                                Html.button [
                                    prop.className "px-3 py-1 bg-red-500/20 hover:bg-red-500/30 text-red-500 rounded font-medium text-sm transition-all"
                                    prop.text "Delete"
                                    prop.onClick (fun _ -> dispatch (DeleteCategory category.Name))
                                ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Keyword row component
let private keywordRow (index: int) (keyword: KeywordReplacement) (isDragging: bool) (dispatch: Msg -> unit) (onEdit: int -> unit) (onDragStart: int -> unit) (onDragEnd: unit -> unit) =
    Html.div [
        prop.className (sprintf "border-t border-white/10 p-4 hover:bg-white/5 transition-colors cursor-move %s" (if isDragging then "opacity-50" else ""))
        prop.draggable true
        prop.onDragStart (fun e ->
            e.dataTransfer.effectAllowed <- "move"
            e.dataTransfer.setData("text/plain", string index) |> ignore
            onDragStart index
        )
        prop.onDragEnd (fun _ -> onDragEnd())
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    // Keyword
                    Html.div [
                        prop.className "flex-1"
                        prop.children [
                            Html.span [
                                prop.className "font-mono text-primary font-medium"
                                prop.text keyword.Keyword
                            ]
                        ]
                    ]

                    // Replacement (truncated if too long)
                    Html.div [
                        prop.className "flex-1 px-4"
                        prop.children [
                            Html.span [
                                prop.className "font-mono text-text-secondary text-sm"
                                prop.text (
                                    let text = keyword.Replacement.Replace("\n", "\\n")
                                    if text.Length > 50 then
                                        text.Substring(0, 47) + "..."
                                    else
                                        text
                                )
                            ]
                        ]
                    ]

                    // Usage Count Badge
                    Html.div [
                        prop.className "flex items-center px-2"
                        prop.children [
                            match keyword.UsageCount with
                            | Some count when count > 0 ->
                                Html.span [
                                    prop.className "px-2 py-1 text-xs font-medium bg-primary/10 text-primary rounded"
                                    prop.title (sprintf "Used %d time%s" count (if count = 1 then "" else "s"))
                                    prop.text (sprintf "×%d" count)
                                ]
                            | _ ->
                                Html.span [
                                    prop.className "px-2 py-1 text-xs font-medium text-text-secondary/50"
                                    prop.text "×0"
                                ]
                        ]
                    ]

                    // Actions
                    Html.div [
                        prop.className "flex space-x-2"
                        prop.children [
                            Html.button [
                                prop.className "px-3 py-1 bg-primary/20 hover:bg-primary/30 text-primary rounded font-medium text-sm transition-all"
                                prop.text "Edit"
                                prop.onClick (fun _ ->
                                    dispatch (EditKeyword index)
                                    onEdit index
                                )
                            ]
                            Html.button [
                                prop.className "px-3 py-1 bg-red-500/20 hover:bg-red-500/30 text-red-500 rounded font-medium text-sm transition-all"
                                prop.text "Delete"
                                prop.onClick (fun _ -> dispatch (DeleteKeyword index))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Category accordion section
[<ReactComponent>]
let private categoryAccordion (categories: KeywordCategory list) (keywords: KeywordReplacement list) (expandedCategories: Set<string>) (dispatch: Msg -> unit) (onEdit: int -> unit) =
    let draggingKeywordIndex, setDraggingKeywordIndex = React.useState<int option>(None)
    let dragOverCategory, setDragOverCategory = React.useState<string option>(None)

    let handleDragStart (index: int) =
        setDraggingKeywordIndex (Some index)

    let handleDragEnd () =
        setDraggingKeywordIndex None
        setDragOverCategory None

    let handleDragOver (categoryName: string) (e: Browser.Types.DragEvent) =
        e.preventDefault()
        e.dataTransfer.dropEffect <- "move"
        setDragOverCategory (Some categoryName)

    let handleDragLeave () =
        setDragOverCategory None

    let handleDrop (categoryName: string) (e: Browser.Types.DragEvent) =
        e.preventDefault()

        match draggingKeywordIndex with
        | Some index ->
            let targetCategory = if categoryName = "Uncategorized" then None else Some categoryName
            dispatch (MoveKeywordToCategory (index, targetCategory))
            setDraggingKeywordIndex None
            setDragOverCategory None
        | None -> ()

    if keywords.IsEmpty then
        Html.div [
            prop.className "bg-background-card rounded-lg p-12 text-center"
            prop.children [
                Html.div [
                    prop.className "text-6xl mb-4"
                    prop.text "📝"
                ]
                Html.h3 [
                    prop.className "text-xl font-semibold text-text-primary mb-2"
                    prop.text "No keywords configured"
                ]
                Html.p [
                    prop.className "text-text-secondary mb-6"
                    prop.text "Add your first keyword to get started. Keywords let you quickly insert frequently used text."
                ]
                Html.button [
                    prop.className "px-6 py-2 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2"
                    prop.children [
                        Html.span "+"
                        Html.span "Add Keyword"
                    ]
                    prop.onClick (fun _ -> onEdit -1)
                ]
            ]
        ]
    else
        Html.div [
            prop.className "space-y-4"
            prop.children (
                categories
                |> List.map (fun category ->
                    let categoryKeywords = getKeywordsByCategory keywords category.Name
                    let isExpanded = expandedCategories.Contains category.Name

                    Html.div [
                        prop.key (sprintf "category-%s" category.Name)
                        prop.className "bg-background-card rounded-lg overflow-hidden"
                        prop.children [
                            // Category header
                            let isDragOver = dragOverCategory = Some category.Name
                            categoryHeader category categoryKeywords.Length isExpanded isDragOver dispatch handleDragOver handleDragLeave handleDrop

                            // Keyword list (shown when expanded)
                            if isExpanded then
                                Html.div [
                                    prop.className "animate-slide-down"
                                    prop.children (
                                        if categoryKeywords.IsEmpty then
                                            [
                                                Html.div [
                                                    prop.className "p-8 text-center text-text-secondary"
                                                    prop.text "No keywords in this category"
                                                ]
                                            ]
                                        else
                                            categoryKeywords
                                            |> List.map (fun (index, kw) ->
                                                let isDragging = draggingKeywordIndex = Some index
                                                keywordRow index kw isDragging dispatch onEdit handleDragStart handleDragEnd
                                            )
                                    )
                                ]
                        ]
                    ]
                )
            )
        ]

// ============================================================================
// Legacy Keyword Table Component (kept for reference, will be removed)
// ============================================================================

let private keywordTable (keywords: KeywordReplacement list) (dispatch: Msg -> unit) (onEdit: int -> unit) =
    if keywords.IsEmpty then
        Html.div [
            prop.className "bg-background-card rounded-lg p-12 text-center"
            prop.children [
                Html.div [
                    prop.className "text-6xl mb-4"
                    prop.text "📝"
                ]
                Html.h3 [
                    prop.className "text-xl font-semibold text-text-primary mb-2"
                    prop.text "No keywords configured"
                ]
                Html.p [
                    prop.className "text-text-secondary mb-6"
                    prop.text "Add your first keyword to get started. Keywords let you quickly insert frequently used text."
                ]
                Html.button [
                    prop.className "px-6 py-2 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2"
                    prop.children [
                        Html.span "+"
                        Html.span "Add Keyword"
                    ]
                    prop.onClick (fun _ -> onEdit -1)
                ]
            ]
        ]
    else
        Html.div [
            prop.className "bg-background-card rounded-lg overflow-hidden"
            prop.children [
                Html.table [
                    prop.className "w-full"
                    prop.children [
                        Html.thead [
                            prop.className "bg-background-sidebar"
                            prop.children [
                                Html.tr [
                                    Html.th [
                                        prop.className "text-left px-6 py-3 text-sm font-semibold text-text-primary"
                                        prop.text "Keyword"
                                    ]
                                    Html.th [
                                        prop.className "text-left px-6 py-3 text-sm font-semibold text-text-primary"
                                        prop.text "Replacement"
                                    ]
                                    Html.th [
                                        prop.className "text-right px-6 py-3 text-sm font-semibold text-text-primary"
                                        prop.text "Actions"
                                    ]
                                ]
                            ]
                        ]
                        Html.tbody [
                            prop.children (
                                keywords
                                |> List.mapi (fun i keyword ->
                                    Html.tr [
                                        prop.key (sprintf "keyword-%d" i)
                                        prop.className "border-t border-white/10 hover:bg-white/5 transition-colors"
                                        prop.children [
                                            Html.td [
                                                prop.className "px-6 py-4"
                                                prop.children [
                                                    Html.span [
                                                        prop.className "font-mono text-primary font-medium"
                                                        prop.text keyword.Keyword
                                                    ]
                                                ]
                                            ]
                                            Html.td [
                                                prop.className "px-6 py-4"
                                                prop.children [
                                                    Html.span [
                                                        prop.className "font-mono text-text-secondary text-sm"
                                                        prop.text (
                                                            let text = keyword.Replacement.Replace("\n", "\\n")
                                                            if text.Length > 50 then
                                                                text.Substring(0, 47) + "..."
                                                            else
                                                                text
                                                        )
                                                    ]
                                                ]
                                            ]
                                            Html.td [
                                                prop.className "px-6 py-4 text-right"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "inline-flex space-x-2"
                                                        prop.children [
                                                            Html.button [
                                                                prop.className "px-3 py-1 bg-primary/20 hover:bg-primary/30 text-primary rounded font-medium text-sm transition-all"
                                                                prop.text "Edit"
                                                                prop.onClick (fun _ ->
                                                                    dispatch (EditKeyword i)
                                                                    onEdit i
                                                                )
                                                            ]
                                                            Html.button [
                                                                prop.className "px-3 py-1 bg-red-500/20 hover:bg-red-500/30 text-red-500 rounded font-medium text-sm transition-all"
                                                                prop.text "Delete"
                                                                prop.onClick (fun _ -> dispatch (DeleteKeyword i))
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                )
                            )
                        ]
                    ]
                ]
            ]
        ]

// ============================================================================
// Main View
// ============================================================================

let view (settings: LoadingState<AppSettings>) (editingKeyword: (int * KeywordReplacement) option) (editingCategory: KeywordCategory option) (expandedCategories: Set<string>) (dispatch: Msg -> unit) =

    let showKeywordModal = editingKeyword.IsSome
    let editIndex = editingKeyword |> Option.map fst
    let editKeyword = editingKeyword |> Option.map snd

    let showCategoryModal = editingCategory.IsSome

    let handleEdit (index: int) =
        match settings with
        | LoadingState.Loaded s when index >= 0 && index < s.KeywordReplacements.Length ->
            dispatch (EditKeyword index)
        | LoadingState.Loaded _ when index = -1 ->
            dispatch AddKeyword
        | _ -> ()

    let handleCloseKeywordModal () =
        dispatch CancelEditKeyword

    let handleCloseCategoryModal () =
        dispatch CancelEditCategory

    Html.div [
        prop.className "space-y-6"
        prop.children [
            // Header
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    Html.div [
                        Html.h2 [
                            prop.className "text-3xl font-bold text-text-primary mb-2"
                            prop.text "Keyword Replacements"
                        ]
                        Html.p [
                            prop.className "text-text-secondary"
                            prop.text "Configure keywords that will be automatically replaced in transcriptions"
                        ]
                    ]
                    match settings with
                    | LoadingState.Loaded s when not s.KeywordReplacements.IsEmpty || not s.Categories.IsEmpty ->
                        Html.div [
                            prop.className "flex space-x-3"
                            prop.children [
                                Html.button [
                                    prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2"
                                    prop.children [
                                        Html.span "📁"
                                        Html.span "Manage Categories"
                                    ]
                                    prop.onClick (fun _ -> dispatch AddCategory)
                                ]
                                Html.button [
                                    prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2"
                                    prop.children [
                                        Html.span "✨"
                                        Html.span "Add Examples"
                                    ]
                                    prop.onClick (fun _ -> dispatch AddExampleKeywords)
                                ]
                                Html.button [
                                    prop.className "px-4 py-2 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all inline-flex items-center space-x-2"
                                    prop.children [
                                        Html.span "+"
                                        Html.span "Add Keyword"
                                    ]
                                    prop.onClick (fun _ -> handleEdit -1)
                                ]
                            ]
                        ]
                    | _ -> Html.none
                ]
            ]

            // Info Box
            Html.div [
                prop.className "bg-primary/10 border border-primary/30 rounded-lg p-4"
                prop.children [
                    Html.div [
                        prop.className "flex items-start space-x-3"
                        prop.children [
                            Html.div [
                                prop.className "text-primary text-xl"
                                prop.text "ℹ️"
                            ]
                            Html.div [
                                Html.p [
                                    prop.className "text-text-primary font-medium mb-1"
                                    prop.text "How keyword replacements work"
                                ]
                                Html.p [
                                    prop.className "text-text-secondary text-sm"
                                    prop.text "When you speak a keyword, it will be automatically replaced with the configured text. For example, saying 'comma' can be replaced with ','. Keywords are matched case-insensitively."
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // Built-in Commands Section
            commandsSection()

            // Keyword Replacements Header
            Html.div [
                prop.className "flex items-center justify-between pt-4"
                prop.children [
                    Html.h3 [
                        prop.className "text-2xl font-bold text-text-primary"
                        prop.text "Custom Keyword Replacements"
                    ]
                ]
            ]

            // Content
            match settings with
            | LoadingState.Loaded s ->
                categoryAccordion s.Categories s.KeywordReplacements expandedCategories dispatch handleEdit
            | LoadingState.Loading ->
                Html.div [
                    prop.className "flex items-center justify-center py-12"
                    prop.children [
                        Html.div [
                            prop.className "animate-spin rounded-full h-12 w-12 border-b-2 border-primary"
                        ]
                    ]
                ]
            | LoadingState.Error err ->
                Html.div [
                    prop.className "bg-red-500/10 border border-red-500/30 rounded p-6"
                    prop.children [
                        Html.p [
                            prop.className "text-red-500"
                            prop.text (sprintf "Error loading keywords: %s" err)
                        ]
                    ]
                ]
            | LoadingState.NotStarted ->
                Html.div [
                    prop.className "flex items-center justify-center py-12"
                    prop.children [
                        Html.div [
                            prop.className "animate-spin rounded-full h-12 w-12 border-b-2 border-primary"
                        ]
                    ]
                ]

            // Modals
            match settings with
            | LoadingState.Loaded s ->
                // Keyword Modal
                if showKeywordModal then
                    KeywordModal editKeyword editIndex s.Categories dispatch handleCloseKeywordModal

                // Category Modal
                if showCategoryModal then
                    CategoryModal editingCategory s.Categories dispatch handleCloseCategoryModal
            | _ -> Html.none
        ]
    ]
