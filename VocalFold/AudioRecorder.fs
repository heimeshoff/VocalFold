module AudioRecorder

open System
open NAudio.Wave
open System.Collections.Generic

// Result type for recording
type RecordingResult = {
    Samples: float32[]
    SampleRate: int
}

// State for active recording
type RecordingState = {
    WaveIn: WaveInEvent
    RecordedSamples: List<float32>
    StartTime: DateTime
    mutable MaxLevel: float32
    mutable AvgLevel: float32
    mutable SampleCount: int
    mutable CurrentLevel: float32  // Current instantaneous level for visualization
    RecordingStopped: System.Threading.ManualResetEvent
    OnLevelUpdate: (float32 -> unit) option  // Callback for real-time level updates
}

// List all available input devices
let listInputDevices () =
    printfn "Available microphones:"
    for i in 0 .. WaveInEvent.DeviceCount - 1 do
        let caps = WaveInEvent.GetCapabilities(i)
        printfn "  [%d] %s (%d channels)" i caps.ProductName caps.Channels
    printfn ""

// Convert byte buffer to float32 array
let private bytesToFloat32 (buffer: byte[]) (bytesRecorded: int) =
    let sampleCount = bytesRecorded / 2 // 16-bit samples = 2 bytes per sample
    let samples = Array.zeroCreate<float32> sampleCount

    for i in 0 .. sampleCount - 1 do
        // Read 16-bit PCM sample (little-endian) using BitConverter for proper sign handling
        let byteIndex = i * 2
        let sample16 = BitConverter.ToInt16(buffer, byteIndex)
        // Convert to float32 in range [-1.0, 1.0]
        samples.[i] <- float32 sample16 / 32768.0f

    samples

// Record audio from a specific microphone (or default if deviceNumber is None)
let recordAudio (maxDurationSeconds: int) (deviceNumber: int option) : RecordingResult =
    match deviceNumber with
    | Some d -> printfn "🎤 Recording for %d seconds from device %d..." maxDurationSeconds d
    | None -> printfn "🎤 Recording for %d seconds from default device..." maxDurationSeconds

    let sampleRate = 16000 // Whisper.NET optimized format
    let channels = 1       // Mono

    // Setup wave format
    let waveFormat = WaveFormat(sampleRate, 16, channels)

    // Storage for recorded samples
    let recordedSamples = List<float32>()
    let mutable maxLevel = 0.0f
    let mutable avgLevel = 0.0f
    let mutable sampleCount = 0

    // Create wave input device
    use waveIn = new WaveInEvent(
        WaveFormat = waveFormat,
        BufferMilliseconds = 100
    )

    // Set device number if specified
    match deviceNumber with
    | Some d -> waveIn.DeviceNumber <- d
    | None -> ()

    // Data available event handler
    waveIn.DataAvailable.Add(fun args ->
        let samples = bytesToFloat32 args.Buffer args.BytesRecorded
        recordedSamples.AddRange(samples)

        // Calculate audio levels for monitoring
        for sample in samples do
            let absLevel = abs sample
            if absLevel > maxLevel then
                maxLevel <- absLevel
            avgLevel <- avgLevel + absLevel
            sampleCount <- sampleCount + 1
    )

    // Recording stopped event
    let recordingStopped = new System.Threading.ManualResetEvent(false)
    waveIn.RecordingStopped.Add(fun _ ->
        recordingStopped.Set() |> ignore
    )

    // Start recording
    waveIn.StartRecording()
    printfn "  🔴 Recording started (speak now)..."

    // Wait for the specified duration
    System.Threading.Thread.Sleep(maxDurationSeconds * 1000)

    // Stop recording
    waveIn.StopRecording()

    // Wait for the recording to fully stop
    recordingStopped.WaitOne(1000) |> ignore

    let totalSamples = recordedSamples.ToArray()
    let avgLevelNormalized = if sampleCount > 0 then avgLevel / float32 sampleCount else 0.0f

    printfn "  ✓ Recording complete (%d samples, %.1f seconds)"
        totalSamples.Length
        (float totalSamples.Length / float sampleRate)
    printfn "  📊 Audio levels - Max: %.3f, Avg: %.3f" maxLevel avgLevelNormalized

    // Warn if audio is too quiet
    if maxLevel < 0.01f then
        printfn "  ⚠️  WARNING: Audio level is very low (max: %.4f)" maxLevel
        printfn "     This might indicate:"
        printfn "     - Wrong microphone selected"
        printfn "     - Microphone is muted or volume is too low"
        printfn "     - No audio input detected"

    {
        Samples = totalSamples
        SampleRate = sampleRate
    }

