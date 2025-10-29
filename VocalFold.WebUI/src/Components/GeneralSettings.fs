module Components.GeneralSettings

open Feliz
open Browser.Types
open Types
open Components.Card
open Components.Button

// ============================================================================
// Key Display Names
// ============================================================================

let getKeyDisplayName (keyCode: uint32) =
    match keyCode with
    | 32u -> "Space"
    | 13u -> "Enter"
    | 9u -> "Tab"
    | 8u -> "Backspace"
    | 27u -> "Escape"
    | 112u -> "F1"
    | 113u -> "F2"
    | 114u -> "F3"
    | 115u -> "F4"
    | 116u -> "F5"
    | 117u -> "F6"
    | 118u -> "F7"
    | 119u -> "F8"
    | 120u -> "F9"
    | 121u -> "F10"
    | 122u -> "F11"
    | 123u -> "F12"
    | k when k >= 65u && k <= 90u -> string (char k)
    | k when k >= 48u && k <= 57u -> string (char k)
    | _ -> sprintf "Key%d" keyCode

let getModifiersDisplay (modifiers: uint32) =
    let parts = System.Collections.Generic.List<string>()
    if modifiers &&& 1u <> 0u then parts.Add("Alt")
    if modifiers &&& 2u <> 0u then parts.Add("Ctrl")
    if modifiers &&& 4u <> 0u then parts.Add("Shift")
    if modifiers &&& 8u <> 0u then parts.Add("Win")
    System.String.Join("+", parts)

let formatHotkey (modifiers: uint32) (key: uint32) =
    let modDisplay = getModifiersDisplay modifiers
    let keyDisplay = getKeyDisplayName key
    if System.String.IsNullOrEmpty(modDisplay) then
        keyDisplay
    else
        sprintf "%s+%s" modDisplay keyDisplay

// ============================================================================
// Hotkey Recorder Component
// ============================================================================

let private hotkeyRecorder (isRecording: bool) (currentKey: uint32) (currentModifiers: uint32) (dispatch: Msg -> unit) =
    let onKeyDown (e: KeyboardEvent) =
        if isRecording then
            e.preventDefault()
            e.stopPropagation()

            let modifiers =
                (if e.altKey then 1u else 0u) |||
                (if e.ctrlKey then 2u else 0u) |||
                (if e.shiftKey then 4u else 0u) |||
                (if e.metaKey then 8u else 0u)

            let keyCode = uint32 e.keyCode

            // Ignore modifier-only keys
            if keyCode <> 16u && keyCode <> 17u && keyCode <> 18u && keyCode <> 91u then
                dispatch (HotkeyRecorded (modifiers, keyCode))

    React.useEffect((fun () ->
        if isRecording then
            let handler (e: Browser.Types.Event) =
                onKeyDown (e :?> KeyboardEvent)
            Browser.Dom.window.addEventListener("keydown", handler)
            React.createDisposable(fun () ->
                Browser.Dom.window.removeEventListener("keydown", handler)
            )
        else
            React.createDisposable(fun () -> ())
    ), [| box isRecording |])

    card {
        Title = "Global Hotkey"
        ClassName = None
        Children = [
            Html.div [
                prop.className "space-y-4"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-between"
                        prop.children [
                            Html.div [
                                Html.p [
                                    prop.className "text-text-secondary text-sm mb-1"
                                    prop.text "Current Hotkey"
                                ]
                                let hotkeyClass = sprintf "text-2xl font-mono font-bold %s" (if isRecording then "text-amber-500 animate-pulse" else "text-primary")
                                Html.p [
                                    prop.className hotkeyClass
                                    prop.text (formatHotkey currentModifiers currentKey)
                                ]
                            ]
                            let buttonClass =
                                sprintf "px-6 py-2 rounded-lg font-medium transition-all duration-200 %s"
                                    (if isRecording then "bg-red-500 hover:bg-red-600 text-white animate-pulse" else "bg-primary hover:bg-primary-600 text-white")
                            Html.button [
                                prop.className buttonClass
                                prop.text (if isRecording then "Cancel" else "Record New Hotkey")
                                prop.onClick (fun _ ->
                                    if isRecording then
                                        dispatch CancelRecordingHotkey
                                    else
                                        dispatch StartRecordingHotkey
                                )
                            ]
                        ]
                    ]

                    if isRecording then
                        Html.div [
                            prop.className "bg-amber-500/10 border border-amber-500/30 rounded p-4"
                            prop.children [
                                Html.p [
                                    prop.className "text-amber-500 font-medium flex items-center"
                                    prop.children [
                                        Html.span [
                                            prop.className "text-xl mr-2"
                                            prop.text "🔴"
                                        ]
                                        Html.span "Press any key combination..."
                                    ]
                                ]
                                Html.p [
                                    prop.className "text-text-secondary text-sm mt-2"
                                    prop.text "Tip: Must include at least one modifier key (Ctrl, Shift, Alt, or Win)"
                                ]
                            ]
                        ]
                    else
                        Html.div [
                            prop.className "bg-primary/10 border border-primary/30 rounded p-4"
                            prop.children [
                                Html.p [
                                    prop.className "text-text-secondary text-sm"
                                    prop.text "Click 'Record New Hotkey' and press your desired key combination. The hotkey will be saved automatically."
                                ]
                            ]
                        ]
                ]
            ]
        ]
    }

// ============================================================================
// Model Selector Component
// ============================================================================

let private modelSelector (currentModel: string) (dispatch: Msg -> unit) (settings: AppSettings) =
    let models = [
        ("Tiny", "Fastest, lowest accuracy (~39MB)")
        ("Base", "Balanced speed and accuracy (~74MB) - Recommended")
        ("Small", "Better accuracy, slower (~244MB)")
        ("Medium", "High accuracy, slow (~769MB)")
        ("Large", "Best accuracy, very slow (~1550MB)")
    ]

    card {
        Title = "Whisper Model"
        ClassName = None
        Children = [
            Html.div [
                prop.className "space-y-4"
                prop.children [
                    Html.select [
                        prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                        prop.value currentModel
                        prop.onChange (fun (value: string) ->
                            let updatedSettings = { settings with ModelSize = value }
                            dispatch (UpdateSettings updatedSettings)
                        )
                        prop.children (
                            models |> List.map (fun (name, desc) ->
                                Html.option [
                                    prop.value name
                                    prop.text (sprintf "%s - %s" name desc)
                                ]
                            )
                        )
                    ]

                    Html.div [
                        prop.className "bg-amber-500/10 border border-amber-500/30 rounded p-4"
                        prop.children [
                            Html.p [
                                prop.className "text-amber-500 font-medium flex items-center"
                                prop.children [
                                    Html.span [
                                        prop.className "text-xl mr-2"
                                        prop.text "⚠️"
                                    ]
                                    Html.span "Requires application restart to take effect"
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
        ("fast", "Fast (5ms delay)", "Best for quick typing")
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

let view (settings: LoadingState<AppSettings>) (isRecordingHotkey: bool) (dispatch: Msg -> unit) =
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
                        hotkeyRecorder isRecordingHotkey s.HotkeyKey s.HotkeyModifiers dispatch

                        Html.div [
                            prop.className "grid grid-cols-1 md:grid-cols-2 gap-6"
                            prop.children [
                                modelSelector s.ModelSize dispatch s
                                typingSpeedSelector s.TypingSpeed dispatch s
                            ]
                        ]
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
                Html.none
        ]
    ]
