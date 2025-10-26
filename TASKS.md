# Implementation Tasks

## Task List for Claude Code

### Phase 1: Project Foundation ⬜

**Task 1.1: Project Setup**
```bash
dotnet new console -lang F# -n VocalFold
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

---

### Phase 7: Windows Tray Integration ⬜

**Task 7.1: Tray Icon Implementation**

Create system tray functionality:
- Add System.Windows.Forms reference (for NotifyIcon)
- Create tray icon with custom icon
- Add context menu with basic options:
  - "Enable/Disable Voice Input" toggle
  - "Settings..."
  - "Exit"
- Remove console window (hide on startup)
- Ensure app runs without visible window

**Acceptance**: Application runs in system tray with working context menu

---

**Task 7.2: Windows Startup Registration**

Implement auto-start on Windows boot:
- Add registry key to `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- Create helper functions:
  - `enableStartup()` - Add to startup
  - `disableStartup()` - Remove from startup
  - `isStartupEnabled()` - Check current state
- Add "Start with Windows" toggle to tray context menu
- Store executable path correctly

**Acceptance**: Application auto-starts after Windows reboot

---

**Task 7.3: Tray Notifications**

Add user feedback via tray notifications:
- Show balloon notification when recording starts
- Show notification when transcription completes
- Show error notifications for failures
- Keep notifications brief and actionable

**Acceptance**: User receives visual feedback without console

---

### Phase 8: Hotkey Configuration ⬜

**Task 8.1: Settings Persistence**

Create configuration system:
- Define settings type:
  ```fsharp
  type AppSettings = {
      HotkeyModifiers: uint32
      HotkeyKey: uint32
      IsEnabled: bool
      ModelSize: string
      RecordingDuration: int
  }
  ```
- Store settings in `%APPDATA%/VocalFold/settings.json`
- Implement save/load functions
- Handle missing/corrupted config files

**Acceptance**: Settings persist across application restarts

---

**Task 8.2: Hotkey Configuration UI**

Create settings window:
- Simple WPF or WinForms dialog
- Hotkey picker control (record key combination)
- Display current hotkey
- "Apply" and "Cancel" buttons
- Unregister old hotkey, register new one
- Validate hotkey combinations (prevent conflicts)

**Acceptance**: User can change hotkey via GUI

---

**Task 8.3: Enable/Disable Toggle**

Implement feature activation control:
- Add toggle state to tray icon
- Change tray icon appearance when disabled (grayed out)
- Unregister hotkey when disabled
- Re-register hotkey when enabled
- Update context menu text ("Enabled ✓" / "Disabled")
- Persist enabled state in settings

**Acceptance**: User can temporarily disable voice input without exiting

---

### Phase 9: Voice Activity Visualization ⬜

**Task 9.1: Overlay Window Structure**

Create transparent overlay for visual feedback:
- WPF layered window (AllowsTransparency=true)
- Topmost window (always visible)
- No taskbar entry
- No window border
- Click-through (ignore mouse input)
- Position near system tray area

**Acceptance**: Transparent window appears on screen

---

**Task 9.2: Voice Activity Animation**

Implement visual recording indicator:
- Design options:
  - Animated microphone icon (pulsing)
  - Simple circular waveform visualization
  - Audio level meter bars
- Use audio buffer data for real-time animation
- Smooth animation (30-60 fps)
- Color coding:
  - Blue/White: Recording
  - Green: Transcribing
  - Red: Error

**Acceptance**: Visual feedback shows during recording

---

**Task 9.3: Positioning & Polish**

Fine-tune overlay behavior:
- Calculate tray icon position from Windows API
- Position overlay near tray (e.g., above and slightly left)
- Add fade-in/fade-out animations
- Auto-hide after transcription complete (500ms delay)
- Handle multi-monitor scenarios
- Scale for different DPI settings

**Acceptance**: Overlay appears smoothly near tray icon and disappears after use

---

### Phase 10: Character-by-Character Typing ⬜

**Context**: Current implementation may use Ctrl+V to paste, which overwrites the system clipboard. Users lose their copied content. Need true character-by-character typing with proper timing.

**Task 10.1: Review Current TextInput Implementation**

Analyze existing typing mechanism:
- Check if using clipboard paste (Ctrl+V)
- Identify where text output happens
- Document current character timing
- Note any issues with dropped characters

**Acceptance**: Clear understanding of current implementation

---

**Task 10.2: Implement Character-by-Character Typing**

Replace clipboard paste with individual character simulation:
- Use InputSimulatorCore's Keyboard.TextEntry() for each character
- Add configurable delay between characters (default: 10ms)
- Handle special characters (newlines, tabs, Unicode)
- Add settings option for typing speed:
  ```fsharp
  type TypingSpeed =
      | Fast      // 5ms delay
      | Normal    // 10ms delay
      | Slow      // 20ms delay
      | Custom of int
  ```
- Test with long text (100+ characters) to ensure no drops
- Measure and log actual typing speed

**Acceptance**: Text types character-by-character without clipboard usage

---

**Task 10.3: Advanced Character Handling**

