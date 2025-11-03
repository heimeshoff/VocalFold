module AudioRecorder

open System
open NAudio.Wave
open NAudio.Dsp
open System.Collections.Generic

// Result type for recording
type RecordingResult = {
    Samples: float32[]
    SampleRate: int
    IsMuted: bool  // True if microphone was muted (audio level too low)
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
    mutable IsMutedRealtime: bool  // Track if currently muted in real-time
    mutable LowAudioBufferCount: int  // Count consecutive buffers with low audio
    RecordingStopped: System.Threading.ManualResetEvent
    OnLevelUpdate: (float32 -> unit) option  // Callback for real-time level updates
    OnSpectrumUpdate: (float32[] -> unit) option  // Callback for frequency spectrum (5 bands)
    OnMuteStateChanged: (bool -> unit) option  // Callback when mute state changes (true = muted, false = unmuted)
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
    // Use strict threshold to detect actual mute (not just quiet audio)
    let isMuted = maxLevel < 0.0001f
    if isMuted then
        printfn "  ⚠️  WARNING: Microphone appears to be muted (max: %.4f)" maxLevel
        printfn "     This might indicate:"
        printfn "     - Microphone is muted"
        printfn "     - Wrong microphone selected"
        printfn "     - No audio input detected"
    // Warn if audio is just quiet (but not muted)
    elif maxLevel < 0.01f then
        printfn "  ⚠️  WARNING: Audio level is very low (max: %.4f)" maxLevel
        printfn "     Transcription may not work well with such quiet audio"

    {
        Samples = totalSamples
        SampleRate = sampleRate
        IsMuted = isMuted
    }

// Calculate frequency spectrum from audio samples (5 bands for 5 bars)
let private calculateSpectrum (samples: float32[]) : float32[] =
    if samples.Length < 256 then
        Array.zeroCreate 5
    else
        // Use 256 samples for FFT (must be power of 2)
        let fftLength = 256
        let fftBuffer = Array.zeroCreate<Complex> fftLength

        // Copy samples to complex buffer (take last 256 samples)
        let startIdx = max 0 (samples.Length - fftLength)
        for i in 0 .. fftLength - 1 do
            if startIdx + i < samples.Length then
                fftBuffer.[i] <- Complex(X = samples.[startIdx + i], Y = 0.0f)

        // Apply Hamming window to reduce spectral leakage
        for i in 0 .. fftLength - 1 do
            let window = float32 (0.54 - 0.46 * Math.Cos(2.0 * Math.PI * float i / float fftLength))
            fftBuffer.[i] <- Complex(X = fftBuffer.[i].X * window, Y = 0.0f)

        // Perform FFT
        FastFourierTransform.FFT(true, int(Math.Log(float fftLength, 2.0)), fftBuffer)

        // Divide spectrum into 5 frequency bands
        // Human voice is typically 85-255 Hz (fundamental) + harmonics up to ~8kHz
        // Sample rate is 16kHz, so Nyquist is 8kHz
        // We'll focus on 0-8kHz range divided into 5 bands
        let bands = Array.zeroCreate<float32> 5
        let totalBins = fftLength / 2  // Only use first half (positive frequencies)

        // Band ranges (in bins):
        // Skip bin 0 (DC component) which is always very high
        // Band 0: 62-250Hz (low bass/fundamental)
        // Band 1: 250-750Hz (vowels low)
        // Band 2: 750-1500Hz (vowels high)
        // Band 3: 1500-3000Hz (consonants)
        // Band 4: 3000-8000Hz (sibilants/high)
        let bandRanges = [|(1, 4); (4, 12); (12, 24); (24, 48); (48, 128)|]

        for bandIdx in 0 .. 4 do
            let (startBin, endBin) = bandRanges.[bandIdx]
            let mutable bandEnergy = 0.0
            let mutable binsInBand = 0

            for bin in startBin .. min (endBin - 1) (totalBins - 1) do
                let magnitude = Math.Sqrt(float fftBuffer.[bin].X * float fftBuffer.[bin].X + float fftBuffer.[bin].Y * float fftBuffer.[bin].Y)
                bandEnergy <- bandEnergy + magnitude
                binsInBand <- binsInBand + 1

            // Average and normalize
            if binsInBand > 0 then
                bands.[bandIdx] <- float32 (bandEnergy / float binsInBand)

        // Find max value for auto-scaling
        let maxBand = bands |> Array.max

        // Normalize to 0-1 range with aggressive scaling
        // Use a much smaller divisor and add boost
        let normalized =
            if maxBand > 0.0001f then
                bands |> Array.map (fun x -> min 1.0f ((x / maxBand) * 1.5f))  // Auto-scale + 50% boost
            else
                Array.zeroCreate 5

        normalized

// Start recording (non-blocking, returns RecordingState)
let startRecording (deviceNumber: int option) (onLevelUpdate: (float32 -> unit) option) (onSpectrumUpdate: (float32[] -> unit) option) (onMuteStateChanged: (bool -> unit) option) : RecordingState =
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
        IsMutedRealtime = false
        LowAudioBufferCount = 0
        RecordingStopped = new System.Threading.ManualResetEvent(false)
        OnLevelUpdate = onLevelUpdate
        OnSpectrumUpdate = onSpectrumUpdate
        OnMuteStateChanged = onMuteStateChanged
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

        // Real-time mute detection
        // Use a very strict threshold to detect actual mute (not just quiet audio)
        let muteThreshold = 0.0001f  // Only trigger for essentially zero audio (actual mute)
        let buffersToDetectMute = 3  // Need 3 consecutive silent buffers to trigger mute

        if bufferMaxLevel < muteThreshold then
            // Audio is low, increment counter
            state.LowAudioBufferCount <- state.LowAudioBufferCount + 1

            // If we've had enough consecutive low buffers and not already marked as muted
            if state.LowAudioBufferCount >= buffersToDetectMute && not state.IsMutedRealtime then
                state.IsMutedRealtime <- true
                Logger.warning "Microphone appears to be muted (real-time detection)"
                // Notify about mute state change
                match state.OnMuteStateChanged with
                | Some callback -> callback true
                | None -> ()
        else
            // Audio is good, reset counter
            if state.LowAudioBufferCount > 0 then
                state.LowAudioBufferCount <- 0

                // If we were muted and now have audio, unmute
                if state.IsMutedRealtime then
                    state.IsMutedRealtime <- false
                    Logger.info "Microphone unmuted (real-time detection)"
                    // Notify about mute state change
                    match state.OnMuteStateChanged with
                    | Some callback -> callback false
                    | None -> ()

        // Call level update callback if provided
        match state.OnLevelUpdate with
        | Some callback -> callback bufferMaxLevel
        | None -> ()

        // Calculate and send frequency spectrum if callback provided
        match state.OnSpectrumUpdate with
        | Some callback ->
            // Use a rolling window of recent samples for FFT
            let recentSamples =
                if state.RecordedSamples.Count >= 512 then
                    let startIdx = state.RecordedSamples.Count - 512
                    let range = state.RecordedSamples.GetRange(startIdx, 512)
                    range.ToArray()
                else
                    state.RecordedSamples.ToArray()

            let spectrum = calculateSpectrum recentSamples
            callback spectrum
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

    // Use real-time mute state if available, otherwise fall back to checking max level
    // Use strict threshold to detect actual mute (not just quiet audio)
    let isMuted = state.IsMutedRealtime || state.MaxLevel < 0.0001f
    if isMuted then
        printfn "  ⚠️  WARNING: Microphone appears to be muted (max: %.4f)" state.MaxLevel
        printfn "     This might indicate:"
        printfn "     - Microphone is muted"
        printfn "     - Wrong microphone selected"
        printfn "     - No audio input detected"
    // Warn if audio is just quiet (but not muted)
    elif state.MaxLevel < 0.01f then
        printfn "  ⚠️  WARNING: Audio level is very low (max: %.4f)" state.MaxLevel
        printfn "     Transcription may not work well with such quiet audio"

    // Warn if recording is too short
    if duration < 0.3 then
        printfn "  ⚠️  WARNING: Recording is very short (%.2f seconds)" duration
        printfn "     Hold the hotkey longer for better transcription results"

    {
        Samples = totalSamples
        SampleRate = sampleRate
        IsMuted = isMuted
    }
