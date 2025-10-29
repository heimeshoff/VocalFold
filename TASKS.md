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

### Phase 12: Web-based Settings UI ⬜

**Context**: Replace the current WinForms settings dialog with a modern web-based UI using F# Fable and TailwindCSS. Settings will be accessed via browser (or Electron) when clicking "Settings" in the tray menu.

**Goals**:
- Modern, clean, beautiful design following contemporary UI/UX principles
- All current functionality preserved and enhanced
- Better user experience with smooth animations and visual feedback
- Built with F# Fable (compiles to JavaScript) + TailwindCSS
- Backend REST API using Giraffe (F# web framework)
- Runs on localhost with embedded web server

**Architecture Overview**:
```
VocalFold.exe
├── Giraffe Web Server (localhost:random-port)
│   ├── REST API (/api/settings, /api/keywords, etc.)
│   └── Static file serving (HTML, JS, CSS)
└── Opens default browser to http://localhost:PORT
    └── Fable SPA (Single Page App)
        ├── Elmish MVU pattern
        ├── Feliz React components
        └── TailwindCSS styling
```

---

**Task 12.1: Backend Web Server - Project Setup**

Set up the web server infrastructure:

**Dependencies to add**:
```xml
<!-- Web Server -->
<PackageReference Include="Giraffe" Version="6.4.0" />
<PackageReference Include="Microsoft.AspNetCore.App" />

<!-- JSON -->
<PackageReference Include="System.Text.Json" /> (already exists)
```

**Implementation**:
1. Create new F# module: `WebServer.fs`
2. Configure Kestrel to run on localhost with random available port
3. Set up Giraffe routing structure
4. Configure CORS for localhost only (security)
5. Add static file middleware for serving frontend assets
6. Create port discovery mechanism (find available port)
7. Add graceful startup/shutdown logic

**Structure**:
```fsharp
module WebServer =
    type ServerConfig = {
        Port: int
        OnSettingsChanged: Settings.AppSettings -> unit
    }

    type ServerState = {
        Port: int
        CancellationToken: CancellationTokenSource
    }

    val start: ServerConfig -> Async<ServerState>
    val stop: ServerState -> Async<unit>
    val getUrl: ServerState -> string
```

**Acceptance**:
- Web server starts on random port
- Can access http://localhost:PORT (returns 404 for now, no frontend yet)
- Server stops cleanly on shutdown
- No port conflicts

---

**Task 12.2: Backend REST API - Settings Endpoints**

Implement REST API endpoints for settings management:

**Endpoints**:
```fsharp
GET  /api/settings          -> Returns current AppSettings as JSON
PUT  /api/settings          -> Updates AppSettings, saves to disk
GET  /api/status            -> Returns app status (enabled, version, etc.)
POST /api/hotkey/validate   -> Validates if hotkey is available
```

**Implementation**:
1. Create `SettingsApi.fs` module
2. Implement JSON serialization for all settings types
3. Add validation logic (e.g., hotkey must have modifiers)
4. Integrate with existing `Settings.fs` module (load/save)
5. Add error handling with proper HTTP status codes
6. Implement thread-safe settings updates
7. Add callback mechanism to notify main app of settings changes

**Example response format**:
```json
{
  "hotkeyKey": 32,
  "hotkeyModifiers": 6,
  "isEnabled": true,
  "modelSize": "Base",
  "recordingDuration": 0,
  "typingSpeed": "normal",
  "keywordReplacements": []
}
```

**Acceptance**:
- Can GET current settings via API
- Can PUT new settings via API (persists to settings.json)
- Settings changes trigger app updates (hotkey re-registration, etc.)
- Proper error responses for invalid data

---

**Task 12.3: Backend REST API - Keywords Endpoints**

Implement REST API endpoints for keyword management:

**Endpoints**:
```fsharp
GET    /api/keywords           -> Returns all keywords
GET    /api/keywords/:index    -> Returns specific keyword
POST   /api/keywords           -> Adds new keyword
PUT    /api/keywords/:index    -> Updates keyword at index
DELETE /api/keywords/:index    -> Deletes keyword at index
POST   /api/keywords/import    -> Import keywords from JSON
GET    /api/keywords/export    -> Export keywords as JSON
POST   /api/keywords/examples  -> Add example keywords
```

**Implementation**:
1. Create keyword CRUD operations
2. Add index-based addressing (0-based)
3. Implement import/export with JSON format
4. Add validation (no empty keywords)
5. Thread-safe list operations
6. Integrate example keywords from `TextProcessor` module

**Acceptance**:
- Full CRUD operations work via API
- Import/export maintains data integrity
- Thread-safe operations prevent race conditions

---

**Task 12.4: Frontend - Project Setup**

Set up the Fable frontend project:

**Project structure**:
```
VocalFold/
├── VocalFold.WebUI/           (NEW - Fable project)
│   ├── package.json           (Node.js dependencies)
│   ├── vite.config.js         (Vite bundler config)
│   ├── tailwind.config.js     (TailwindCSS config)
│   ├── postcss.config.js      (PostCSS for Tailwind)
│   ├── src/
│   │   ├── App.fs             (Main Elmish app)
│   │   ├── Api.fs             (API client)
│   │   ├── Types.fs           (Shared types)
│   │   ├── Components/        (React components)
│   │   └── Styles/
│   │       └── main.css       (TailwindCSS imports)
│   ├── public/
│   │   └── index.html         (HTML entry point)
│   └── dist/                  (Build output)
└── VocalFold/
    └── wwwroot/               (Static files served by Giraffe)
        └── (copy from dist/)
```

**Dependencies** (package.json):
```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  },
  "devDependencies": {
    "vite": "^5.0.0",
    "tailwindcss": "^3.4.0",
    "postcss": "^8.4.0",
    "autoprefixer": "^10.4.0",
    "@heroicons/react": "^2.1.0"
  }
}
```

**Fable dependencies** (.fsproj):
```xml
<PackageReference Include="Fable.Core" Version="4.3.0" />
<PackageReference Include="Feliz" Version="2.7.0" />
<PackageReference Include="Feliz.UseElmish" Version="2.4.0" />
<PackageReference Include="Thoth.Json" Version="11.0.0" />
```

**Implementation**:
1. Create VocalFold.WebUI directory
2. Initialize npm project
3. Install Vite, TailwindCSS, React
4. Configure TailwindCSS with custom theme
5. Set up Fable build pipeline
6. Create basic index.html
7. Create build scripts (dev and production)

**TailwindCSS custom theme** (dark mode, accent colors):
```js
module.exports = {
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: {
          dark: '#1a1a1a',      // DarkBackground
          card: '#2a2a2a',      // CardBackground
          sidebar: '#232323',   // SidebarBackground
        },
        accent: {
          blue: '#4A9EFF',      // AccentBlue
        },
        text: {
          primary: '#FFFFFF',   // PrimaryText
          secondary: '#B0B0B0', // SecondaryText
        }
      }
    }
  }
}
```

**Acceptance**:
- Fable project compiles successfully
- Vite dev server runs (npm run dev)
- TailwindCSS processes correctly
- Basic "Hello World" renders in browser

---

**Task 12.5: Frontend - API Client & Shared Types**

Create type-safe API client and shared types:

**Types.fs** (matching backend types):
```fsharp
module Types

type KeywordReplacement = {
    Keyword: string
    Replacement: string
    CaseSensitive: bool
    WholePhrase: bool
}

type AppSettings = {
    HotkeyKey: uint32
    HotkeyModifiers: uint32
    IsEnabled: bool
    ModelSize: string
    RecordingDuration: int
    TypingSpeed: string
    KeywordReplacements: KeywordReplacement list
}

type AppStatus = {
    IsEnabled: bool
    Version: string
    CurrentHotkey: string
}
```

**Api.fs** (HTTP client):
```fsharp
module Api

open Thoth.Json
open Fable.SimpleHttp

// API endpoints
val getSettings: unit -> Async<Result<AppSettings, string>>
val updateSettings: AppSettings -> Async<Result<unit, string>>
val getStatus: unit -> Async<Result<AppStatus, string>>
val getKeywords: unit -> Async<Result<KeywordReplacement list, string>>
val addKeyword: KeywordReplacement -> Async<Result<unit, string>>
val updateKeyword: int -> KeywordReplacement -> Async<Result<unit, string>>
val deleteKeyword: int -> Async<Result<unit, string>>
```

**Implementation**:
1. Create types matching backend exactly
2. Use Thoth.Json for JSON encoding/decoding
3. Use Fable.SimpleHttp or Fetch API
4. Implement proper error handling
5. Add loading states
6. Add timeout handling (10s timeout)

**Acceptance**:
- API calls succeed and decode properly
- Type safety enforced at compile time
- Errors handled gracefully

---

**Task 12.6: Frontend - Elmish Architecture Setup**

Set up the Elmish MVU (Model-View-Update) architecture:

**Model** (application state):
```fsharp
type Page =
    | Dashboard
    | GeneralSettings
    | KeywordSettings

type LoadingState<'T> =
    | NotStarted
    | Loading
    | Loaded of 'T
    | Error of string

type Model = {
    CurrentPage: Page
    Settings: LoadingState<AppSettings>
    Status: LoadingState<AppStatus>
    IsRecordingHotkey: bool
    EditingKeyword: (int * KeywordReplacement) option
    Toast: string option
}
```

**Messages**:
```fsharp
type Msg =
    // Navigation
    | NavigateToPage of Page

    // Settings
    | LoadSettings
    | SettingsLoaded of Result<AppSettings, string>
    | UpdateSettings of AppSettings
    | SettingsSaved of Result<unit, string>

    // Hotkey
    | StartRecordingHotkey
    | HotkeyRecorded of uint32 * uint32
    | CancelRecordingHotkey

    // Keywords
    | LoadKeywords
    | KeywordsLoaded of Result<KeywordReplacement list, string>
    | AddKeyword
    | EditKeyword of int
    | DeleteKeyword of int
    | SaveKeyword of KeywordReplacement
    | CancelEditKeyword

    // UI
    | ShowToast of string
    | DismissToast
```

**Update function** (state transitions):
```fsharp
let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | LoadSettings ->
        { model with Settings = Loading },
        Cmd.OfAsync.perform Api.getSettings () SettingsLoaded

    | SettingsLoaded (Ok settings) ->
        { model with Settings = Loaded settings }, Cmd.none

    | SettingsLoaded (Error err) ->
        { model with Settings = Error err }, Cmd.none

    // ... more cases
```

**Acceptance**:
- Elmish loop works correctly
- State updates trigger re-renders
- Commands execute async operations
- No state inconsistencies

---

**Task 12.7: Frontend - Layout & Navigation**

Create the main layout with sidebar navigation:

**Design**:
```
┌─────────────┬──────────────────────────────────┐
│             │                                  │
│   [Logo]    │         Page Title               │
│             │                                  │
│  VocalFold  │  ┌────────────────────────┐     │
│             │  │                        │     │
│ Dashboard   │  │    Main Content        │     │
│             │  │    (Cards, Forms)      │     │
│ General     │  │                        │     │
│             │  └────────────────────────┘     │
│ Keywords    │                                  │
│             │                                  │
│             │                                  │
│             │                                  │
│             │         [Apply]  [Cancel]        │
└─────────────┴──────────────────────────────────┘
```

**Components**:
1. **Sidebar** (fixed left, 240px wide):
   - Logo at top
   - App name
   - Navigation items with icons
   - Hover states
   - Active indicator

2. **Main content area** (scrollable):
   - Page title
   - Content area
   - Bottom button bar

3. **Navigation** (client-side state):
   - Highlight active page
   - Smooth transitions between pages
   - Keyboard navigation (arrow keys)

**TailwindCSS classes**:
- Use `bg-primary-dark`, `bg-primary-card`, etc.
- Smooth transitions: `transition-all duration-200`
- Hover effects: `hover:bg-opacity-80`
- Shadows: `shadow-lg`

**Acceptance**:
- Clean, modern layout renders correctly
- Navigation works smoothly
- Responsive to window resize
- Sidebar items show active state

---

**Task 12.8: Frontend - Dashboard Page**

Create the dashboard page showing current status:

**Design** (card-based layout):
```
Dashboard
┌─────────────────────────────────────────┐
│  Status                                 │
│  ● Enabled    [Toggle]                  │
│  Hotkey: Ctrl+Shift+Space               │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  Quick Stats                            │
│  Model: Base                            │
│  Typing Speed: Normal                   │
│  Keywords: 12 configured                │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  Quick Actions                          │
│  [Configure Hotkey]  [Manage Keywords]  │
└─────────────────────────────────────────┘
```

**Components**:
1. **StatusCard**:
   - Large enabled/disabled toggle
   - Current hotkey display
   - Visual status indicator (green/gray)

2. **StatsCard**:
   - Current model size
   - Typing speed
   - Keyword count
   - Recording duration

3. **QuickActionsCard**:
   - Button shortcuts to other pages

**Styling**:
- Large, readable text
- Color-coded status (green = enabled, gray = disabled)
- Icons from Heroicons
- Smooth animations on toggle

**Acceptance**:
- Dashboard shows real-time status
- Toggle switch works (calls API)
- Quick action buttons navigate correctly
- Visual feedback on interactions

---

**Task 12.9: Frontend - Hotkey Configuration UI**

Create the hotkey configuration component:

**Design**:
```
General Settings

┌─────────────────────────────────────────┐
│  Global Hotkey                          │
│                                         │
│  Current: Ctrl+Shift+Space              │
│                                         │
│  [Record New Hotkey]                    │
│                                         │
│  ℹ️ Press the button above, then press  │
│     your desired key combination        │
└─────────────────────────────────────────┘
```

**Recording state**:
```
┌─────────────────────────────────────────┐
│  Global Hotkey                          │
│                                         │
│  🔴 Recording...                        │
│     Press any key combination           │
│                                         │
│  [Cancel]                               │
│                                         │
│  ⚠️ Must include at least one modifier  │
│     (Ctrl, Shift, Alt, or Win)          │
└─────────────────────────────────────────┘
```

**Implementation**:
1. **HotkeyDisplay component**:
   - Shows current hotkey in readable format
   - Large, prominent display
   - Color-coded (blue for set, amber for recording)

2. **HotkeyRecorder component**:
   - Listens to keydown events when active
   - Detects modifiers (Ctrl, Shift, Alt, Win)
   - Detects key code
   - Validates combination
   - Prevents recording modifier-only keys
   - Shows real-time preview while recording

3. **State management**:
   - `isRecording` flag
   - Temporary recording buffer
   - Validation before saving

4. **Visual states**:
   - Default: Blue button "Record New Hotkey"
   - Recording: Red button "Cancel", animated pulse
   - Success: Green checkmark, then back to default

**Key mapping** (display names):
- Use same logic as Settings.fs `getKeyDisplayName`
- A-Z, 0-9, F1-F12, Space, Enter, etc.

**Acceptance**:
- Can record new hotkey by pressing keys
- Shows real-time preview
- Validates modifiers required
- Saves to API on success
- Visual feedback for all states

---

**Task 12.10: Frontend - Recording & Typing Settings**

Create settings for model, recording duration, and typing speed:

**Design**:
```
┌─────────────────────────────────────────┐
│  Whisper Model                          │
│                                         │
│  Select model size:                     │
│  [ Tiny ▼]                             │
│                                         │
│  ⚠️ Requires restart to take effect     │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  Typing Speed                           │
│                                         │
│  ○ Fast (5ms delay)                     │
│  ● Normal (10ms) - Recommended          │
│  ○ Slow (20ms)                          │
│                                         │
│  Preview: [Test Typing Speed]           │
└─────────────────────────────────────────┘
```

**Components**:

1. **ModelSelector**:
   - Dropdown with options: Tiny, Base, Small, Medium, Large
   - Description of each model (size, speed, accuracy tradeoff)
   - Warning about restart requirement
   - Styled select with custom dropdown

2. **TypingSpeedSelector**:
   - Radio buttons for Fast/Normal/Slow
   - Visual description of each option
   - Optional: Custom speed input
   - Test button to preview typing speed
   - Inline preview showing speed visually

3. **RecordingDurationSettings** (optional for now):
   - Slider for max duration
   - 0 = unlimited (press and hold)
   - 1-30 seconds

**Styling**:
- Custom radio buttons (larger, more visible)
- Dropdown with Tailwind custom styles
- Test button shows animated preview
- Clear visual hierarchy

**Acceptance**:
- Model selection saves to API
- Typing speed selection saves to API
- Visual feedback on selection
- Test button works (shows typing animation)

---

**Task 12.11: Frontend - Keyword Manager UI**

Create the keyword management interface:

**Design** (table view with actions):
```
Keyword Replacements

ℹ️ Configure keywords that will be replaced in transcriptions
   Example: Say "comma" and it will be replaced with ","

[Add Keyword]  [Import]  [Export]  [Add Examples]

┌───────────────────────────────────────────────────────┐
│ Keyword     │ Replacement        │ Case │ Whole Word │
├───────────────────────────────────────────────────────┤
│ comma       │ ,                  │ ☐    │ ☑          │ [Edit] [Delete]
│ period      │ .                  │ ☐    │ ☑          │ [Edit] [Delete]
│ new line    │ \n                 │ ☐    │ ☑          │ [Edit] [Delete]
│ German f... │ Best regards,\n... │ ☐    │ ☑          │ [Edit] [Delete]
└───────────────────────────────────────────────────────┘
```

**Modal for Add/Edit**:
```
┌─────────────────────────────────────────┐
│  Add Keyword                      [×]   │
├─────────────────────────────────────────┤
│                                         │
│  Keyword (what you say):                │
│  [_____________________________]        │
│                                         │
│  Replacement (what to type):            │
│  [_____________________________]        │
│  [_____________________________]        │
│  [_____________________________]        │
│                                         │
│  ☑ Case Sensitive                       │
│  ☑ Match Whole Phrase Only              │
│                                         │
│         [Cancel]  [Save]                │
└─────────────────────────────────────────┘
```

**Components**:

1. **KeywordTable**:
   - Sortable table (by keyword, replacement)
   - Search/filter box at top
   - Row actions (edit, delete)
   - Truncate long replacements with tooltip
   - Visual indicators for case/whole word settings
   - Empty state with helpful message

2. **KeywordModal**:
   - Add or edit mode
   - Form validation (no empty keywords)
   - Multi-line textarea for replacement
   - Checkboxes for options
   - Preview of replacement
   - Escape key to cancel
   - Enter key to save (if valid)

3. **Action buttons**:
   - Add: Opens modal in add mode
   - Edit: Opens modal with pre-filled data
   - Delete: Shows confirmation dialog
   - Import: File upload (JSON)
   - Export: Downloads JSON file
   - Add Examples: Adds predefined examples

**Features**:
- Drag-and-drop for import (optional)
- Keyboard shortcuts (Ctrl+F for search)
- Bulk delete (select multiple)
- Undo last delete (optional)

**Acceptance**:
- Can add, edit, delete keywords
- Search/filter works in real-time
- Import/export maintains data integrity
- Modal UX is smooth and intuitive
- All operations call API correctly

---

**Task 12.12: Frontend - Toast Notifications & Loading States**

Add visual feedback throughout the app:

**Toast notifications** (top-right corner):
```
┌─────────────────────────────────┐
│ ✓ Settings saved successfully   │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ ⚠️ Failed to save settings       │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ ℹ️ Hotkey updated               │
└─────────────────────────────────┘
```

**Loading states**:
- Skeleton screens while loading settings
- Spinner on button during save
- Progress bar for import/export
- Dimmed overlay during operations

**Implementation**:
1. **Toast component**:
   - Auto-dismiss after 3 seconds
   - Slide-in animation from right
   - Color-coded by type (success=green, error=red, info=blue)
   - Stack multiple toasts
   - Click to dismiss early

2. **Loading component**:
   - Spinner with Tailwind animation
   - Skeleton screens for content
   - Button loading states

3. **Error boundaries**:
   - Catch React errors
   - Show friendly error message
   - Offer reload option

**Acceptance**:
- Toasts appear and auto-dismiss
- Loading states show during async operations
- Errors display user-friendly messages
- No UI freezing during operations

---

**Task 12.13: Frontend - Theme & Polish**

Add final polish and theme customization:

**Dark theme** (default, matching current app):
- Background: `#1a1a1a`
- Cards: `#2a2a2a`
- Text: `#FFFFFF` / `#B0B0B0`
- Accent: `#4A9EFF`
- Borders: `#3a3a3a`

**Animations & transitions**:
- Page transitions: Fade in with slight scale
- Card hover: Subtle lift with shadow
- Button hover: Brightness increase
- Modal: Backdrop blur with slide-up
- Toast: Slide-in from right

**Polish checklist**:
- [ ] All text is readable and sized appropriately
- [ ] Focus states for keyboard navigation
- [ ] Consistent spacing (Tailwind scale: 4, 8, 12, 16, 24, 32px)
- [ ] Responsive breakpoints (even if desktop-only)
- [ ] Icons for all actions (Heroicons)
- [ ] Smooth scrolling
- [ ] No janky animations
- [ ] Accessible color contrast (WCAG AA)
- [ ] Loading skeletons match final layout
- [ ] Empty states with helpful messages
- [ ] Tooltips for complex UI elements

**Optional light theme**:
- Background: `#FFFFFF`
- Cards: `#F5F5F5`
- Text: `#1a1a1a` / `#666666`
- Toggle in settings (saved to localStorage)

**Acceptance**:
- App looks polished and professional
- Animations are smooth (60fps)
- Theme is consistent throughout
- Keyboard navigation works
- Visually appealing and modern

---

**Task 12.14: Backend - Static File Embedding**

Embed the compiled frontend into the executable:

**Build pipeline**:
```bash
# 1. Build Fable frontend
cd VocalFold.WebUI
npm run build
# Output: VocalFold.WebUI/dist/

# 2. Copy to wwwroot
cp -r dist/* ../VocalFold/wwwroot/

# 3. Build F# project
cd ../VocalFold
dotnet build
```

**Embed resources** in .fsproj:
```xml
<ItemGroup>
  <EmbeddedResource Include="wwwroot\**\*" />
</ItemGroup>
```

**Serve embedded resources**:
```fsharp
// In WebServer.fs
let app =
    choose [
        route "/" >=> htmlFile "wwwroot/index.html"
        route "/api" >=> apiRoutes
        Files.browseHome
    ]

// Configure static files from embedded resources
```

**Implementation**:
1. Add build script (build.sh / build.bat)
2. Configure embedded resources
3. Serve files from embedded resources at runtime
4. Handle SPA routing (always serve index.html for client routes)
5. Add cache headers for static assets

**Acceptance**:
- Frontend builds and embeds in exe
- Standalone exe includes all web assets
- No external file dependencies
- Build script automates entire process

---

**Task 12.15: Integration - Browser Launch & Lifecycle**

Integrate web server with tray icon settings action:

**Changes to TrayIcon.fs**:
```fsharp
// In config.OnSettings callback:
let OnSettings = fun () ->
    match webServerState with
    | Some state ->
        // Server already running, just open browser
        let url = WebServer.getUrl state
        System.Diagnostics.Process.Start(url) |> ignore
    | None ->
        // Start server first
        let state = WebServer.start config |> Async.RunSynchronously
        webServerState <- Some state
        let url = WebServer.getUrl state
        System.Diagnostics.Process.Start(url) |> ignore
```

**Lifecycle management**:
1. Start server on first settings access (lazy)
2. Keep server running after browser closes
3. Stop server on app exit
4. Handle multiple browser windows (all access same server)
5. Show loading notification while server starts

**Browser selection**:
- Use default browser (System.Diagnostics.Process.Start)
- Alternative: Detect and prefer Chrome/Edge for better experience

**Optional: Electron integration**:
- If user prefers, bundle Electron
- More native feel
- Controlled environment
- Larger bundle size (~150MB extra)

**Acceptance**:
- Clicking "Settings" in tray opens browser to settings page
- Server starts automatically on first access
- Multiple clicks don't spawn multiple servers
- Server stops cleanly on app exit
- Tray notification shows "Opening settings..."

---

**Task 12.16: Integration - Settings Synchronization**

Ensure settings changes in web UI immediately affect running app:

**Synchronization flow**:
```
Web UI → API PUT /api/settings → Settings.save() → Notify app → Apply changes
```

**Changes needed**:

1. **HotkeyManager** - re-register hotkey:
   ```fsharp
   let updateHotkey (oldSettings: AppSettings) (newSettings: AppSettings) =
       if oldSettings.HotkeyKey <> newSettings.HotkeyKey ||
          oldSettings.HotkeyModifiers <> newSettings.HotkeyModifiers then
           unregisterHotkey oldSettings
           registerHotkey newSettings
   ```

2. **TextInput** - update typing speed:
   ```fsharp
   let updateTypingSpeed (settings: AppSettings) =
       typingDelayMs := Settings.getTypingDelay (Settings.getTypingSpeed settings)
   ```

3. **TextProcessor** - reload keywords:
   ```fsharp
   let reloadKeywords (settings: AppSettings) =
       keywords := settings.KeywordReplacements
   ```

4. **Enable/disable toggle**:
   ```fsharp
   let updateEnabled (settings: AppSettings) =
       if settings.IsEnabled then
           enableVoiceInput()
       else
           disableVoiceInput()
   ```

**Implementation**:
1. Add callback in WebServer when settings change
2. Implement `applySettingsChanges` function in Program.fs
3. Call appropriate update functions for each setting
4. Show tray notification on settings change
5. Handle errors gracefully (e.g., hotkey in use)

**Acceptance**:
- Settings changes immediately apply to running app
- Hotkey re-registers without restart
- Keywords reload instantly
- Typing speed updates immediately
- No restart required for any setting (except model)
- Tray notification confirms changes

---

**Task 12.17: Testing & Bug Fixes**

Comprehensive testing of the web-based settings UI:

**Frontend testing**:
1. All pages render correctly
2. Navigation works smoothly
3. All forms validate properly
4. API calls succeed and handle errors
5. Loading states display correctly
6. Toasts appear and dismiss
7. Keyboard shortcuts work
8. Modal dialogs function properly
9. Import/export works with valid/invalid files
10. Hotkey recording works across all keys

**Backend testing**:
1. API endpoints return correct data
2. Settings persist to disk
3. Concurrent requests handled safely
4. Invalid data rejected with proper errors
5. Server starts/stops cleanly
6. Port conflicts handled gracefully

**Integration testing**:
1. Settings changes apply to running app
2. Hotkey re-registration works
3. Keywords update immediately
4. Multiple browser sessions work correctly
5. Server survives browser close/reopen

**Edge cases**:
1. Very long keyword replacements (>1000 chars)
2. Unicode in keywords/replacements
3. Empty keyword list
4. Corrupted settings.json
5. Network timeout (slow API)
6. Browser doesn't open (fallback message)

**Performance testing**:
1. Large keyword lists (100+ keywords)
2. Rapid setting changes
3. Memory usage stable
4. No memory leaks in SPA

**Acceptance**:
- All test scenarios pass
- No crashes or errors
- Smooth user experience
- Performance is good

---

**Task 12.18: Documentation & Migration**

Document the new web-based settings and create migration guide:

**Update SPECIFICATION.md**:
```markdown
### Settings Management (Phase 12)
- Web-based settings UI (F# Fable + TailwindCSS)
- Accessible via tray menu → Settings
- Opens in default browser (localhost)
- All settings changes apply immediately (no restart needed)
```

**Update ARCHITECTURE.md**:
```markdown
### Web Settings Module
- Giraffe web server on localhost
- REST API for settings CRUD
- Fable SPA with Elmish architecture
- Embedded static files in executable
```

**Create USER_GUIDE.md** (new file):
- How to access settings
- Explanation of each setting
- Screenshots of web UI
- Troubleshooting (browser doesn't open, etc.)

**Migration notes** (for developers):
- Old SettingsDialog.fs can be removed after Phase 12
- ModernTheme.fs no longer needed (replaced by TailwindCSS)
- Settings.fs remains unchanged (backward compatible)

**Acceptance**:
- Documentation is complete and accurate
- Screenshots included
- Migration path clear
- Old code marked for removal

---

**Task 12.19: Build & Deployment Updates**

Update build scripts for web-based settings:

**Build script** (build.sh / build.bat):
```bash
#!/bin/bash

echo "Building VocalFold with Web Settings..."

# 1. Build frontend
echo "Building frontend..."
cd VocalFold.WebUI
npm install
npm run build

# 2. Copy to wwwroot
echo "Copying assets..."
rm -rf ../VocalFold/wwwroot/*
cp -r dist/* ../VocalFold/wwwroot/

# 3. Build F# project
echo "Building backend..."
cd ../VocalFold
dotnet build -c Release

# 4. Publish standalone exe
echo "Publishing..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

echo "Build complete: ./bin/Release/net9.0/win-x64/publish/VocalFold.exe"
```

**Development workflow**:
```bash
# Terminal 1: Frontend dev server (hot reload)
cd VocalFold.WebUI
npm run dev

# Terminal 2: Backend
cd VocalFold
dotnet run
```

**CI/CD considerations** (if using GitHub Actions):
- Install Node.js
- Run npm install & build
- Build .NET project
- Package for distribution

**Acceptance**:
- Build script works end-to-end
- Development workflow is smooth
- Standalone exe includes all web assets
- Size is reasonable (<150MB)

---

**Task 12.20: Performance Optimization & Cleanup**

Final optimizations and code cleanup:

**Frontend optimizations**:
1. Code splitting (if bundle is large)
2. Lazy loading for pages
3. Memoize expensive components
4. Optimize re-renders
5. Compress images/assets
6. Minify JavaScript/CSS

**Backend optimizations**:
1. Cache settings in memory (reload on change)
2. Compress API responses (gzip)
3. HTTP/2 support
4. Static file caching headers

**Code cleanup**:
1. Remove old SettingsDialog.fs
2. Remove ModernTheme.fs (if not used elsewhere)
3. Remove unused dependencies
4. Clean up commented code
5. Add XML documentation
6. Format code consistently

**Security review**:
1. Bind server to 127.0.0.1 only (not 0.0.0.0)
2. Validate all API inputs
3. Sanitize file upload (import)
4. Rate limiting (prevent abuse)
5. CORS restricted to localhost

**Acceptance**:
- Bundle size optimized
- Fast page loads (<1s)
- No unused code
- Security best practices followed
- Clean, maintainable codebase

---

## Phase 12 Summary

**What we've built**:
- Modern web-based settings UI using F# Fable + TailwindCSS
- Embedded web server using Giraffe
- REST API for all settings operations
- Beautiful, clean UI following modern design principles
- All existing functionality preserved and enhanced
- Immediate settings application (no restart)
- Better UX with animations, loading states, and visual feedback

**Technology stack**:
- Backend: F#, Giraffe, ASP.NET Core, Kestrel
- Frontend: F#, Fable 4, Feliz, React, TailwindCSS, Vite
- Communication: REST API with JSON

**Benefits over WinForms version**:
✅ Modern, beautiful design
✅ Better UX (animations, feedback)
✅ Easier to maintain and extend
✅ Web technologies (more developers familiar)
✅ Type-safe full-stack F# (shared types)
✅ Better testing (can test frontend separately)
✅ Potential for remote access (future)

**Tradeoffs**:
- Slightly larger bundle size (+20-30MB)
- Dependency on Node.js for development
- More complex build process
- Browser dependency (but very common)

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
✅ **Web-based settings UI is modern, beautiful, and fully functional**

---

**Status**: Ready for implementation
**Estimated Time**:
- Phases 1-6: 4-6 hours
- Phases 7-9: 6-8 hours
- Phase 10: 2-3 hours
- Phase 11: 4-6 hours
- **Phase 12: 20-30 hours** (comprehensive web UI overhaul)
**Next Task**: 12.1 Backend Web Server - Project Setup
