module Types

// ============================================================================
// Domain Types (matching backend)
// ============================================================================

type KeywordCategory = {
    Name: string
    IsExpanded: bool
    Color: string option
}

type KeywordReplacement = {
    Keyword: string
    Replacement: string
    Category: string option
}

type AppSettings = {
    HotkeyKey: uint32
    HotkeyModifiers: uint32
    ModelSize: string
    RecordingDuration: int
    TypingSpeed: string
    StartWithWindows: bool
    KeywordReplacements: KeywordReplacement list
    Categories: KeywordCategory list
}

type AppStatus = {
    Version: string
    CurrentHotkey: string
}

// ============================================================================
// UI State Types
// ============================================================================

type Page =
    | Dashboard
    | SystemSettings
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
    PendingHotkey: (uint32 * uint32) option  // (modifiers, key)
    EditingKeyword: (int * KeywordReplacement) option
    EditingCategory: KeywordCategory option  // Category being created/edited
    ExpandedCategories: Set<string>  // Set of expanded category names
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
    | ApplyHotkey
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
    | MoveKeywordToCategory of int * string option

    // Categories
    | ToggleCategory of string
    | AddCategory
    | EditCategory of string
    | SaveCategory of KeywordCategory
    | DeleteCategory of string
    | CancelEditCategory

    // UI
    | ShowToast of string * ToastType
    | DismissToast of string
