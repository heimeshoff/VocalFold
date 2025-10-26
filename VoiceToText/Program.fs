// Voice-to-Text Application
// Phase 6: Hold-to-Record Implementation

open System
open Whisper.net.Ggml

[<STAThread>]
[<EntryPoint>]
let main argv =
    printfn "==================================="
    printfn "Voice-to-Text Application v0.5"
    printfn "==================================="
    printfn ""

    // List available microphones
    AudioRecorder.listInputDevices()

    printfn "Initializing Whisper.NET..."

    // Initialize Whisper service
    let whisperService =
        TranscriptionService.createService GgmlType.Base
        |> Async.RunSynchronously

    printfn "✓ Whisper.NET ready"
    printfn ""
    printfn "Hotkey: Ctrl+Shift+Space"
    printfn "Hold the hotkey and speak, release to transcribe..."
    printfn ""

    // Mutable state to track active recording
    let mutable currentRecording: AudioRecorder.RecordingState option = None

    // Key down callback - Start recording
    let onKeyDown () =
        printfn ""
        if currentRecording.IsNone then
            try
                // Start recording
                let state = AudioRecorder.startRecording (Some 0)
                currentRecording <- Some state
            with
            | ex ->
                eprintfn "  ✗ Error starting recording: %s" ex.Message
                currentRecording <- None
        else
            printfn "  ⚠️  Already recording, ignoring..."

    // Key up callback - Stop recording and transcribe
    let onKeyUp () =
        match currentRecording with
        | Some state ->
            try
                // Stop recording
                let recording = AudioRecorder.stopRecording state
                currentRecording <- None

                // Check if we have any samples
                if recording.Samples.Length = 0 then
                    printfn "  ⚠️  No audio captured"
                else
                    // Transcribe the audio
                    printfn "  🔄 Transcribing..."
                    let transcription =
                        whisperService.Transcribe(recording.Samples)
                        |> Async.RunSynchronously

                    if String.IsNullOrWhiteSpace(transcription) then
                        printfn "  ⚠️  No speech detected"
                    else
                        printfn "  📝 Transcription: \"%s\"" transcription

                        // Type the transcribed text
                        TextInput.typeText transcription

            with
            | ex ->
                eprintfn "  ✗ Error: %s" ex.Message
                currentRecording <- None

            printfn ""
        | None ->
            // Key up without key down - ignore
            ()

    // Install keyboard hook
    let hookInstalled = HotkeyManager.installKeyboardHook onKeyDown onKeyUp

    if hookInstalled then
        try
            // Start the message loop
            HotkeyManager.messageLoop()
        finally
            // Cleanup
            HotkeyManager.uninstallKeyboardHook() |> ignore

            // Clean up any active recording
            match currentRecording with
            | Some state ->
                try
                    state.WaveIn.StopRecording()
                    state.WaveIn.Dispose()
                    state.RecordingStopped.Dispose()
                with
                | _ -> ()
            | None -> ()

            (whisperService :> IDisposable).Dispose()
            printfn "✓ Cleanup complete"
    else
        eprintfn "✗ Failed to install keyboard hook. Exiting..."
        Environment.Exit(1)

    0 // Return success code
