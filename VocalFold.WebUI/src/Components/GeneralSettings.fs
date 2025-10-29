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
// Main View
// ============================================================================

let view (settings: LoadingState<AppSettings>) (_isRecordingHotkey: bool) (_pendingHotkey: (uint32 * uint32) option) (dispatch: Msg -> unit) =
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
