// Voice-to-Text Application
// Phase 7: Tray Icon Integration

open System
open System.Windows.Forms
open Whisper.net.Ggml

[<STAThread>]
[<EntryPoint>]
let main argv =
    try
        // Initialize logger and cleanup old logs
        Logger.info "========================================="
        Logger.info "VocalFold - Voice to Text Application"
        Logger.info "========================================="
        Logger.info (sprintf "Version: .NET %s" (System.Environment.Version.ToString()))
        Logger.info (sprintf "OS: %s" (System.Environment.OSVersion.ToString()))
        Logger.info (sprintf "Log file: %s" (Logger.getLatestLogPath()))
        Logger.cleanupOldLogs()

        // Add global unhandled exception handlers
        AppDomain.CurrentDomain.UnhandledException.Add(fun args ->
            let ex = args.ExceptionObject :?> System.Exception
            Logger.critical "UNHANDLED APPDOMAIN EXCEPTION!"
            Logger.logException ex "Unhandled AppDomain exception"
        )

        Application.ThreadException.Add(fun args ->
            Logger.critical "UNHANDLED THREAD EXCEPTION!"
            Logger.logException args.Exception "Unhandled thread exception"
        )

        // Initialize Windows Forms application
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(false)
        Logger.info "Windows Forms initialized"

        // Initialize Whisper service
        Logger.info "Starting Whisper service initialization..."
        let whisperService =
            TranscriptionService.createService GgmlType.Base
            |> Async.RunSynchronously

        // Load settings
        let mutable currentSettings = Settings.load()
        Logger.info "Loaded settings:"
        Logger.info (sprintf "   Hotkey: %s" (Settings.getHotkeyDisplayName currentSettings))
        Logger.info (sprintf "   Model: %s" currentSettings.ModelSize)
        Logger.info (sprintf "   Enabled: %b" currentSettings.IsEnabled)

        // Mutable state
        let mutable currentRecording: AudioRecorder.RecordingState option = None
        let mutable isEnabled = currentSettings.IsEnabled
        let mutable shouldExit = false
        let mutable trayState: TrayIcon.TrayState option = None

        // Create overlay manager
        let overlayManager = OverlayWindow.OverlayManager()

        // Key down callback - Start recording
        let onKeyDown () =
            try
                if not isEnabled then
                    Logger.debug "Hotkey pressed but application is disabled"
                elif currentRecording.IsNone then
                    Logger.info "Starting recording..."
                    try
                        // Show overlay in ready state immediately (transparent background)
                        overlayManager.ShowReady()

                        // Start recording with level update callback
                        let onLevelUpdate level =
                            overlayManager.UpdateLevel(float level)

                        let state = AudioRecorder.startRecording (Some 0) (Some onLevelUpdate)
                        currentRecording <- Some state
                        Logger.info "Recording started successfully"
                    with
                    | ex ->
                        Logger.logException ex "Recording failed to start"
                        currentRecording <- None
                        overlayManager.ShowError()
                        System.Threading.Thread.Sleep(1000)
                        overlayManager.Hide()
                        match trayState with
                        | Some tray -> TrayIcon.notifyError tray ("Recording error: " + ex.Message)
                        | None -> ()
                else
                    Logger.debug "Hotkey pressed but recording already in progress"
            with
            | ex ->
                Logger.logException ex "Unhandled exception in onKeyDown"

        // Key up callback - Stop recording and transcribe
        let onKeyUp () =
            try
                if not isEnabled then
                    Logger.debug "Key released but application is disabled"
                else
                    match currentRecording with
                    | Some state ->
                        try
                            Logger.info "Stopping recording..."

                            // Stop recording first
                            let recording = AudioRecorder.stopRecording state
                            currentRecording <- None
                            Logger.info (sprintf "Recording stopped. Captured %d samples" recording.Samples.Length)

                            // Show transcribing state IMMEDIATELY (non-blocking)
                            overlayManager.ShowTranscribing()

                            // Check if we have any samples
                            if recording.Samples.Length = 0 then
                                Logger.warning "No audio captured"
                                overlayManager.Hide()
                                match trayState with
                                | Some tray -> TrayIcon.notifyWarning tray "No audio captured"
                                | None -> ()
                            else
                                // Run transcription asynchronously on background thread
                                async {
                                    try
                                        // Transcribe the audio
                                        Logger.info "Starting transcription..."
                                        let! transcription = whisperService.Transcribe(recording.Samples)

                                        Logger.info (sprintf "Transcription result: \"%s\"" transcription)

                                        if String.IsNullOrWhiteSpace(transcription) then
                                            Logger.warning "No speech detected in audio"
                                            overlayManager.Hide()
                                            match trayState with
                                            | Some tray -> TrayIcon.notifyWarning tray "No speech detected"
                                            | None -> ()
                                        else
                                            // Hide overlay BEFORE typing so input goes to the correct window
                                            Logger.debug "Hiding overlay before typing"
                                            overlayManager.Hide()

                                            // Small delay to let the previous window regain focus
                                            do! Async.Sleep(100)

                                            // Type the transcribed text
                                            Logger.info "Typing transcribed text..."
                                            TextInput.typeTextWithSettings transcription currentSettings
                                            Logger.info "Text typing completed"
                                            Logger.info "Transcription flow completed successfully"
                                    with
                                    | ex ->
                                        Logger.logException ex "Error during transcription"
                                        overlayManager.ShowError()
                                        do! Async.Sleep(1000)
                                        overlayManager.Hide()
                                        match trayState with
                                        | Some tray -> TrayIcon.notifyError tray ("Error: " + ex.Message)
                                        | None -> ()
                                } |> Async.Start  // Start on background thread, don't block

                        with
                        | ex ->
                            Logger.logException ex "Error during recording stop"
                            currentRecording <- None
                            overlayManager.ShowError()
                            System.Threading.Thread.Sleep(1000)
                            overlayManager.Hide()
                            match trayState with
                            | Some tray -> TrayIcon.notifyError tray ("Error: " + ex.Message)
                            | None -> ()

                    | None ->
                        Logger.debug "Key released but no active recording"
            with
            | ex ->
                Logger.logException ex "Unhandled exception in onKeyUp"

        // Create tray icon
        let trayConfig : TrayIcon.TrayConfig = {
            ApplicationName = "VocalFold"
            InitialEnabled = currentSettings.IsEnabled
            OnExit = fun () ->
                Logger.info "User requested exit via tray icon"
                shouldExit <- true
                HotkeyManager.exitMessageLoop()
            OnToggleEnabled = fun enabled ->
                isEnabled <- enabled
                // Update and save settings
                currentSettings <- { currentSettings with IsEnabled = enabled }
                Settings.save currentSettings |> ignore
            OnSettings = fun () ->
                // Open settings dialog
                match SettingsDialog.show currentSettings with
                | SettingsDialog.Accepted newSettings ->
                    // Check if hotkey changed
                    let hotkeyChanged =
                        newSettings.HotkeyKey <> currentSettings.HotkeyKey ||
                        newSettings.HotkeyModifiers <> currentSettings.HotkeyModifiers

                    // Update settings
                    currentSettings <- newSettings

                    // Save settings
                    if Settings.save currentSettings then
                        match trayState with
                        | Some tray ->
                            if hotkeyChanged then
                                // Reinstall keyboard hook with new hotkey
                                HotkeyManager.uninstallKeyboardHook() |> ignore
                                let hookInstalled = HotkeyManager.installKeyboardHook onKeyDown onKeyUp currentSettings.HotkeyKey currentSettings.HotkeyModifiers
                                if hookInstalled then
                                    TrayIcon.notifyInfo tray (sprintf "Hotkey changed to: %s" (Settings.getHotkeyDisplayName currentSettings))
                                else
                                    TrayIcon.notifyError tray "Failed to register new hotkey!"
                            else
                                TrayIcon.notifyInfo tray "Settings saved. Restart required for model changes."
                        | None -> ()
                | SettingsDialog.Cancelled ->
                    () // Do nothing
        }

        let tray = TrayIcon.create trayConfig
        trayState <- Some tray

        // Install keyboard hook with configured hotkey
        Logger.info (sprintf "Installing keyboard hook for: %s" (Settings.getHotkeyDisplayName currentSettings))
        let hookInstalled = HotkeyManager.installKeyboardHook onKeyDown onKeyUp currentSettings.HotkeyKey currentSettings.HotkeyModifiers

        if hookInstalled then
            Logger.info "Keyboard hook installed successfully"
            Logger.info "Application started successfully!"
            Logger.info "Entering message loop..."
            try
                // Start the message loop
                HotkeyManager.messageLoop()
                Logger.info "Message loop exited normally"
            finally
                // Cleanup
                Logger.info "Shutting down application..."
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

                // Cleanup overlay
                overlayManager.Cleanup()

                // Dispose tray icon
                TrayIcon.dispose tray
                Logger.info "Application shutdown complete"
        else
            Logger.error "Failed to install keyboard hook!"
            TrayIcon.notifyError tray "Failed to install keyboard hook!"
            System.Threading.Thread.Sleep(3000) // Give user time to see the error
            Environment.Exit(1)

        0 // Return success code
    with
    | ex ->
        // Global exception handler - catch any unhandled exceptions
        Logger.critical "========================================="
        Logger.critical "FATAL ERROR - APPLICATION CRASHED"
        Logger.critical "========================================="
        Logger.logException ex "Unhandled exception in main"
        Logger.critical (sprintf "Log file location: %s" (Logger.getLatestLogPath()))

        // Show error dialog to user
        try
            let errorMessage = sprintf "VocalFold has encountered a fatal error and must close.\n\nError: %s\n\nPlease check the log file at:\n%s\n\nCommon issues:\n- Missing NVIDIA GPU or drivers\n- CUDA compatibility problems\n- Network issues during first run (model download)\n\nPress OK to exit." ex.Message (Logger.getLatestLogPath())

            System.Windows.Forms.MessageBox.Show(
                errorMessage,
                "VocalFold - Fatal Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error
            ) |> ignore
        with
        | _ -> () // Silently fail if MessageBox fails

        1 // Return error code
