// Voice-to-Text Application
// Task 1.3: Test WinAPI Module - Hotkey Detection

open System

printfn "==================================="
printfn "Voice-to-Text Application v0.1"
printfn "==================================="
printfn ""
printfn "Testing hotkey detection..."
printfn "Hotkey: Ctrl+Shift+Space"
printfn "(We'll switch to Ctrl+Windows after testing)"
printfn ""

// Hotkey callback function
let onHotkeyPressed () =
    printfn ""
    printfn "✓ Hotkey detected!"

    try
        // Record audio for 5 seconds
        let recording = AudioRecorder.recordAudio 5
        printfn "  ✓ Audio captured: %d samples at %d Hz" recording.Samples.Length recording.SampleRate
        printfn "  (Next: transcribe with Whisper.NET)"
    with
    | ex ->
        eprintfn "  ✗ Recording error: %s" ex.Message

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
        printfn "✓ Cleanup complete"
else
    eprintfn "✗ Failed to register hotkey. Exiting..."
    Environment.Exit(1)
