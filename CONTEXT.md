# Context for Claude Code

## Project: Voice-to-Text Application

### Quick Summary
Windows desktop app that transcribes voice to text using Whisper.NET AI. Press Ctrl+Windows → speak → text appears at cursor.

### Technology Stack
- **Language**: F# (.NET 9.0)
- **Platform**: Windows 11, x64
- **GPU**: RTX 3080 with CUDA
- **AI**: Whisper.NET with CUDA runtime

### Key Files to Reference
1. **SPECIFICATION.md** - Requirements & constraints
2. **ARCHITECTURE.md** - Technical design decisions
3. **TASKS.md** - Implementation roadmap

### Current Status
**Phase**: Phase 8 - Hotkey Configuration (COMPLETED)
**Last Completed**: Phase 8: Hotkey Configuration
**Next Task**: Phase 9: Voice Activity Visualization

### Important Constraints

**MUST Use F#** - Not C#, not Python
- Functional-first approach
- Pattern matching for errors
- Module-based organization

**MUST Use These Libraries**:
```xml
<PackageReference Include="Whisper.net" Version="1.7.1" />
<PackageReference Include="Whisper.net.Runtime.Cuda" Version="1.7.1" />
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="InputSimulatorCore" Version="1.0.5" />
```

**MUST Support**:
- GPU acceleration (CUDA)
- System-wide hotkeys
- Background operation
- Offline processing

### Architecture Overview
```
Hotkey Press (WinAPI)
  → Audio Recording (NAudio, 16kHz mono)
  → Transcription (Whisper.NET + CUDA)
  → Text Typing (InputSimulator)
```

### Modules to Implement
1. **WinAPI** - Windows hotkey registration & message loop
2. **AudioRecorder** - Microphone capture
3. **Transcription** - Whisper.NET Service class
4. **TextInput** - Keyboard simulation
5. **HotkeyManager** - Callback dispatch
6. **Main** - Orchestration

### Key Design Decisions

**Why F#?**
- Better module organization than C#
- Natural functional composition
- Cleaner code for this use case

**Why Whisper.NET?**
- Native .NET (no Python)
- CUDA support built-in
- Single .exe deployment

**Why Modular Monolith?**
- Simple deployment
- Fast module communication
- Easy to understand

### Performance Targets
- Model load: <3s (first time)
- Transcription: <1s for 5s speech (Base model, RTX 3080)
- Memory: <2GB
- Single .exe: <100MB

### Testing Strategy
- Test each module independently
- Manual testing in Notepad, browser, Word
- Console output for all operations
- Graceful error handling

### Common Pitfalls to Avoid
❌ Don't use Python subprocess  
❌ Don't use clipboard for text input  
❌ Don't store audio to disk  
❌ Don't use Windows Forms for hotkeys (use Win32 API)  
❌ Don't crash on errors (fail gracefully)  

✅ Do use P/Invoke for Win32 API  
✅ Do use InputSimulator for typing  
✅ Do process audio in-memory  
✅ Do provide console feedback  
✅ Do continue running after errors  

### Code Style Guidelines

**F# Conventions**:
```fsharp
// Module-level organization
module AudioRecorder =
    type RecordingResult = { ... }
    let recordAudio duration = ...

// Clear type signatures
let processAudio (samples: float32[]) : string = ...

// Pattern matching for errors
match result with
| Ok value -> ...
| Error msg -> ...

// Async for I/O
let transcribe audio = async { ... }
```

### User Experience Requirements
- Clear console output (emojis OK: 🎤 ✓ ⚠️ ✗)
- Status for each operation
- Error messages are actionable
- Timing information shown
- First-run instructions clear

### Build Commands

**Development**:
```bash
dotnet restore
dotnet build
dotnet run
```

**Production**:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Session Workflow

When starting work:
1. Read TASKS.md for current task
2. Check SPECIFICATION.md for requirements
3. Reference ARCHITECTURE.md for design
4. Implement & test
5. Update this file with progress

### Questions to Consider
- Does this task match the spec?
- Is this module testable independently?
- Are errors handled gracefully?
- Is console output clear?
- Would this work on a clean machine?

### Resources
- Whisper.NET: https://github.com/sandrohanea/whisper.net
- NAudio: https://github.com/naudio/NAudio
- F# Docs: https://learn.microsoft.com/en-us/dotnet/fsharp/

---

**Remember**: 
- F# not C#
- GPU acceleration essential
- Fail gracefully, never crash
- Clear user feedback
- Test each module
