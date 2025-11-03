module Components.MicrophoneSettings

open Feliz
open Feliz.UseElmish
open Elmish
open Types
open Browser.Types

// ============================================================================
// Model & Messages
// ============================================================================

type private Model = {
    Microphones: MicrophoneDevice list
    SelectedIndex: int option
    TestStatus: MicrophoneTestStatus
    CurrentLevel: float
    MaxLevel: float
    AvgLevel: float
    IsLoading: bool
    Error: string option
    TranscriptionText: string option
    IsTranscribing: bool
}

type private Msg =
    | LoadMicrophones
    | MicrophonesLoaded of Result<MicrophoneDevice list, string>
    | SelectMicrophone of int option
    | StartTest
    | StopTest
    | TestStarted of Result<unit, string>
    | TestStopped of Result<MicrophoneTestResult, string>
    | UpdateLevel of float
    | PollLevels
    | LevelsPolled of Result<MicrophoneTestResult, string>
    | StartTranscriptionTest
    | TranscriptionCompleted of Result<string, string>
    | SaveSettings

// ============================================================================
// Init & Update
// ============================================================================

let private init selectedIndex =
    {
        Microphones = []
        SelectedIndex = selectedIndex
        TestStatus = Idle
        CurrentLevel = 0.0
        MaxLevel = 0.0
        AvgLevel = 0.0
        IsLoading = true
        Error = None
        TranscriptionText = None
        IsTranscribing = false
    }, Cmd.ofMsg LoadMicrophones

let private update (onSave: int option -> unit) msg model =
    match msg with
    | LoadMicrophones ->
        let cmd = Cmd.OfPromise.either Api.getMicrophones () MicrophonesLoaded (fun ex -> MicrophonesLoaded (FSharp.Core.Result.Error ex.Message))
        model, cmd

    | MicrophonesLoaded (Ok devices) ->
        { model with Microphones = devices; IsLoading = false }, Cmd.none

    | MicrophonesLoaded (FSharp.Core.Result.Error err) ->
        { model with Error = Some err; IsLoading = false }, Cmd.none

    | SelectMicrophone idx ->
        { model with SelectedIndex = idx }, Cmd.ofMsg SaveSettings

    | StartTest ->
        let cmd = Cmd.OfPromise.either (fun () -> Api.startMicrophoneTest model.SelectedIndex) () TestStarted (fun ex -> TestStarted (FSharp.Core.Result.Error ex.Message))
        { model with TestStatus = Testing; CurrentLevel = 0.0; MaxLevel = 0.0; AvgLevel = 0.0 }, cmd

    | StopTest ->
        let cmd = Cmd.OfPromise.either (fun () -> Api.stopMicrophoneTest ()) () TestStopped (fun ex -> TestStopped (FSharp.Core.Result.Error ex.Message))
        { model with TestStatus = Stopped }, cmd

    | TestStarted (Ok ()) ->
        // Note: Polling will be handled by React.useEffect in the view
        { model with TestStatus = Testing }, Cmd.none

    | TestStarted (FSharp.Core.Result.Error err) ->
        { model with Error = Some err; TestStatus = Idle }, Cmd.none

    | TestStopped (Ok result) ->
        { model with
            MaxLevel = result.MaxLevel
            AvgLevel = result.AvgLevel
            CurrentLevel = 0.0
            TestStatus = Idle
        }, Cmd.none

    | TestStopped (FSharp.Core.Result.Error err) ->
        { model with Error = Some err; TestStatus = Idle }, Cmd.none

    | PollLevels ->
        if model.TestStatus = Testing then
            let cmd = Cmd.OfPromise.either (fun () -> Api.getMicrophoneLevels ()) () LevelsPolled (fun ex -> LevelsPolled (FSharp.Core.Result.Error ex.Message))
            model, cmd
        else
            model, Cmd.none

    | LevelsPolled (Ok result) ->
        { model with
            CurrentLevel = result.CurrentLevel
            MaxLevel = result.MaxLevel
            AvgLevel = result.AvgLevel
        }, Cmd.none

    | LevelsPolled (FSharp.Core.Result.Error _) ->
        // Silently ignore polling errors
        model, Cmd.none

    | UpdateLevel level ->
        { model with
            CurrentLevel = level
            MaxLevel = max level model.MaxLevel
        }, Cmd.none

    | SaveSettings ->
        onSave model.SelectedIndex
        model, Cmd.none

    | StartTranscriptionTest ->
        let cmd = Cmd.OfPromise.either (fun () -> Api.testMicrophoneWithTranscription model.SelectedIndex 5) () TranscriptionCompleted (fun ex -> TranscriptionCompleted (FSharp.Core.Result.Error ex.Message))
        { model with IsTranscribing = true; TranscriptionText = None; Error = None }, cmd

    | TranscriptionCompleted result ->
        match result with
        | Ok text -> { model with TranscriptionText = Some text; IsTranscribing = false }, Cmd.none
        | FSharp.Core.Result.Error err -> { model with Error = Some err; IsTranscribing = false }, Cmd.none

