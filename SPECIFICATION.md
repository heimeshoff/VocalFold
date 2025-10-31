# Voice-to-Text Application - Specification

## Project Overview
A Windows 11 desktop application that transcribes voice to text using AI (Whisper.NET), activated by a global hotkey, and types the result at the cursor position.

## Core Requirements

### Functional Requirements

**FR-1: Voice Recording**
- MUST activate recording via global hotkey (Ctrl+Windows)
- MUST capture audio from system microphone
- MUST record at 16kHz mono (Whisper.NET-optimized format)
- MUST support configurable max duration (default: 10 seconds)
- SHOULD provide audio feedback when recording starts

**FR-2: AI Transcription**
- MUST use Whisper.NET AI for speech-to-text
- MUST support GPU acceleration (CUDA for RTX 3080)
- MUST support multiple model sizes (Tiny, Base, Small, Medium, Large)
- MUST process audio locally (no cloud services)
- MUST provide fallback to CPU if GPU unavailable

**FR-3: Text Output**
- MUST type transcribed text at current cursor position
- MUST work in any Windows application that accepts keyboard input
- MUST simulate natural typing (not clipboard paste)
- SHOULD type at configurable speed

**FR-4: Hotkey Management**
- MUST register system-wide hotkey
- MUST respond to hotkey regardless of active window
- MUST release hotkey on application exit
- SHOULD support configurable hotkey combinations

### Non-Functional Requirements

**NFR-1: Performance**
- Target: <1s transcription time for 5s of speech (Base model, RTX 3080)
  - NVIDIA RTX 3080 (CUDA): ~0.5s
  - AMD RX 6700 XT (Vulkan): ~1.0-2.0s
  - AMD RX 5700 XT (Vulkan): ~2.0-3.0s
  - Intel Arc A750 (Vulkan): ~2.0-3.0s
  - CPU fallback: ~5-10s
- Max memory: <2GB for Base model
- GPU utilization: Efficient CUDA/Vulkan usage

**NFR-2: Reliability**
- MUST handle microphone disconnection gracefully
- MUST handle CUDA/Vulkan errors with fallback
- MUST not crash on invalid audio input
- SHOULD continue running after individual errors
- MUST provide automatic runtime selection (CUDA → Vulkan → CPU)

**NFR-3: Usability**
- MUST provide clear console feedback for all actions
- MUST auto-download Whisper model on first run
- SHOULD complete setup in <5 minutes
- SHOULD work "out of the box" after prerequisites

**NFR-4: Distribution**
- MUST build as standalone .exe
- MUST bundle all dependencies except CUDA Toolkit
- Target size: <100MB for packaged executable

## Technical Constraints

### Platform
- **OS**: Windows 11 (primary), Windows 10 (secondary)
- **Architecture**: x64 only
- **GPU**:
  - NVIDIA GPUs with CUDA support (RTX 20 series or newer)
  - AMD Radeon GPUs with Vulkan support (RX 6000 series or newer)
  - Intel Arc GPUs with Vulkan support
  - CPU fallback if no compatible GPU available
- **Runtime**: .NET 9.0

### Technology Stack (REQUIRED)
- **Language**: F# (functional-first .NET language)
- **AI Engine**: Whisper.NET with multi-runtime support
  - CUDA runtime for NVIDIA GPUs
  - Vulkan runtime for AMD/Intel GPUs (Phase 14)
  - CPU fallback for unsupported hardware
- **Audio**: NAudio library
- **Input Simulation**: InputSimulatorCore
- **Global Hotkeys**: Windows API (P/Invoke)

### Dependencies
```xml
<PackageReference Include="Whisper.net" Version="1.7.1" />
<PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.7.1" />
<PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.7.1" />
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="InputSimulatorCore" Version="1.0.5" />
```

## User Stories

### US-1: Quick Voice Note
```
AS A user writing an email
I WANT to press Ctrl+Windows and speak
SO THAT my words appear as text without typing
```

