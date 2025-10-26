// Voice-to-Text Application
// Phase 7: Tray Icon Integration

open System
open System.Windows.Forms
open Whisper.net.Ggml

[<STAThread>]
[<EntryPoint>]
let main argv =
    // Initialize Windows Forms application
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    // Initialize Whisper service
    let whisperService =
        TranscriptionService.createService GgmlType.Base
        |> Async.RunSynchronously

    // Mutable state
    let mutable currentRecording: AudioRecorder.RecordingState option = None
    let mutable isEnabled = true
    let mutable shouldExit = false
    let mutable trayState: TrayIcon.TrayState option = None

    // Key down callback - Start recording
    let onKeyDown () =
        if not isEnabled then () // Ignore if disabled
        elif currentRecording.IsNone then
            try
                // Start recording
                let state = AudioRecorder.startRecording (Some 0)
                currentRecording <- Some state
            with
            | ex ->
                currentRecording <- None
                match trayState with
                | Some tray -> TrayIcon.notifyError tray ("Recording error: " + ex.Message)
                | None -> ()

    // Key up callback - Stop recording and transcribe
    let onKeyUp () =
        if not isEnabled then () // Ignore if disabled
        else
            match currentRecording with
            | Some state ->
                try
                    // Stop recording
                    let recording = AudioRecorder.stopRecording state
                    currentRecording <- None

                    // Check if we have any samples
                    if recording.Samples.Length = 0 then
                        match trayState with
                        | Some tray -> TrayIcon.notifyWarning tray "No audio captured"
                        | None -> ()
                    else
                        // Transcribe the audio
                        let transcription =
                            whisperService.Transcribe(recording.Samples)
                            |> Async.RunSynchronously

                        if String.IsNullOrWhiteSpace(transcription) then
                            match trayState with
                            | Some tray -> TrayIcon.notifyWarning tray "No speech detected"
                            | None -> ()
                        else
                            // Type the transcribed text
                            TextInput.typeText transcription

                with
                | ex ->
                    currentRecording <- None
                    match trayState with
                    | Some tray -> TrayIcon.notifyError tray ("Error: " + ex.Message)
                    | None -> ()

            | None ->
                // Key up without key down - ignore
                ()

    // Create tray icon
    let trayConfig : TrayIcon.TrayConfig = {
        ApplicationName = "VocalFold"
        OnExit = fun () ->
            shouldExit <- true
            Application.Exit()
        OnToggleEnabled = fun enabled ->
            isEnabled <- enabled
        OnSettings = fun () ->
            // TODO: Phase 8 - Open settings dialog
            ()
    }

    let tray = TrayIcon.create trayConfig
    trayState <- Some tray

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

            // Dispose whisper service
            (whisperService :> IDisposable).Dispose()

            // Dispose tray icon
            TrayIcon.dispose tray
    else
        TrayIcon.notifyError tray "Failed to install keyboard hook!"
        System.Threading.Thread.Sleep(3000) // Give user time to see the error
        Environment.Exit(1)

    0 // Return success code
