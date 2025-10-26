# Implementation Tasks

## Task List for Claude Code

### Phase 1: Project Foundation ⬜

**Task 1.1: Project Setup**
```bash
dotnet new console -lang F# -n VoiceToText
# Update .fsproj with dependencies
```

Dependencies:
- Whisper.net (1.7.1) - Whisper.NET library
- Whisper.net.Runtime.Cuda (1.7.1) - CUDA runtime for Whisper.NET
- NAudio (2.2.1)
- InputSimulatorCore (1.0.5)

**Acceptance**: `dotnet build` succeeds

---

**Task 1.2: WinAPI Module - P/Invoke Setup**

Create module with Windows API signatures:
- RegisterHotKey
- UnregisterHotKey
- GetMessage, TranslateMessage, DispatchMessage
- Constants: MOD_CONTROL, MOD_SHIFT, VK_SPACE, WM_HOTKEY

**Acceptance**: Module compiles, constants defined

---

**Task 1.3: WinAPI Module - Message Loop**

Implement Windows message loop:
- Continuous loop calling GetMessage
- Detect WM_HOTKEY messages
- Dispatch to callback
- Handle loop exit (Ctrl+C)

**Acceptance**: Can detect when Ctrl+Windows is pressed

---

### Phase 2: Audio Capture ⬜

**Task 2.1: AudioRecorder Module Structure**

Define types:
```fsharp
type RecordingResult = {
    Samples: float32[]
    SampleRate: int
}
```

**Acceptance**: Types compile

---

**Task 2.2: Audio Recording Implementation**

Implement recordAudio function:
- Use NAudio WaveInEvent
- 16kHz sample rate, mono
- Convert byte buffer to float32[]
- Max duration parameter
- Print feedback ("🎤 Recording...")

**Acceptance**: Records 5s of audio, returns valid samples

---

### Phase 3: Whisper Transcription ⬜

**Task 3.1: Model Download**

Implement auto-download logic:
- Check ~/.whisper-models/ for model
- Download Base model if missing
- Use Whisper.NET ModelDownloader
- Show progress

**Acceptance**: Whisper.NET model downloads on first run

---

**Task 3.2: Whisper.NET Service Class**

Create transcription service:
- Constructor loads Whisper.NET model from path
- Configure for English, beam size 5
- Transcribe method (async)
- Returns concatenated segment text
- IDisposable implementation

**Acceptance**: Can transcribe test audio using Whisper.NET

---

### Phase 4: Text Output ⬜

**Task 4.1: TextInput Module**

Implement typeText function:
- Use InputSimulatorCore
- Small delay before typing (100ms)
- Type character by character
- Handle empty strings

**Acceptance**: Types "Hello World" in Notepad

---

### Phase 5: Integration ⬜

**Task 5.1: HotkeyManager Module**

Create hotkey management wrapper:
- registerHotkey function (stores callback)
- messageLoop function (calls WinAPI)
- unregisterHotkey function

**Acceptance**: Callback executes on hotkey press

---

**Task 5.2: Main Application**

Wire all modules together:
```
Hotkey Press → Record Audio → Transcribe → Type Text
```

Add:
- Startup logging (app name, model, hotkey)
- Error handling in main callback
- Cleanup on exit
- Try-catch around each step

**Acceptance**: End-to-end workflow works

---

**Task 5.3: Polish & Test**

- Test in multiple apps (Notepad, browser, Word)
- Verify performance (<1s transcription)
- Check memory usage
- Test error scenarios
- Update documentation with results

**Acceptance**: All manual tests pass

---

### Phase 6: Build & Deploy ⬜

**Task 6.1: Create Helper Scripts**

Create:
- run.bat (quick start)
- build-exe.bat (standalone build)

**Acceptance**: Scripts work correctly

---

**Task 6.2: Standalone Build**

Create self-contained executable:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Acceptance**: Single .exe works on clean machine

---

## Implementation Order

1. **Start here**: Task 1.1 (Project Setup)
2. **Then**: Task 1.2-1.3 (WinAPI - test hotkeys work)
3. **Then**: Task 2.1-2.2 (Audio - test recording)
4. **Then**: Task 3.1-3.2 (Whisper - test transcription)
5. **Then**: Task 4.1 (TextInput - test typing)
6. **Then**: Task 5.1-5.3 (Integration - wire it all together)
7. **Finally**: Task 6.1-6.2 (Build & deploy)

## Testing After Each Task

- Compile check: `dotnet build`
- Run check: `dotnet run`
- Module test: Call function with test input
- Console output: Verify feedback messages

## Success Criteria

✅ Compiles without errors
✅ Hotkey detection works
✅ Audio recording works
✅ Whisper.NET transcription works
✅ Text typing works
✅ End-to-end workflow complete
✅ Standalone exe builds  

---

**Status**: Ready for implementation  
**Estimated Time**: 4-6 hours  
**Next Task**: 1.1 Project Setup
