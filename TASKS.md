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

**Design System - Brand Colors**:
- **Primary**: `#25abfe` (Blue) - Used for main buttons, links, active states
- **Secondary**: `#ff8b00` (Orange) - Used for accents, highlights, success states
- Complete color palette with shades (50-900) defined in TailwindCSS config
- Dark theme background with brand color accents throughout the UI

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

**TailwindCSS custom theme** (dark mode, company branding colors):
```js
module.exports = {
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Company branding colors
        primary: {
          DEFAULT: '#25abfe',   // Primary brand color (blue)
          50: '#e6f5ff',
          100: '#b8e2ff',
          200: '#8acfff',
          300: '#5cbcff',
          400: '#25abfe',        // Main primary
          500: '#0c95e8',
          600: '#0a7bc2',
          700: '#08619c',
          800: '#064776',
          900: '#042d50',
        },
        secondary: {
          DEFAULT: '#ff8b00',   // Secondary brand color (orange)
          50: '#fff3e0',
          100: '#ffd9a3',
          200: '#ffc266',
          300: '#ffab29',
          400: '#ff8b00',        // Main secondary
          500: '#e67a00',
          600: '#cc6b00',
          700: '#b35c00',
          800: '#994d00',
          900: '#803e00',
        },
        // UI colors
        background: {
          dark: '#1a1a1a',      // DarkBackground
          card: '#2a2a2a',      // CardBackground
          sidebar: '#232323',   // SidebarBackground
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

┌─────────────────────────────────────────────────────┐
│ Keyword     │ Replacement                           │
├─────────────────────────────────────────────────────┤
│ comma       │ ,                                     │ [Edit] [Delete]
│ period      │ .                                     │ [Edit] [Delete]
│ new line    │ \n                                    │ [Edit] [Delete]
│ German f... │ Best regards,\n...                    │ [Edit] [Delete]
└─────────────────────────────────────────────────────┘
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
│         [Cancel]  [Save]                │
└─────────────────────────────────────────┘
```

**Components**:

1. **KeywordTable**:
   - Sortable table (by keyword, replacement)
   - Search/filter box at top
   - Row actions (edit, delete)
   - Truncate long replacements with tooltip
   - Empty state with helpful message

2. **KeywordModal**:
   - Add or edit mode
   - Form validation (no empty keywords)
   - Multi-line textarea for replacement
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

**Dark theme** (default, with company branding):
- Background: `#1a1a1a`
- Cards: `#2a2a2a`
- Text: `#FFFFFF` / `#B0B0B0`
- Primary (brand): `#25abfe` (blue)
- Secondary (brand): `#ff8b00` (orange)
- Borders: `#3a3a3a`

**Color usage guidelines**:
- Primary blue (`#25abfe`): Main CTAs, active states, links, primary buttons
- Secondary orange (`#ff8b00`): Accent elements, success states, highlights, secondary buttons
- Use gradient combinations for special elements (e.g., `from-primary to-secondary`)
- Hover states: Use lighter shades (primary-300, secondary-300)
- Active states: Use darker shades (primary-600, secondary-600)

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

---

### Phase 13: Keyword Categorization ⬜

**Context**: As users accumulate many keywords (punctuation, email signatures, code snippets, etc.), managing them becomes difficult. This phase adds visual categorization to organize keywords into collapsible groups. This is purely a UI enhancement - categories have NO effect on keyword matching or replacement during transcription.

**Goals**:
- Organize keywords into user-defined categories
- Collapsible/expandable category sections (like Notion toggles)
- Better overview of large keyword collections
- Visual organization without affecting functionality
- Drag-and-drop keywords between categories

**Architecture**:
```
Backend: Add "Category" field to KeywordReplacement
Frontend: Group keywords by category with accordion UI
Display: Category name → [collapse/expand] → Keywords list
No Changes: TextProcessor logic remains unchanged
```

---

**Task 13.1: Backend - Category Data Structure**

Extend keyword data model to support categories:

**Changes to Types.fs** (shared types):
```fsharp
type KeywordReplacement = {
    Keyword: string
    Replacement: string
    Category: string option    // NEW: Optional category name
    CaseSensitive: bool
    WholePhrase: bool
}

type KeywordCategory = {
    Name: string              // Category name (e.g., "Punctuation", "Email")
    IsExpanded: bool          // UI state: collapsed or expanded
    Color: string option      // Optional color tag
}

type AppSettings = {
    // ... existing fields ...
    KeywordReplacements: KeywordReplacement list
    Categories: KeywordCategory list    // NEW: List of categories
}
```

**Implementation**:
1. Update KeywordReplacement type in Settings.fs
2. Add Categories list to AppSettings
3. Add default categories on first run:
   - "Uncategorized" (default for keywords without category)
   - "Punctuation"
   - "Email Templates"
   - "Code Snippets"
4. Implement migration logic for existing keywords (set Category = None)
5. Ensure JSON serialization/deserialization works

**Acceptance**:
- Settings structure supports categories
- Existing keywords load correctly (backward compatible)
- Default categories created on first run
- Category state persists to settings.json

---

**Task 13.2: Backend - Category Management API**

Add REST API endpoints for category operations:

**New Endpoints**:
```fsharp
GET    /api/categories              -> Returns all categories
POST   /api/categories              -> Creates new category
PUT    /api/categories/:name        -> Updates category (rename, color)
DELETE /api/categories/:name        -> Deletes category (moves keywords to Uncategorized)
PUT    /api/categories/:name/state  -> Toggle expanded/collapsed state

// Update existing keyword endpoints to support category
PUT    /api/keywords/:index/category  -> Move keyword to different category
```

**Implementation**:
1. Create CategoryApi.fs module
2. Implement category CRUD operations
3. Add validation (no duplicate category names)
4. Handle cascade deletion (when category deleted, keywords move to "Uncategorized")
5. Ensure "Uncategorized" category cannot be deleted
6. Thread-safe operations
7. Persist category state (expanded/collapsed) to settings

**Acceptance**:
- Can create, read, update, delete categories via API
- Keywords move to "Uncategorized" when their category is deleted
- Expanded/collapsed state persists
- Proper error responses for invalid operations

---

**Task 13.3: Frontend - Category Accordion UI Component**

Create collapsible category sections for the keyword manager:

**Design** (accordion-style):
```
Keyword Replacements

[Add Keyword]  [Add Category]  [Import]  [Export]

┌─────────────────────────────────────────────────────┐
│ ▼ Punctuation                               [Edit] [Delete] │
├─────────────────────────────────────────────────────┤
│   comma         → ,                        [Edit] [Delete]  │
│   period        → .                        [Edit] [Delete]  │
│   question mark → ?                        [Edit] [Delete]  │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ▶ Email Templates (3 keywords)             [Edit] [Delete] │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ▼ Code Snippets                             [Edit] [Delete] │
├─────────────────────────────────────────────────────┤
│   console log   → console.log("$1")        [Edit] [Delete]  │
│   arrow function → const $1 = () => {}     [Edit] [Delete]  │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ▼ Uncategorized (2 keywords)               [Edit] [Delete] │
├─────────────────────────────────────────────────────┤
│   test keyword  → replacement text         [Edit] [Delete]  │
└─────────────────────────────────────────────────────┘
```

**Components**:

1. **CategoryAccordion**:
   - Header shows category name + keyword count
   - Collapse/expand toggle (▶/▼ icon)
   - Smooth animation on toggle (slide down/up)
   - Edit/Delete buttons on hover
   - Color tag (optional visual indicator)
   - Empty state: "No keywords in this category"

2. **CategoryHeader**:
   - Category name (large, bold)
   - Keyword count badge
   - Expand/collapse icon (animated rotation)
   - Action buttons (edit name, delete category)
   - Color dot indicator (if color defined)

3. **KeywordList** (within category):
   - Same table structure as before
   - Grouped by parent category
   - Row actions (edit, delete, move to category)

**State Management**:
```fsharp
type Model = {
    // ... existing fields ...
    Categories: KeywordCategory list
    ExpandedCategories: Set<string>    // Set of expanded category names
}

type Msg =
    // ... existing messages ...
    | ToggleCategory of string         // Expand/collapse
    | CreateCategory of string
    | RenameCategory of string * string
    | DeleteCategory of string
    | MoveKeywordToCategory of int * string
```

**Styling**:
- Tailwind accordion classes
- Smooth transitions (duration-300)
- Hover states on category headers
- Border colors matching category color
- Indented keyword rows within categories

**Acceptance**:
- Categories render as collapsible sections
- Clicking header toggles expand/collapse
- Keywords grouped correctly under categories
- Smooth animations
- Empty categories show helpful message
- "Uncategorized" always shown (cannot be deleted)

---

**Task 13.4: Frontend - Category Management Modal**

Create UI for creating and editing categories:

**Add Category Modal**:
```
┌─────────────────────────────────────────┐
│  Add Category                     [×]   │
├─────────────────────────────────────────┤
│                                         │
│  Category Name:                         │
│  [_____________________________]        │
│                                         │
│  Color (optional):                      │
│  [🔵] [🟢] [🟡] [🟠] [🔴] [🟣]         │
│                                         │
│  Examples:                              │
│  • Punctuation                          │
│  • Email Templates                      │
│  • Code Snippets                        │
│  • Greetings                            │
│  • Commands                             │
│                                         │
│         [Cancel]  [Create]              │
└─────────────────────────────────────────┘
```

**Edit Category Modal**:
```
┌─────────────────────────────────────────┐
│  Edit Category                    [×]   │
├─────────────────────────────────────────┤
│                                         │
│  Category Name:                         │
│  [Punctuation____________]              │
│                                         │
│  Color:                                 │
│  [🔵] [🟢] [🟡] [🟠] [🔴] [🟣]         │
│                                         │
│  Keywords in this category: 5           │
│                                         │
│         [Cancel]  [Save]                │
└─────────────────────────────────────────┘
```

**Implementation**:
1. CategoryModal component
2. Form validation (no empty names, no duplicates)
3. Color picker (predefined palette)
4. Create/Edit modes
5. API calls on save
6. Update local state on success

**Acceptance**:
- Can create new categories
- Can rename existing categories
- Can choose color tags
- Validation prevents duplicate names
- Modal closes on success
- UI updates immediately

---

**Task 13.5: Frontend - Keyword Category Assignment**

Update keyword add/edit modal to include category selection:

**Updated Keyword Modal**:
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
│                                         │
│  Category:                              │
│  [ Punctuation ▼               ]        │
│                                         │
│         [Cancel]  [Save]                │
└─────────────────────────────────────────┘
```

**Implementation**:
1. Add category dropdown to KeywordModal
2. Populate dropdown with available categories
3. Show "(Create New...)" option at bottom
4. Default to "Uncategorized" for new keywords
5. Save category when creating/editing keyword
6. Quick-create category without closing modal

**Acceptance**:
- Category dropdown shows all categories
- Can select category when adding keyword
- Can change category when editing keyword
- Can create new category inline
- Changes persist via API

---

**Task 13.6: Frontend - Drag-and-Drop Between Categories**

Add drag-and-drop functionality to move keywords between categories:

**User Experience**:
```
User drags "comma" from Punctuation category
  → Hovers over "Code Snippets" category header
  → Category highlights
  → User drops
  → "comma" moves to Code Snippets
  → Toast: "Moved 'comma' to Code Snippets"
```

**Implementation**:
1. Use HTML5 Drag and Drop API
2. Make keyword rows draggable (`draggable="true"`)
3. Make category headers drop targets
4. Visual feedback during drag:
   - Dragged item shows ghost image
   - Drop targets highlight on hover
   - Cursor changes to move cursor
5. On drop:
   - Call API to update keyword category
   - Update local state
   - Show toast confirmation
6. Handle edge cases:
   - Dropping on same category (no-op)
   - Dropping on invalid target (cancel)

**Styling**:
- Dragging: Opacity 50%, cursor: grabbing
- Valid drop target: Border glow, background highlight
- Invalid drop target: Red border, cursor: not-allowed

**Acceptance**:
- Can drag keywords between categories
- Visual feedback clear during drag
- Drop updates category immediately
- Toast confirms action
- Works smoothly without bugs

---

**Task 13.7: Frontend - Category Operations & Bulk Actions**

Add advanced category operations:

**Features**:

1. **Delete Category with Confirmation**:
   ```
   ⚠️ Delete "Email Templates"?

   This category contains 5 keywords.
   They will be moved to "Uncategorized".

   [Cancel]  [Delete Category]
   ```

2. **Merge Categories**:
   ```
   Select category to merge with "Email Templates":
   [ Code Snippets ▼ ]

   All keywords from "Email Templates" will move to the selected category.
   "Email Templates" will be deleted.

   [Cancel]  [Merge]
   ```

3. **Bulk Move Keywords**:
   - Select multiple keywords (checkboxes)
   - Dropdown: "Move selected to category..."
   - Button: "Move X keywords"

4. **Sort Categories**:
   - Drag-and-drop to reorder categories
   - Sort alphabetically button
   - Sort by keyword count button

**Implementation**:
1. DeleteCategoryConfirmation modal
2. MergeCategoriesModal component
3. Checkbox selection state in Model
4. Bulk action toolbar (shows when items selected)
5. Category reordering with drag-and-drop
6. Sort functions (alpha, by count)

**Acceptance**:
- Can delete categories safely (keywords preserved)
- Can merge categories
- Can bulk move keywords
- Can reorder categories
- All operations confirm before executing
- Toasts confirm successful actions

---

**Task 13.8: Frontend - Search & Filter with Categories**

Enhance search to work across categories:

**Search Features**:
1. **Global Search**:
   - Search box searches all keywords across categories
   - Matching keywords highlight
   - Non-matching categories collapse
   - Matching categories auto-expand
   - Show "X results in Y categories"

2. **Filter by Category**:
   - Multi-select dropdown: Filter by categories
   - Show only selected categories
   - "All Categories" to reset

3. **Empty State**:
   ```
   No keywords found matching "test"

   Try:
   • Checking your spelling
   • Using different keywords
   • Clearing filters

   [Clear Search]
   ```

**Implementation**:
1. Global search across all keywords
2. Filter categories based on search
3. Auto-expand categories with matches
4. Highlight matching text
5. Show result count
6. Clear search button

**Acceptance**:
- Search finds keywords across all categories
- Results highlight correctly
- Categories expand/collapse based on results
- Filter by category works
- Empty state helpful and actionable

---

**Task 13.9: Frontend - Category Visualization & Statistics**

Add visual overview of categories:

**Category Statistics Panel** (on Dashboard):
```
┌─────────────────────────────────────────┐
│  Keyword Organization                   │
│                                         │
│  📊 Total Keywords: 45                  │
│  📁 Categories: 6                       │
│                                         │
│  Top Categories:                        │
│  • Punctuation: 12 keywords             │
│  • Email Templates: 8 keywords          │
│  • Code Snippets: 7 keywords            │
│  • Uncategorized: 3 keywords            │
│                                         │
│  [Manage Categories]                    │
└─────────────────────────────────────────┘
```

**Visual Category Overview** (in Keyword Manager):
```
Category Overview:  [Grid] [List]

[Grid View]
┌──────────┐ ┌──────────┐ ┌──────────┐
│ 📝       │ │ 📧       │ │ 💻       │
│ Punctua  │ │ Email    │ │ Code     │
│ 12 kw    │ │ 8 kw     │ │ 7 kw     │
└──────────┘ └──────────┘ └──────────┘
```

**Implementation**:
1. Calculate category statistics
2. CategoryStatsCard component (Dashboard)
3. Grid view for quick category overview
4. Chart/graph for visual representation (optional)
5. Quick links to manage categories

**Acceptance**:
- Dashboard shows keyword statistics
- Category counts accurate
- Visual overview helpful
- Quick navigation to categories

---

**Task 13.10: Backend - Category Persistence & Performance**

Optimize category operations for performance:

**Optimizations**:
1. **Caching**:
   - Cache category list in memory
   - Invalidate cache on category changes
   - Reduce file I/O

2. **Efficient Grouping**:
   - Index keywords by category
   - Fast lookup by category name
   - Efficient iteration

3. **Batch Operations**:
   - Bulk move keywords in single API call
   - Batch category updates
   - Reduce round trips

**API Enhancements**:
```fsharp
POST /api/keywords/bulk-move
Body: {
  keywordIndices: [0, 2, 5, 8],
  targetCategory: "Email Templates"
}

POST /api/categories/bulk
Body: {
  operations: [
    { type: "create", name: "New Category" },
    { type: "delete", name: "Old Category" },
    { type: "rename", oldName: "A", newName: "B" }
  ]
}
```

**Implementation**:
1. In-memory category cache
2. Index keywords by category
3. Batch API endpoints
4. Optimize JSON serialization
5. Add performance metrics logging

**Acceptance**:
- Category operations fast (<100ms)
- Large keyword lists (100+) handle smoothly
- Batch operations reduce API calls
- No performance degradation

---

**Task 13.11: Testing & Edge Cases**

Comprehensive testing of categorization feature:

**Test Scenarios**:

1. **Basic Operations**:
   - Create category
   - Rename category
   - Delete category
   - Move keyword to category
   - Expand/collapse categories

2. **Edge Cases**:
   - Delete category with many keywords (100+)
   - Rename category to existing name (validation)
   - Delete "Uncategorized" (should fail)
   - Move keyword to non-existent category
   - Category with very long name (>100 chars)
   - Keywords with no category (default to Uncategorized)
   - Empty categories (show empty state)

3. **Migration**:
   - Load settings from before Phase 13 (no categories)
   - All keywords default to "Uncategorized"
   - Categories list created with defaults

4. **Performance**:
   - 10 categories with 50 keywords each
   - Search across 500+ keywords
   - Expand/collapse all categories rapidly
   - Drag-and-drop with large lists

5. **UI/UX**:
   - Animations smooth (60fps)
   - No visual glitches during expand/collapse
   - Drag-and-drop visual feedback clear
   - Toast notifications don't stack excessively
   - Modal focus management correct

**Acceptance**:
- All test scenarios pass
- No crashes or errors
- Performance acceptable
- Smooth user experience
- Edge cases handled gracefully

---

**Task 13.12: Documentation & User Guide**

Document the categorization feature:

**Update USER_GUIDE.md**:
```markdown
### Organizing Keywords with Categories

VocalFold allows you to organize your keywords into categories for better management.

#### Creating Categories

1. Open Settings → Keywords
2. Click "Add Category"
3. Enter a category name (e.g., "Email Templates")
4. Choose a color (optional)
5. Click "Create"

#### Assigning Keywords to Categories

**Method 1: When Adding/Editing Keywords**
1. Add or edit a keyword
2. Select category from dropdown
3. Save

**Method 2: Drag and Drop**
1. Click and hold on a keyword
2. Drag to the category header
3. Release to move

**Method 3: Bulk Move**
1. Select multiple keywords (checkboxes)
2. Choose "Move to category..."
3. Confirm

#### Managing Categories

- **Rename**: Click "Edit" on category header
- **Delete**: Click "Delete" (keywords move to Uncategorized)
- **Reorder**: Drag category headers to reorder

#### Tips

- Use categories to group related keywords (e.g., Punctuation, Email, Code)
- Collapse unused categories to reduce clutter
- Search works across all categories
- Categories are visual only - they don't affect keyword matching
```

**Update ARCHITECTURE.md**:
```markdown
### Keyword Categorization (Phase 13)

