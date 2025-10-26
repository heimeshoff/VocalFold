# Voice-to-Text Application - Claude Code Project

> **Local AI-powered voice transcription for Windows using F# and Whisper.NET**

Press Ctrl+Windows → Speak → Text appears at your cursor. 100% offline.

---

## 🚀 Quick Start with Claude Code

### 1. Setup (One Time)

**Prerequisites**:
- Windows 11 (x64)
- .NET 9.0 SDK
- CUDA Toolkit 12.x (for GPU acceleration)
- Claude Code installed

**Copy these files to your project directory**:
```
SPECIFICATION.md        (Requirements)
ARCHITECTURE.md         (Technical design)
TASKS.md               (Implementation plan)
CONTEXT.md             (Session context)
CLAUDE_CODE_WORKFLOW.md (How to use Claude Code)
```

### 2. Start Implementation

**Open Claude Code** in your project directory:
```bash
cd C:\path\to\VoiceToText
claude
```

**First prompt**:
```
Hi! I want to implement the Voice-to-Text application.

Please read:
- SPECIFICATION.md
- ARCHITECTURE.md  
- TASKS.md
- CONTEXT.md

Then start with Task 1.1. Let me know when you're ready.
```

**Claude Code will**:
1. Read all context documents
2. Create F# project structure
3. Add NuGet dependencies
4. Implement modules step-by-step
5. Test after each module
6. Build standalone .exe

---

## 📁 Project Structure

### Core Documentation

| File | Purpose | When to Read |
|------|---------|-------------|
| **SPECIFICATION.md** | Requirements & constraints | Before starting |
| **ARCHITECTURE.md** | Technical design decisions | During implementation |
| **TASKS.md** | Step-by-step implementation plan | Follow this order |
| **CONTEXT.md** | Session continuity & quick ref | Every session |

### Supporting Documentation

| File | Purpose |
|------|---------|
| **CLAUDE_CODE_WORKFLOW.md** | How to work with Claude Code |
| **METHODOLOGY_COMPARISON.md** | Why this structure |
| **QUICKSTART.md** | End-user quick start guide |
| **TROUBLESHOOTING.md** | Common issues & solutions |
| **DEVELOPER_GUIDE.md** | Extension & customization |

---

## 🎯 What Gets Built

### End Result
A Windows desktop application that:
- ✅ Runs in background
- ✅ Activates with Ctrl+Windows
- ✅ Records your voice
- ✅ Transcribes using Whisper.NET AI (GPU accelerated)
- ✅ Types text at cursor position
- ✅ Works in any application
- ✅ 100% offline after setup
- ✅ Single .exe deployment

### Technical Stack
- **Language**: F# (.NET 9.0)
- **AI Engine**: Whisper.NET with CUDA
- **Audio**: NAudio
- **Input**: InputSimulatorCore
- **Hotkeys**: Windows API (P/Invoke)

### Performance Targets
- Transcription: <1s for 5s of speech (RTX 3080, Base model)
- Memory: <2GB
- Model load: <3s first time
- Accuracy: >90% for clear English

---

## 📋 Implementation Overview

### Phase 1: Foundation (Tasks 1.1-1.3)
- Create F# project
- Add dependencies
- Implement Windows hotkey detection

**Checkpoint**: Ctrl+Windows press detected

### Phase 2: Audio (Tasks 2.1-2.2)
- Implement audio recording module
- 16kHz mono format
- Convert to float32 samples

**Checkpoint**: Can record 5s of audio

### Phase 3: AI (Tasks 3.1-3.2)
- Auto-download Whisper.NET model
- Implement transcription service
- GPU acceleration setup

**Checkpoint**: Can transcribe test audio

### Phase 4: Output (Task 4.1)
- Implement text typing
- Keyboard simulation
- Works in any app

**Checkpoint**: Can type "Hello World" in Notepad

### Phase 5: Integration (Tasks 5.1-5.3)
- Wire all modules together
- Error handling
- Logging & feedback

**Checkpoint**: End-to-end demo works

### Phase 6: Build (Tasks 6.1-6.2)
- Create standalone .exe
- Helper scripts
- Documentation

**Checkpoint**: Distributable application ready

