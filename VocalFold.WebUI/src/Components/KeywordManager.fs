module Components.KeywordManager

open Feliz
open Types
open Components.Card
open Components.Button

// ============================================================================
// Keyword Modal Component
// ============================================================================

let private keywordModal (keyword: KeywordReplacement option) (index: int option) (dispatch: Msg -> unit) (onClose: unit -> unit) =
    let keywordInput, setKeywordInput = React.useState(keyword |> Option.map (fun k -> k.Keyword) |> Option.defaultValue "")
    let replacementInput, setReplacementInput = React.useState(keyword |> Option.map (fun k -> k.Replacement) |> Option.defaultValue "")

    let isValid = not (System.String.IsNullOrWhiteSpace(keywordInput))

    let handleSave () =
        if isValid then
            let newKeyword = {
                Keyword = keywordInput.Trim()
                Replacement = replacementInput
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
// Keyword Table Component
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

let view (settings: LoadingState<AppSettings>) (editingKeyword: (int * KeywordReplacement) option) (dispatch: Msg -> unit) =
    let showModal, setShowModal = React.useState(false)
    let editIndex, setEditIndex = React.useState<int option>(None)
    let editKeyword, setEditKeyword = React.useState<KeywordReplacement option>(None)

    let handleEdit (index: int) =
        match settings with
        | LoadingState.Loaded s when index >= 0 && index < s.KeywordReplacements.Length ->
            setEditIndex (Some index)
            setEditKeyword (Some s.KeywordReplacements.[index])
            setShowModal true
        | LoadingState.Loaded _ when index = -1 ->
            setEditIndex None
            setEditKeyword None
            setShowModal true
        | _ -> ()

    let handleCloseModal () =
        setShowModal false
        setEditIndex None
        setEditKeyword None

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
                    | LoadingState.Loaded s when not s.KeywordReplacements.IsEmpty ->
                        Html.div [
                            prop.className "flex space-x-3"
                            prop.children [
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

            // Content
            match settings with
            | LoadingState.Loaded s ->
                keywordTable s.KeywordReplacements dispatch handleEdit
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

            // Modal
            if showModal then
                keywordModal editKeyword editIndex dispatch handleCloseModal
        ]
    ]
