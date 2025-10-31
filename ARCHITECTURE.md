# Voice-to-Text Application - Architecture

## Architectural Overview

### Design Philosophy
- **Functional-First**: Leverage F#'s functional programming strengths
- **Modular**: Clear separation of concerns for testability
- **Type-Safe**: Compile-time guarantees prevent runtime errors
- **Windows-Native**: Direct OS integration for performance

### Architecture Style
**Modular Monolith** - Single executable with well-defined internal modules

```
┌─────────────────────────────────────────────────┐
│              Main Application                    │
│  (Orchestration & Windows Message Loop)          │
└────────┬────────────────────────────────────────┘
         │
    ┌────┴────┬─────────┬──────────┬──────────┐
    │         │         │          │          │
┌───▼───┐ ┌──▼───┐ ┌───▼────────┐ ┌───▼────┐ ┌──▼────┐
│WinAPI │ │Audio │ │Whisper.NET │ │TextIn  │ │Hotkey │
│Module │ │Rec.  │ │  Service   │ │Module  │ │Mgr    │
└───────┘ └──┬───┘ └───┬────────┘ └───┬────┘ └───┬───┘
             │         │              │          │
         ┌───▼─────────▼──────────────▼──────────▼───┐
         │      External Dependencies                 │
         │  (NAudio, Whisper.NET, InputSim, .NET)     │
         └────────────────────────────────────────────┘
```

## Module Design

### 1. WinAPI Module
**Purpose**: Windows API interoperability layer

**Responsibilities**:
- Global hotkey registration/unregistration
- Windows message loop handling
- Message dispatching

**Key Design Decisions**:
- **P/Invoke over C++/CLI**: Simpler, no mixed-mode assemblies
- **Direct Win32 API**: Better control than Windows Forms hotkeys
- **IntPtr handling**: Careful memory management for unmanaged code

**Interface**:
```fsharp
module WinAPI =
    val MOD_CONTROL: uint32
    val MOD_SHIFT: uint32
    val VK_SPACE: uint32
    val RegisterHotKey: IntPtr -> int -> uint32 -> uint32 -> bool
    val messageLoop: unit -> unit
```

### 2. AudioRecorder Module
**Purpose**: Capture microphone audio in Whisper.NET-compatible format

**Key Design Decisions**:
- **NAudio WaveInEvent**: Non-blocking callback-based recording
- **16kHz mono**: Optimal for Whisper.NET models
- **In-memory only**: No disk persistence for privacy

**Interface**:
```fsharp
module AudioRecorder =
    type RecordingResult = { Samples: float32[]; SampleRate: int }
    val recordAudio: maxDurationSeconds:int -> RecordingResult
```

### 3. Transcription Module
**Purpose**: AI speech-to-text using Whisper.NET

**Key Design Decisions**:
- **Whisper.NET with CUDA**: Native .NET, GPU acceleration
- **Singleton pattern**: Load model once for performance
- **IDisposable**: Proper resource cleanup

**Interface**:
```fsharp
type WhisperService =
    new: modelPath:string -> WhisperService
    member Transcribe: audioData:float32[] -> Async<string>
    interface IDisposable
```

### 4. TextInput Module
**Purpose**: Simulate keyboard input

**Interface**:
```fsharp
module TextInput =
    val typeText: text:string -> unit
```

### 5. HotkeyManager Module
**Purpose**: Global hotkey management

**Interface**:
```fsharp
module HotkeyManager =
    type HotkeyAction = unit -> unit
    val registerHotkey: callback:HotkeyAction -> int
    val messageLoop: unit -> unit
```

## Data Flow

```
Hotkey Press → AudioRecorder → Whisper.NET → TextInput
     ↓              ↓                ↓           ↓
  WinAPI      Float32 samples     String     Keyboard
  Message          16kHz           Text      Simulation
```

## Technology Stack

### Core Dependencies
```xml
<PackageReference Include="Whisper.net" Version="1.7.1" />
<PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.7.1" />
<PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.7.1" />
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="InputSimulatorCore" Version="1.0.5" />
```

### Why F# over C#?
- Natural functional composition
- Better type inference
- Cleaner module organization
- Pattern matching for error handling

### Why Whisper.NET?
- Native .NET (no Python runtime)
- Multi-runtime GPU support (CUDA + Vulkan)
- Automatic runtime selection (CUDA → Vulkan → CPU)
- Single .exe deployment
- Better Windows integration
- Broad hardware compatibility (NVIDIA, AMD, Intel GPUs)

## Performance Targets

```
Operation              | Target    | NVIDIA RTX 3080 (CUDA) | AMD RX 6700 XT (Vulkan)
-----------------------|-----------|------------------------|-------------------------
Model Load (first)     | <3s       | ~2s                    | ~2s
Transcription (Base)   | <2s       | ~0.5s                  | ~1.0-2.0s
Total (5s speech)      | <7s       | ~6s                    | ~7s
```

**Runtime Selection:**
- Whisper.NET automatically selects the best available runtime
- Priority order: CUDA (NVIDIA) → Vulkan (AMD/Intel) → CPU
- No code changes required - runtime selection is transparent

## Security & Privacy

- ✅ No network requests (except model download)
- ✅ No audio persistence
- ✅ All processing local
- ✅ No telemetry

## Build Configuration

```bash
# Development
dotnet run

# Production single-file exe
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Error Handling Strategy

- **GPU unavailable**: Automatic fallback to CPU mode (Whisper.NET handles this)
- **CUDA unavailable**: Automatic fallback to Vulkan or CPU
- **Vulkan unavailable**: Automatic fallback to CPU
- **Mic error**: Log error, skip attempt, ready for next
- **Transcription error**: Log error, continue running
- **Typing error**: Log error, continue running

**Philosophy**: Fail gracefully, never crash the background service

## GPU Acceleration (Phase 14)

### Multi-Runtime Support
VocalFold supports multiple GPU runtimes for broad hardware compatibility:

**CUDA Runtime** (NVIDIA GPUs):
- Best performance on NVIDIA RTX 20 series and newer
- Requires: NVIDIA CUDA Toolkit 12.x
- Package: `Whisper.net.Runtime.Cuda.Windows v1.7.1`

**Vulkan Runtime** (AMD/Intel GPUs):
- Good performance on AMD Radeon RX 6000+ and Intel Arc GPUs
- Requires: Latest GPU drivers with Vulkan 1.0+ support
- Package: `Whisper.net.Runtime.Vulkan v1.7.1`

**CPU Fallback**:
- Available when no compatible GPU is detected
- Significantly slower (~5-10x) but functional
- Recommended: Use Tiny or Base model for acceptable speed

### Automatic Runtime Selection
Whisper.NET automatically detects and uses the best available runtime:
1. First tries CUDA (if NVIDIA GPU + CUDA Toolkit available)
2. Falls back to Vulkan (if AMD/Intel GPU + drivers available)
3. Falls back to CPU (if no GPU acceleration available)

No code changes or configuration required - the selection is completely transparent to the application.

---

**Last Updated**: 2025-10-26  
**Status**: Ready for Implementation
