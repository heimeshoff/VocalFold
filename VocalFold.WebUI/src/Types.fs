module Types

// ============================================================================
// Domain Types (matching backend)
// ============================================================================

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

// ============================================================================
// UI State Types
// ============================================================================

type Page =
    | Dashboard
    | GeneralSettings
    | KeywordSettings

type LoadingState<'T> =
    | NotStarted
    | Loading
    | Loaded of 'T
    | Error of string

type Toast = {
    Id: string
    Message: string
    Type: ToastType
}
and ToastType =
    | Success
    | Error
    | Info
    | Warning

type Model = {
    CurrentPage: Page
    Settings: LoadingState<AppSettings>
    Status: LoadingState<AppStatus>
    IsRecordingHotkey: bool
    EditingKeyword: (int * KeywordReplacement) option
    Toasts: Toast list
}

// ============================================================================
// Messages
// ============================================================================

type Msg =
    // Navigation
    | NavigateToPage of Page

    // Settings
    | LoadSettings
    | SettingsLoaded of Result<AppSettings, string>
    | UpdateSettings of AppSettings
    | SettingsSaved of Result<unit, string>

    // Status
    | LoadStatus
    | StatusLoaded of Result<AppStatus, string>

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
    | AddExampleKeywords
    | ExampleKeywordsAdded of Result<int, string>

    // Enable/Disable
    | ToggleEnabled

    // UI
    | ShowToast of string * ToastType
    | DismissToast of string
