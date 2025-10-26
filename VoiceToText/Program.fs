// Voice-to-Text Application
// Phase 3: Testing Whisper.NET Transcription

open System
open Whisper.net.Ggml

printfn "==================================="
printfn "Voice-to-Text Application v0.3"
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
printfn "Press the hotkey and speak for 5 seconds..."
printfn ""

// Hotkey callback function
let onHotkeyPressed () =
    printfn ""
    printfn "✓ Hotkey detected!"

    try
        // Record audio for 5 seconds (using default device)
        let recording = AudioRecorder.recordAudio 5 (Some 0)

        // Transcribe the audio
        let transcription =
            whisperService.Transcribe(recording.Samples)
            |> Async.RunSynchronously

        if String.IsNullOrWhiteSpace(transcription) then
            printfn "  ⚠️  No speech detected"
        else
            printfn "  📝 Transcription: \"%s\"" transcription
            printfn "  (Next: type this text)"

    with
    | ex ->
        eprintfn "  ✗ Error: %s" ex.Message

    printfn ""

// Register the hotkey: Ctrl+Shift+Space (safer for testing)
let hotkeyId = 1
let modifiers = WinAPI.MOD_CONTROL ||| WinAPI.MOD_SHIFT
let virtualKey = WinAPI.VK_SPACE

let registered = WinAPI.registerHotkey hotkeyId modifiers virtualKey onHotkeyPressed

if registered then
    try
        // Start the message loop
        WinAPI.messageLoop()
    finally
        // Cleanup
        WinAPI.unregisterHotkey hotkeyId |> ignore
        (whisperService :> IDisposable).Dispose()
        printfn "✓ Cleanup complete"
else
    eprintfn "✗ Failed to register hotkey. Exiting..."
    Environment.Exit(1)