// ============================================================================
// View
// ============================================================================

[<ReactComponent>]
let MicrophoneSettingsCard (selectedIndex: int option) (onSave: int option -> unit) =
    let model, dispatch = React.useElmish(init selectedIndex, update onSave, [||])

    // Polling effect - polls for audio levels when testing
    React.useEffect((fun () ->
        if model.TestStatus = Testing then
            let timerId = Browser.Dom.window.setInterval((fun () ->
                // Dispatch PollLevels message every 100ms
                dispatch PollLevels
            ), 100)

            React.createDisposable(fun () ->
                Browser.Dom.window.clearInterval(timerId)
            )
        else
            React.createDisposable(fun () -> ())
    ), [| box model.TestStatus |])

    Html.div [
        prop.className "bg-gray-800 rounded-lg p-6 space-y-4"
        prop.children [
            // Card title
            Html.h2 [
                prop.className "text-xl font-semibold text-white mb-4"
                prop.text "Microphone Setup"
            ]

            // Loading state
            if model.IsLoading then
                Html.div [
                    prop.className "flex items-center justify-center py-8"
                    prop.children [
                        Html.div [
                            prop.className "animate-spin rounded-full h-8 w-8 border-b-2 border-primary-500"
                        ]
                    ]
                ]
            else
                // Microphone dropdown
                Html.div [
                    prop.className "space-y-2"
                    prop.children [
                        Html.label [
                            prop.className "block text-sm font-medium text-gray-300"
                            prop.text "Select Microphone"
                        ]
                        Html.select [
                            prop.className "w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2 text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                            prop.value (match model.SelectedIndex with | Some idx -> string idx | None -> "-1")
                            prop.onChange (fun (evt: Event) ->
                                let value = (evt.target :?> Browser.Types.HTMLSelectElement).value
                                let idx =
                                    match System.Int32.TryParse(value) with
                                    | (true, -1) -> None
                                    | (true, i) -> Some i
                                    | _ -> None
                                dispatch (SelectMicrophone idx)
                            )
                            prop.children [
                                Html.option [
                                    prop.value "-1"
                                    prop.text "🎤 Default Microphone"
                                ]
                                for device in model.Microphones do
                                    Html.option [
                                        prop.value (string device.Index)
                                        prop.text (
                                            if device.IsDefault then
                                                sprintf "%s (Default)" device.Name
                                            else
                                                device.Name
                                        )
                                    ]
                            ]
                        ]
                    ]
                ]

                // Test button
                let buttonClass =
                    match model.TestStatus with
                    | Testing -> "px-6 py-2 rounded-lg font-medium transition-colors duration-200 bg-red-500 hover:bg-red-600 text-white"
                    | _ -> "px-6 py-2 rounded-lg font-medium transition-colors duration-200 bg-primary-500 hover:bg-primary-600 text-white"

                let buttonText =
                    match model.TestStatus with
                    | Testing -> "⏹ Stop Test"
                    | _ -> "🎙 Test Microphone"

                Html.div [
                    prop.className "flex items-center gap-4"
                    prop.children [
                        Html.button [
                            prop.className buttonClass
                            prop.onClick (fun _ ->
                                match model.TestStatus with
                                | Testing -> dispatch StopTest
                                | _ -> dispatch StartTest
                            )
                            prop.text buttonText
                        ]

                        // Status indicator
                        if model.TestStatus = Testing then
                            Html.span [
                                prop.className "text-sm text-gray-400 animate-pulse"
                                prop.text "Recording..."
                            ]
                    ]
                ]

                // Audio level visualization
                if model.TestStatus = Testing || model.MaxLevel > 0.0 then
                    Html.div [
                        prop.className "space-y-2"
                        prop.children [
                            Html.label [
                                prop.className "block text-sm font-medium text-gray-300"
                                prop.text "Audio Level"
                            ]

                            // Level bar
                            let levelBarColor =
                                if model.CurrentLevel < 0.01 then "bg-gray-500"
                                elif model.CurrentLevel < 0.3 then "bg-blue-500"
                                elif model.CurrentLevel < 0.8 then "bg-secondary-500"
                                else "bg-red-500"

                            Html.div [
                                prop.className "w-full h-8 bg-gray-700 rounded-lg overflow-hidden"
                                prop.children [
                                    Html.div [
                                        prop.className ("h-full transition-all duration-75 " + levelBarColor)
                                        prop.style [
                                            style.width (length.percent (model.CurrentLevel * 100.0))
                                        ]
                                    ]
                                ]
                            ]

                            // Statistics
                            Html.div [
                                prop.className "flex justify-between text-xs text-gray-400"
                                prop.children [
                                    Html.span [
                                        prop.text (sprintf "Current: %.1f%%" (model.CurrentLevel * 100.0))
                                    ]
                                    Html.span [
                                        prop.text (sprintf "Max: %.1f%%" (model.MaxLevel * 100.0))
                                    ]
                                    Html.span [
                                        prop.text (sprintf "Avg: %.1f%%" (model.AvgLevel * 100.0))
                                    ]
                                ]
                            ]

                            // Feedback
                            if model.MaxLevel > 0.0 then
                                let feedbackClass =
                                    if model.MaxLevel < 0.01 then "text-sm text-red-400"
                                    elif model.MaxLevel < 0.1 then "text-sm text-yellow-400"
                                    else "text-sm text-green-400"

                                let feedbackText =
                                    if model.MaxLevel < 0.01 then
                                        "⚠️ No audio detected. Check microphone connection and permissions."
                                    elif model.MaxLevel < 0.1 then
                                        "⚠️ Audio level is low. Speak louder or move closer to the microphone."
                                    else
                                        "✅ Microphone is working correctly!"

                                Html.div [
                                    prop.className feedbackClass
                                    prop.text feedbackText
                                ]
                        ]
                    ]

            // Transcription test section
            Html.div [
                prop.className "border-t border-gray-700 pt-4 space-y-3"
                prop.children [
                    Html.h3 [
                        prop.className "text-lg font-medium text-gray-300"
                        prop.text "Test with Transcription"
                    ]

                    Html.p [
                        prop.className "text-sm text-gray-400"
                        prop.text "Record for 5 seconds and see what the system transcribes to verify quality."
                    ]

                    Html.button [
                        prop.className (
                            if model.IsTranscribing then
                                "px-6 py-2 rounded-lg font-medium bg-gray-500 text-white cursor-wait"
                            else
                                "px-6 py-2 rounded-lg font-medium bg-secondary-500 hover:bg-secondary-600 text-white transition-colors"
                        )
                        prop.disabled model.IsTranscribing
                        prop.onClick (fun _ -> dispatch StartTranscriptionTest)
                        prop.text (
                            if model.IsTranscribing then
                                "🎙 Recording and transcribing..."
                            else
                                "🎙 Test with Transcription (5s)"
                        )
                    ]

                    // Transcription result display
                    match model.TranscriptionText with
                    | Some text when not (System.String.IsNullOrWhiteSpace text) ->
                        Html.div [
                            prop.className "space-y-2"
                            prop.children [
                                Html.label [
                                    prop.className "block text-sm font-medium text-gray-300"
                                    prop.text "Transcription Result:"
                                ]
                                Html.textarea [
                                    prop.className "w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-3 text-white font-mono text-sm resize-none"
                                    prop.readOnly true
                                    prop.rows 4
                                    prop.value text
                                ]
                                Html.p [
                                    prop.className "text-xs text-green-400"
                                    prop.text "✅ Transcription successful! If the text matches what you said, your microphone is working correctly."
                                ]
                            ]
                        ]
                    | Some text ->
                        Html.div [
                            prop.className "bg-yellow-900/20 border border-yellow-500 rounded-lg p-3 text-yellow-400 text-sm"
                            prop.text "⚠️ No text transcribed. Try speaking louder or closer to the microphone."
                        ]
                    | None -> Html.none
                ]
            ]

            // Error display
            match model.Error with
            | Some err ->
                Html.div [
                    prop.className "bg-red-900/20 border border-red-500 rounded-lg p-3 text-red-400 text-sm"
                    prop.text err
                ]
            | None -> Html.none
        ]
    ]