---

## 🎓 Why This Structure?

### Problem: Claude Code Needs Guidance

**Without structure**:
- ❌ Might use wrong technology (Python instead of F#)
- ❌ Might over-engineer or under-engineer
- ❌ Hard to resume in new session
- ❌ Unclear what "done" looks like

**With this structure**:
- ✅ Technology stack locked in
- ✅ Clear module boundaries
- ✅ Step-by-step plan
- ✅ Quality gates built in
- ✅ Easy to resume

### Comparison with Other Methods

| Method | Setup | Claude Fit | For This Project |
|--------|-------|------------|------------------|
| **This structure** | 1h | ⭐⭐⭐⭐⭐ | ✅ Perfect |
| PRD only | 30m | ⭐⭐⭐ | ❌ Too vague |
| Full spec | 3h | ⭐⭐⭐⭐ | ❌ Overkill |

See METHODOLOGY_COMPARISON.md for detailed analysis.

---

## 🔧 How to Use This Repository

### For Implementation

1. **Read CLAUDE_CODE_WORKFLOW.md** first
2. **Follow TASKS.md** step-by-step
3. **Reference SPECIFICATION.md** for requirements
4. **Reference ARCHITECTURE.md** for design
5. **Update CONTEXT.md** with progress

### For Understanding

1. **SPECIFICATION.md**: What we're building and why
2. **ARCHITECTURE.md**: How it's structured
3. **METHODOLOGY_COMPARISON.md**: Why this approach

### For End Users

After implementation complete:
1. **QUICKSTART.md**: Get running in 5 minutes
2. **README.md** (you're here): Overview
3. **TROUBLESHOOTING.md**: Common issues

---

## 📊 Expected Timeline

| Phase | Time | Key Deliverable |
|-------|------|----------------|
| Phase 1 | 30m | Hotkey detection |
| Phase 2 | 45m | Audio recording |
| Phase 3 | 1.5h | AI transcription |
| Phase 4 | 30m | Text typing |
| Phase 5 | 1h | Integration |
| Phase 6 | 30m | Build & deploy |
| **Total** | **4-5h** | **Working application** |

*Assumes Claude Code handles implementation, you do testing*

---

## 🎯 Success Criteria

### Minimum Viable Product (MVP)

✅ **Functional**:
- [ ] Hotkey (Ctrl+Windows) works system-wide
- [ ] Records audio from microphone
- [ ] Transcribes with Whisper.NET AI
- [ ] Types result at cursor position
- [ ] Works in Notepad, browser, Word

✅ **Performance**:
- [ ] <1s transcription (5s speech, Base model, GPU)
- [ ] <2GB memory usage
- [ ] Stable for hours of use

✅ **Quality**:
- [ ] >90% accuracy for clear English
- [ ] Graceful error handling
- [ ] Clear console feedback

✅ **Deployment**:
- [ ] Single .exe builds successfully
- [ ] Works on clean Windows 11 machine
- [ ] Complete documentation

---

## 🔍 Module Architecture

```
┌─────────────────────────────────────┐
│         Main Application            │
│  (Hotkey → Record → Transcribe →   │
│   Type at cursor)                   │
└───────────┬─────────────────────────┘
            │
    ┌───────┴────┬─────────┬────────┐
    │            │         │        │
┌───▼──┐  ┌─────▼──┐  ┌───▼───┐  ┌─▼────┐
│WinAPI│  │Audio   │  │Whisper│  │Text  │
│      │  │Recorder│  │Service│  │Input │
└──────┘  └────────┘  └───────┘  └──────┘
```

**WinAPI Module**: Global hotkey registration & message loop  
**AudioRecorder Module**: Microphone capture, 16kHz mono  
**Whisper.NET Service**: AI transcription with GPU acceleration  
**TextInput Module**: Keyboard simulation  

---

## 🛡️ Key Design Decisions

### Why F# over C#?
- Natural functional composition
- Better type inference
- Cleaner module organization
- Pattern matching for error handling

### Why Whisper.NET over Python?
- Native .NET integration
- No Python runtime needed
- Single .exe deployment
- Better Windows integration
- Built-in CUDA support

### Why Modular Monolith?
- Simple deployment (one exe)
- Fast module communication
- Easy to understand
- Right size for this scope

---

## 📚 Documentation Guide

### Before Starting
1. Read this README
2. Read SPECIFICATION.md
3. Read CLAUDE_CODE_WORKFLOW.md
4. Skim ARCHITECTURE.md

### During Implementation
- Follow TASKS.md
- Reference ARCHITECTURE.md
- Update CONTEXT.md

### After Completion
- Users read QUICKSTART.md
- Devs read DEVELOPER_GUIDE.md
- Issues: TROUBLESHOOTING.md

---

## ⚙️ Configuration

### Easy to Change (in code)
- Hotkey combination
- Recording duration
- Whisper.NET model size
- Language
- Typing speed

### Requires CUDA
- GPU acceleration
- Install CUDA Toolkit 12.x
- RTX 3080 has 10GB VRAM (can use up to Medium model)

---

## 🚨 Important Notes

### Technology Constraints
- **MUST use F#** (not C#, not Python)
- **MUST use specified libraries** (Whisper.NET, NAudio, etc.)
- **Windows-only** (uses Win32 API)
- **NVIDIA GPU only** (CUDA requirement)

### Development Guidelines
- Test each module independently
- Follow task order in TASKS.md
- Don't skip checkpoints
- Update CONTEXT.md regularly

### Quality Standards
- All errors handled gracefully
- Clear console output
- No crashes
- Continue running after errors

---

## 🎮 Usage (After Implementation)

### Quick Start
1. Run `VoiceToText.exe`
2. See console: "Press Ctrl+Windows"
3. Click in any text field
4. Press Ctrl+Windows
5. Speak clearly
6. Text appears!

### Tips for Best Results
- Use quiet environment
- Speak clearly at normal pace
- Use quality microphone
- Close GPU-heavy apps
- Position cursor before recording

---

## 🔧 Extending the Application

See DEVELOPER_GUIDE.md for:
- Adding voice commands
- Implementing silence detection
- Multiple hotkeys
- System tray icon
- Configuration file
- Custom vocabulary

---

## 📞 Getting Help

### Issues During Implementation
1. Check TASKS.md acceptance criteria
2. Review ARCHITECTURE.md design
3. Verify SPECIFICATION.md requirements
4. See CLAUDE_CODE_WORKFLOW.md tips

### Issues After Implementation
1. Check TROUBLESHOOTING.md
2. Verify prerequisites installed
3. Test microphone in Windows
4. Check CUDA installation

---

## 📄 License & Usage

This is a reference implementation. Core dependencies:
- Whisper.NET: MIT License
- NAudio: MIT License
- InputSimulatorCore: MIT License

---

## 🎉 What You Get

### Deliverables
- ✅ Working voice-to-text application
- ✅ F# source code (well-structured, documented)
- ✅ Standalone .exe (<100MB)
- ✅ Helper scripts (run.bat, build-exe.bat)
- ✅ Complete documentation
- ✅ Troubleshooting guide

### Benefits
- 🎯 100% privacy (offline processing)
- ⚡ GPU-accelerated (fast transcription)
- 🎨 Works anywhere (any Windows app)
- 🔒 No telemetry, no cloud services
- 📦 Easy distribution (single file)

---

## 🚀 Ready to Start?

### Next Steps:

1. **Install prerequisites**:
   - .NET 9.0 SDK
   - CUDA Toolkit 12.x

2. **Read the workflow**:
   - Open CLAUDE_CODE_WORKFLOW.md
   - Understand the process

3. **Start Claude Code**:
   - `cd` to project directory
   - Run `claude`
   - Give it the first prompt

4. **Follow the plan**:
   - Claude Code reads docs
   - Implements TASKS.md
   - You test each checkpoint

**That's it! In 4-5 hours you'll have a working voice-to-text app! 🎤→📝**

---

**Questions?** Check CLAUDE_CODE_WORKFLOW.md and TROUBLESHOOTING.md first.

**Want to understand why this structure?** Read METHODOLOGY_COMPARISON.md.

**Ready to dive deep?** Read SPECIFICATION.md and ARCHITECTURE.md.

**Let's build this! 🚀**