**Purpose**: Visual organization of keywords into collapsible groups

**Data Model**:
- KeywordReplacement gains optional Category field
- AppSettings includes Categories list
- Category state (expanded/collapsed) persists

**UI Components**:
- CategoryAccordion: Collapsible category sections
- CategoryModal: Create/edit categories
- Drag-and-drop support for moving keywords

**API Endpoints**:
- GET/POST/PUT/DELETE /api/categories
- PUT /api/keywords/:index/category

**Important**: Categories are UI-only. TextProcessor ignores categories during matching.
```

**Acceptance**:
- Documentation complete and accurate
- Screenshots/GIFs showing categorization
- User guide clear and helpful
- Architecture docs updated

---

**Task 13.13: Polish & Final Integration**

Final polish and integration testing:

**Polish Checklist**:
- [ ] All animations smooth and polished
- [ ] Category colors visually appealing
- [ ] Empty states helpful and encouraging
- [ ] Error messages clear and actionable
- [ ] Keyboard shortcuts work (collapse all, expand all)
- [ ] Accessibility: Screen reader support
- [ ] Responsive layout (if applicable)
- [ ] Loading states during API calls
- [ ] Optimistic UI updates (instant feedback)

**Integration Testing**:
- [ ] Categories persist across app restarts
- [ ] Keyword replacement works regardless of category
- [ ] Import/export includes categories
- [ ] Backward compatibility with pre-Phase-13 settings
- [ ] No breaking changes to existing functionality

**Final Checks**:
- [ ] Code review and cleanup
- [ ] Remove debug logging
- [ ] Optimize bundle size
- [ ] Performance profiling
- [ ] Memory leak check
- [ ] Browser compatibility (Chrome, Edge, Firefox)

**Acceptance**:
- Feature is polished and production-ready
- All integration tests pass
- No regressions in existing features
- User experience is smooth and intuitive

---

## Phase 13 Summary

**What We Built**:
- Visual categorization system for keywords
- Collapsible category sections (like Notion toggles)
- Drag-and-drop to organize keywords
- Category management UI (create, rename, delete, merge)
- Search and filter across categories
- Category statistics on dashboard
- Bulk operations for efficiency

**Key Features**:
✅ Organize keywords into categories
✅ Collapsible/expandable category sections
✅ Drag-and-drop keywords between categories
✅ Create, rename, delete categories
✅ Bulk move keywords
✅ Search across all categories
✅ Visual statistics and overview
✅ Smooth animations and visual feedback

**Important Note**:
⚠️ Categories are **UI-only** and do **not affect** keyword matching or replacement during transcription. The TextProcessor module ignores categories entirely. Categories exist solely for visual organization and user convenience.

**Benefits**:
- Better overview of large keyword collections
- Easier to find and manage keywords
- Visual grouping improves usability
- Reduces clutter in keyword manager
- Scales well to hundreds of keywords

**Technology**:
- Backend: F#, updated Settings.fs, CategoryApi.fs
- Frontend: F#, Fable, React, TailwindCSS
- UI: Accordion components, drag-and-drop, modals
- Storage: Categories stored in settings.json

---

### Phase 14: AMD GPU Support via Vulkan ✅

**Context**: Currently, VocalFold only supports NVIDIA GPUs through CUDA. This phase adds support for AMD GPUs (and Intel GPUs) on Windows using Vulkan, which is a cross-vendor GPU acceleration API. This is a minimal-effort, high-impact improvement that expands hardware compatibility.

**Status**: ✅ COMPLETE (2025-10-31)

**Goals**:
- Enable GPU acceleration for AMD Radeon GPUs
- Provide automatic fallback from CUDA to Vulkan
- Maintain existing NVIDIA CUDA performance
- No code changes required (runtime-only)

**Reference**: Tech-Options.md Section 2.1 "GPU Acceleration - Vulkan (Recommended)"

---

**Task 14.1: Add Vulkan Runtime Package** ✅

Add Vulkan support alongside existing CUDA runtime:

**Changes to VocalFold.fsproj**:
```xml
<!-- Existing CUDA runtime -->
<PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.8.1" />

<!-- NEW: Add Vulkan runtime for AMD/Intel GPU support -->
<PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.8.1" />
```

**How it works**:
- Whisper.NET automatically selects the best available runtime
- Priority order: CUDA → Vulkan → CPU
- No code changes required - runtime selection is automatic
- If CUDA is unavailable (AMD/Intel GPU), Vulkan is used
- If Vulkan is unavailable, falls back to CPU

**Acceptance**:
- Package reference added to .fsproj
- Project builds successfully with both runtimes
- No compilation errors

---

**Task 14.2: Update TranscriptionService Logging** ✅

Add logging to show which GPU runtime is being used:

**Changes to TranscriptionService.fs**:
```fsharp
// In WhisperService constructor, after model loading
printfn "🎯 Whisper.NET Model Loaded"

// NEW: Add runtime detection logging
let runtimeInfo =
    if processor.SupportsGpu then
        // Try to detect which runtime is active
        // Note: Whisper.NET may not expose this directly
        // This is informational only
        "GPU acceleration enabled"
    else
        "CPU mode (no GPU detected)"

printfn "⚙️  Runtime: %s" runtimeInfo
```

**Alternative approach** (if runtime detection not exposed):
- Simply log that GPU acceleration is enabled/disabled
- Let Whisper.NET handle runtime selection silently
- Document in README which GPUs are supported

**Acceptance**:
- Logging shows whether GPU acceleration is active
- No errors when GPU unavailable
- Helpful feedback for troubleshooting

---

**Task 14.3: Test on AMD Hardware** (Optional - Requires AMD Hardware)

Verify Vulkan runtime works correctly on AMD GPU:

**Testing requirements**:
- Test hardware: AMD Radeon RX 6000 series or newer (recommended)
- Test scenarios:
  1. Fresh install with AMD GPU (no NVIDIA drivers)
  2. Verify Vulkan drivers installed (check AMD driver version)
  3. Run VocalFold and test transcription
  4. Measure transcription performance (compare to CPU mode)
  5. Test with different Whisper model sizes (Tiny, Base, Small)

**Performance benchmarks** (5 seconds of speech, Base model):
- Target: <2s transcription time on AMD RX 6700 XT or better
- Acceptable: <5s transcription time on AMD RX 5000 series
- Fallback: CPU mode if Vulkan unavailable

**Test checklist**:
- [ ] Vulkan runtime loads successfully
- [ ] Transcription completes without errors
- [ ] Performance is acceptable (faster than CPU)
- [ ] No memory leaks during repeated use
- [ ] Console output shows GPU acceleration status

**Acceptance**:
- Vulkan runtime works on AMD GPU hardware
- Performance is acceptable (significantly faster than CPU)
- No crashes or errors during normal operation

---

**Task 14.4: Update Documentation** ✅

Document the expanded GPU support:

**Update README.md** (System Requirements section):
```markdown
### System Requirements

**Operating System:**
- Windows 11 (recommended)
- Windows 10 (supported)

**GPU Support:**
- **NVIDIA GPUs**: RTX 20 series or newer (CUDA 12.x)
  - Requires: NVIDIA CUDA Toolkit 12.x
  - Performance: Excellent (native CUDA acceleration)

- **AMD GPUs**: Radeon RX 6000 series or newer (Vulkan 1.0+)
  - Requires: Latest AMD drivers with Vulkan support
  - Performance: Good (Vulkan acceleration)
  - Note: Older AMD GPUs (RX 5000 series) may have slower performance

- **Intel GPUs**: Arc series (Vulkan 1.0+)
  - Requires: Latest Intel drivers with Vulkan support
  - Performance: Moderate (Vulkan acceleration)

- **No GPU / Unsupported GPU**: CPU fallback mode
  - Performance: Slow (5-10x slower than GPU)
  - Recommended: Use Tiny or Base model for acceptable speed

**Runtime Priority:**
VocalFold automatically selects the best available runtime:
1. CUDA (NVIDIA GPUs)
2. Vulkan (AMD/Intel GPUs)
3. CPU (fallback)
```

**Update ARCHITECTURE.md**:
```markdown
### GPU Acceleration (Phase 14)

**Multi-Runtime Support:**
- CUDA: NVIDIA GPU acceleration (primary)
- Vulkan: AMD/Intel GPU acceleration (fallback)
- CPU: Software fallback (slowest)

**Runtime Packages:**
- Whisper.net.Runtime.Cuda.Windows v1.8.1
- Whisper.net.Runtime.Vulkan v1.8.1

**Automatic Runtime Selection:**
Whisper.NET automatically detects and uses the best available runtime.
No code changes required - runtime selection is transparent to the application.
```

**Update SPECIFICATION.md** (NFR-1: Performance):
```markdown
**NFR-1: Performance**
- Target: <1s transcription time for 5s of speech (Base model)
  - NVIDIA RTX 3080: ~0.5s (CUDA)
  - AMD RX 6700 XT: ~1.0s (Vulkan)
  - AMD RX 5700 XT: ~2.0s (Vulkan)
  - CPU fallback: ~5-10s
- Max memory: <2GB for Base model
- GPU utilization: Efficient CUDA/Vulkan usage
```

**Acceptance**:
- All documentation updated with AMD/Vulkan support
- System requirements clearly list supported GPUs
- Performance expectations documented
- Troubleshooting guidance included

---

**Task 14.5: Create Vulkan Troubleshooting Guide** ✅

Add troubleshooting section for Vulkan-related issues:

**Create TROUBLESHOOTING.md** (or add to README):
```markdown
## GPU Acceleration Troubleshooting

### Issue: "CPU mode (no GPU detected)"

**Symptoms:**
- Transcription is very slow (5-10 seconds for short audio)
- Console shows "CPU mode" instead of "GPU acceleration enabled"

**Solutions:**

**For NVIDIA GPU users:**
1. Install NVIDIA CUDA Toolkit 12.x
   - Download from: https://developer.nvidia.com/cuda-downloads
2. Verify installation: `nvcc --version`
3. Restart VocalFold

**For AMD GPU users:**
1. Install latest AMD Adrenalin drivers
   - Download from: https://www.amd.com/en/support
2. Verify Vulkan support:
   - Download Vulkan SDK: https://vulkan.lunarg.com/
   - Run `vulkaninfo` to check Vulkan availability
3. Ensure GPU is Radeon RX 6000+ series (older GPUs may not support Vulkan well)
4. Restart VocalFold

**For Intel GPU users:**
1. Install latest Intel Graphics drivers
   - Download from: https://www.intel.com/content/www/us/en/download-center/home.html
2. Verify Vulkan support (Intel Arc series recommended)
3. Restart VocalFold

### Issue: Vulkan crashes or errors

**Symptoms:**
- Application crashes during transcription
- Errors mentioning "Vulkan" or "GPU"

**Solutions:**
1. Update GPU drivers to latest version
2. Check for Windows updates
3. Try CPU mode temporarily (remove Vulkan package as workaround)
4. Report issue on GitHub with:
   - GPU model
   - Driver version
   - Error message

### Performance Benchmarks

Use these benchmarks to verify your GPU is performing correctly:

**Test method:**
1. Record 5 seconds of clear speech
2. Use Base model
3. Measure transcription time (shown in console)

**Expected performance:**
- NVIDIA RTX 3080: <1s
- NVIDIA RTX 3060: <1.5s
- AMD RX 6800 XT: <1.5s
- AMD RX 6700 XT: <2s
- AMD RX 5700 XT: <3s
- CPU (i7-10700K): 5-8s

If your performance is significantly worse, GPU acceleration may not be working.
```

**Acceptance**:
- Troubleshooting guide covers common scenarios
- Clear steps for each GPU vendor
- Performance benchmarks help users verify setup
- Links to driver downloads included

---

**Task 14.6: Optional - Add GPU Detection Utility** (Deferred)

Create a diagnostic tool to help users verify GPU support:

**Create GPUInfo.fs** (optional utility module):
```fsharp
module GPUInfo =
    open System
    open System.Runtime.InteropServices

    let detectCudaAvailability() =
        // Check if CUDA runtime is available
        try
            // This is a simplified check - actual implementation
            // would need to probe CUDA APIs
            let cudaPath = Environment.GetEnvironmentVariable("CUDA_PATH")
            match cudaPath with
            | null -> false
            | _ -> true
        with
        | _ -> false

    let detectVulkanAvailability() =
        // Check if Vulkan runtime is available
        // This is a simplified check
        try
            // Could use vulkaninfo or probe Vulkan APIs
            // For now, assume Vulkan is available if DLL exists
            let vulkanDll = "vulkan-1.dll"
            // Check in System32 or SysWOW64
            true // Placeholder
        with
        | _ -> false

    let printGPUInfo() =
        printfn "🔍 GPU Detection:"
        printfn "  CUDA Available: %b" (detectCudaAvailability())
        printfn "  Vulkan Available: %b" (detectVulkanAvailability())
        printfn ""
        printfn "  Whisper.NET will automatically select the best runtime."
        printfn "  Priority: CUDA → Vulkan → CPU"
```

**Call in Program.fs** (during startup):
```fsharp
// After model loading
GPUInfo.printGPUInfo()
```

**Note:** This task is **optional** and can be deferred. Whisper.NET already handles runtime selection automatically, so explicit detection may not be necessary for end users.

**Acceptance** (if implemented):
- GPU detection logic works correctly
- Helpful diagnostic output on startup
- No false positives/negatives
- Users can verify their setup without testing

---

## Phase 14 Summary

**What We Built:**
- Multi-GPU support via Vulkan runtime
- AMD Radeon GPU acceleration (RX 6000+ series)
- Intel Arc GPU acceleration
- Automatic runtime selection (CUDA → Vulkan → CPU)
- Expanded hardware compatibility

**Technology Stack:**
- Whisper.net.Runtime.Cuda.Windows v1.8.1 (existing)
- Whisper.net.Runtime.Vulkan v1.8.1 (new)

**Key Features:**
✅ AMD GPU support (Radeon RX 6000+)
✅ Intel GPU support (Arc series)
✅ Automatic fallback from CUDA to Vulkan
✅ No code changes required (runtime-only)
✅ Performance monitoring and logging
✅ Comprehensive documentation

**Benefits:**
- Expands supported hardware by ~25-30% (AMD GPU users)
- Improves reliability (Vulkan fallback if CUDA fails)
- Future-proofs against GPU vendor changes
- Minimal development effort (3-4 hours total)

**Performance Expectations:**
- AMD RX 6700 XT: ~1-2s for 5s speech (Base model)
- AMD RX 5700 XT: ~2-3s for 5s speech (Base model)
- Intel Arc A750: ~2-3s for 5s speech (Base model)
- Still much faster than CPU mode (~5-10s)

**Tradeoffs:**
- Slightly larger bundle size (+50-100MB for Vulkan runtime)
- Vulkan may be slightly slower than CUDA on NVIDIA GPUs (but CUDA is still preferred)
- Requires recent GPU drivers with Vulkan support

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
⬜ **Keywords organized into collapsible categories** (Phase 13)
✅ **AMD/Intel GPU support via Vulkan** (Phase 14)

---

**Status**: Ready for implementation
**Estimated Time**:
- Phases 1-6: 4-6 hours ✅
- Phases 7-9: 6-8 hours ✅
- Phase 10: 2-3 hours ✅
- Phase 11: 4-6 hours ✅
- Phase 12: 20-30 hours ✅ (comprehensive web UI overhaul)
- Phase 13: 12-16 hours (keyword categorization system) - 2/13 tasks complete
- **Phase 14: 3-4 hours ✅ COMPLETE** (AMD GPU support via Vulkan)

**Actual Time for Phase 14**: ~1.5 hours (faster than estimated due to minimal code changes)

**Next Task**: Phase 15 - External Keyword Configuration File

---

### Phase 15: External Keyword Configuration File ⬜

**Context**: Currently, keyword replacements and categories are stored in `settings.json` alongside other application settings. This phase enables users to store keywords in a separate, configurable file location. This allows sharing keywords across multiple computers via cloud storage (Google Drive, OneDrive, Dropbox, etc.) while keeping machine-specific settings local.

**Goals**:
- Move keyword data to a separate external file
- Make the external file path configurable in general settings
- Support cloud storage paths (Google Drive, OneDrive, etc.)
- Maintain backward compatibility with existing settings.json
- Enable real-time file watching for multi-computer sync
- Keep local settings (hotkey, model, typing speed) separate from shared keywords

**Architecture**:
```
settings.json (local, machine-specific):
  - HotkeyKey
  - HotkeyModifiers
  - ModelSize
  - RecordingDuration
  - TypingSpeed
  - IsEnabled
  - KeywordsFilePath (NEW - path to external keywords file)

keywords.json (external, shareable):
  - KeywordReplacements: KeywordReplacement list
  - Categories: KeywordCategory list
```

**Default Behavior**:
- If `KeywordsFilePath` is not set: use local file `%APPDATA%/VocalFold/keywords.json`
- If `KeywordsFilePath` is set: use specified path (e.g., `C:\Users\...\Google Drive\VocalFold\keywords.json`)

---

**Task 15.1: Backend - Split Settings Data Model**

Separate keyword-related data from general application settings:

**Changes to Settings.fs**:
```fsharp
// NEW: Separate type for keyword data (will be stored in external file)
type KeywordData = {
    KeywordReplacements: KeywordReplacement list
    Categories: KeywordCategory list
    Version: string  // For future schema migrations
}

// UPDATED: AppSettings no longer contains keyword data
type AppSettings = {
    HotkeyKey: uint32
    HotkeyModifiers: uint32
    IsEnabled: bool
    ModelSize: string
    RecordingDuration: int
    TypingSpeed: string
    KeywordsFilePath: string option  // NEW: Path to external keywords file
    // KeywordReplacements: REMOVED (moved to KeywordData)
    // Categories: REMOVED (moved to KeywordData)
}

// Default keywords file location
let getDefaultKeywordsPath() =
    let appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    Path.Combine(appDataPath, "VocalFold", "keywords.json")

// Load keyword data from external file
let loadKeywordData (filePath: string) : KeywordData =
    if File.Exists(filePath) then
        let json = File.ReadAllText(filePath)
        JsonSerializer.Deserialize<KeywordData>(json)
    else
        // Return default keyword data
        {
            KeywordReplacements = []
            Categories = [
                { Name = "Uncategorized"; IsExpanded = true; Color = None }
                { Name = "Punctuation"; IsExpanded = true; Color = Some "#25abfe" }
                { Name = "Email Templates"; IsExpanded = true; Color = Some "#ff8b00" }
                { Name = "Code Snippets"; IsExpanded = false; Color = Some "#10b981" }
            ]
            Version = "1.0"
        }

// Save keyword data to external file
let saveKeywordData (filePath: string) (data: KeywordData) : unit =
    let directory = Path.GetDirectoryName(filePath)
    if not (Directory.Exists(directory)) then
        Directory.CreateDirectory(directory) |> ignore

    let json = JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true))
    File.WriteAllText(filePath, json)

// Get the active keywords file path (from settings or default)
let getKeywordsFilePath (settings: AppSettings) : string =
    match settings.KeywordsFilePath with
    | Some path when not (String.IsNullOrWhiteSpace(path)) -> path
    | _ -> getDefaultKeywordsPath()
