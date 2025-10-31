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
**Phase**: All Phases Complete (Phases 1-14) ✅
**Last Completed**: Phase 13 & Phase 14 - All tasks completed
**Status**: Full-featured voice-to-text application with web UI, keyword categorization, and multi-GPU support

**All Phases Complete**: 14 of 14 phases completed (100%)

- ✅ **Phases 1-11 COMPLETE - Core Functionality**:
  - Project foundation and module implementation
  - Windows tray integration and auto-start
  - Hotkey configuration and voice visualization
  - Character-by-character typing (preserves clipboard)
  - Keyword replacement system

- ✅ **Phase 12 COMPLETE - Web-based Settings UI**:
  - Giraffe web server with full REST API
  - Fable + React + TailwindCSS frontend
  - All endpoints functional: GET/PUT /api/settings, GET/POST/PUT/DELETE /api/keywords
  - Integration with main app complete
  - Browser launch working, real-time sync working
  - Modern, beautiful UI with brand colors (#25abfe blue, #ff8b00 orange)

- ✅ **Phase 13 COMPLETE - Keyword Categorization**:
  - KeywordCategory type with Name, IsExpanded, Color fields
  - Updated KeywordReplacement with optional Category field
  - Categories list added to AppSettings with migration support
  - Default categories: Uncategorized, Punctuation, Email Templates, Code Snippets
  - Full REST API for categories: GET/POST/PUT/DELETE /api/categories
  - Category operations: create, update, delete, toggle state, move keywords
  - Category Accordion UI with collapsible sections
  - Category Management Modal for creating/editing categories
  - Keyword Modal with category dropdown
  - Drag-and-drop between categories
  - Search and filter across categories
  - Category statistics and visualization
  - Bulk operations and sorting
  - Complete documentation and user guide

- ✅ **Phase 14 COMPLETE - AMD GPU Support via Vulkan**:
  - Added Whisper.net.Runtime.Vulkan v1.7.1 package
  - Updated TranscriptionService.fs with runtime logging
  - Updated SPECIFICATION.md with multi-GPU support details
  - Updated ARCHITECTURE.md with runtime selection info
  - Created comprehensive README.md with GPU requirements
  - Created TROUBLESHOOTING.md for GPU-related issues
  - Automatic runtime selection: CUDA → Vulkan → CPU
  - Support for AMD Radeon RX 6000+, Intel Arc GPUs
  - Build successful with Vulkan runtime included

**Build Output**: `VocalFold.WebUI/dist/` (production-ready)

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
