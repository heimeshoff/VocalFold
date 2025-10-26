# Developer Guide - Voice-to-Text Application

## Architecture Overview

The application is built with a modular architecture in F# with clear separation of concerns:

```
Program.fs
├── WinAPI Module          (Windows API interop)
├── AudioRecorder Module   (Microphone capture)
├── Transcription Module   (Whisper.NET AI processing)
├── TextInput Module       (Keyboard simulation)
├── HotkeyManager Module   (Global hotkey handling)
└── Main Entry Point       (Orchestration)
```

## Key Modules Explained

### 1. WinAPI Module
**Purpose**: Windows API interoperability for global hotkeys

**Key Functions**:
- `RegisterHotKey`: Registers system-wide keyboard shortcuts
- `GetMessage/DispatchMessage`: Windows message loop for hotkey events

**Customization Points**:
```fsharp
// Add new modifier keys
let MOD_WIN = 0x0008u  // Windows key

// Add new virtual keys
let VK_F1 = 0x70u      // F1 key
let VK_A = 0x41u       // A key
```

### 2. AudioRecorder Module
**Purpose**: Captures audio from the system microphone

**Key Components**:
- Uses NAudio's `WaveInEvent` for real-time recording
- Outputs 16kHz mono audio (Whisper.NET's preferred format)
- Automatic timeout after specified duration

**Extension Ideas**:
```fsharp
// Add silence detection
let detectSilence (samples: float32[]) threshold =
    samples 
    |> Array.averageBy abs 
    |> fun avg -> avg < threshold

// Add different audio formats
let waveFormat = new WaveFormat(
    rate = 44100,      // Higher quality
    bits = 16,
    channels = 2       // Stereo
)
```

### 3. Transcription Module
**Purpose**: AI-powered speech-to-text using Whisper.NET

**Configuration Options**:
```fsharp
// Language selection
.WithLanguage("en")     // English
.WithLanguage("de")     // German
.WithLanguage("es")     // Spanish
.WithLanguage("fr")     // French

// Add custom prompts for better accuracy
.WithPrompt("Technical documentation about programming")

// Enable translation (to English)
.WithTranslate(true)
```

**Performance Tuning**:
```fsharp
// For faster processing, reduce beam size
factory.CreateBuilder()
    .WithLanguage("en")
    .WithBeamSize(1)      // Faster, less accurate
    .Build()

// For better accuracy, increase beam size
factory.CreateBuilder()
    .WithLanguage("en")
    .WithBeamSize(10)     // Slower, more accurate
    .Build()
```

### 4. TextInput Module
**Purpose**: Simulates keyboard input to type transcribed text

**Current Implementation**:
- Uses InputSimulatorCore library
- Types text character by character

**Advanced Options**:
```fsharp
// Add support for rich text formatting
let typeWithFormatting (text: string) =
    let simulator = new InputSimulator()
    
    // Bold text
    simulator.Keyboard
        .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_B)
        .TextEntry(text)
        .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_B)
    |> ignore

// Add clipboard alternative (faster for long text)
let typeViaClipboard (text: string) =
    Windows.Forms.Clipboard.SetText(text)
    let simulator = new InputSimulator()
    simulator.Keyboard
        .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V)
    |> ignore
```

### 5. HotkeyManager Module
**Purpose**: Manages global hotkey registration and event handling

**Multiple Hotkeys Example**:
```fsharp
// Register multiple hotkeys
let registerMultipleHotkeys () =
    // Ctrl+Windows - Record and transcribe
    WinAPI.RegisterHotKey(IntPtr.Zero, 1, 
        WinAPI.MOD_CONTROL ||| WinAPI.MOD_SHIFT, 
        WinAPI.VK_SPACE)
    
    // Ctrl+Shift+R - Toggle recording
    WinAPI.RegisterHotKey(IntPtr.Zero, 2,
        WinAPI.MOD_CONTROL ||| WinAPI.MOD_SHIFT,
        0x52u) // R key
    
    // Ctrl+Shift+C - Cancel recording
    WinAPI.RegisterHotKey(IntPtr.Zero, 3,
        WinAPI.MOD_CONTROL ||| WinAPI.MOD_SHIFT,
        0x43u) // C key
```

## Common Extensions

### 1. Add Voice Commands
```fsharp
let processVoiceCommand (text: string) =
    match text.ToLower().Trim() with
    | "new line" -> "\n"
    | "period" -> "."
    | "comma" -> ","
    | "question mark" -> "?"
    | "open browser" -> 
        System.Diagnostics.Process.Start("explorer.exe", "https://google.com") |> ignore
        ""
    | _ -> text
```

### 2. Add System Tray Icon
```fsharp
open System.Windows.Forms

let createTrayIcon () =
    let trayIcon = new NotifyIcon()
    trayIcon.Icon <- SystemIcons.Microphone
    trayIcon.Text <- "Voice-to-Text Service"
    trayIcon.Visible <- true
    
    let menu = new ContextMenuStrip()
    menu.Items.Add("Exit", null, fun _ _ -> Application.Exit()) |> ignore
    trayIcon.ContextMenuStrip <- menu
    
    trayIcon
```

### 3. Add Configuration File
```fsharp
open System.Text.Json

type AppConfig = {
    ModelType: string
    RecordingDuration: int
    Language: string
    HotkeyModifiers: uint32
    HotkeyKey: uint32
}

let loadConfig () =
    let configPath = "config.json"
    if File.Exists(configPath) then
        let json = File.ReadAllText(configPath)
        JsonSerializer.Deserialize<AppConfig>(json)
    else
        // Default configuration
        {
            ModelType = "Base"
            RecordingDuration = 10
            Language = "en"
            HotkeyModifiers = WinAPI.MOD_CONTROL ||| WinAPI.MOD_SHIFT
            HotkeyKey = WinAPI.VK_SPACE
        }
```

### 4. Add Logging
```fsharp
open System.IO

let log message =
    let timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    let logMessage = sprintf "[%s] %s" timestamp message
    File.AppendAllText("app.log", logMessage + "\n")
    printfn "%s" logMessage
```

### 5. Add Silence Detection (VAD)
```fsharp
module VoiceActivityDetection =
    let calculateRMS (samples: float32[]) =
        samples
        |> Array.map (fun x -> x * x)
        |> Array.average
        |> sqrt
    
    let isSilence (samples: float32[]) (threshold: float32) =
        calculateRMS samples < threshold
    
    let recordUntilSilence (maxDuration: int) (silenceThreshold: float32) =
        // Implementation that stops recording after detecting silence
        // This would need to be integrated into AudioRecorder module
        ()
```

## GPU Optimization

### Checking GPU Usage
```fsharp
// Add CUDA info logging
open System.Runtime.InteropServices

[<DllImport("cudart64_12.dll")>]
extern int cudaGetDeviceCount(int& count)

let checkCudaAvailability () =
    let mutable count = 0
    let result = cudaGetDeviceCount(&count)
    if result = 0 && count > 0 then
        printfn "✓ CUDA available: %d GPU(s) detected" count
        true
    else
        printfn "✗ CUDA not available"
        false
```

### CPU Fallback
```fsharp
let createWhisperProcessor useCuda =
    if useCuda && checkCudaAvailability() then
        // Use CUDA
        WhisperFactory.FromPath(modelPath)
    else
        // Fallback to CPU
        printfn "⚠ Using CPU inference (slower)"
        WhisperFactory.FromPath(modelPath)
```

## Error Handling Best Practices

```fsharp
// Robust error handling for audio recording
let recordAudioSafe maxDuration =
    try
        recordAudio maxDuration |> Some
    with
    | :? NAudio.MmException as ex ->
        printfn "✗ Microphone error: %s" ex.Message
        None
    | ex ->
        printfn "✗ Unexpected error: %s" ex.Message
        None

// Retry logic for transcription
let rec transcribeWithRetry (service: WhisperNetService) audioData retries =
    async {
        try
            return! service.Transcribe(audioData)
        with
        | ex when retries > 0 ->
            printfn "⚠ Transcription failed, retrying... (%d attempts left)" retries
            do! Async.Sleep(1000)
            return! transcribeWithRetry service audioData (retries - 1)
        | ex ->
            printfn "✗ Transcription failed: %s" ex.Message
            return ""
    }
```

## Testing Tips

### Test Audio Recording
```fsharp
let testRecording () =
    printfn "Testing audio recording..."
    let result = AudioRecorder.recordAudio 5
    printfn "Recorded %d samples" result.Samples.Length
    
    // Save to WAV file for testing
    let waveFormat = new WaveFormat(result.SampleRate, 1)
    use writer = new WaveFileWriter("test.wav", waveFormat)
    let bytes = result.Samples |> Array.map (fun s -> int16 (s * 32767.0f))
    writer.WriteSamples(bytes, 0, bytes.Length)
    printfn "Saved to test.wav"
```

### Test Transcription
```fsharp
let testTranscription () =
    use service = new Transcription.WhisperNetService(modelPath)
    
    // Load test audio file
    let testAudio = ... // Load from file
    
    let result = service.Transcribe(testAudio) |> Async.RunSynchronously
    printfn "Transcription result: %s" result
```

## Performance Profiling

```fsharp
let timeOperation name operation =
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let result = operation()
    sw.Stop()
    printfn "⏱ %s took %dms" name sw.ElapsedMilliseconds
    result

// Usage
let recording = timeOperation "Recording" (fun () -> AudioRecorder.recordAudio 10)
let text = timeOperation "Transcription" (fun () -> service.Transcribe(recording.Samples) |> Async.RunSynchronously)
```

## Deployment Checklist

- [ ] Test on clean Windows machine
- [ ] Verify CUDA Toolkit is installed
- [ ] Test all hotkey combinations
- [ ] Check microphone permissions
- [ ] Verify model download on first run
- [ ] Test typing in different applications
- [ ] Check memory usage with Task Manager
- [ ] Test with different Whisper.NET models
- [ ] Verify error handling
- [ ] Test standalone .exe build

## Useful Resources

- **Whisper.NET Library Docs**: https://github.com/sandrohanea/whisper.net
- **NAudio Docs**: https://github.com/naudio/NAudio
- **F# for Fun and Profit**: https://fsharpforfunandprofit.com/
- **CUDA Programming**: https://docs.nvidia.com/cuda/
- **Windows API Reference**: https://learn.microsoft.com/en-us/windows/win32/api/

## Contributing

When extending this application:
1. Keep modules independent and testable
2. Use F# idiomatic patterns (immutability, pattern matching)
3. Add proper error handling
4. Document your changes
5. Test on different Windows versions

---

Happy coding! 🚀
