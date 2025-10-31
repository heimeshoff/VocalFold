module Components.GeneralSettings

open Feliz
open Types
open Components.Card
open Components.Button

// ============================================================================
// Start with Windows Toggle Component
// ============================================================================

let private startWithWindowsToggle (isEnabled: bool) (dispatch: Msg -> unit) (settings: AppSettings) =
    card {
        Title = "Startup"
        ClassName = None
        Children = [
            Html.div [
                prop.className "space-y-3"
                prop.children [
                    Html.label [
                        prop.className "flex items-start space-x-3 p-3 rounded border border-text-secondary/30 hover:border-primary/50 cursor-pointer transition-colors"
                        prop.children [
                            Html.input [
                                prop.type'.checkbox
                                prop.isChecked isEnabled
                                prop.onChange (fun (isChecked: bool) ->
                                    let updatedSettings = { settings with StartWithWindows = isChecked }
                                    dispatch (UpdateSettings updatedSettings)
                                )
                                prop.className "mt-1"
                            ]
                            Html.div [
                                Html.p [
                                    prop.className "font-medium text-text-primary"
                                    prop.text "Start with Windows"
                                ]
                                Html.p [
                                    prop.className "text-sm text-text-secondary"
                                    prop.text "Automatically launch VocalFold when Windows starts"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    }

// ============================================================================
// Typing Speed Selector Component
// ============================================================================

let private typingSpeedSelector (currentSpeed: string) (dispatch: Msg -> unit) (settings: AppSettings) =
    let speeds = [
        ("fast", "Fast (no delay)", "Best for quick typing")
        ("normal", "Normal (10ms delay)", "Recommended for most users")
        ("slow", "Slow (20ms delay)", "More reliable for older systems")
    ]

    card {
        Title = "Typing Speed"
        ClassName = None
        Children = [
            Html.div [
                prop.className "space-y-3"
                prop.children (
                    speeds |> List.map (fun (value, label, desc) ->
                        Html.label [
                            prop.className "flex items-start space-x-3 p-3 rounded border border-text-secondary/30 hover:border-primary/50 cursor-pointer transition-colors"
                            prop.children [
                                Html.input [
                                    prop.type'.radio
                                    prop.name "typingSpeed"
                                    prop.value value
                                    prop.isChecked (currentSpeed.ToLower() = value)
                                    prop.onChange (fun (isChecked: bool) ->
                                        if isChecked then
                                            let updatedSettings = { settings with TypingSpeed = value }
                                            dispatch (UpdateSettings updatedSettings)
                                    )
                                    prop.className "mt-1"
                                ]
                                Html.div [
                                    Html.p [
                                        prop.className "font-medium text-text-primary"
                                        prop.text label
                                    ]
                                    Html.p [
                                        prop.className "text-sm text-text-secondary"
                                        prop.text desc
                                    ]
                                ]
                            ]
                        ]
                    )
                )
            ]
        ]
    }

// ============================================================================
// Keywords File Location Component
// ============================================================================

let private keywordsFileLocation (keywordsPath: LoadingState<KeywordsPathInfo>) (editingKeywordsPath: string option) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "bg-background-card rounded-lg shadow-lg p-6"
        prop.children [
            Html.h3 [
                prop.className "text-xl font-semibold mb-2 text-text-primary"
                prop.text "☁️ Keywords File Location"
            ]
            Html.p [
                prop.className "text-text-secondary text-sm mb-4"
                prop.text "Configure where your keywords are stored. Use cloud storage (Google Drive, OneDrive, Dropbox) to sync keywords across multiple computers."
            ]
            match keywordsPath with
            | LoadingState.Loaded pathInfo ->
                Html.div [
                    prop.className "space-y-4"
                    prop.children [
                        // Current Path Display
                        Html.div [
                            prop.className "space-y-2"
                            prop.children [
                                Html.label [
                                    prop.className "block text-sm font-medium text-text-secondary"
                                    prop.text "Current Keywords File:"
                                ]
                                match editingKeywordsPath with
                                | Some path ->
                                    // Editing mode
                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-sm font-medium text-text-secondary mb-2"
                                                    prop.text "Enter the full path to your keywords.json file:"
                                                ]
                                                Html.input [
                                                    prop.type' "text"
                                                    prop.value path
                                                    prop.onChange (fun (value: string) -> dispatch (UpdateEditingKeywordsPath value))
                                                    prop.className "w-full px-4 py-2 bg-background-dark border border-white/10 rounded-lg text-text-primary placeholder-text-secondary focus:outline-none focus:border-primary font-mono text-sm"
                                                    prop.placeholder "C:\\Users\\YourName\\Google Drive\\VocalFold\\keywords.json"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs text-text-secondary mt-2"
                                                    prop.text "💡 Example cloud paths: Google Drive: C:\\Users\\YourName\\Google Drive\\VocalFold\\keywords.json | OneDrive: C:\\Users\\YourName\\OneDrive\\VocalFold\\keywords.json"
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "flex space-x-2"
                                                prop.children [
                                                    Button.primaryButton "💾 Save" (fun () -> dispatch SaveKeywordsPath)
                                                    Button.secondaryButton "❌ Cancel" (fun () -> dispatch CancelEditingKeywordsPath)
                                                ]
                                            ]
                                        ]
                                    ]
                                | None ->
                                    // Display mode
                                    Html.div [
                                        prop.className "flex items-center justify-between gap-3"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex-1 flex items-center gap-2 px-4 py-2 bg-background-dark border border-white/10 rounded-lg"
                                                prop.children [
                                                    Html.span [
                                                        prop.className (if pathInfo.IsDefault then "text-text-secondary" else "text-primary")
                                                        prop.text (if pathInfo.IsDefault then "📁" else "☁️")
                                                    ]
                                                    Html.code [
                                                        prop.className "text-sm text-text-primary flex-1"
                                                        prop.text pathInfo.CurrentPath
                                                    ]
                                                    if not pathInfo.IsDefault then
                                                        Html.span [
                                                            prop.className "text-xs bg-primary/20 text-primary px-2 py-1 rounded"
                                                            prop.text "Cloud Synced"
                                                        ]
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "flex space-x-2"
                                                prop.children [
                                                    Button.secondaryButton "✏️ Edit Path" (fun () -> dispatch StartEditingKeywordsPath)
                                                    if not pathInfo.IsDefault then
                                                        Button.secondaryButton "🔄 Reset to Default" (fun () -> dispatch ResetKeywordsPathToDefault)
                                                ]
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]
            | LoadingState.Loading ->
                Html.div [
                    prop.className "flex items-center justify-center py-4"
                    prop.children [
                        Html.div [
                            prop.className "animate-spin rounded-full h-8 w-8 border-b-2 border-primary"
                        ]
                    ]
                ]
            | LoadingState.Error err ->
                Html.div [
                    prop.className "text-red-500 text-sm"
                    prop.text (sprintf "Error loading path: %s" err)
                ]
            | LoadingState.NotStarted ->
                Html.div [
                    prop.className "text-text-secondary text-sm"
                    prop.text "Loading..."
                ]
        ]
    ]

// ============================================================================
// Main View
// ============================================================================

let view (settings: LoadingState<AppSettings>) (_isRecordingHotkey: bool) (_pendingHotkey: (uint32 * uint32) option) (keywordsPath: LoadingState<KeywordsPathInfo>) (editingKeywordsPath: string option) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "General Settings"
            ]

            match settings with
            | LoadingState.Loaded s ->
                Html.div [
                    prop.className "space-y-6"
                    prop.children [
                        typingSpeedSelector s.TypingSpeed dispatch s
                        startWithWindowsToggle s.StartWithWindows dispatch s
                        keywordsFileLocation keywordsPath editingKeywordsPath dispatch
                    ]
                ]
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
                            prop.text (sprintf "Error loading settings: %s" err)
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
        ]
    ]