Handle edge cases:
- Preserve whitespace (spaces, newlines, tabs)
- Support Unicode characters (emoji, accented letters)
- Handle keyboard layout differences
- Add retry logic for failed key presses
- Option to pause/cancel mid-typing (future enhancement placeholder)

**Acceptance**: Reliable typing across all character types

---

**Task 10.4: Add Typing Speed to Settings**

Integrate typing speed into configuration:
- Add `TypingSpeed` field to AppSettings
- Update settings UI to show typing speed options
- Add radio buttons or dropdown: Fast / Normal / Slow / Custom
- Persist typing speed preference
- Apply speed setting in TextInput module

**Acceptance**: User can configure typing speed via settings UI

---

### Phase 11: Keyword Replacement System ⬜

**Context**: Enable users to speak keywords that get replaced with longer configured text. Example: say "German email footer" → types actual email signature.

**Task 11.1: Settings Data Structure for Keywords**

Design keyword replacement storage:
- Add to AppSettings:
  ```fsharp
  type KeywordReplacement = {
      Keyword: string           // What to listen for
      Replacement: string       // What to type instead
      CaseSensitive: bool       // Match case exactly?
      WholePhrase: bool         // Match only complete phrase?
  }

  type AppSettings = {
      // ... existing fields ...
      KeywordReplacements: KeywordReplacement list
  }
  ```
- Store in settings.json
- Default to empty list on first run
- Handle JSON serialization/deserialization

**Acceptance**: Settings structure supports keyword mappings

---

**Task 11.2: Keyword Replacement Logic**

Implement text processing pipeline:
- Create new module: `TextProcessor`
- Function: `processTranscription: string -> KeywordReplacement list -> string`
- Replacement algorithm:
  1. Sort replacements by keyword length (longest first)
  2. For each replacement:
     - If `WholePhrase = true`: match only complete words/phrases
     - If `CaseSensitive = false`: case-insensitive matching
     - Replace all occurrences
  3. Return processed text
- Handle overlapping keywords (longest match wins)
- Preserve original text if no matches
- Log replacements made (for debugging)

Example:
```fsharp
Input: "Dear Sir comma German email footer"
Keywords: [("comma", ","), ("German email footer", "Best regards,\nJohn Doe\n...")]
Output: "Dear Sir, Best regards,\nJohn Doe\n..."
```

**Acceptance**: Keyword replacement works correctly with test cases

---

**Task 11.3: Integration into Transcription Flow**

Wire keyword processing into main pipeline:
```
Old: Transcribe → Type Text
New: Transcribe → Process Keywords → Type Text
```

Changes needed:
- Modify main workflow in Program.fs or Main
- Load keyword replacements from settings
- Call `TextProcessor.processTranscription` before typing
- Add timing logs to track processing overhead
- Handle errors gracefully (skip replacement on error)

**Acceptance**: Keywords are replaced in transcribed text before typing

---

**Task 11.4: Keyword Management UI**

Create interface for managing keywords:
- Settings window section: "Keyword Replacements"
- List view showing all configured keywords
- Buttons:
  - "Add New" - Opens dialog to add keyword/replacement pair
  - "Edit" - Modify existing keyword
  - "Delete" - Remove keyword
  - "Import/Export" - Save/load keyword sets (JSON file)
- Input dialog fields:
  - Keyword text box
  - Replacement text box (multi-line for long text)
  - "Case Sensitive" checkbox
  - "Whole Phrase Only" checkbox
- Validate inputs (no empty keywords)
- Save changes to settings.json

**Acceptance**: User can manage keyword replacements via GUI

---

**Task 11.5: Keyword Replacement Testing & Polish**

Comprehensive testing:
- Test with common use cases:
  - Email signatures
  - Code snippets
  - Frequently used phrases
  - Punctuation shortcuts ("comma", "period", "question mark")
- Test edge cases:
  - Very long replacements (1000+ characters)
  - Unicode in keywords/replacements
  - Overlapping keywords
  - Case sensitivity
  - Multiple replacements in one transcription
- Performance testing:
  - Measure processing time with 50+ keywords
  - Ensure <50ms overhead for replacement processing
- Add documentation:
  - Example keyword sets
  - Best practices (e.g., use whole phrase for multi-word keywords)
  - Performance considerations

**Acceptance**: Keyword replacement is fast, reliable, and well-documented

---

## Success Criteria

✅ Compiles without errors
✅ Hotkey detection works
✅ Audio recording works
✅ Whisper.NET transcription works
✅ Text typing works (character-by-character, no clipboard)
✅ End-to-end workflow complete
✅ Standalone exe builds
✅ Runs in system tray
✅ Auto-starts with Windows
✅ Hotkey is user-configurable
✅ Voice visualization provides clear feedback
✅ Character-by-character typing preserves clipboard
✅ Keyword replacement system functional

---

**Status**: Ready for implementation
**Estimated Time**:
- Phases 1-6: 4-6 hours
- Phases 7-9: 6-8 hours
- Phase 10: 2-3 hours
- Phase 11: 4-6 hours
**Next Task**: 1.1 Project Setup (or continue from current phase)
