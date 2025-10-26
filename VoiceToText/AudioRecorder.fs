module AudioRecorder

open System
open NAudio.Wave
open System.Collections.Generic

// Result type for recording
type RecordingResult = {
    Samples: float32[]
    SampleRate: int
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