// Start recording (non-blocking, returns RecordingState)
let startRecording (deviceNumber: int option) (onLevelUpdate: (float32 -> unit) option) : RecordingState =
    match deviceNumber with
    | Some d -> printfn "🎤 Recording started from device %d..." d
    | None -> printfn "🎤 Recording started from default device..."

    let sampleRate = 16000 // Whisper.NET optimized format
    let channels = 1       // Mono

    // Setup wave format
    let waveFormat = WaveFormat(sampleRate, 16, channels)

    // Storage for recorded samples
    let recordedSamples = List<float32>()

    // Create wave input device
    let waveIn = new WaveInEvent(
        WaveFormat = waveFormat,
        BufferMilliseconds = 100
    )

    // Set device number if specified
    match deviceNumber with
    | Some d -> waveIn.DeviceNumber <- d
    | None -> ()

    // Create state
    let state = {
        WaveIn = waveIn
        RecordedSamples = recordedSamples
        StartTime = DateTime.Now
        MaxLevel = 0.0f
        AvgLevel = 0.0f
        SampleCount = 0
        CurrentLevel = 0.0f
        RecordingStopped = new System.Threading.ManualResetEvent(false)
        OnLevelUpdate = onLevelUpdate
    }

    // Data available event handler
    waveIn.DataAvailable.Add(fun args ->
        let samples = bytesToFloat32 args.Buffer args.BytesRecorded
        state.RecordedSamples.AddRange(samples)

        // Calculate audio levels for monitoring
        let mutable bufferMaxLevel = 0.0f
        for sample in samples do
            let absLevel = abs sample
            if absLevel > state.MaxLevel then
                state.MaxLevel <- absLevel
            if absLevel > bufferMaxLevel then
                bufferMaxLevel <- absLevel
            state.AvgLevel <- state.AvgLevel + absLevel
            state.SampleCount <- state.SampleCount + 1

        // Update current level (use buffer max for visualization)
        state.CurrentLevel <- bufferMaxLevel

        // Call level update callback if provided
        match state.OnLevelUpdate with
        | Some callback -> callback bufferMaxLevel
        | None -> ()
    )

    // Recording stopped event
    waveIn.RecordingStopped.Add(fun _ ->
        try
            state.RecordingStopped.Set() |> ignore
        with
        | :? ObjectDisposedException ->
            // Event already disposed, ignore
            ()
        | ex ->
            // Log other exceptions but don't crash
            printfn "Warning: Error in RecordingStopped handler: %s" ex.Message
    )

    // Start recording
    waveIn.StartRecording()
    printfn "  🔴 Recording... (release hotkey to stop)"

    state

// Stop recording and return result
let stopRecording (state: RecordingState) : RecordingResult =
    // Stop recording
    state.WaveIn.StopRecording()

    // Wait for the recording to fully stop (max 2 seconds)
    state.RecordingStopped.WaitOne(2000) |> ignore

    // Dispose of the wave input first (this will stop any pending events)
    state.WaveIn.Dispose()

    // Small delay to ensure all event handlers have finished executing
    System.Threading.Thread.Sleep(50)

    // Now safe to dispose the ManualResetEvent
    state.RecordingStopped.Dispose()

    let totalSamples = state.RecordedSamples.ToArray()
    let sampleRate = 16000
    let duration = (DateTime.Now - state.StartTime).TotalSeconds
    let avgLevelNormalized = if state.SampleCount > 0 then state.AvgLevel / float32 state.SampleCount else 0.0f

    printfn "  ✓ Recording complete (%.1f seconds, %d samples)"
        duration
        totalSamples.Length
    printfn "  📊 Audio levels - Max: %.3f, Avg: %.3f" state.MaxLevel avgLevelNormalized

    // Warn if audio is too quiet
    if state.MaxLevel < 0.01f then
        printfn "  ⚠️  WARNING: Audio level is very low (max: %.4f)" state.MaxLevel
        printfn "     This might indicate:"
        printfn "     - Wrong microphone selected"
        printfn "     - Microphone is muted or volume is too low"
        printfn "     - No audio input detected"

    // Warn if recording is too short
    if duration < 0.3 then
        printfn "  ⚠️  WARNING: Recording is very short (%.2f seconds)" duration
        printfn "     Hold the hotkey longer for better transcription results"

    {
        Samples = totalSamples
        SampleRate = sampleRate
    }
