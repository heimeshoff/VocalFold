module Components.SystemSettings

// Suppress deprecation warning for keyCode usage - required for Windows hotkey system
#nowarn "44"

open Feliz
open Browser.Types
open Types
open Components.Card
open Components.Button
open Components.MicrophoneSettings

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
    | 46u -> "Delete"
    | 33u -> "Page Up"
    | 34u -> "Page Down"
    | 35u -> "End"
    | 36u -> "Home"
    | 37u -> "Left"
    | 38u -> "Up"
    | 39u -> "Right"
    | 40u -> "Down"
    | 91u -> "Left Win"
    | 92u -> "Right Win"
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
// System Settings Card (Combined Hotkey + Model)
// ============================================================================

[<ReactComponent>]
let private SystemSettingsCard (isRecording: bool) (currentKey: uint32) (currentModifiers: uint32) (pendingHotkey: (uint32 * uint32) option) (currentModel: string) (dispatch: Msg -> unit) (settings: AppSettings) =
    let onKeyDown (e: KeyboardEvent) =
        if isRecording then
            e.preventDefault()
            e.stopPropagation()

            let keyCode = uint32 e.keyCode

            // Calculate modifiers, but exclude Win from modifiers if the key itself is Win
            let modifiers =
                (if e.altKey then 1u else 0u) |||
                (if e.ctrlKey then 2u else 0u) |||
                (if e.shiftKey then 4u else 0u) |||
                (if e.metaKey && keyCode <> 91u && keyCode <> 92u then 8u else 0u)

            // Ignore standalone modifier keys (Shift, Ctrl, Alt) but allow Win key
            if keyCode <> 16u && keyCode <> 17u && keyCode <> 18u then
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

    let models = [
        ("Tiny", "Fastest, lowest accuracy (~39MB)")
        ("Base", "Balanced speed and accuracy (~74MB) - Recommended")
        ("Small", "Better accuracy, slower (~244MB)")
        ("Medium", "High accuracy, slow (~769MB)")
        ("Large", "Best accuracy, very slow (~1550MB)")
    ]

    card {
        Title = ""
        ClassName = None
        Children = [
            Html.div [
                prop.className "space-y-6"
                prop.children [
                    // Warning: Requires Restart (at the top)
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
                                    Html.span "All changes require application restart to take effect"
                                ]
                            ]
                        ]
                    ]

                    // Global Hotkey Section
                    Html.div [
                        prop.className "space-y-3"
                        prop.children [
                            Html.h3 [
                                prop.className "text-lg font-semibold text-text-primary"
                                prop.text "Global Hotkey"
                            ]
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
                                        match pendingHotkey with
                                        | Some (pendingMods, pendingKey) ->
                                            Html.div [
                                                prop.className "mt-2"
                                                prop.children [
                                                    Html.p [
                                                        prop.className "text-text-secondary text-xs mb-1"
                                                        prop.text "New Hotkey:"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-xl font-mono font-bold text-green-500"
                                                        prop.text (formatHotkey pendingMods pendingKey)
                                                    ]
                                                ]
                                            ]
                                        | None -> Html.none
                                    ]
                                    match pendingHotkey with
                                    | Some _ ->
                                        Html.div [
                                            prop.className "flex gap-2"
                                            prop.children [
                                                Html.button [
                                                    prop.className "px-6 py-2 rounded-lg font-medium transition-all duration-200 bg-green-500 hover:bg-green-600 text-white"
                                                    prop.text "Apply"
                                                    prop.onClick (fun _ -> dispatch ApplyHotkey)
                                                ]
                                                Html.button [
                                                    prop.className "px-6 py-2 rounded-lg font-medium transition-all duration-200 bg-red-500 hover:bg-red-600 text-white"
                                                    prop.text "Cancel"
                                                    prop.onClick (fun _ -> dispatch CancelRecordingHotkey)
                                                ]
                                            ]
                                        ]
                                    | None ->
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
                                            prop.text "Click 'Record New Hotkey' and press your desired key combination. Click 'Apply' to confirm or keep pressing different keys until you find the right one."
                                        ]
                                    ]
                                ]
                        ]
                    ]

                    // Divider
                    Html.div [
                        prop.className "border-t border-text-secondary/20"
                    ]

                    // Whisper Model Section
                    Html.div [
                        prop.className "space-y-3"
                        prop.children [
                            Html.h3 [
                                prop.className "text-lg font-semibold text-text-primary"
                                prop.text "Whisper Model"
                            ]
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
                        ]
                    ]
                ]
            ]
        ]
    }

// ============================================================================
// Main View
// ============================================================================

let view (settings: LoadingState<AppSettings>) (isRecordingHotkey: bool) (pendingHotkey: (uint32 * uint32) option) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "System Settings"
            ]

            match settings with
            | LoadingState.Loaded s ->
                Html.div [
                    prop.className "space-y-6"
                    prop.children [
                        SystemSettingsCard isRecordingHotkey s.HotkeyKey s.HotkeyModifiers pendingHotkey s.ModelSize dispatch s

                        // Microphone Settings Card
                        MicrophoneSettingsCard s.SelectedMicrophoneIndex (fun index ->
                            let updatedSettings = { s with SelectedMicrophoneIndex = index }
                            dispatch (UpdateSettings updatedSettings)
                        )
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