### US-2: First-Time Setup
```
AS A new user
I WANT the app to download the AI model automatically
SO THAT I don't need to manually configure anything
```

### US-3: Background Operation
```
AS A user working on multiple tasks
I WANT the app to run in background
SO THAT I can use voice-to-text anytime
```

## Success Criteria

1. ✅ User can press hotkey in any app and speak
2. ✅ Transcription appears in <2 seconds
3. ✅ Accuracy >90% for clear English speech
4. ✅ Application remains stable for hours of use
5. ✅ First-time setup completes in <5 minutes
6. ✅ Works offline after initial setup

## Out of Scope

### Explicitly NOT included:
- ❌ Cloud-based transcription services
- ❌ Real-time streaming transcription (processes after recording)
- ❌ Multiple language detection (single language per session)
- ❌ Voice commands for application control
- ❌ GUI configuration interface (console-based)
- ❌ macOS or Linux support
- ❌ Mobile device support
- ❌ Audio file transcription (microphone only)

### Future Enhancements (not in MVP):
- Voice Activity Detection (auto-stop on silence)
- System tray icon
- Configuration file support
- Multiple hotkey profiles
- Punctuation voice commands
- Real-time confidence scores

## Data & Privacy

### Privacy Requirements
- MUST NOT send audio data to external services
- MUST NOT store recordings to disk (process in memory)
- MUST NOT log sensitive transcription content
- SHOULD inform user that all processing is local

### Data Storage
- Whisper.NET models: `%USERPROFILE%\.whisper-models\`
- Application logs: In-memory or optional file output
- No user data persistence required

## Configuration Options

### User-Configurable (via code edits, future: config file)
- Hotkey combination
- Recording duration
- Whisper.NET model size
- Language
- Typing speed
- Beam size (accuracy vs speed)

### System Requirements
- **Required**: .NET 9.0 SDK
- **GPU Requirements** (optional, CPU fallback available):
  - **NVIDIA**: CUDA Toolkit 12.x, RTX 20 series or newer (8GB+ VRAM recommended)
  - **AMD**: Latest Adrenalin drivers with Vulkan support, RX 6000 series or newer (8GB+ VRAM recommended)
  - **Intel**: Latest drivers with Vulkan support, Arc series (8GB+ VRAM recommended)
- **Recommended**: 16GB RAM
- **Minimum**: 8GB RAM

## Quality Attributes

### Maintainability
- Clear module separation
- Type-safe F# code
- Well-documented functions
- Consistent naming conventions

### Testability
- Modules should be independently testable
- Audio recording mock-able
- Whisper service injectable

### Observability
- Console logging for all major operations
- Clear error messages
- Timing information for performance monitoring

## Acceptance Criteria

### For MVP Release:
1. Application starts and registers hotkey
2. Pressing hotkey captures audio
3. Audio is transcribed using Whisper.NET
4. Text appears at cursor position
5. Process repeats reliably
6. Application handles common errors gracefully
7. Can build standalone .exe
8. Documentation covers installation and usage

### Performance Benchmarks:
- Base model on RTX 3080: <1s for 5s speech
- Memory usage: <1.5GB (Base model)
- Application startup: <3s
- First transcription: <5s (includes model load)

## Technical Debt & Limitations

### Known Limitations:
- No support for elevated applications (unless app also elevated)
- Some games with anti-cheat may block keyboard simulation
- Global hotkey conflicts not auto-resolved
- CPU fallback significantly slower than GPU

### Acceptable Trade-offs:
- Console-based UI (no GUI) for simplicity
- Manual configuration via code edits (no config file in MVP)
- Single language per session (no auto-detection)
- Fixed recording duration (no dynamic VAD)

---

## Version History

**v1.0 (MVP)** - Initial specification
- Core voice-to-text functionality
- GPU acceleration
- Global hotkey support
- Console-based operation

---

**Last Updated**: 2025-10-26
**Status**: Ready for Implementation
**Priority**: High
