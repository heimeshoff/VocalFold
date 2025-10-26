module AudioRecorder

open System
open NAudio.Wave
open System.Collections.Generic

// Result type for recording
type RecordingResult = {
    Samples: float32[]
    SampleRate: int
}

// Convert byte buffer to float32 array
let private bytesToFloat32 (buffer: byte[]) (bytesRecorded: int) =
    let sampleCount = bytesRecorded / 2 // 16-bit samples = 2 bytes per sample
    let samples = Array.zeroCreate<float32> sampleCount

    for i in 0 .. sampleCount - 1 do
        // Read 16-bit PCM sample (little-endian)
        let byteIndex = i * 2
        let sample16 = int16 (buffer.[byteIndex] ||| (buffer.[byteIndex + 1] <<< 8))
        // Convert to float32 in range [-1.0, 1.0]
        samples.[i] <- float32 sample16 / 32768.0f

    samples

// Record audio from the default microphone
let recordAudio (maxDurationSeconds: int) : RecordingResult =
    printfn "🎤 Recording for %d seconds..." maxDurationSeconds

    let sampleRate = 16000 // Whisper.NET optimized format
    let channels = 1       // Mono

    // Setup wave format
    let waveFormat = WaveFormat(sampleRate, 16, channels)

    // Storage for recorded samples
    let recordedSamples = List<float32>()

    // Create wave input device
    use waveIn = new WaveInEvent(
        WaveFormat = waveFormat,
        BufferMilliseconds = 100
    )

    // Data available event handler
    waveIn.DataAvailable.Add(fun args ->
        let samples = bytesToFloat32 args.Buffer args.BytesRecorded
        recordedSamples.AddRange(samples)
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
    printfn "  ✓ Recording complete (%d samples, %.1f seconds)"
        totalSamples.Length
        (float totalSamples.Length / float sampleRate)

    {
        Samples = totalSamples
        SampleRate = sampleRate
    }
