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

        // Load settings and check if this is the first run
        let mutable currentSettings, isFirstRun = Settings.loadWithFirstRunCheck()
        Logger.info "Loaded settings:"
        Logger.info (sprintf "   Hotkey: %s" (Settings.getHotkeyDisplayName currentSettings))
        Logger.info (sprintf "   Model: %s" currentSettings.ModelSize)
        if isFirstRun then
            Logger.info "First run detected - will open settings after initialization"

        // Mutable state
        let mutable currentRecording: AudioRecorder.RecordingState option = None
        let mutable shouldExit = false
        let mutable trayState: TrayIcon.TrayState option = None
        let mutable webServerState: WebServer.ServerState option = None

        // Create overlay manager
        let overlayManager = OverlayWindow.OverlayManager()

        // Function to open settings dialog (can be called from tray icon or voice command)
        // Note: Using 'let rec ... and ...' for mutual recursion with onKeyDown/onKeyUp
        let rec openSettingsDialog () =
            try
                Logger.info "Opening web-based settings UI..."

                // Callback for when settings change via web UI
                let onSettingsChanged (newSettings: Settings.AppSettings) =
                    Logger.info "Settings changed via web UI"

                    // Check what changed
                    let hotkeyChanged =
                        newSettings.HotkeyKey <> currentSettings.HotkeyKey ||
                        newSettings.HotkeyModifiers <> currentSettings.HotkeyModifiers

                    let startupChanged =
                        newSettings.StartWithWindows <> currentSettings.StartWithWindows

                    // Update current settings
                    currentSettings <- newSettings

                    // Apply changes
                    match trayState with
                    | Some tray ->
                        if hotkeyChanged then
                            // Reinstall keyboard hook with new hotkey
                            HotkeyManager.uninstallKeyboardHook() |> ignore
                            let hookInstalled = HotkeyManager.installKeyboardHook onKeyDown onKeyUp currentSettings.HotkeyKey currentSettings.HotkeyModifiers
                            if hookInstalled then
                                Logger.info (sprintf "Hotkey changed to: %s" (Settings.getHotkeyDisplayName currentSettings))
                            else
                                Logger.error "Failed to register new hotkey!"

                        if startupChanged then
                            // Handle start with Windows setting change
                            if newSettings.StartWithWindows then
                                let exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
                                let exePath =
                                    if exePath.EndsWith(".dll") then
                                        // Running via dotnet, use the actual exe path
                                        System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
                                    else
                                        exePath
                                if TrayIcon.Startup.enable exePath then
                                    Logger.info "Enabled start with Windows"
                                else
                                    Logger.error "Failed to enable start with Windows"
                            else
                                if TrayIcon.Startup.disable() then
                                    Logger.info "Disabled start with Windows"
                                else
                                    Logger.error "Failed to disable start with Windows"
                    | None -> ()

                match webServerState with
                | Some state ->
                    // Server already running, just open browser
                    Logger.info "Web server already running, opening browser..."
                    let url = WebServer.getUrl state
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url, UseShellExecute = true)) |> ignore
                | None ->
                    // Start web server
                    Logger.info "Starting web server..."

                    let serverConfig: WebServer.ServerConfig = {
                        OnSettingsChanged = onSettingsChanged
                    }

                    let state = WebServer.start serverConfig |> Async.RunSynchronously
                    webServerState <- Some state

                    // Open browser to settings page
                    let url = WebServer.getUrl state
                    Logger.info (sprintf "Opening browser to: %s" url)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url, UseShellExecute = true)) |> ignore
            with
            | ex ->
                Logger.logException ex "Error opening settings UI"
                match trayState with
                | Some tray -> TrayIcon.notifyError tray "Failed to open settings"
                | None -> ()

        // Key down callback - Start recording
        and onKeyDown () =
            try
                if currentRecording.IsNone then
                    Logger.info "Starting recording..."
                    try
                        // Show overlay in ready state immediately (transparent background)
                        overlayManager.ShowReady()

                        // Start recording with level, spectrum, and mute state callbacks
                        let onLevelUpdate level =
                            overlayManager.UpdateLevel(float level)

                        let onSpectrumUpdate spectrum =
                            overlayManager.UpdateSpectrum(spectrum)

                        let onMuteStateChanged isMuted =
                            if isMuted then
                                Logger.debug "Switching overlay to muted state (real-time)"
                                overlayManager.ShowMuted()
                            else
                                Logger.debug "Switching overlay back to recording state (real-time)"
                                overlayManager.ShowReady()

                        let state = AudioRecorder.startRecording (Some 0) (Some onLevelUpdate) (Some onSpectrumUpdate) (Some onMuteStateChanged)
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
        and onKeyUp () =
            try
                match currentRecording with
                | Some state ->
                    try
                        Logger.info "Stopping recording..."

                        // Stop recording first
                        let recording = AudioRecorder.stopRecording state
                        currentRecording <- None
                        Logger.info (sprintf "Recording stopped. Captured %d samples" recording.Samples.Length)

                        // Check if microphone was muted
                        if recording.IsMuted then
                            Logger.warning "Microphone is muted or audio level too low"
                            // Ensure muted overlay is showing (it should already be if detected in real-time)
                            overlayManager.ShowMuted()
                            System.Threading.Thread.Sleep(2000)  // Show muted icon for 2 seconds
                            overlayManager.Hide()
                        // Check if we have any samples
                        elif recording.Samples.Length = 0 then
                            Logger.warning "No audio captured"
                            overlayManager.Hide()
                        else
                            // Show transcribing state IMMEDIATELY (non-blocking)
                            overlayManager.ShowTranscribing()

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
                                    else
                                        // Process transcription and check for special commands
                                        let processingResult = TextProcessor.processTranscriptionWithCommands transcription currentSettings.KeywordReplacements

                                        match processingResult with
                                        | TextProcessor.OpenSettings ->
                                            // Hide overlay first
                                            Logger.info "Opening settings via voice command..."
                                            overlayManager.Hide()
                                            do! Async.Sleep(400)

                                            // Open settings dialog
                                            openSettingsDialog()
                                            Logger.info "Settings command completed successfully"

                                        | TextProcessor.TypeText processedText ->
                                            // Store this transcription for "repeat last message" command
                                            // (only if it's not the "repeat last message" command itself)
                                            if not (transcription.ToLowerInvariant().Contains("repeat last message")) then
                                                TextProcessor.storeLastTranscription processedText

                                            // Hide overlay BEFORE typing so input goes to the correct window
                                            Logger.debug "Hiding overlay before typing"
                                            overlayManager.Hide()

                                            // Wait for overlay fade-out animation (300ms) + extra time for focus to return
                                            Logger.debug "Waiting for overlay to hide and focus to return..."
                                            do! Async.Sleep(400)

                                            // Type the processed text
                                            Logger.info "Typing transcribed text..."
                                            TextInput.typeTextWithSettings processedText currentSettings
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
            OnExit = fun () ->
                Logger.info "User requested exit via tray icon"
                shouldExit <- true
                HotkeyManager.exitMessageLoop()
            OnSettings = openSettingsDialog
        }

        let tray = TrayIcon.create trayConfig
        trayState <- Some tray

        // Sync StartWithWindows setting with actual registry state
        let actualStartupEnabled = TrayIcon.Startup.isEnabled()
        if currentSettings.StartWithWindows <> actualStartupEnabled then
            Logger.info (sprintf "Syncing StartWithWindows setting with actual state: setting=%b, actual=%b" currentSettings.StartWithWindows actualStartupEnabled)
            if currentSettings.StartWithWindows then
                // Setting says it should be enabled but it's not - enable it
                let exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
                let exePath =
                    if exePath.EndsWith(".dll") then
                        System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
                    else
                        exePath
                if TrayIcon.Startup.enable exePath then
                    Logger.info "Enabled start with Windows"
                else
                    Logger.error "Failed to enable start with Windows"
            else
                // Setting says it should be disabled but it's enabled - disable it
                if TrayIcon.Startup.disable() then
                    Logger.info "Disabled start with Windows"
                else
                    Logger.error "Failed to disable start with Windows"

        // Install keyboard hook with configured hotkey
        Logger.info (sprintf "Installing keyboard hook for: %s" (Settings.getHotkeyDisplayName currentSettings))
        let hookInstalled = HotkeyManager.installKeyboardHook onKeyDown onKeyUp currentSettings.HotkeyKey currentSettings.HotkeyModifiers

        if hookInstalled then
            Logger.info "Keyboard hook installed successfully"
            Logger.info "Application started successfully!"

            // Open settings on first run
            if isFirstRun then
                Logger.info "Opening settings page for first-time setup..."
                openSettingsDialog()

            Logger.info "Entering message loop..."
            try
                // Start the message loop
                HotkeyManager.messageLoop()
                Logger.info "Message loop exited normally"
            finally
                // Cleanup
                Logger.info "Shutting down application..."
                HotkeyManager.uninstallKeyboardHook() |> ignore

                // Stop web server if running
                match webServerState with
                | Some state ->
                    try
                        Logger.info "Stopping web server..."
                        WebServer.stop state |> Async.RunSynchronously
                    with
                    | ex -> Logger.logException ex "Error stopping web server"
                | None -> ()

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