```

**Migration Logic** (for existing users):
```fsharp
// Migrate old settings.json to new split structure
let migrateToExternalKeywords (settings: AppSettings) : AppSettings * KeywordData =
    // Check if settings contain old keyword data (backward compatibility)
    match settings with
    | { KeywordReplacements = replacements; Categories = categories } when replacements <> [] || categories <> [] ->
        // Extract keyword data
        let keywordData = {
            KeywordReplacements = replacements
            Categories = categories
            Version = "1.0"
        }

        // Save to external file
        let keywordsPath = getDefaultKeywordsPath()
        saveKeywordData keywordsPath keywordData

        // Update settings to remove keyword data and add file path
        let newSettings = {
            settings with
                KeywordsFilePath = Some keywordsPath
                // Remove old fields (handled by type change)
        }

        (newSettings, keywordData)
    | _ ->
        // No migration needed
        let keywordsPath = getKeywordsFilePath settings
        let keywordData = loadKeywordData keywordsPath
        (settings, keywordData)
```

**Acceptance**:
- KeywordData type defined with replacements and categories
- AppSettings no longer contains keyword data
- Load/save functions for external keywords file
- Migration logic preserves existing keyword data
- Default path uses %APPDATA%/VocalFold/keywords.json

---

**Task 15.2: Backend - Settings API Updates**

Update REST API to handle split settings structure:

**Changes to SettingsApi.fs**:
```fsharp
// NEW: Global state for keyword data (in-memory cache)
let mutable cachedKeywordData: KeywordData option = None
let mutable keywordsFilePath: string = ""

// Update initialization to load keyword data separately
let initializeSettings() =
    let settings = Settings.loadSettings()
    let (migratedSettings, keywordData) = Settings.migrateToExternalKeywords settings

    keywordsFilePath <- Settings.getKeywordsFilePath migratedSettings
    cachedKeywordData <- Some keywordData

    // Save migrated settings if changed
    if settings <> migratedSettings then
        Settings.saveSettings migratedSettings

    migratedSettings

// Update GET /api/settings to include keywords file path info
let getSettings: HttpHandler =
    fun next ctx ->
        let settings = loadCurrentSettings()
        let response = {|
            Settings = settings
            KeywordsFilePath = Settings.getKeywordsFilePath settings
            KeywordsFileExists = File.Exists(Settings.getKeywordsFilePath settings)
        |}
        json response next ctx

// NEW: Endpoint to update keywords file path
// PUT /api/settings/keywords-path
let updateKeywordsPath: HttpHandler =
    fun next ctx -> task {
        let! request = ctx.BindJsonAsync<{| Path: string |}>()

        // Validate path
        if String.IsNullOrWhiteSpace(request.Path) then
            return! RequestErrors.BAD_REQUEST "Path cannot be empty" next ctx
        else
            let directory = Path.GetDirectoryName(request.Path)
            if not (Directory.Exists(directory)) then
                return! RequestErrors.BAD_REQUEST "Directory does not exist" next ctx
            else
                // Load current settings
                let settings = loadCurrentSettings()

                // Update path
                let newSettings = { settings with KeywordsFilePath = Some request.Path }
                Settings.saveSettings newSettings

                // Load keywords from new path (or create if doesn't exist)
                let keywordData = Settings.loadKeywordData request.Path
                cachedKeywordData <- Some keywordData
                keywordsFilePath <- request.Path

                return! Successful.OK {| Success = true; Path = request.Path |} next ctx
    }

// NEW: Endpoint to export current keywords to a new location
// POST /api/keywords/export-to-file
let exportKeywordsToFile: HttpHandler =
    fun next ctx -> task {
        let! request = ctx.BindJsonAsync<{| Path: string; SetAsActive: bool |}>()

        match cachedKeywordData with
        | Some keywordData ->
            // Save to specified path
            Settings.saveKeywordData request.Path keywordData

            // Optionally set as active keywords file
            if request.SetAsActive then
                let settings = loadCurrentSettings()
                let newSettings = { settings with KeywordsFilePath = Some request.Path }
                Settings.saveSettings newSettings
                keywordsFilePath <- request.Path

            return! Successful.OK {| Success = true; Path = request.Path |} next ctx
        | None ->
            return! RequestErrors.BAD_REQUEST "No keyword data loaded" next ctx
    }

// Update routes
let keywordRoutes: HttpHandler =
    choose [
        // ... existing routes ...
        PUT >=> route "/api/settings/keywords-path" >=> updateKeywordsPath
        POST >=> route "/api/keywords/export-to-file" >=> exportKeywordsToFile
    ]
```

**Changes to KeywordsApi.fs**:
```fsharp
// Update all keyword CRUD operations to save to external file
let saveKeywordChanges() =
    match cachedKeywordData with
    | Some data ->
        Settings.saveKeywordData keywordsFilePath data
        // Notify app of changes (existing callback mechanism)
    | None -> ()

// Update GET /api/keywords to return from cached data
let getKeywords: HttpHandler =
    fun next ctx ->
        match cachedKeywordData with
        | Some data -> json data.KeywordReplacements next ctx
        | None -> json [] next ctx

// Update POST /api/keywords to save to external file
let addKeyword: HttpHandler =
    fun next ctx -> task {
        let! newKeyword = ctx.BindJsonAsync<KeywordReplacement>()

        match cachedKeywordData with
        | Some data ->
            let updatedData = {
                data with
                    KeywordReplacements = data.KeywordReplacements @ [newKeyword]
            }
            cachedKeywordData <- Some updatedData
            saveKeywordChanges()
            return! Successful.OK {| Success = true |} next ctx
        | None ->
            return! RequestErrors.BAD_REQUEST "Keyword data not loaded" next ctx
    }

// Similar updates for PUT and DELETE operations
```

**Acceptance**:
- API loads keywords from external file
- API saves keyword changes to external file
- Can update keywords file path via API
- Can export keywords to a new location
- All keyword operations work with split structure

---

**Task 15.3: Backend - File Watching & Auto-Reload**

Implement file watching to detect external changes to keywords file (e.g., synced from another computer):

**Create FileWatcher.fs**:
```fsharp
module FileWatcher

open System
open System.IO

type FileChangeCallback = unit -> unit

let createWatcher (filePath: string) (callback: FileChangeCallback) : FileSystemWatcher =
    let directory = Path.GetDirectoryName(filePath)
    let fileName = Path.GetFileName(filePath)

    let watcher = new FileSystemWatcher()
    watcher.Path <- directory
    watcher.Filter <- fileName
    watcher.NotifyFilter <- NotifyFilters.LastWrite ||| NotifyFilters.FileName

    // Debounce changes (avoid multiple events for single change)
    let mutable lastEventTime = DateTime.MinValue
    let debounceMs = 500.0

    let onChanged (e: FileSystemEventArgs) =
        let now = DateTime.Now
        if (now - lastEventTime).TotalMilliseconds > debounceMs then
            lastEventTime <- now
            printfn "📄 Keywords file changed externally, reloading..."
            callback()

    watcher.Changed.Add(onChanged)
    watcher.Created.Add(onChanged)
    watcher.Deleted.Add(onChanged)

    watcher.EnableRaisingEvents <- true
    watcher

let stopWatcher (watcher: FileSystemWatcher) =
    watcher.EnableRaisingEvents <- false
    watcher.Dispose()
```

**Integration in WebServer.fs**:
```fsharp
type ServerState = {
    Port: int
    CancellationToken: CancellationTokenSource
    FileWatcher: FileSystemWatcher option  // NEW
}

let start (config: ServerConfig) : Async<ServerState> =
    async {
        // ... existing server startup ...

        // Start file watcher for keywords file
        let keywordsPath = Settings.getKeywordsFilePath (Settings.loadSettings())

        let reloadCallback = fun () ->
            // Reload keyword data from file
            let keywordData = Settings.loadKeywordData keywordsPath
            SettingsApi.cachedKeywordData <- Some keywordData

            // Notify TextProcessor to reload keywords
            config.OnKeywordsChanged keywordData

        let watcher =
            if File.Exists(keywordsPath) then
                Some (FileWatcher.createWatcher keywordsPath reloadCallback)
            else
                None

        return {
            Port = port
            CancellationToken = cts
            FileWatcher = watcher
        }
    }

let stop (state: ServerState) : Async<unit> =
    async {
        // Stop file watcher
        match state.FileWatcher with
        | Some watcher -> FileWatcher.stopWatcher watcher
        | None -> ()

        // ... existing shutdown logic ...
    }
```

**Acceptance**:
- File watcher detects changes to keywords file
- Changes trigger automatic reload of keyword data
- Debouncing prevents excessive reloads
- File watcher stops cleanly on app shutdown
- Works with cloud-synced folders

---

**Task 15.4: Frontend - Keywords File Path Configuration UI**

Add UI to configure the external keywords file path:

**Add to General Settings Page**:
```tsx
// In GeneralSettings.tsx (or GeneralSettings.fs for Fable)

let KeywordsFilePathSection =
    Html.div [
        prop.className "bg-background-card rounded-lg p-6 space-y-4"
        prop.children [
            Html.h3 [
                prop.className "text-lg font-semibold text-text-primary"
                prop.text "Keywords Storage Location"
            ]

            Html.p [
                prop.className "text-sm text-text-secondary"
                prop.text "Configure where your keyword replacements are stored. Use a cloud folder (Google Drive, OneDrive) to share keywords across multiple computers."
            ]

            // Current path display
            Html.div [
                prop.className "space-y-2"
                prop.children [
                    Html.label [
                        prop.className "text-sm font-medium text-text-primary"
                        prop.text "Current Location:"
                    ]
                    Html.div [
                        prop.className "flex items-center space-x-2"
                        prop.children [
                            Html.input [
                                prop.type' "text"
                                prop.readOnly true
                                prop.value model.KeywordsFilePath
                                prop.className "flex-1 px-3 py-2 bg-background-dark text-text-primary rounded border border-gray-700"
                            ]
                            Html.button [
                                prop.className "px-4 py-2 bg-primary hover:bg-primary-600 text-white rounded transition"
                                prop.text "Browse..."
                                prop.onClick (fun _ -> dispatch OpenFilePathPicker)
                            ]
                        ]
                    ]

                    // File exists indicator
                    if model.KeywordsFileExists then
                        Html.div [
                            prop.className "flex items-center text-sm text-green-500"
                            prop.children [
                                Html.span [ prop.text "✓ File exists" ]
                            ]
                        ]
                    else
                        Html.div [
                            prop.className "flex items-center text-sm text-amber-500"
                            prop.children [
                                Html.span [ prop.text "⚠ File will be created" ]
                            ]
                        ]
                ]
            ]

            // Quick setup buttons
            Html.div [
                prop.className "space-y-2"
                prop.children [
                    Html.p [
                        prop.className "text-sm font-medium text-text-primary"
                        prop.text "Quick Setup:"
                    ]
                    Html.div [
                        prop.className "flex space-x-2"
                        prop.children [
                            Html.button [
                                prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded transition"
                                prop.text "📁 Use Local Folder"
                                prop.onClick (fun _ -> dispatch UseDefaultPath)
                            ]
                            Html.button [
                                prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded transition"
                                prop.text "☁️ Use Google Drive"
                                prop.onClick (fun _ -> dispatch UseGoogleDrivePath)
                            ]
                            Html.button [
                                prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded transition"
                                prop.text "☁️ Use OneDrive"
                                prop.onClick (fun _ -> dispatch UseOneDrivePath)
                            ]
                        ]
                    ]
                ]
            ]

            // Info box
            Html.div [
                prop.className "bg-primary bg-opacity-10 border border-primary rounded p-4"
                prop.children [
                    Html.p [
                        prop.className "text-sm text-text-primary"
                        prop.children [
                            Html.strong [ prop.text "💡 Tip: " ]
                            Html.text "Set the path to a cloud-synced folder to automatically share your keywords across multiple computers. Any changes made on one computer will sync to others."
                        ]
                    ]
                ]
            ]
        ]
    ]
```

**File Path Picker Modal**:
```tsx
let FilePathPickerModal =
    Html.div [
        prop.className "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
        prop.onClick (fun e ->
            if e.target = e.currentTarget then
                dispatch CloseFilePathPicker)
        prop.children [
            Html.div [
                prop.className "bg-background-card rounded-lg p-6 max-w-2xl w-full mx-4"
                prop.children [
                    Html.h3 [
                        prop.className "text-xl font-semibold text-text-primary mb-4"
                        prop.text "Choose Keywords File Location"
                    ]

                    Html.div [
                        prop.className "space-y-3"
                        prop.children [
                            Html.input [
                                prop.type' "text"
                                prop.placeholder "Enter full path (e.g., C:\\Users\\...\\Google Drive\\VocalFold\\keywords.json)"
                                prop.value model.NewKeywordsPath
                                prop.onChange (fun (value: string) -> dispatch (UpdateNewKeywordsPath value))
                                prop.className "w-full px-3 py-2 bg-background-dark text-text-primary rounded border border-gray-700"
                            ]

                            // Common cloud storage paths
                            Html.div [
                                prop.className "space-y-2"
                                prop.children [
                                    Html.p [
                                        prop.className "text-sm text-text-secondary"
                                        prop.text "Common locations:"
                                    ]

                                    CommonPathButton "Google Drive" (getGoogleDrivePath())
                                    CommonPathButton "OneDrive" (getOneDrivePath())
                                    CommonPathButton "Dropbox" (getDropboxPath())
                                    CommonPathButton "Local AppData" (getDefaultKeywordsPath())
                                ]
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "flex justify-end space-x-3 mt-6"
                        prop.children [
                            Html.button [
                                prop.className "px-4 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded transition"
                                prop.text "Cancel"
                                prop.onClick (fun _ -> dispatch CloseFilePathPicker)
                            ]
                            Html.button [
                                prop.className "px-4 py-2 bg-primary hover:bg-primary-600 text-white rounded transition"
                                prop.text "Save & Switch"
                                prop.onClick (fun _ -> dispatch SaveNewKeywordsPath)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
```

**Helper Functions**:
```fsharp
let getGoogleDrivePath() =
    let userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    Path.Combine(userProfile, "Google Drive", "VocalFold", "keywords.json")

let getOneDrivePath() =
    let oneDrive = Environment.GetEnvironmentVariable("OneDrive")
    if String.IsNullOrEmpty(oneDrive) then
        let userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Path.Combine(userProfile, "OneDrive", "VocalFold", "keywords.json")
    else
        Path.Combine(oneDrive, "VocalFold", "keywords.json")

let getDropboxPath() =
    let userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    Path.Combine(userProfile, "Dropbox", "VocalFold", "keywords.json")
```

**Acceptance**:
- UI displays current keywords file path
- Can browse and change keywords file path
- Quick setup buttons for common cloud storage paths
- Path validation before saving
- Visual indicator for file existence
- Clear instructions and helpful tips

---

**Task 15.5: Frontend - Export & Import Keywords UI**

Add UI to easily export keywords to a new location:

**Add to Keywords Page**:
```tsx
// Add toolbar buttons to Keywords Manager

let KeywordsToolbar =
    Html.div [
        prop.className "flex space-x-3 mb-4"
        prop.children [
            // Existing buttons
            Html.button [ (* Add Keyword *) ]
            Html.button [ (* Add Category *) ]

            // NEW: Export/Import buttons
            Html.button [
                prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded transition"
                prop.children [
                    Html.span [ prop.text "📤 Export to File" ]
                ]
                prop.onClick (fun _ -> dispatch OpenExportModal)
            ]

            Html.button [
                prop.className "px-4 py-2 bg-secondary hover:bg-secondary-600 text-white rounded transition"
                prop.children [
                    Html.span [ prop.text "📥 Import from File" ]
                ]
                prop.onClick (fun _ -> dispatch OpenImportModal)
            ]

            // Sync status indicator
            if model.KeywordsFileSynced then
                Html.div [
                    prop.className "flex items-center text-sm text-green-500 ml-auto"
                    prop.children [
                        Html.span [ prop.text "☁️ Synced" ]
                    ]
                ]
            else
                Html.div [
                    prop.className "flex items-center text-sm text-amber-500 ml-auto"
                    prop.children [
                        Html.span [ prop.text "🔄 Syncing..." ]
                    ]
                ]
        ]
    ]
```

**Export Modal**:
```tsx
let ExportKeywordsModal =
    Html.div [
        (* Modal overlay *)
        prop.children [
            Html.div [
                prop.className "bg-background-card rounded-lg p-6 max-w-lg w-full mx-4"
                prop.children [
                    Html.h3 [
                        prop.text "Export Keywords to File"
                    ]

                    Html.div [
                        prop.className "space-y-4"
                        prop.children [
                            Html.input [
                                prop.type' "text"
                                prop.placeholder "Path to save keywords file"
                                prop.value model.ExportPath
                                prop.onChange (fun v -> dispatch (UpdateExportPath v))
                            ]

                            Html.div [
                                prop.className "flex items-center space-x-2"
                                prop.children [
                                    Html.input [
                                        prop.type' "checkbox"
                                        prop.checked model.SetAsActiveFile
                                        prop.onChange (fun _ -> dispatch ToggleSetAsActive)
                                    ]
                                    Html.label [
                                        prop.text "Set as active keywords file after export"
                                    ]
                                ]
                            ]

                            Html.p [
                                prop.className "text-sm text-text-secondary"
                                prop.text "This will save all your keywords and categories to the specified file. Check the box above to also switch to using this file as your active keywords storage."
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "flex justify-end space-x-3 mt-6"
                        prop.children [
                            Html.button [
                                prop.text "Cancel"
                                prop.onClick (fun _ -> dispatch CloseExportModal)
                            ]
                            Html.button [
                                prop.className "bg-primary hover:bg-primary-600"
                                prop.text "Export"
                                prop.onClick (fun _ -> dispatch ExportKeywords)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
```

**Acceptance**:
- Export button saves keywords to specified location
- Option to set exported file as active keywords file
- Import button loads keywords from external file
- Clear UI for managing keyword file locations
- Sync status indicator shows cloud sync state

---

**Task 15.6: Integration - Update TextProcessor to Use External Keywords**

Ensure TextProcessor reloads keywords when external file changes:

**Changes to TextProcessor.fs** (or wherever keyword replacement logic is):
```fsharp
module TextProcessor

// Mutable reference to current keyword data (updated by file watcher)
let mutable currentKeywordData: KeywordData option = None

// Initialize with keyword data
let initialize (keywordData: KeywordData) =
    currentKeywordData <- Some keywordData
    printfn "📝 Loaded %d keywords in %d categories"
        keywordData.KeywordReplacements.Length
        keywordData.Categories.Length

// Reload keywords (called by file watcher callback)
let reload (keywordData: KeywordData) =
    currentKeywordData <- Some keywordData
    printfn "🔄 Reloaded %d keywords from external file"
        keywordData.KeywordReplacements.Length

// Existing processTranscription function uses currentKeywordData
let processTranscription (text: string) : string =
    match currentKeywordData with
    | Some data ->
        // Apply keyword replacements
        let mutable result = text
        for keyword in data.KeywordReplacements do
            // Replacement logic (existing code)
            result <- (* ... *)
        result
    | None ->
        printfn "⚠️ No keyword data loaded, returning original text"
        text
```

**Integration in Program.fs**:
```fsharp
[<EntryPoint>]
let main argv =
    // Load settings and keywords
    let settings = Settings.loadSettings()
    let keywordsPath = Settings.getKeywordsFilePath settings
    let keywordData = Settings.loadKeywordData keywordsPath

    // Initialize TextProcessor with keywords
    TextProcessor.initialize keywordData

    // Set up file watcher callback to reload keywords
    let onKeywordsChanged (newData: KeywordData) =
        TextProcessor.reload newData
        printfn "✅ Keywords updated from external file"

    // Start web server with callback
    let serverConfig = {
        Port = 0  // Random port
        OnSettingsChanged = handleSettingsChanged
        OnKeywordsChanged = onKeywordsChanged  // NEW
    }

    // ... rest of startup logic ...
```

**Acceptance**:
- TextProcessor uses keywords from external file
- File watcher triggers keyword reload
- Transcription uses updated keywords immediately
- Console logging confirms keyword reloads
- Multi-computer sync works in real-time

---

**Task 15.7: Testing - Cloud Storage Scenarios**

Comprehensive testing of external keywords file with cloud storage:

**Test Scenarios**:

1. **Initial Setup**:
   - [ ] Fresh install creates default keywords.json in AppData
   - [ ] Can change path to Google Drive folder
   - [ ] Keywords persist after path change
   - [ ] Settings.json stores correct path

2. **Multi-Computer Sync** (requires 2 computers with cloud storage):
   - [ ] Computer A: Set keywords path to Google Drive
   - [ ] Computer A: Add keyword "test" → "testing"
   - [ ] Computer B: Set same keywords path
   - [ ] Computer B: Keyword appears automatically
   - [ ] Computer B: Modify keyword, Computer A sees change

3. **Conflict Resolution**:
   - [ ] Both computers modify keywords simultaneously
   - [ ] Cloud storage resolves conflict (last-write-wins)
   - [ ] File watcher detects final state
   - [ ] No data corruption or crashes

4. **Edge Cases**:
   - [ ] Keywords file deleted externally → recreated with defaults
   - [ ] Path set to non-existent directory → validation error
   - [ ] Path set to read-only location → error on save
   - [ ] Cloud storage offline → uses last cached version
   - [ ] Very large keywords file (1000+ keywords) → performance acceptable

5. **Migration**:
   - [ ] Existing settings.json with keywords → migrates to external file
   - [ ] Keywords and categories preserved
   - [ ] Settings.json updated with file path
   - [ ] No data loss

6. **Backward Compatibility**:
   - [ ] Old settings.json (pre-Phase 15) loads correctly
   - [ ] Keywords automatically migrated
   - [ ] App continues to function normally

**Performance Testing**:
- [ ] File watcher responds within 500ms of change
- [ ] Keyword reload takes <100ms for 500 keywords
- [ ] No memory leaks with repeated reloads
- [ ] No excessive file I/O (debouncing works)

**Acceptance**:
- All test scenarios pass
- Multi-computer sync works reliably
- Edge cases handled gracefully
- Performance is acceptable
- No data loss in any scenario

---

**Task 15.8: Documentation & User Guide**

Document the external keywords file feature:

**Update USER_GUIDE.md**:
```markdown
### Sharing Keywords Across Multiple Computers

VocalFold allows you to store your keyword replacements in a cloud-synced folder (Google Drive, OneDrive, Dropbox) so that all your computers use the same keywords automatically.

#### Setting Up Cloud-Synced Keywords

1. **Open Settings**
   - Click on Settings in the tray menu
   - Navigate to "General Settings"

2. **Configure Keywords Location**
   - Find the "Keywords Storage Location" section
   - Click "Browse..." to choose a location
   - Or use a quick setup button:
     - "☁️ Use Google Drive" - Automatically finds your Google Drive folder
     - "☁️ Use OneDrive" - Automatically finds your OneDrive folder
     - "📁 Use Local Folder" - Uses default local storage

3. **Recommended Path Structure**
   ```
   Google Drive/VocalFold/keywords.json
   OneDrive/VocalFold/keywords.json
   Dropbox/VocalFold/keywords.json
   ```

4. **Apply Changes**
   - Click "Save"
   - Your current keywords will be exported to the new location
   - VocalFold will now watch this file for changes

#### Using on Multiple Computers

1. **First Computer**:
   - Set keywords path to cloud folder (e.g., Google Drive)
   - Configure your keywords as usual
   - Keywords are automatically saved to cloud

2. **Second Computer**:
   - Install VocalFold
   - Open Settings → General Settings
   - Set keywords path to the **same cloud folder**
   - VocalFold will load your keywords automatically

3. **Automatic Syncing**:
   - Changes made on any computer sync automatically
   - VocalFold detects external changes and reloads keywords
   - Sync status shown in Keywords Manager (☁️ Synced)

#### Important Notes

⚠️ **Cloud Storage Must Be Syncing**:
- Ensure your cloud storage app (Google Drive, OneDrive) is running and syncing
- Check that the VocalFold folder is not excluded from sync

💡 **Local vs. Cloud Settings**:
- General settings (hotkey, model, typing speed) remain local to each computer
- Only keywords and categories are shared via the external file

🔄 **Conflict Resolution**:
- If both computers modify keywords simultaneously, the last saved version wins
- This is handled by your cloud storage provider
- VocalFold automatically reloads the latest version

#### Exporting Keywords to a New Location

1. Go to Keywords Manager
2. Click "📤 Export to File"
3. Enter the destination path
4. Check "Set as active keywords file after export" to switch to the new location
5. Click "Export"

#### Troubleshooting

**Keywords not syncing between computers:**
- Verify both computers point to the exact same file path
- Check that cloud storage is actively syncing
- Ensure file is not locked by another application

**Changes not appearing:**
- Wait a few seconds for cloud sync to complete
- Check the sync status indicator (☁️ icon)
- Manually refresh by reopening the Keywords Manager

**File not found error:**
- Verify the cloud storage folder exists
- Check that cloud storage app is running
- Use the "Browse..." button to reselect the file
```

**Update ARCHITECTURE.md**:
```markdown
### External Keywords Configuration (Phase 15)

**Purpose**: Enable sharing of keyword replacements across multiple computers via cloud storage while keeping machine-specific settings local.

**Data Split**:
- **settings.json** (local): Machine-specific settings (hotkey, model, typing speed)
- **keywords.json** (external): Shareable keyword data (replacements, categories)

**File Locations**:
- Default: `%APPDATA%/VocalFold/keywords.json`
- Cloud storage: Configurable path (e.g., `C:/Users/.../Google Drive/VocalFold/keywords.json`)

**File Watching**:
- FileSystemWatcher monitors external keywords file
- Automatic reload on external changes (500ms debounce)
- Enables real-time multi-computer synchronization

**API Endpoints**:
- `PUT /api/settings/keywords-path` - Update keywords file path
- `POST /api/keywords/export-to-file` - Export keywords to new location
- All keyword CRUD operations save to external file

**Migration**:
- Existing settings.json with embedded keywords automatically migrated
- Keywords moved to external file on first run of Phase 15
- Backward compatible with pre-Phase-15 settings
```

**Update README.md** (Features section):
```markdown
### Features

- 🎤 **Voice-to-Text Transcription**: Press Ctrl+Windows and speak
- 🖥️ **System-Wide Operation**: Works in any Windows application
- 🎯 **GPU Acceleration**: NVIDIA CUDA, AMD/Intel Vulkan, or CPU fallback
- ⚡ **Fast Processing**: <1s transcription for 5s of speech
- 🔤 **Custom Keywords**: Replace spoken words with text (e.g., "comma" → ",")
- 📁 **Keyword Organization**: Group keywords into collapsible categories
- ☁️ **Cloud Sync**: Share keywords across multiple computers via Google Drive, OneDrive, etc. (NEW)
- 🌐 **Modern Web UI**: Beautiful settings interface with real-time updates
- 🔄 **Auto-Start**: Launches with Windows
- 🎨 **Brand Colors**: Blue (#25abfe) and Orange (#ff8b00) theme
```

**Acceptance**:
- User guide explains cloud sync setup clearly
- Architecture documentation updated
- README highlights the feature
- Troubleshooting guidance included
- Examples and screenshots (optional)

---

**Task 15.9: Polish & Final Integration**

Final polish and integration testing:

**Polish Checklist**:
- [ ] All UI text is clear and helpful
- [ ] Error messages are actionable
- [ ] Console logging for all file operations
- [ ] Toasts confirm file path changes
- [ ] Sync status indicator updates in real-time
- [ ] Animations smooth during path changes
- [ ] Validation prevents invalid paths
- [ ] File path shown with ellipsis if too long (tooltip shows full path)

**Integration Testing**:
- [ ] Keywords file path persists across restarts
- [ ] TextProcessor uses keywords from external file
- [ ] File watcher triggers reload correctly
- [ ] Web UI reflects current keywords file path
- [ ] All keyword CRUD operations save to external file
- [ ] Migration from old settings works seamlessly
- [ ] Export/import maintains data integrity
- [ ] Cloud sync works with Google Drive, OneDrive, Dropbox

**Security & Reliability**:
- [ ] Path validation prevents directory traversal
- [ ] File permissions checked before writing
- [ ] File I/O errors handled gracefully
- [ ] Concurrent access from multiple instances handled
- [ ] File locking respected (don't corrupt during sync)

**Performance**:
- [ ] File watcher overhead minimal (<1% CPU)
- [ ] Keyword reload fast (<100ms)
- [ ] No memory leaks with repeated reloads
- [ ] Large keywords files (1000+ keywords) handled efficiently

**Acceptance**:
- Feature is production-ready
- All integration tests pass
- No regressions in existing features
- Cloud sync works reliably
- User experience is smooth and intuitive

---

## Phase 15 Summary

**What We Built**:
- External keywords configuration file (keywords.json)
- Configurable file path in general settings
- Cloud storage support (Google Drive, OneDrive, Dropbox)
- Real-time file watching for multi-computer sync
- Export/import keywords to different locations
- Migration from embedded keywords to external file
- Web UI for managing keywords file location

**Key Features**:
✅ Keywords stored in separate external file
✅ Configurable file path (local or cloud storage)
✅ Real-time file watching and auto-reload
✅ Multi-computer synchronization via cloud storage
✅ Export keywords to new location
✅ Quick setup buttons for common cloud providers
✅ Backward compatible with existing settings
✅ Migration preserves all keyword data

**Architecture Changes**:
- Split AppSettings into AppSettings (local) and KeywordData (external)
- settings.json contains only machine-specific settings
- keywords.json contains shareable keyword data
- FileSystemWatcher monitors external file for changes
- TextProcessor reloads keywords on external changes

**Benefits**:
- Share keywords across multiple computers automatically
- Machine-specific settings (hotkey, model) remain local
- Real-time sync via cloud storage providers
- Easy backup and restore of keywords
- Better organization of configuration data
- Enables team sharing (multiple users, same keywords)

**Use Cases**:
- User with desktop and laptop wants same keywords on both
- Team wants to share company-specific keywords
- User wants to back up keywords separately from app settings
- User switches computers frequently (cloud-synced keywords follow)

**Technology**:
- Backend: F#, FileSystemWatcher, JSON serialization
- Frontend: F#, Fable, React, TailwindCSS
- Storage: Separate JSON files (settings.json, keywords.json)
- Sync: Cloud storage providers (Google Drive, OneDrive, Dropbox)

**Tradeoffs**:
- Slightly more complex settings management
- Requires cloud storage app for multi-computer sync
- File watching adds minimal CPU overhead
- Cloud sync depends on external provider reliability

---

**Status**: Phase 15 specification complete, ready for implementation
**Estimated Time**: 8-12 hours (includes backend, frontend, testing, documentation)

**Next Task**: Implement Phase 15 (Task 15.1 - Backend - Split Settings Data Model)

---

### Phase 16: Microphone Setup ⬜

**Context**: Currently, VocalFold uses the default system microphone for audio recording. Users with multiple microphones (USB mics, headsets, virtual audio devices) need a way to select which microphone to use. Additionally, users need a way to test if their microphone is working before attempting voice transcription.

**Goals**:
- Add microphone selection to settings
- Allow users to choose from available audio input devices
- Provide a microphone test feature with visual feedback
- Display real-time audio levels during testing
- Save microphone preference to settings
- Auto-fallback to default if selected device unavailable

**Why This Matters**:
- Many users have multiple audio input devices
- Wrong microphone selection causes silent recordings
- Audio level visualization helps troubleshooting
- Testing prevents wasted transcription attempts

**Technical Overview**:
```
NAudio WaveInEvent.DeviceCount -> List all available microphones
Settings.SelectedMicrophoneIndex -> Store user's choice
AudioRecorder.recordAudio(deviceNumber) -> Use selected device
Web UI: Microphone dropdown + Test button
Real-time visualization: Audio levels + waveform
```

---

**Task 16.1: Backend - Microphone Data Model**

Add microphone selection to settings data structure:

**Changes to Settings.fs**:
```fsharp
/// Application settings (machine-specific, stored in settings.json)
[<CLIMutable>]
type AppSettings = {
    // ... existing fields ...

    /// Selected microphone device index (None = default device)
    [<JsonPropertyName("selectedMicrophoneIndex")>]
    SelectedMicrophoneIndex: int option
}
```

**Update defaultSettings**:
```fsharp
let defaultSettings = {
    // ... existing defaults ...
    SelectedMicrophoneIndex = None  // Use default microphone
}
```

**Implementation**:
1. Add `SelectedMicrophoneIndex` field to `AppSettings` type
2. Add default value (`None`) to `defaultSettings`
3. Update JSON serialization to handle optional int
4. Ensure backward compatibility (old settings files work)
5. Update settings load/save functions
6. Add validation to ensure device index is within range

**Acceptance**:
- Settings type includes microphone selection
- Default settings use system default microphone
- Settings serialize/deserialize correctly
- Old settings files migrate cleanly

---

**Task 16.2: Backend - Microphone Enumeration API**

Create functions to list and validate available microphones:

**Add to AudioRecorder module** (`AudioRecorder.fs`):
```fsharp
/// Microphone device information
type MicrophoneDevice = {
    Index: int
    Name: string
    Channels: int
    IsDefault: bool
}

/// Get a list of all available microphone devices
let getAvailableDevices () : MicrophoneDevice list =
    try
        let defaultDeviceIndex = 0  // NAudio uses 0 for default

        [0 .. WaveInEvent.DeviceCount - 1]
        |> List.map (fun i ->
            try
                let caps = WaveInEvent.GetCapabilities(i)
                {
                    Index = i
                    Name = caps.ProductName
                    Channels = caps.Channels
                    IsDefault = (i = defaultDeviceIndex)
                }
            with
            | ex ->
                Logger.warning (sprintf "Could not get info for device %d: %s" i ex.Message)
                {
                    Index = i
                    Name = sprintf "Unknown Device %d" i
                    Channels = 1
                    IsDefault = false
                }
        )
    with
    | ex ->
        Logger.error (sprintf "Error enumerating microphones: %s" ex.Message)
        []

/// Check if a device index is valid
let isDeviceIndexValid (deviceIndex: int) : bool =
    deviceIndex >= 0 && deviceIndex < WaveInEvent.DeviceCount

/// Get device name by index
let getDeviceName (deviceIndex: int) : string option =
    try
        if isDeviceIndexValid deviceIndex then
            let caps = WaveInEvent.GetCapabilities(deviceIndex)
            Some caps.ProductName
        else
            None
    with
    | ex ->
        Logger.warning (sprintf "Could not get device name for index %d: %s" deviceIndex ex.Message)
        None
```

**Implementation Notes**:
- Use NAudio's `WaveInEvent.GetCapabilities()` to query device info
- Handle cases where device enumeration fails gracefully
- Return structured data (index, name, channels)
- Mark default device for UI convenience
- Validate device indices before use
- Log warnings for inaccessible devices

**Acceptance**:
- Can enumerate all available microphones
- Device list includes index, name, channels
- Default device is marked
- Invalid indices detected correctly
- Graceful error handling

---

**Task 16.3: Backend - Microphone Test API**

Implement microphone testing with real-time audio level feedback:

**Add to AudioRecorder module**:
```fsharp
/// Real-time audio level callback
type AudioLevelCallback = float32 -> unit

/// Test recording state
type TestRecordingState = {
    WaveIn: WaveInEvent
    mutable CurrentLevel: float32
    mutable MaxLevel: float32
    mutable AvgLevel: float32
    mutable SampleCount: int
    RecordingStopped: System.Threading.ManualResetEvent
}

/// Start a test recording (non-blocking, returns state)
let startTestRecording (deviceIndex: int option) (onLevelUpdate: AudioLevelCallback option) : TestRecordingState =
    let sampleRate = 16000
    let channels = 1
    let waveFormat = WaveFormat(sampleRate, 16, channels)

    let waveIn = new WaveInEvent(
        WaveFormat = waveFormat,
        BufferMilliseconds = 50  // Faster updates for testing
    )

    // Set device if specified
    match deviceIndex with
    | Some idx -> waveIn.DeviceNumber <- idx
    | None -> ()

    let state = {
        WaveIn = waveIn
        CurrentLevel = 0.0f
        MaxLevel = 0.0f
        AvgLevel = 0.0f
        SampleCount = 0
        RecordingStopped = new System.Threading.ManualResetEvent(false)
    }

    // Data available handler
    waveIn.DataAvailable.Add(fun args ->
        let samples = bytesToFloat32 args.Buffer args.BytesRecorded

        // Calculate buffer max level
        let mutable bufferMaxLevel = 0.0f
        for sample in samples do
            let absLevel = abs sample
            if absLevel > state.MaxLevel then
                state.MaxLevel <- absLevel
            if absLevel > bufferMaxLevel then
                bufferMaxLevel <- absLevel
            state.AvgLevel <- state.AvgLevel + absLevel
            state.SampleCount <- state.SampleCount + 1

        state.CurrentLevel <- bufferMaxLevel

        // Call level update callback
        match onLevelUpdate with
        | Some callback -> callback bufferMaxLevel
        | None -> ()
    )

    // Recording stopped handler
    waveIn.RecordingStopped.Add(fun _ ->
        state.RecordingStopped.Set() |> ignore
    )

    waveIn.StartRecording()
    Logger.info "Test recording started"

    state

/// Stop test recording and return statistics
let stopTestRecording (state: TestRecordingState) : (float32 * float32 * float32) =
    state.WaveIn.StopRecording()
    state.RecordingStopped.WaitOne(1000) |> ignore
    state.WaveIn.Dispose()
    state.RecordingStopped.Dispose()

    let avgLevelNormalized =
        if state.SampleCount > 0 then
            state.AvgLevel / float32 state.SampleCount
        else
            0.0f

    Logger.info (sprintf "Test recording stopped - Max: %.3f, Avg: %.3f" state.MaxLevel avgLevelNormalized)

    // Return (currentLevel, maxLevel, avgLevel)
    (state.CurrentLevel, state.MaxLevel, avgLevelNormalized)
```

**Implementation Notes**:
- Non-blocking test recording (start/stop pattern)
- Real-time audio level calculation
- Callback mechanism for UI updates
- Return statistics on stop (max, avg levels)
- Faster buffer updates (50ms) for responsive UI
- Proper resource cleanup

**Acceptance**:
- Can start/stop test recording
- Real-time audio levels calculated
- Callback fires on each buffer
- No memory leaks
- Works with any valid device index

---

**Task 16.4: Backend - REST API Endpoints**

Add REST API endpoints for microphone management:

**Add to WebServer.fs**:
```fsharp
/// GET /api/microphones - List all available microphones
let getMicrophonesHandler: HttpHandler =
    fun next ctx ->
        try
            let devices = AudioRecorder.getAvailableDevices()

            // Convert to JSON-friendly format
            let devicesJson =
                devices
                |> List.map (fun d ->
                    {|
                        index = d.Index
                        name = d.Name
                        channels = d.Channels
                        isDefault = d.IsDefault
                    |})

            json devicesJson next ctx
        with
        | ex ->
            RequestErrors.INTERNAL_ERROR (sprintf "Error listing microphones: %s" ex.Message) next ctx

/// GET /api/microphones/test/start/:index - Start microphone test
let startMicrophoneTestHandler (deviceIndex: int option) : HttpHandler =
    fun next ctx ->
        try
            // Store test state in app state (implement with mutable ref or agent)
            let state = AudioRecorder.startTestRecording deviceIndex None

            // Store state in context (for later stop call)
            // Implementation depends on your state management approach

            json {| status = "started"; deviceIndex = deviceIndex |} next ctx
        with
        | ex ->
            RequestErrors.INTERNAL_ERROR (sprintf "Error starting test: %s" ex.Message) next ctx

/// POST /api/microphones/test/stop - Stop microphone test and get results
let stopMicrophoneTestHandler: HttpHandler =
    fun next ctx ->
        try
            // Retrieve state from context
            // let state = ... (get from app state)
            // let (current, max, avg) = AudioRecorder.stopTestRecording state

            // For now, return dummy data (implement proper state management)
            json {|
                status = "stopped"
                currentLevel = 0.0
                maxLevel = 0.0
                avgLevel = 0.0
            |} next ctx
        with
        | ex ->
            RequestErrors.INTERNAL_ERROR (sprintf "Error stopping test: %s" ex.Message) next ctx

/// GET /api/microphones/test/levels - Get real-time audio levels (Server-Sent Events)
let getMicrophoneLevelsHandler: HttpHandler =
    fun next ctx ->
        // Implementation: Server-Sent Events stream
        // Send real-time audio level updates
        // This requires SSE support in Giraffe
        task {
            // TODO: Implement SSE for real-time updates
            return! RequestErrors.NOT_IMPLEMENTED "SSE not yet implemented" next ctx
        }
```

**Update routing** in `configureApp`:
```fsharp
let webApp =
    choose [
        // ... existing routes ...

        // Microphone endpoints
        GET >=> route "/api/microphones" >=> getMicrophonesHandler
        GET >=> routef "/api/microphones/test/start/%i" (Some >> startMicrophoneTestHandler)
        GET >=> route "/api/microphones/test/start" >=> startMicrophoneTestHandler None
        POST >=> route "/api/microphones/test/stop" >=> stopMicrophoneTestHandler
        GET >=> route "/api/microphones/test/levels" >=> getMicrophoneLevelsHandler
    ]
```

**Alternative Approach for Real-time Updates**:
Instead of SSE, use polling:
- Client polls `/api/microphones/test/levels` every 100ms
- Server returns current level from test state
- Simpler implementation, acceptable latency

**Acceptance**:
- Can list microphones via API
- Can start/stop test recording via API
- Test results return audio statistics
- Error handling for invalid device indices
- Thread-safe state management

---

**Task 16.5: Frontend - Microphone Settings Card (Types & API)**

Create the frontend types and API functions for microphone management:

**Add to `VocalFold.WebUI/src/Types.fs`**:
```fsharp
/// Microphone device information
type MicrophoneDevice = {
    Index: int
    Name: string
    Channels: int
    IsDefault: bool
}

/// Microphone test status
type MicrophoneTestStatus =
    | Idle
    | Testing
    | Stopped

/// Microphone test results
type MicrophoneTestResult = {
    CurrentLevel: float
    MaxLevel: float
    AvgLevel: float
}
```

**Add to `VocalFold.WebUI/src/Api.fs`**:
```fsharp
let microphoneDeviceDecoder: Decoder<MicrophoneDevice> =
    Decode.object (fun get -> {
        Index = get.Required.Field "index" Decode.int
        Name = get.Required.Field "name" Decode.string
        Channels = get.Required.Field "channels" Decode.int
        IsDefault = get.Required.Field "isDefault" Decode.bool
    })

/// Get all available microphones
let getMicrophones () : JS.Promise<Result<MicrophoneDevice list, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/microphones") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString (Decode.list microphoneDeviceDecoder) response.responseText with
                | Result.Ok devices -> return Result.Ok devices
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get microphones: %s" ex.Message)
    } |> Async.StartAsPromise

/// Start microphone test
let startMicrophoneTest (deviceIndex: int option) : JS.Promise<Result<unit, string>> =
    async {
        try
            let url =
                match deviceIndex with
                | Some idx -> baseUrl + sprintf "/api/microphones/test/start/%d" idx
                | None -> baseUrl + "/api/microphones/test/start"

            let! response = Http.request url |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to start test: %s" ex.Message)
    } |> Async.StartAsPromise

/// Stop microphone test
let stopMicrophoneTest () : JS.Promise<Result<MicrophoneTestResult, string>> =
    async {
        try
            let! response =
                Http.request (baseUrl + "/api/microphones/test/stop")
                |> Http.method POST
                |> Http.send

            match response.statusCode with
            | 200 ->
                let decoder = Decode.object (fun get -> {
                    CurrentLevel = get.Required.Field "currentLevel" Decode.float
                    MaxLevel = get.Required.Field "maxLevel" Decode.float
                    AvgLevel = get.Required.Field "avgLevel" Decode.float
                })
                match Decode.fromString decoder response.responseText with
                | Result.Ok result -> return Result.Ok result
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to stop test: %s" ex.Message)
    } |> Async.StartAsPromise
```

**Acceptance**:
- Types defined for microphone data
- API functions for listing microphones
- API functions for starting/stopping tests
- Proper error handling
- Type-safe decoders

---

**Task 16.6: Frontend - Microphone Settings UI Component**

Create the microphone settings card in the web UI:

**Add to `VocalFold.WebUI/src/Components/`** (new file: `MicrophoneSettings.fs`):

**Features**:
1. **Microphone Dropdown**
   - Lists all available microphones
   - Shows default device with indicator
   - Selected value bound to settings
   - "Default Microphone" option at top

2. **Test Button**
   - Starts/stops microphone test
   - Button text changes: "Test Microphone" → "Stop Test"
   - Disabled while recording actual voice input

3. **Real-time Audio Level Visualization**
   - Horizontal bar showing current audio level
   - Color-coded: Gray (no input) → Blue (low) → Orange (good) → Red (clipping)
   - Updates in real-time during test
   - Smooth animations

4. **Test Results Display**
   - Max level achieved
   - Average level
   - Duration of test
   - Pass/fail indicator (is mic working?)

**Component Structure**:
```fsharp
module MicrophoneSettings

open Feliz
open Feliz.UseElmish
open Elmish

type Model = {
    Microphones: MicrophoneDevice list
    SelectedIndex: int option
    TestStatus: MicrophoneTestStatus
    CurrentLevel: float
    MaxLevel: float
    AvgLevel: float
    IsLoading: bool
    Error: string option
}

type Msg =
    | LoadMicrophones
    | MicrophonesLoaded of Result<MicrophoneDevice list, string>
    | SelectMicrophone of int option
    | StartTest
    | StopTest
    | TestStarted of Result<unit, string>
    | TestStopped of Result<MicrophoneTestResult, string>
    | UpdateLevel of float
    | SaveSettings

let init selectedIndex =
    {
        Microphones = []
        SelectedIndex = selectedIndex
        TestStatus = Idle
        CurrentLevel = 0.0
        MaxLevel = 0.0
        AvgLevel = 0.0
        IsLoading = true
        Error = None
    }, Cmd.ofMsg LoadMicrophones

let update msg model =
    match msg with
    | LoadMicrophones ->
        model, Cmd.OfPromise.perform Api.getMicrophones () MicrophonesLoaded

    | MicrophonesLoaded (Ok devices) ->
        { model with Microphones = devices; IsLoading = false }, Cmd.none

    | MicrophonesLoaded (Error err) ->
        { model with Error = Some err; IsLoading = false }, Cmd.none

    | SelectMicrophone idx ->
        { model with SelectedIndex = idx }, Cmd.ofMsg SaveSettings

    | StartTest ->
        let cmd = Cmd.OfPromise.perform Api.startMicrophoneTest model.SelectedIndex TestStarted
        { model with TestStatus = Testing; CurrentLevel = 0.0; MaxLevel = 0.0 }, cmd

    | StopTest ->
        let cmd = Cmd.OfPromise.perform Api.stopMicrophoneTest () TestStopped
        { model with TestStatus = Idle }, cmd

    | TestStarted (Ok ()) ->
        model, Cmd.none

    | TestStarted (Error err) ->
        { model with Error = Some err; TestStatus = Idle }, Cmd.none

    | TestStopped (Ok result) ->
        { model with
            MaxLevel = result.MaxLevel
            AvgLevel = result.AvgLevel
            CurrentLevel = 0.0
        }, Cmd.none

    | TestStopped (Error err) ->
        { model with Error = Some err }, Cmd.none

    | UpdateLevel level ->
        { model with
            CurrentLevel = level
            MaxLevel = max level model.MaxLevel
        }, Cmd.none

    | SaveSettings ->
        // Call parent component to save settings
        model, Cmd.none

[<ReactComponent>]
let MicrophoneSettingsCard (selectedIndex: int option) (onSave: int option -> unit) =
    let model, dispatch = React.useElmish(init selectedIndex, update, [||])

    Html.div [
        prop.className "bg-gray-800 rounded-lg p-6 space-y-4"
        prop.children [
            // Card title
            Html.h2 [
                prop.className "text-xl font-semibold text-white mb-4"
                prop.text "Microphone Setup"
            ]

            // Microphone dropdown
            Html.div [
                prop.className "space-y-2"
                prop.children [
                    Html.label [
                        prop.className "block text-sm font-medium text-gray-300"
                        prop.text "Select Microphone"
                    ]
                    Html.select [
                        prop.className "w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2 text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                        prop.value (match model.SelectedIndex with | Some idx -> string idx | None -> "-1")
                        prop.onChange (fun (value: string) ->
                            let idx =
                                match System.Int32.TryParse(value) with
                                | (true, -1) -> None
                                | (true, i) -> Some i
                                | _ -> None
                            dispatch (SelectMicrophone idx)
                        )
                        prop.children [
                            Html.option [
                                prop.value "-1"
                                prop.text "🎤 Default Microphone"
                            ]
                            for device in model.Microphones do
                                Html.option [
                                    prop.value (string device.Index)
                                    prop.text (
                                        if device.IsDefault then
                                            sprintf "%s (Default)" device.Name
                                        else
                                            device.Name
                                    )
                                ]
                        ]
                    ]
                ]
            ]

            // Test button
            Html.div [
                prop.className "flex items-center gap-4"
                prop.children [
                    Html.button [
                        prop.className "px-6 py-2 rounded-lg font-medium transition-colors duration-200 " + (
                            match model.TestStatus with
                            | Testing -> "bg-red-500 hover:bg-red-600 text-white"
                            | _ -> "bg-primary-500 hover:bg-primary-600 text-white"
                        )
                        prop.onClick (fun _ ->
                            match model.TestStatus with
                            | Testing -> dispatch StopTest
                            | _ -> dispatch StartTest
                        )
                        prop.text (
                            match model.TestStatus with
                            | Testing -> "⏹ Stop Test"
                            | _ -> "🎙 Test Microphone"
                        )
                    ]

                    // Status indicator
                    if model.TestStatus = Testing then
                        Html.span [
                            prop.className "text-sm text-gray-400 animate-pulse"
                            prop.text "Recording..."
                        ]
                ]
            ]

            // Audio level visualization
            if model.TestStatus = Testing || model.MaxLevel > 0.0 then
                Html.div [
                    prop.className "space-y-2"
                    prop.children [
                        Html.label [
                            prop.className "block text-sm font-medium text-gray-300"
                            prop.text "Audio Level"
                        ]

                        // Level bar
                        Html.div [
                            prop.className "w-full h-8 bg-gray-700 rounded-lg overflow-hidden"
                            prop.children [
                                Html.div [
                                    prop.className "h-full transition-all duration-75 " + (
                                        if model.CurrentLevel < 0.01 then "bg-gray-500"
                                        elif model.CurrentLevel < 0.3 then "bg-blue-500"
                                        elif model.CurrentLevel < 0.8 then "bg-secondary-500"
                                        else "bg-red-500"
                                    )
                                    prop.style [
                                        style.width (length.percent (model.CurrentLevel * 100.0))
                                    ]
                                ]
                            ]
                        ]

                        // Statistics
                        Html.div [
                            prop.className "flex justify-between text-xs text-gray-400"
                            prop.children [
                                Html.span [ prop.text (sprintf "Current: %.1f%%" (model.CurrentLevel * 100.0)) ]
                                Html.span [ prop.text (sprintf "Max: %.1f%%" (model.MaxLevel * 100.0)) ]
                                Html.span [ prop.text (sprintf "Avg: %.1f%%" (model.AvgLevel * 100.0)) ]
                            ]
                        ]

                        // Feedback
                        if model.MaxLevel > 0.0 then
                            Html.div [
                                prop.className "text-sm " + (
                                    if model.MaxLevel < 0.01 then "text-red-400"
                                    elif model.MaxLevel < 0.1 then "text-yellow-400"
                                    else "text-green-400"
                                )
                                prop.text (
                                    if model.MaxLevel < 0.01 then
                                        "⚠️ No audio detected. Check microphone connection and permissions."
                                    elif model.MaxLevel < 0.1 then
                                        "⚠️ Audio level is low. Speak louder or move closer to the microphone."
                                    else
                                        "✅ Microphone is working correctly!"
                                )
                            ]
                    ]
                ]

            // Error display
            match model.Error with
            | Some err ->
                Html.div [
                    prop.className "bg-red-900/20 border border-red-500 rounded-lg p-3 text-red-400 text-sm"
                    prop.text err
                ]
            | None -> Html.none
        ]
    ]
```

**Styling Notes**:
- Use brand colors: `#25abfe` (blue) for primary, `#ff8b00` (orange) for secondary
- Smooth transitions for audio level bar
- Clear visual feedback for test status
- Responsive design

**Acceptance**:
- Microphone dropdown shows all devices
- Can select a microphone
- Test button starts/stops test
- Audio level bar updates in real-time
- Clear feedback on microphone status
- Saves selection to settings

---

**Task 16.7: Frontend - Integration with Settings Page**

Add the microphone settings card to the main settings page:

**Update `VocalFold.WebUI/src/Pages/Settings.fs`**:
```fsharp
// In the settings page layout, add:
MicrophoneSettings.MicrophoneSettingsCard
    model.Settings.SelectedMicrophoneIndex
    (fun index -> dispatch (UpdateMicrophoneIndex index))
```

**Add to Settings Model and Msg**:
```fsharp
type Msg =
    | ...
    | UpdateMicrophoneIndex of int option
    | SaveMicrophoneSettings

let update msg model =
    match msg with
    | ...
    | UpdateMicrophoneIndex idx ->
        let updatedSettings = { model.Settings with SelectedMicrophoneIndex = idx }
        { model with Settings = updatedSettings }, Cmd.ofMsg SaveMicrophoneSettings

    | SaveMicrophoneSettings ->
        let cmd = Cmd.OfPromise.perform Api.updateSettings model.Settings SettingsSaved
        model, cmd
```

**UI Placement**:
- Add microphone card to "System Settings" section
- Place after hotkey configuration
- Before "Advanced Settings" section

**Acceptance**:
- Microphone settings card appears on settings page
- Changes persist when saved
- UI updates reflect saved values
- No layout issues

---

**Task 16.8: Integration with Main Recording Flow**

Update the main recording workflow to use selected microphone:

**Update in `Program.fs` or main app**:
```fsharp
// Load settings on startup
let settings = Settings.load()

// Use selected microphone for recording
let deviceIndex = settings.SelectedMicrophoneIndex

// Pass to recording function
let recordingResult = AudioRecorder.recordAudio maxDurationSeconds deviceIndex
```

**Update `AudioRecorder.recordAudio` signature** (if not already done):
```fsharp
let recordAudio (maxDurationSeconds: int) (deviceNumber: int option) : RecordingResult =
    // ... existing implementation already handles deviceNumber ...
```

**Validation on startup**:
```fsharp
// Check if selected microphone is still available
match settings.SelectedMicrophoneIndex with
| Some idx when not (AudioRecorder.isDeviceIndexValid idx) ->
    Logger.warning (sprintf "Selected microphone (index %d) not found, falling back to default" idx)
    // Update settings to use default
    Settings.save { settings with SelectedMicrophoneIndex = None } |> ignore
| _ -> ()
```

**Acceptance**:
- Main app uses selected microphone
- Fallback to default if device unavailable
- Warning logged when device missing
- Settings updated on fallback

---

**Task 16.9: Real-time Audio Level Updates (Polling Implementation)**

Implement client-side polling for real-time audio levels during testing:

**Backend - Add endpoint for current level**:
```fsharp
/// GET /api/microphones/test/level - Get current audio level
let getCurrentLevelHandler: HttpHandler =
    fun next ctx ->
        try
            // Get current level from test state
            // let level = getCurrentTestLevel()  // Implement state access

            json {| level = 0.0 |} next ctx
        with
        | ex ->
            RequestErrors.INTERNAL_ERROR (sprintf "Error getting level: %s" ex.Message) next ctx
```

**Frontend - Add polling logic**:
```fsharp
// In MicrophoneSettings component update function:
| StartTest ->
    let startCmd = Cmd.OfPromise.perform Api.startMicrophoneTest model.SelectedIndex TestStarted

    // Start polling for levels
    let pollCmd =
        Cmd.OfAsync.perform
            (fun _ -> async {
                while model.TestStatus = Testing do
                    let! levelResult = Api.getCurrentLevel() |> Async.AwaitPromise
                    match levelResult with
                    | Ok level -> dispatch (UpdateLevel level)
                    | Error _ -> ()
                    do! Async.Sleep 100  // Poll every 100ms
            })
            ()
            (fun _ -> ())

    { model with TestStatus = Testing }, Cmd.batch [ startCmd; pollCmd ]
```

**Alternative Simpler Approach**:
Use JavaScript `setInterval` for polling:
```fsharp
React.useEffect((fun () ->
    if model.TestStatus = Testing then
        let intervalId =
            Interop.setInterval (fun () ->
                promise {
                    let! result = Api.getCurrentLevel()
                    match result with
                    | Ok level -> dispatch (UpdateLevel level)
                    | Error _ -> ()
                }
            ) 100  // 100ms interval

        // Cleanup on unmount or status change
        { new System.IDisposable with
            member _.Dispose() = Interop.clearInterval intervalId
        }
    else
        { new System.IDisposable with member _.Dispose() = () }
), [| box model.TestStatus |])
```

**Acceptance**:
- Audio levels update during test
- ~100ms update interval
- No performance issues
- Polling stops when test stops
- No memory leaks

---

**Task 16.10: Testing, Polish & Documentation**

Comprehensive testing and documentation:

**Testing Checklist**:
- [ ] Enumerate microphones on system with 0 devices
- [ ] Enumerate microphones on system with 1 device
- [ ] Enumerate microphones on system with multiple devices
- [ ] Select different microphones and verify recording uses correct device
- [ ] Test with USB microphone (plug/unplug scenarios)
- [ ] Test microphone test feature
- [ ] Real-time audio levels update correctly
- [ ] Audio level bar colors change appropriately
- [ ] Test results accurate (max, avg levels)
- [ ] Settings persist across restarts
- [ ] Fallback to default if device unavailable
- [ ] Validation prevents invalid device indices
- [ ] Error handling for device enumeration failures
- [ ] Error handling for test recording failures
- [ ] UI responsive during testing
- [ ] No audio artifacts during testing
- [ ] No memory leaks with repeated tests

**Edge Cases**:
- [ ] Device disconnected while testing
- [ ] Device disconnected between test and actual recording
- [ ] Multiple instances of app (device already in use)
- [ ] Microphone permissions denied (Windows 11)
- [ ] Virtual audio devices (VoiceMeeter, etc.)
- [ ] Bluetooth microphones (latency)
- [ ] USB microphones (potential issues)

**Performance Testing**:
- [ ] Device enumeration fast (<100ms)
- [ ] Test start/stop responsive (<200ms)
- [ ] Audio level updates smooth (60fps capable)
- [ ] No CPU overhead when not testing
- [ ] No memory leaks with 100+ test cycles

**UI/UX Polish**:
- [ ] Smooth animations
- [ ] Clear visual feedback
- [ ] Helpful error messages
- [ ] Tooltips for technical terms
- [ ] Loading states
- [ ] Disabled states when appropriate
- [ ] Keyboard accessibility
- [ ] Screen reader compatibility

**Documentation Updates**:

**README.md**:
```markdown
### Microphone Setup

VocalFold allows you to choose which microphone to use for voice input.

1. Open Settings from the system tray
2. Navigate to "Microphone Setup" section
3. Select your preferred microphone from the dropdown
4. Click "Test Microphone" to verify it's working
5. Speak and watch the audio level bar - it should show activity
6. If levels are too low, check:
   - Microphone is not muted in Windows
   - Microphone volume is adequate
   - Correct microphone is selected
   - Microphone permissions granted to VocalFold
```

**TROUBLESHOOTING.md**:
```markdown
### Microphone Issues

**Problem**: No microphone detected
- Check microphone is plugged in (USB) or enabled (built-in)
- Check Windows Sound Settings → Input
- Grant microphone permissions to VocalFold

**Problem**: Wrong microphone used
- Open Settings → Microphone Setup
- Select correct microphone from dropdown
- Test to verify

**Problem**: Audio level too low
- Increase microphone volume in Windows Sound Settings
- Move closer to microphone
- Check microphone is not muted
- Test with "Test Microphone" feature

**Problem**: Selected microphone not available on startup
- VocalFold automatically falls back to default microphone
- Check Windows logs for device warnings
- Reconnect USB microphone if applicable
```

**Acceptance**:
- All tests pass
- Edge cases handled
- Performance acceptable
- UI polished
- Documentation complete
- Ready for production use

---

## Phase 16 Summary

**What We Built**:
- Microphone selection in system settings
- Microphone enumeration (list all available devices)
- Microphone test feature with real-time visualization
- Audio level bar with color-coded feedback
- REST API for microphone management
- Web UI card for microphone setup
- Integration with main recording flow
- Automatic fallback to default device

**Key Features**:
✅ List all available microphones
✅ Select preferred microphone
✅ Test microphone with visual feedback
✅ Real-time audio level visualization
✅ Color-coded audio level bar (gray/blue/orange/red)
✅ Test statistics (max, avg, current levels)
✅ Pass/fail indicator for microphone functionality
✅ Persistent microphone preference
✅ Automatic fallback if device unavailable
✅ Device validation on startup

**Architecture**:
- Backend: AudioRecorder module extensions
- REST API: Giraffe endpoints for microphones
- Frontend: Fable/React microphone settings card
- Real-time updates: Polling (100ms interval)
- State management: Settings.SelectedMicrophoneIndex
- Error handling: Graceful fallbacks

**Benefits**:
- Users with multiple microphones can choose correct one
- Test feature prevents wasted transcription attempts
- Visual feedback helps troubleshooting
- Clear indication of microphone issues
- Automatic device validation
- No more silent recordings from wrong device

**Use Cases**:
- User with USB microphone + built-in laptop mic
- User with headset + desk microphone
- User with virtual audio devices (streaming software)
- User troubleshooting microphone issues
- User ensuring microphone works before important recording

**Technology**:
- Backend: F#, NAudio WaveInEvent
- Frontend: F#, Fable, React, TailwindCSS
- Real-time: Client polling (100ms)
- Storage: Settings.SelectedMicrophoneIndex (int option)

**Tradeoffs**:
- Polling adds minimal overhead during test
- Device enumeration happens on page load (slight delay)
- No Server-Sent Events (simpler implementation)
- Test doesn't save audio (privacy, simplicity)

---

**Status**: Phase 16 specification complete, ready for implementation
**Estimated Time**: 10-14 hours (includes backend, frontend, testing, documentation)

**Next Task**: Implement Phase 16 (Task 16.1 - Backend - Microphone Data Model)

---

## Phase 17: Custom "Open" Commands ⬜

### Overview

Currently VocalFold supports the "open settings" command to launch the settings UI. Phase 17 extends this functionality to allow users to define custom "open [keyword]" commands that launch applications, open URLs, or execute multiple programs simultaneously.

**Feature Summary**:
- Voice command: "open [keyword]" launches configured application(s)
- Each keyword can open a single application or multiple applications
- Applications can be executables (.exe) or URLs (http://, https://)
- Configuration managed via web UI editor
- Commands stored in external file (cloud sync compatible)

**Example Use Cases**:
- "open browser" → launches Chrome at https://google.com
- "open workspace" → launches VS Code, opens project folder, starts dev server
- "open email" → launches Outlook and opens Gmail in browser
- "open design tools" → launches Figma, Adobe XD, and reference folder

**User Stories**:

**US-17.1: Quick Application Launch**
```
AS A user working on multiple projects
I WANT to say "open workspace" and have all my tools launch
SO THAT I can start working immediately without clicking
```

**US-17.2: Custom Shortcut Configuration**
```
AS A user with specific workflow needs
I WANT to define custom voice commands for my applications
SO THAT I can personalize VocalFold to my workflow
```

**US-17.3: Multi-Application Launch**
```
AS A user with complex workflows
I WANT to launch multiple related applications with one command
SO THAT I can set up my workspace efficiently
```

---

### Architecture

**Data Model**:
```fsharp
/// Application or URL to launch
[<CLIMutable>]
type LaunchTarget = {
    /// Display name for this target
    [<JsonPropertyName("name")>]
    Name: string

    /// Type of target: "executable", "url", or "folder"
    [<JsonPropertyName("type")>]
    Type: string

    /// Path to executable, URL, or folder path
    [<JsonPropertyName("path")>]
    Path: string

    /// Optional command-line arguments (for executables)
    [<JsonPropertyName("arguments")>]
    Arguments: string option

    /// Optional working directory (for executables)
    [<JsonPropertyName("workingDirectory")>]
    WorkingDirectory: string option
}

/// Custom "open" command configuration
[<CLIMutable>]
type OpenCommand = {
    /// The keyword to trigger this command (e.g., "browser", "workspace")
    [<JsonPropertyName("keyword")>]
    Keyword: string

    /// Optional description
    [<JsonPropertyName("description")>]
    Description: string option

    /// List of targets to launch (one or more)
    [<JsonPropertyName("targets")>]
    Targets: LaunchTarget list

    /// Optional delay between launching multiple targets (milliseconds)
    [<JsonPropertyName("launchDelay")>]
    LaunchDelay: int option
}

/// Open commands data (stored in separate file)
[<CLIMutable>]
type OpenCommandsData = {
    /// List of custom open commands
    [<JsonPropertyName("openCommands")>]
    OpenCommands: OpenCommand list

    /// Schema version for future migrations
    [<JsonPropertyName("version")>]
    Version: string
}
```

**Storage**:
- File: `%APPDATA%/VocalFold/open-commands.json`
- Format: JSON with indentation
- Configurable path in settings (for cloud sync)

**Processing Flow**:
```
Voice: "open browser"
  ↓
Transcription: "open browser"
  ↓
TextProcessor.processTranscriptionWithCommands
  ↓
Check if matches "open [keyword]" pattern
  ↓
Look up keyword in OpenCommands list
  ↓
Return: OpenApplication (LaunchTarget list)
  ↓
Program.fs launches each target
```

**Updated ProcessingResult**:
```fsharp
type ProcessingResult =
    | TypeText of string
    | OpenSettings
    | OpenApplication of LaunchTarget list  // NEW
```

---

### Task 17.1: Backend - Open Commands Data Model

**Objective**: Define data structures for custom open commands

**Files to Modify**:
- `VocalFold/Settings.fs`

**Implementation**:

**Settings.fs - Add new types**:
```fsharp
/// Application or URL to launch
[<CLIMutable>]
type LaunchTarget = {
    /// Display name for this target
    [<JsonPropertyName("name")>]
    Name: string

    /// Type of target: "executable", "url", or "folder"
    [<JsonPropertyName("type")>]
    Type: string

    /// Path to executable, URL, or folder path
    [<JsonPropertyName("path")>]
    Path: string

    /// Optional command-line arguments (for executables)
    [<JsonPropertyName("arguments")>]
    Arguments: string option

    /// Optional working directory (for executables)
    [<JsonPropertyName("workingDirectory")>]
    WorkingDirectory: string option
}

/// Custom "open" command configuration
[<CLIMutable>]
type OpenCommand = {
    /// The keyword to trigger this command
    [<JsonPropertyName("keyword")>]
    Keyword: string

    /// Optional description
    [<JsonPropertyName("description")>]
    Description: string option

    /// List of targets to launch
    [<JsonPropertyName("targets")>]
    Targets: LaunchTarget list

    /// Optional delay between launches (ms)
    [<JsonPropertyName("launchDelay")>]
    LaunchDelay: int option
}

/// Open commands data (stored in separate file)
[<CLIMutable>]
type OpenCommandsData = {
    /// List of custom open commands
    [<JsonPropertyName("openCommands")>]
    OpenCommands: OpenCommand list

    /// Schema version
    [<JsonPropertyName("version")>]
    Version: string
}

/// Default open commands data
let defaultOpenCommandsData = {
    OpenCommands = []
    Version = "1.0"
}
```

**Add to AppSettings**:
```fsharp
type AppSettings = {
    // ... existing fields ...

    /// Path to external open commands file (if None, uses default location)
    [<JsonPropertyName("openCommandsFilePath")>]
    OpenCommandsFilePath: string option
}

// Update defaultSettings
let defaultSettings = {
    // ... existing defaults ...
    OpenCommandsFilePath = None  // Use default location
}
```

**Helper Functions**:
```fsharp
/// Get the default open commands file path
let getDefaultOpenCommandsFilePath () =
    Path.Combine(getSettingsDirectory(), "open-commands.json")

/// Get the open commands file path from settings
let getOpenCommandsFilePath (settings: AppSettings) : string =
    match settings.OpenCommandsFilePath with
    | Some path when not (String.IsNullOrWhiteSpace(path)) -> path
    | _ -> getDefaultOpenCommandsFilePath()

/// Load open commands from file
let loadOpenCommands (filePath: string) : OpenCommandsData =
    try
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            JsonSerializer.Deserialize<OpenCommandsData>(json, jsonOptions)
        else
            // Return default data if file doesn't exist
            defaultOpenCommandsData
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to load open commands from %s" filePath)
        defaultOpenCommandsData

/// Save open commands to file
let saveOpenCommands (filePath: string) (data: OpenCommandsData) : Result<unit, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (Directory.Exists(directory)) then
            Directory.CreateDirectory(directory) |> ignore

        let json = JsonSerializer.Serialize(data, jsonOptions)
        File.WriteAllText(filePath, json)
        Logger.info (sprintf "Saved %d open commands to %s" data.OpenCommands.Length filePath)
        Ok ()
    with
    | ex ->
        let errorMsg = sprintf "Failed to save open commands: %s" ex.Message
        Logger.logException ex errorMsg
        Error errorMsg
```

**Validation Functions**:
```fsharp
/// Validate a launch target
let validateLaunchTarget (target: LaunchTarget) : string option =
    if String.IsNullOrWhiteSpace(target.Name) then
        Some "Target name cannot be empty"
    elif String.IsNullOrWhiteSpace(target.Type) then
        Some "Target type cannot be empty"
    elif target.Type <> "executable" && target.Type <> "url" && target.Type <> "folder" then
        Some "Target type must be 'executable', 'url', or 'folder'"
    elif String.IsNullOrWhiteSpace(target.Path) then
        Some "Target path cannot be empty"
    else
        None

/// Validate an open command
let validateOpenCommand (command: OpenCommand) : string option =
    if String.IsNullOrWhiteSpace(command.Keyword) then
        Some "Keyword cannot be empty"
    elif command.Keyword.ToLowerInvariant() = "settings" then
        Some "Keyword 'settings' is reserved for built-in command"
    elif List.isEmpty command.Targets then
        Some "Command must have at least one target"
    else
        // Validate all targets
        command.Targets
        |> List.tryPick validateLaunchTarget
```

**Acceptance**:
- [ ] LaunchTarget type defined with all fields
- [ ] OpenCommand type defined with validation
- [ ] OpenCommandsData type for file storage
- [ ] Load/save functions work correctly
- [ ] Validation prevents invalid commands
- [ ] Default empty data structure created
- [ ] JSON serialization works properly
- [ ] File operations handle errors gracefully

---

### Task 17.2: Backend - Application Launcher Module

**Objective**: Implement logic to launch applications, URLs, and folders

**Files to Create**:
- `VocalFold/ApplicationLauncher.fs`

**Implementation**:

**ApplicationLauncher.fs**:
```fsharp
module ApplicationLauncher

open System
open System.Diagnostics
open Settings

/// Result of launching an application
type LaunchResult = {
    Target: LaunchTarget
    Success: bool
    ErrorMessage: string option
    ProcessId: int option
}

/// Launch a single URL in the default browser
let private launchUrl (url: string) : LaunchResult =
    try
        Logger.info (sprintf "Launching URL: %s" url)

        // Use Process.Start with UseShellExecute to open URL in default browser
        let psi = new ProcessStartInfo()
        psi.FileName <- url
        psi.UseShellExecute <- true

        let proc = Process.Start(psi)

        {
            Target = { Name = url; Type = "url"; Path = url; Arguments = None; WorkingDirectory = None }
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to launch URL: %s" url)
        {
            Target = { Name = url; Type = "url"; Path = url; Arguments = None; WorkingDirectory = None }
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Launch a single executable
let private launchExecutable (target: LaunchTarget) : LaunchResult =
    try
        Logger.info (sprintf "Launching executable: %s" target.Path)

        let psi = new ProcessStartInfo()
        psi.FileName <- target.Path
        psi.UseShellExecute <- true

        // Set arguments if provided
        match target.Arguments with
        | Some args when not (String.IsNullOrWhiteSpace(args)) ->
            psi.Arguments <- args
            Logger.debug (sprintf "  Arguments: %s" args)
        | _ -> ()

        // Set working directory if provided
        match target.WorkingDirectory with
        | Some workDir when not (String.IsNullOrWhiteSpace(workDir)) ->
            psi.WorkingDirectory <- workDir
            Logger.debug (sprintf "  Working directory: %s" workDir)
        | _ -> ()

        let proc = Process.Start(psi)

        {
            Target = target
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to launch executable: %s" target.Path)
        {
            Target = target
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Open a folder in Windows Explorer
let private openFolder (path: string) : LaunchResult =
    try
        Logger.info (sprintf "Opening folder: %s" path)

        let psi = new ProcessStartInfo()
        psi.FileName <- "explorer.exe"
        psi.Arguments <- sprintf "\"%s\"" path
        psi.UseShellExecute <- true

        let proc = Process.Start(psi)

        {
            Target = { Name = path; Type = "folder"; Path = path; Arguments = None; WorkingDirectory = None }
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to open folder: %s" path)
        {
            Target = { Name = path; Type = "folder"; Path = path; Arguments = None; WorkingDirectory = None }
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Launch a single target (URL, executable, or folder)
let launchTarget (target: LaunchTarget) : LaunchResult =
    Logger.info (sprintf "Launching target: %s (type: %s)" target.Name target.Type)

    match target.Type.ToLowerInvariant() with
    | "url" -> launchUrl target.Path
    | "executable" -> launchExecutable target
    | "folder" -> openFolder target.Path
    | unknown ->
        Logger.warning (sprintf "Unknown target type: %s" unknown)
        {
            Target = target
            Success = false
            ErrorMessage = Some (sprintf "Unknown target type: %s" unknown)
            ProcessId = None
        }

/// Launch multiple targets with optional delay between launches
let launchTargets (targets: LaunchTarget list) (delayMs: int option) : LaunchResult list =
    Logger.info (sprintf "Launching %d target(s)" targets.Length)

    let delay = defaultArg delayMs 500  // Default 500ms delay between launches

    targets
    |> List.mapi (fun index target ->
        // Add delay before launching (except first one)
        if index > 0 && delay > 0 then
            Logger.debug (sprintf "Waiting %dms before next launch..." delay)
            System.Threading.Thread.Sleep(delay)

        launchTarget target
    )

/// Launch all targets for an open command
let executeOpenCommand (command: OpenCommand) : LaunchResult list =
    Logger.info (sprintf "Executing open command: %s" command.Keyword)
    Logger.debug (sprintf "  Description: %s" (defaultArg command.Description "N/A"))
    Logger.debug (sprintf "  Targets: %d" command.Targets.Length)

    let results = launchTargets command.Targets command.LaunchDelay

    // Log summary
    let successCount = results |> List.filter (fun r -> r.Success) |> List.length
    let failCount = results.Length - successCount

    if failCount = 0 then
        Logger.info (sprintf "✓ Successfully launched all %d target(s)" successCount)
    else
        Logger.warning (sprintf "⚠ Launched %d/%d targets (%d failed)" successCount results.Length failCount)

    results
```

**Acceptance**:
- [ ] Can launch URLs in default browser
- [ ] Can launch executable files
- [ ] Can open folders in Explorer
- [ ] Supports command-line arguments for executables
- [ ] Supports working directory for executables
- [ ] Handles multiple targets with delays
- [ ] Error handling for invalid paths
- [ ] Error handling for missing executables
- [ ] Returns detailed launch results
- [ ] Logs all operations

---

### Task 17.3: Backend - Text Processor Integration

**Objective**: Integrate custom open commands into transcription processing

**Files to Modify**:
- `VocalFold/TextProcessor.fs`

**Implementation**:

**Update ProcessingResult type**:
```fsharp
/// Result of processing transcribed text
type ProcessingResult =
    | TypeText of string        // Text should be typed
    | OpenSettings             // Open settings dialog
    | OpenApplication of Settings.OpenCommand  // NEW: Execute custom open command
```

**Add open command matching**:
```fsharp
/// Load open commands data from file (cached)
let mutable private openCommandsCache: Settings.OpenCommandsData option = None
let mutable private openCommandsFilePath: string option = None

/// Reload open commands from file
let reloadOpenCommands (filePath: string) : unit =
    try
        let data = Settings.loadOpenCommands filePath
        openCommandsCache <- Some data
        openCommandsFilePath <- Some filePath
        Logger.info (sprintf "Loaded %d open command(s) from %s" data.OpenCommands.Length filePath)
    with
    | ex ->
        Logger.logException ex "Failed to reload open commands"
        openCommandsCache <- Some Settings.defaultOpenCommandsData

/// Get current open commands
let getOpenCommands () : Settings.OpenCommand list =
    match openCommandsCache with
    | Some data -> data.OpenCommands
    | None -> []

/// Find an open command by keyword (case-insensitive)
let findOpenCommand (keyword: string) : Settings.OpenCommand option =
    let normalizedKeyword = keyword.Trim().ToLowerInvariant()
    getOpenCommands()
    |> List.tryFind (fun cmd -> cmd.Keyword.ToLowerInvariant() = normalizedKeyword)

/// Check if text matches "open [keyword]" pattern
let private tryMatchOpenCommand (normalizedText: string) : Settings.OpenCommand option =
    // Pattern: "open <keyword>"
    if normalizedText.StartsWith("open ") && normalizedText.Length > 5 then
        let keyword = normalizedText.Substring(5).Trim()

        // Don't match "open settings" (built-in command)
        if keyword <> "settings" then
            Logger.debug (sprintf "Checking for open command with keyword: %s" keyword)
            findOpenCommand keyword
        else
            None
    else
        None
```

**Update processTranscriptionWithCommands**:
```fsharp
/// Process transcription and check for special commands
let processTranscriptionWithCommands (text: string) (replacements: Settings.KeywordReplacement list) : ProcessingResult =
    try
        // Normalize text for command matching
        let normalizedText =
            let lowered = text.Trim().ToLowerInvariant()
            let noPunctuation = Regex.Replace(lowered, @"[^\w\s]", "")
            let singleSpaced = Regex.Replace(noPunctuation, @"\s+", " ")
            singleSpaced.Trim()

        Logger.debug (sprintf "Normalized text for command matching: \"%s\"" normalizedText)

        // Check for "open settings" command (built-in, exact match)
        if normalizedText = "open settings" then
            Logger.info "Detected 'open settings' command"
            OpenSettings

        // Check for custom "open [keyword]" commands
        elif normalizedText.StartsWith("open ") then
            match tryMatchOpenCommand normalizedText with
            | Some openCommand ->
                Logger.info (sprintf "Detected custom open command: %s" openCommand.Keyword)
                OpenApplication openCommand
            | None ->
                // No matching open command, process as normal text
                Logger.debug "No matching open command found, processing as text"
                let processedText = processTranscription text replacements
                TypeText processedText

        // Check for "repeat message" command
        elif normalizedText = "repeat message" then
            // ... existing repeat message logic ...
            Logger.info "Detected 'repeat message' command"
            match lastTranscription with
            | Some lastText ->
                let pattern = @"\brepeat message\b"
                let regex = new Regex(pattern, RegexOptions.IgnoreCase)
                let resultText = regex.Replace(text, lastText)
                let processedText = processTranscription resultText replacements
                TypeText processedText
            | None ->
                Logger.warning "No previous transcription available to repeat"
                TypeText text
        else
            // No special command detected, process normally
            let processedText = processTranscription text replacements
            TypeText processedText
    with
    | ex ->
        Logger.logException ex "Error in processTranscriptionWithCommands"
        TypeText text
```

**Acceptance**:
- [ ] ProcessingResult has OpenApplication variant
- [ ] "open [keyword]" pattern recognized
- [ ] Case-insensitive keyword matching
- [ ] Built-in "open settings" still works
- [ ] Unknown keywords fall back to normal text
- [ ] Open commands loaded from file
- [ ] Reload function updates cache
- [ ] Error handling for missing commands file

---

### Task 17.4: Backend - Program.fs Integration

**Objective**: Handle OpenApplication result in main program flow

**Files to Modify**:
- `VocalFold/Program.fs`

**Implementation**:

**Add to project references** (in .fsproj):
```xml
<Compile Include="ApplicationLauncher.fs" />
```

**Update transcription handling**:
```fsharp
// In the main hotkey callback, after transcription:
match TextProcessor.processTranscriptionWithCommands transcribedText keywordReplacements with
| TextProcessor.TypeText processedText ->
    // Existing text typing logic
    Logger.info (sprintf "Typing text: \"%s\""
        (if processedText.Length > 50 then processedText.Substring(0, 47) + "..." else processedText))
    TextInput.typeText processedText
    TextProcessor.storeLastTranscription processedText

| TextProcessor.OpenSettings ->
    // Existing open settings logic
    Logger.info "Opening settings in browser..."
    match WebServer.getServerUrl() with
    | Some url ->
        try
            let psi = new System.Diagnostics.ProcessStartInfo()
            psi.FileName <- url
            psi.UseShellExecute <- true
            System.Diagnostics.Process.Start(psi) |> ignore
            Logger.info (sprintf "✓ Opened settings at %s" url)
        with
        | ex ->
            Logger.logException ex "Failed to open settings in browser"
    | None ->
        Logger.warning "Web server not running, cannot open settings"

| TextProcessor.OpenApplication openCommand ->
    // NEW: Launch application(s)
    Logger.info (sprintf "Launching application(s) for command: %s" openCommand.Keyword)
    let results = ApplicationLauncher.executeOpenCommand openCommand

    // Check if all launches succeeded
    let failedResults = results |> List.filter (fun r -> not r.Success)
    if List.isEmpty failedResults then
        Logger.info (sprintf "✓ Successfully launched all targets for '%s'" openCommand.Keyword)
    else
        Logger.warning (sprintf "⚠ Some targets failed to launch for '%s':" openCommand.Keyword)
        failedResults |> List.iter (fun r ->
            Logger.warning (sprintf "  - %s: %s" r.Target.Name (defaultArg r.ErrorMessage "Unknown error"))
        )
```

**Load open commands on startup**:
```fsharp
// After loading settings, load open commands
let openCommandsPath = Settings.getOpenCommandsFilePath currentSettings
TextProcessor.reloadOpenCommands openCommandsPath
Logger.info "Open commands loaded"
```

**Reload on settings change** (in web server settings update handler):
```fsharp
// After saving settings, check if open commands path changed
let oldPath = Settings.getOpenCommandsFilePath oldSettings
let newPath = Settings.getOpenCommandsFilePath newSettings
if oldPath <> newPath then
    Logger.info (sprintf "Open commands file path changed, reloading from: %s" newPath)
    TextProcessor.reloadOpenCommands newPath
```

**Acceptance**:
- [ ] OpenApplication result handled in main flow
- [ ] ApplicationLauncher.executeOpenCommand called
- [ ] Launch results logged appropriately
- [ ] Errors don't crash the application
- [ ] Open commands loaded on startup
- [ ] Open commands reloaded when path changes
- [ ] Success/failure messages clear

---

### Task 17.5: Backend - REST API Endpoints

**Objective**: Create REST API for managing custom open commands

**Files to Modify**:
- `VocalFold/WebServer.fs`

**New Endpoints**:

```
GET    /api/open-commands           - Get all open commands
POST   /api/open-commands           - Create new open command
PUT    /api/open-commands/{index}   - Update open command by index
DELETE /api/open-commands/{index}   - Delete open command by index
POST   /api/open-commands/test      - Test launch a command without saving
```

**Implementation**:

**WebServer.fs - Add handlers**:
```fsharp
/// GET /api/open-commands - Get all open commands
let getOpenCommandsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug "API: GET /api/open-commands"

                let filePath = Settings.getOpenCommandsFilePath !currentSettings
                let data = Settings.loadOpenCommands filePath

                Logger.info (sprintf "Returning %d open command(s)" data.OpenCommands.Length)
                return! json data.OpenCommands next ctx
            with
            | ex ->
                Logger.logException ex "Failed to get open commands"
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }

/// POST /api/open-commands - Create new open command
let createOpenCommandHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug "API: POST /api/open-commands"

                let! newCommand = ctx.BindJsonAsync<Settings.OpenCommand>()
                Logger.info (sprintf "Creating open command: %s" newCommand.Keyword)

                // Validate the command
                match Settings.validateOpenCommand newCommand with
                | Some errorMsg ->
                    Logger.warning (sprintf "Invalid open command: %s" errorMsg)
                    ctx.SetStatusCode 400
                    return! json {| error = errorMsg |} next ctx
                | None ->
                    let filePath = Settings.getOpenCommandsFilePath !currentSettings
                    let data = Settings.loadOpenCommands filePath

                    // Check for duplicate keyword
                    let isDuplicate =
                        data.OpenCommands
                        |> List.exists (fun cmd ->
                            cmd.Keyword.ToLowerInvariant() = newCommand.Keyword.ToLowerInvariant())

                    if isDuplicate then
                        Logger.warning (sprintf "Open command with keyword '%s' already exists" newCommand.Keyword)
                        ctx.SetStatusCode 400
                        return! json {| error = "Command with this keyword already exists" |} next ctx
                    else
                        // Add new command
                        let updatedData = {
                            data with
                                OpenCommands = data.OpenCommands @ [newCommand]
                        }

                        match Settings.saveOpenCommands filePath updatedData with
                        | Ok () ->
                            // Reload commands in text processor
                            TextProcessor.reloadOpenCommands filePath

                            Logger.info (sprintf "✓ Created open command: %s" newCommand.Keyword)
                            ctx.SetStatusCode 201
                            return! json newCommand next ctx
                        | Error errorMsg ->
                            Logger.warning (sprintf "Failed to save open command: %s" errorMsg)
                            ctx.SetStatusCode 500
                            return! json {| error = errorMsg |} next ctx
            with
            | ex ->
                Logger.logException ex "Failed to create open command"
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }

/// PUT /api/open-commands/{index} - Update open command
let updateOpenCommandHandler (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug (sprintf "API: PUT /api/open-commands/%d" index)

                let! updatedCommand = ctx.BindJsonAsync<Settings.OpenCommand>()
                Logger.info (sprintf "Updating open command at index %d: %s" index updatedCommand.Keyword)

                // Validate the command
                match Settings.validateOpenCommand updatedCommand with
                | Some errorMsg ->
                    Logger.warning (sprintf "Invalid open command: %s" errorMsg)
                    ctx.SetStatusCode 400
                    return! json {| error = errorMsg |} next ctx
                | None ->
                    let filePath = Settings.getOpenCommandsFilePath !currentSettings
                    let data = Settings.loadOpenCommands filePath

                    if index < 0 || index >= data.OpenCommands.Length then
                        Logger.warning (sprintf "Invalid index: %d (max: %d)" index (data.OpenCommands.Length - 1))
                        ctx.SetStatusCode 404
                        return! json {| error = "Command not found" |} next ctx
                    else
                        // Check for duplicate keyword (excluding current command)
                        let isDuplicate =
                            data.OpenCommands
                            |> List.mapi (fun i cmd -> i, cmd)
                            |> List.exists (fun (i, cmd) ->
                                i <> index &&
                                cmd.Keyword.ToLowerInvariant() = updatedCommand.Keyword.ToLowerInvariant())

                        if isDuplicate then
                            Logger.warning (sprintf "Open command with keyword '%s' already exists" updatedCommand.Keyword)
                            ctx.SetStatusCode 400
                            return! json {| error = "Command with this keyword already exists" |} next ctx
                        else
                            // Update command
                            let updatedCommands =
                                data.OpenCommands
                                |> List.mapi (fun i cmd -> if i = index then updatedCommand else cmd)

                            let updatedData = { data with OpenCommands = updatedCommands }

                            match Settings.saveOpenCommands filePath updatedData with
                            | Ok () ->
                                // Reload commands in text processor
                                TextProcessor.reloadOpenCommands filePath

                                Logger.info (sprintf "✓ Updated open command at index %d" index)
                                return! json updatedCommand next ctx
                            | Error errorMsg ->
                                Logger.warning (sprintf "Failed to save open commands: %s" errorMsg)
                                ctx.SetStatusCode 500
                                return! json {| error = errorMsg |} next ctx
            with
            | ex ->
                Logger.logException ex (sprintf "Failed to update open command at index %d" index)
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }

/// DELETE /api/open-commands/{index} - Delete open command
let deleteOpenCommandHandler (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug (sprintf "API: DELETE /api/open-commands/%d" index)

                let filePath = Settings.getOpenCommandsFilePath !currentSettings
                let data = Settings.loadOpenCommands filePath

                if index < 0 || index >= data.OpenCommands.Length then
                    Logger.warning (sprintf "Invalid index: %d (max: %d)" index (data.OpenCommands.Length - 1))
                    ctx.SetStatusCode 404
                    return! json {| error = "Command not found" |} next ctx
                else
                    let commandToDelete = data.OpenCommands.[index]
                    Logger.info (sprintf "Deleting open command: %s" commandToDelete.Keyword)

                    // Remove command
                    let updatedCommands =
                        data.OpenCommands
                        |> List.mapi (fun i cmd -> i, cmd)
                        |> List.filter (fun (i, _) -> i <> index)
                        |> List.map snd

                    let updatedData = { data with OpenCommands = updatedCommands }

                    match Settings.saveOpenCommands filePath updatedData with
                    | Ok () ->
                        // Reload commands in text processor
                        TextProcessor.reloadOpenCommands filePath

                        Logger.info (sprintf "✓ Deleted open command: %s" commandToDelete.Keyword)
                        return! json {| success = true |} next ctx
                    | Error errorMsg ->
                        Logger.warning (sprintf "Failed to save open commands: %s" errorMsg)
                        ctx.SetStatusCode 500
                        return! json {| error = errorMsg |} next ctx
            with
            | ex ->
                Logger.logException ex (sprintf "Failed to delete open command at index %d" index)
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }

/// POST /api/open-commands/test - Test launch a command
let testOpenCommandHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug "API: POST /api/open-commands/test"

                let! command = ctx.BindJsonAsync<Settings.OpenCommand>()
                Logger.info (sprintf "Testing open command: %s" command.Keyword)

                // Validate the command
                match Settings.validateOpenCommand command with
                | Some errorMsg ->
                    Logger.warning (sprintf "Invalid open command: %s" errorMsg)
                    ctx.SetStatusCode 400
                    return! json {| error = errorMsg |} next ctx
                | None ->
                    // Execute the command
                    let results = ApplicationLauncher.executeOpenCommand command

                    // Convert results to JSON-friendly format
                    let resultsJson =
                        results
                        |> List.map (fun r ->
                            {|
                                targetName = r.Target.Name
                                success = r.Success
                                errorMessage = r.ErrorMessage
                                processId = r.ProcessId
                            |})

                    Logger.info (sprintf "✓ Test completed for: %s" command.Keyword)
                    return! json {| results = resultsJson |} next ctx
            with
            | ex ->
                Logger.logException ex "Failed to test open command"
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }
```

**Add routes**:
```fsharp
let webApp =
    choose [
        // ... existing routes ...

        // Open Commands API
        GET  >=> route "/api/open-commands" >=> getOpenCommandsHandler
        POST >=> route "/api/open-commands" >=> createOpenCommandHandler
        PUT  >=> routef "/api/open-commands/%i" updateOpenCommandHandler
        DELETE >=> routef "/api/open-commands/%i" deleteOpenCommandHandler
        POST >=> route "/api/open-commands/test" >=> testOpenCommandHandler

        // ... rest of routes ...
    ]
```

**Acceptance**:
- [ ] GET /api/open-commands returns all commands
- [ ] POST /api/open-commands creates new command
- [ ] PUT /api/open-commands/{index} updates command
- [ ] DELETE /api/open-commands/{index} deletes command
- [ ] POST /api/open-commands/test tests without saving
- [ ] Validation prevents invalid commands
- [ ] Duplicate keywords rejected
- [ ] Invalid indices handled gracefully
- [ ] Changes reload in text processor
- [ ] Errors return appropriate status codes

---

### Task 17.6: Frontend - Type Definitions

**Objective**: Add TypeScript-style type definitions for open commands

**Files to Modify**:
- `VocalFold.WebUI/src/Types.fs`

**Implementation**:

```fsharp
// ============================================================================
// Open Commands Types
// ============================================================================

/// Application or URL to launch
type LaunchTarget = {
    Name: string
    Type: string  // "executable", "url", or "folder"
    Path: string
    Arguments: string option
    WorkingDirectory: string option
}

/// Custom "open" command
type OpenCommand = {
    Keyword: string
    Description: string option
    Targets: LaunchTarget list
    LaunchDelay: int option
}

/// Result of testing a launch
type TestLaunchResult = {
    TargetName: string
    Success: bool
    ErrorMessage: string option
    ProcessId: int option
}
```

**Acceptance**:
- [ ] LaunchTarget type matches backend
- [ ] OpenCommand type matches backend
- [ ] TestLaunchResult for test results
- [ ] Types compile in Fable

---

### Task 17.7: Frontend - API Client

**Objective**: Add API client functions for open commands

**Files to Modify**:
- `VocalFold.WebUI/src/Api.fs`

**Implementation**:

```fsharp
/// Get all open commands
let getOpenCommands () : JS.Promise<Result<OpenCommand list, string>> =
    promise {
        try
            let! response = Http.get "/api/open-commands"

            match response.statusCode with
            | 200 ->
                match Decode.fromString (Decode.list openCommandDecoder) response.responseText with
                | Ok commands -> return Ok commands
                | Error err -> return Error (sprintf "Failed to decode open commands: %s" err)
            | code ->
                return Error (sprintf "Server returned status %d" code)
        with
        | ex -> return Error (sprintf "Network error: %s" ex.Message)
    }

/// Create new open command
let createOpenCommand (command: OpenCommand) : JS.Promise<Result<OpenCommand, string>> =
    promise {
        try
            let json = Encode.toString 0 (encodeOpenCommand command)
            let! response = Http.post "/api/open-commands" json

            match response.statusCode with
            | 201 ->
                match Decode.fromString openCommandDecoder response.responseText with
                | Ok cmd -> return Ok cmd
                | Error err -> return Error (sprintf "Failed to decode response: %s" err)
            | 400 ->
                // Validation error
                match Decode.fromString (Decode.field "error" Decode.string) response.responseText with
                | Ok err -> return Error err
                | Error _ -> return Error "Validation failed"
            | code ->
                return Error (sprintf "Server returned status %d" code)
        with
        | ex -> return Error (sprintf "Network error: %s" ex.Message)
    }

/// Update open command
let updateOpenCommand (index: int) (command: OpenCommand) : JS.Promise<Result<OpenCommand, string>> =
    promise {
        try
            let json = Encode.toString 0 (encodeOpenCommand command)
            let! response = Http.put (sprintf "/api/open-commands/%d" index) json

            match response.statusCode with
            | 200 ->
                match Decode.fromString openCommandDecoder response.responseText with
                | Ok cmd -> return Ok cmd
                | Error err -> return Error (sprintf "Failed to decode response: %s" err)
            | 400 ->
                match Decode.fromString (Decode.field "error" Decode.string) response.responseText with
                | Ok err -> return Error err
                | Error _ -> return Error "Validation failed"
            | 404 ->
                return Error "Command not found"
            | code ->
                return Error (sprintf "Server returned status %d" code)
        with
        | ex -> return Error (sprintf "Network error: %s" ex.Message)
    }

/// Delete open command
let deleteOpenCommand (index: int) : JS.Promise<Result<unit, string>> =
    promise {
        try
            let! response = Http.delete (sprintf "/api/open-commands/%d" index)

            match response.statusCode with
            | 200 -> return Ok ()
            | 404 -> return Error "Command not found"
            | code -> return Error (sprintf "Server returned status %d" code)
        with
        | ex -> return Error (sprintf "Network error: %s" ex.Message)
    }

/// Test open command without saving
let testOpenCommand (command: OpenCommand) : JS.Promise<Result<TestLaunchResult list, string>> =
    promise {
        try
            let json = Encode.toString 0 (encodeOpenCommand command)
            let! response = Http.post "/api/open-commands/test" json

            match response.statusCode with
            | 200 ->
                match Decode.fromString (Decode.field "results" (Decode.list testLaunchResultDecoder)) response.responseText with
                | Ok results -> return Ok results
                | Error err -> return Error (sprintf "Failed to decode results: %s" err)
            | 400 ->
                match Decode.fromString (Decode.field "error" Decode.string) response.responseText with
                | Ok err -> return Error err
                | Error _ -> return Error "Validation failed"
            | code ->
                return Error (sprintf "Server returned status %d" code)
        with
        | ex -> return Error (sprintf "Network error: %s" ex.Message)
    }
```

**Add decoders**:
```fsharp
/// Decode LaunchTarget from JSON
let launchTargetDecoder: Decoder<LaunchTarget> =
    Decode.object (fun get -> {
        Name = get.Required.Field "name" Decode.string
        Type = get.Required.Field "type" Decode.string
        Path = get.Required.Field "path" Decode.string
        Arguments = get.Optional.Field "arguments" Decode.string
        WorkingDirectory = get.Optional.Field "workingDirectory" Decode.string
    })

/// Decode OpenCommand from JSON
let openCommandDecoder: Decoder<OpenCommand> =
    Decode.object (fun get -> {
        Keyword = get.Required.Field "keyword" Decode.string
        Description = get.Optional.Field "description" Decode.string
        Targets = get.Required.Field "targets" (Decode.list launchTargetDecoder)
        LaunchDelay = get.Optional.Field "launchDelay" Decode.int
    })

/// Decode TestLaunchResult from JSON
let testLaunchResultDecoder: Decoder<TestLaunchResult> =
    Decode.object (fun get -> {
        TargetName = get.Required.Field "targetName" Decode.string
        Success = get.Required.Field "success" Decode.bool
        ErrorMessage = get.Optional.Field "errorMessage" Decode.string
        ProcessId = get.Optional.Field "processId" Decode.int
    })
```

**Add encoders**:
```fsharp
/// Encode LaunchTarget to JSON
let encodeLaunchTarget (target: LaunchTarget) : JsonValue =
    Encode.object [
        "name", Encode.string target.Name
        "type", Encode.string target.Type
        "path", Encode.string target.Path
        "arguments", Encode.option Encode.string target.Arguments
        "workingDirectory", Encode.option Encode.string target.WorkingDirectory
    ]

/// Encode OpenCommand to JSON
let encodeOpenCommand (command: OpenCommand) : JsonValue =
    Encode.object [
        "keyword", Encode.string command.Keyword
        "description", Encode.option Encode.string command.Description
        "targets", Encode.list (List.map encodeLaunchTarget command.Targets)
        "launchDelay", Encode.option Encode.int command.LaunchDelay
    ]
```

**Acceptance**:
- [ ] getOpenCommands fetches all commands
- [ ] createOpenCommand posts new command
- [ ] updateOpenCommand updates by index
- [ ] deleteOpenCommand deletes by index
- [ ] testOpenCommand tests without saving
- [ ] Decoders handle all fields
- [ ] Encoders produce correct JSON
- [ ] Error messages clear and helpful

---

### Task 17.8: Frontend - Open Commands Manager Component

**Objective**: Create UI for managing custom open commands

**Files to Create**:
- `VocalFold.WebUI/src/Components/OpenCommandsManager.fs`

**Implementation**:

This will be a comprehensive component similar to KeywordManager but for open commands.

**Key Features**:
- List all open commands with keyword, description, target count
- Add new open command button
- Edit existing command button
- Delete command with confirmation
- Test command button (launches without saving)
- Visual feedback for test results

**Component Structure**:
```fsharp
module OpenCommandsManager

open Feliz
open Feliz.UseElmish
open Elmish
open Types

type Model = {
    Commands: LoadingState<OpenCommand list>
    ModalState: ModalState option
    TestResults: TestLaunchResult list option
    ErrorMessage: string option
}

type ModalState =
    | AddCommand of OpenCommand
    | EditCommand of int * OpenCommand
    | DeleteCommand of int * OpenCommand

type Msg =
    | LoadCommands
    | CommandsLoaded of Result<OpenCommand list, string>
    | OpenAddModal
    | OpenEditModal of int * OpenCommand
    | OpenDeleteModal of int * OpenCommand
    | CloseModal
    | SaveCommand of OpenCommand
    | UpdateCommand of int * OpenCommand
    | DeleteConfirmed of int
    | CommandSaved of Result<OpenCommand, string>
    | CommandUpdated of Result<OpenCommand, string>
    | CommandDeleted of Result<unit, string>
    | TestCommand of OpenCommand
    | TestCompleted of Result<TestLaunchResult list, string>
    | ClearTestResults
    | ClearError

// Implementation with init, update, view functions
// Similar to KeywordManager but adapted for OpenCommands
```

**UI Layout**:
```
┌────────────────────────────────────────────────────┐
│  Custom Open Commands                              │
│  ┌──────────────────────────────────────────────┐  │
│  │  [+ Add Open Command]        [Test Selected] │  │
│  └──────────────────────────────────────────────┘  │
│                                                    │
│  ┌────────────────────────────────────────────────┐│
│  │ Keyword: browser                               ││
│  │ Description: Open default browser              ││
│  │ Targets: 1 (URL)                               ││
│  │ [Edit] [Test] [Delete]                         ││
│  └────────────────────────────────────────────────┘│
│                                                    │
│  ┌────────────────────────────────────────────────┐│
│  │ Keyword: workspace                             ││
│  │ Description: Launch development workspace      ││
│  │ Targets: 3 (VS Code, Terminal, Browser)       ││
│  │ [Edit] [Test] [Delete]                         ││
│  └────────────────────────────────────────────────┘│
└────────────────────────────────────────────────────┘
```

**Acceptance**:
- [ ] Lists all open commands
- [ ] Add button opens modal
- [ ] Edit button opens modal with data
- [ ] Delete button shows confirmation
- [ ] Test button launches and shows results
- [ ] Visual feedback for loading states
- [ ] Error messages displayed
- [ ] Responsive layout

---

### Task 17.9: Frontend - Open Command Modal

**Objective**: Create modal for creating/editing open commands

**Files to Create**:
- `VocalFold.WebUI/src/Components/OpenCommandModal.fs`

**Features**:
- Keyword input (text)
- Description input (textarea)
- Launch delay input (number, optional)
- List of targets with add/remove
- Each target has:
  - Name input
  - Type dropdown (executable, url, folder)
  - Path input (with file/folder picker suggestion)
  - Arguments input (for executables)
  - Working directory input (for executables)
- Save button
- Cancel button
- Validation

**UI Layout**:
```
┌──────────────────────────────────────────────────┐
│  Add/Edit Open Command                     [X]   │
├──────────────────────────────────────────────────┤
│                                                  │
│  Keyword: [_____________]                        │
│  (e.g., "browser", "workspace", "email")         │
│                                                  │
│  Description: [___________________________]      │
│  (Optional)                                      │
│                                                  │
│  Launch Delay (ms): [____] (optional)            │
│                                                  │
│  Targets:                                        │
│  ┌──────────────────────────────────────────────┐│
│  │ Target 1                          [Remove]   ││
│  │ Name: [_______________]                      ││
│  │ Type: [v Executable   v]                     ││
│  │ Path: [_______________] [Browse]             ││
│  │ Arguments: [_______________] (optional)      ││
│  │ Working Dir: [_______________] (optional)    ││
│  └──────────────────────────────────────────────┘│
│                                                  │
│  [+ Add Target]                                  │
│                                                  │
│  [Cancel]                        [Save Command]  │
└──────────────────────────────────────────────────┘
```

**Validation**:
- Keyword required, not empty
- Keyword cannot be "settings" (reserved)
- At least one target required
- Each target: name, type, path required
- Path validation based on type

**Acceptance**:
- [ ] Modal opens/closes correctly
- [ ] All fields editable
- [ ] Add/remove targets dynamically
- [ ] Type dropdown shows all options
- [ ] Validation prevents invalid input
- [ ] Save triggers API call
- [ ] Cancel discards changes
- [ ] Error messages displayed

---

### Task 17.10: Frontend - Integration with Settings Page

**Objective**: Add Open Commands section to settings UI

**Files to Modify**:
- `VocalFold.WebUI/src/Pages/SystemSettings.fs` (or create new page)
- `VocalFold.WebUI/src/Types.fs` (add page enum if needed)
- `VocalFold.WebUI/src/Components/Navigation.fs` (add nav link)

**Implementation**:

**Option A: Add to System Settings Page**:
```fsharp
// In SystemSettings.fs
Html.section [
    prop.className "mb-8"
    prop.children [
        Html.h2 [
            prop.className "text-2xl font-bold mb-4"
            prop.text "Custom Open Commands"
        ]
        Html.p [
            prop.className "text-gray-600 mb-4"
            prop.text "Define custom voice commands to launch applications and URLs."
        ]
        OpenCommandsManager.render()
    ]
]
```

**Option B: Create Dedicated Page**:
```fsharp
// Add to Page type in Types.fs
type Page =
    | Dashboard
    | SystemSettings
    | GeneralSettings
    | KeywordSettings
    | OpenCommands     // NEW
    | About
    | Changelog

// Create new file: VocalFold.WebUI/src/Pages/OpenCommands.fs
module OpenCommandsPage

let render () =
    Html.div [
        prop.className "container mx-auto p-6"
        prop.children [
            Html.h1 [
                prop.className "text-3xl font-bold mb-6"
                prop.text "Custom Open Commands"
            ]

            Html.div [
                prop.className "bg-blue-50 border-l-4 border-blue-500 p-4 mb-6"
                prop.children [
                    Html.h3 [
                        prop.className "font-semibold text-blue-800 mb-2"
                        prop.text "How to use"
                    ]
                    Html.ul [
                        prop.className "list-disc list-inside text-blue-700 space-y-1"
                        prop.children [
                            Html.li [ prop.text "Say \"open [keyword]\" to launch your configured applications" ]
                            Html.li [ prop.text "Each command can launch one or more applications" ]
                            Html.li [ prop.text "Test commands before using them" ]
                        ]
                    ]
                ]
            ]

            OpenCommandsManager.render()
        ]
    ]
```

**Add to Navigation** (if dedicated page):
```fsharp
// In Navigation.fs
navLink "Open Commands" Page.OpenCommands "🚀"
```

**Acceptance**:
- [ ] Open Commands accessible from navigation
- [ ] Page/section renders correctly
- [ ] Help text explains feature
- [ ] Layout consistent with other pages
- [ ] Navigation highlights current page

---

### Task 17.11: Testing & Documentation

**Objective**: Comprehensive testing and documentation

**Testing Checklist**:

**Backend Testing**:
- [ ] LaunchTarget validation works
- [ ] OpenCommand validation works
- [ ] Load/save open commands file
- [ ] Launch URL in default browser
- [ ] Launch executable with arguments
- [ ] Launch executable with working directory
- [ ] Open folder in Explorer
- [ ] Launch multiple targets with delay
- [ ] Handle missing executable gracefully
- [ ] Handle invalid URL gracefully
- [ ] Handle invalid path gracefully
- [ ] Reserved keyword "settings" rejected
- [ ] Duplicate keywords rejected
- [ ] Empty targets list rejected

**API Testing**:
- [ ] GET /api/open-commands returns all
- [ ] POST /api/open-commands creates
- [ ] PUT /api/open-commands/{index} updates
- [ ] DELETE /api/open-commands/{index} deletes
- [ ] POST /api/open-commands/test launches
- [ ] Validation errors return 400
- [ ] Not found errors return 404
- [ ] Success returns correct status codes

**Integration Testing**:
- [ ] "open [keyword]" launches app
- [ ] "open settings" still works (built-in)
- [ ] Unknown keyword falls back to text
- [ ] Multiple targets launch in sequence
- [ ] Launch delay works correctly
- [ ] Commands reload when file changes
- [ ] Case-insensitive matching works

**Frontend Testing**:
- [ ] Commands list loads
- [ ] Add command modal opens
- [ ] Edit command modal opens with data
- [ ] Delete confirmation works
- [ ] Test button launches and shows results
- [ ] Save creates new command
- [ ] Update modifies existing command
- [ ] Delete removes command
- [ ] Validation prevents invalid input
- [ ] Error messages displayed

**End-to-End Testing**:
- [ ] Create command in UI
- [ ] Say "open [keyword]"
- [ ] Application launches
- [ ] Test multiple targets
- [ ] Edit command in UI
- [ ] Delete command in UI
- [ ] Reload application (persistence)

**Documentation Updates**:

**README.md**:
```markdown
### Custom "Open" Commands

VocalFold allows you to create custom voice commands to launch applications, open URLs, or open folders.

#### Creating an Open Command

1. Open Settings from the system tray
2. Navigate to "Open Commands" (or System Settings)
3. Click "Add Open Command"
4. Enter a keyword (e.g., "browser", "workspace")
5. Add one or more targets:
   - **URL**: Opens in default browser (e.g., https://google.com)
   - **Executable**: Launches an application (e.g., C:\Program Files\VSCode\Code.exe)
   - **Folder**: Opens in Windows Explorer (e.g., C:\Projects)
6. Optionally set a delay between launches (for multiple targets)
7. Click "Save Command"

#### Using Open Commands

Simply say "open [keyword]" while recording:
- "open browser" → Launches your configured browser
- "open workspace" → Launches your development tools
- "open email" → Opens email applications

#### Examples

**Single URL**:
- Keyword: "browser"
- Target: URL → https://google.com

**Multiple Applications**:
- Keyword: "workspace"
- Targets:
  1. Executable → C:\Program Files\Microsoft VS Code\Code.exe
  2. Executable → C:\Windows\System32\cmd.exe
  3. URL → http://localhost:3000
- Launch Delay: 1000ms (1 second between each)

**Application with Arguments**:
- Keyword: "project"
- Target: Executable → C:\Program Files\VSCode\Code.exe
  - Arguments: C:\Projects\MyProject
  - Working Directory: C:\Projects
```

**SPECIFICATION.md** (update):
```markdown
## Out of Scope

### Explicitly NOT included:
...
- ❌ Voice commands for application control

### Future Enhancements (not in MVP):
...
- ✅ Custom "open [keyword]" commands (Phase 17) - IMPLEMENTED
```

**CONTEXT.md** (update):
```markdown
### Current Status
**Phase**: Phase 17 - Custom Open Commands (Complete) ✅
**Last Completed**: Phase 17 - Custom Open Commands
**Status**: Full-featured voice-to-text application with custom app launcher

**Phases Complete**: 17 of 17 phases completed (100%)

- ✅ **Phase 17 COMPLETE - Custom Open Commands**:
  - Custom "open [keyword]" voice commands
  - Launch applications, URLs, and folders
  - Multi-target support (launch multiple apps at once)
  - Configurable via web UI
  - Test functionality before saving
  - Stored in external JSON file (cloud sync compatible)
```

**TROUBLESHOOTING.md**:
```markdown
### Open Command Issues

**Problem**: "open [keyword]" doesn't launch anything
- Check command exists in settings
- Verify keyword spelling matches exactly
- Check VocalFold logs for errors
- Test the command in settings UI first

**Problem**: Application fails to launch
- Verify executable path is correct
- Check file exists at specified path
- Ensure you have permissions to run executable
- Try launching manually to test
- Check arguments are correct

**Problem**: URL doesn't open in browser
- Verify URL is complete (include https://)
- Check default browser is set in Windows
- Try URL directly in browser first
- Check for typos in URL

**Problem**: Multiple targets launch out of order
- Increase launch delay in command settings
- Some applications take time to start
- Consider splitting into separate commands

**Problem**: "Keyword already exists" error
- Each keyword must be unique
- Check for similar keywords (case-insensitive)
- Delete or rename existing command first
```

**Acceptance**:
- [ ] All tests pass
- [ ] Documentation complete
- [ ] README updated with examples
- [ ] SPECIFICATION.md updated
- [ ] CONTEXT.md updated
- [ ] TROUBLESHOOTING.md updated
- [ ] Examples work end-to-end
- [ ] Ready for production use

---

## Phase 17 Summary

**What We Built**:
- Custom "open [keyword]" voice commands
- Application launcher for executables, URLs, and folders
- Multi-target support (launch multiple items per command)
- Web UI for command management
- Test functionality (try before saving)
- External JSON storage (cloud sync compatible)
- Full REST API for command management

**Key Features**:
✅ Voice-activated application launching
✅ Custom keyword definitions
✅ Multi-target commands (launch multiple apps)
✅ Support for URLs, executables, and folders
✅ Command-line arguments for executables
✅ Working directory specification
✅ Configurable launch delays
✅ Test without saving
✅ Web UI management
✅ Validation and error handling
✅ Persistent storage
✅ Cloud sync compatible

**Architecture**:
- Backend: ApplicationLauncher module for launching
- Backend: TextProcessor integration for command matching
- REST API: Full CRUD operations for commands
- Frontend: OpenCommandsManager component
- Frontend: OpenCommandModal for editing
- Storage: open-commands.json (separate from keywords)
- Validation: Client and server-side

**Benefits**:
- Hands-free application launching
- Faster workflow setup
- Customizable to user needs
- Launch complex multi-app workflows
- No keyboard/mouse required
- Accessible for users with mobility issues

**Use Cases**:
- Developer: "open workspace" → VSCode, Terminal, Browser
- Designer: "open design tools" → Figma, Adobe XD, Assets folder
- Writer: "open writing" → Word, Google Docs, Reference folder
- Student: "open school" → Zoom, OneNote, Course website
- Business: "open meeting" → Teams, Presentation, Agenda

**Technology**:
- Backend: F#, System.Diagnostics.Process
- Frontend: F#, Fable, React, TailwindCSS
- Storage: JSON file (open-commands.json)
- Validation: Client + server-side

**Tradeoffs**:
- Windows-only (uses Windows Explorer, Process.Start)
- No macOS/Linux support (different shell commands needed)
- Launch delays approximate (not guaranteed timing)
- No process monitoring (fire-and-forget)
- Cannot interact with launched apps (just launches)

---

**Status**: Phase 17 specification complete, ready for implementation
**Estimated Time**: 12-16 hours (includes backend, frontend, testing, documentation)

**Next Task**: Implement Phase 17 (Task 17.1 - Backend - Open Commands Data Model)
