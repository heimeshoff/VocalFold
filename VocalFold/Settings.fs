module Settings

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

/// Keyword category configuration
[<CLIMutable>]
type KeywordCategory = {
    /// Category name
    [<JsonPropertyName("name")>]
    Name: string

    /// UI state: collapsed or expanded
    [<JsonPropertyName("isExpanded")>]
    IsExpanded: bool

    /// Optional color tag
    [<JsonPropertyName("color")>]
    Color: string option
}

/// Keyword replacement configuration
[<CLIMutable>]
type KeywordReplacement = {
    /// The keyword to listen for (what user says)
    [<JsonPropertyName("keyword")>]
    Keyword: string

    /// What to type instead
    [<JsonPropertyName("replacement")>]
    Replacement: string

    /// Optional category name
    [<JsonPropertyName("category")>]
    Category: string option
}

/// External keywords data (stored in separate file for cloud sync)
[<CLIMutable>]
type KeywordData = {
    /// List of keyword replacements
    [<JsonPropertyName("keywordReplacements")>]
    KeywordReplacements: KeywordReplacement list

    /// List of keyword categories
    [<JsonPropertyName("categories")>]
    Categories: KeywordCategory list

    /// Schema version for future migrations
    [<JsonPropertyName("version")>]
    Version: string
}

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

/// Typing speed configuration
type TypingSpeed =
    | Fast      // 0ms delay
    | Normal    // 10ms delay
    | Slow      // 20ms delay
    | Custom of int  // Custom delay in ms

/// Get the delay in milliseconds for a typing speed
let getTypingDelay (speed: TypingSpeed) : int =
    match speed with
    | Fast -> 0
    | Normal -> 10
    | Slow -> 20
    | Custom ms -> ms

/// Parse typing speed from string
let parseTypingSpeed (str: string) : TypingSpeed =
    // Handle null or empty string
    if String.IsNullOrEmpty(str) then
        Normal
    else
        match str.ToLowerInvariant() with
        | "fast" -> Fast
        | "slow" -> Slow
        | s when s.StartsWith("custom:") ->
            let parts = s.Split(':')
            if parts.Length = 2 then
                match Int32.TryParse(parts.[1]) with
                | (true, ms) -> Custom ms
                | _ -> Normal
            else
                Normal
        | _ -> Normal

/// Convert typing speed to string
let typingSpeedToString (speed: TypingSpeed) : string =
    match speed with
    | Fast -> "fast"
    | Normal -> "normal"
    | Slow -> "slow"
    | Custom ms -> sprintf "custom:%d" ms

/// Application settings (machine-specific, stored in settings.json)
[<CLIMutable>]
type AppSettings = {
    /// Virtual key code for the hotkey (e.g., 0x20 for Space)
    [<JsonPropertyName("hotkeyKey")>]
    HotkeyKey: uint32

    /// Modifiers for the hotkey (combination of MOD_CONTROL, MOD_SHIFT, etc.)
    [<JsonPropertyName("hotkeyModifiers")>]
    HotkeyModifiers: uint32

    /// Whisper model size (Tiny, Base, Small, Medium, Large)
    [<JsonPropertyName("modelSize")>]
    ModelSize: string

    /// Maximum recording duration in seconds (0 = no limit)
    [<JsonPropertyName("recordingDuration")>]
    RecordingDuration: int

    /// Typing speed configuration (stored as string in JSON)
    [<JsonPropertyName("typingSpeed")>]
    TypingSpeedStr: string

    /// Whether to start application with Windows
    [<JsonPropertyName("startWithWindows")>]
    StartWithWindows: bool

    /// Path to external keywords file (if None, uses default location)
    [<JsonPropertyName("keywordsFilePath")>]
    KeywordsFilePath: string option

    /// Path to external open commands file (if None, uses default location)
    [<JsonPropertyName("openCommandsFilePath")>]
    OpenCommandsFilePath: string option

    /// DEPRECATED: Keyword replacements list (for migration from old format)
    [<JsonPropertyName("keywordReplacements")>]
    KeywordReplacements: KeywordReplacement list option

    /// DEPRECATED: Categories list (for migration from old format)
    [<JsonPropertyName("categories")>]
    Categories: KeywordCategory list option

    /// Selected microphone device index (None = default device)
    [<JsonPropertyName("selectedMicrophoneIndex")>]
    SelectedMicrophoneIndex: int option
}

/// Default categories
let defaultCategories = [
    { Name = "Uncategorized"; IsExpanded = true; Color = None }
    { Name = "Punctuation"; IsExpanded = true; Color = Some "#25abfe" }
    { Name = "Email Templates"; IsExpanded = true; Color = Some "#ff8b00" }
    { Name = "Code Snippets"; IsExpanded = true; Color = Some "#00d4aa" }
]

/// Default keyword data
let defaultKeywordData = {
    KeywordReplacements = []
    Categories = defaultCategories
    Version = "1.0"
}

/// Default open commands data
let defaultOpenCommandsData = {
    OpenCommands = []
    Version = "1.0"
}

/// Default settings
let defaultSettings = {
    HotkeyKey = 0x5Bu  // Left Win key
    HotkeyModifiers = 0x0002u  // Ctrl (modifier)
    ModelSize = "Base"
    RecordingDuration = 0  // No limit (press and hold)
    TypingSpeedStr = "normal"  // Default to normal typing speed
    StartWithWindows = false  // Don't start with Windows by default
    KeywordsFilePath = None  // Use default keywords file location
    OpenCommandsFilePath = None  // Use default open commands file location
    KeywordReplacements = None  // DEPRECATED - for migration only
    Categories = None  // DEPRECATED - for migration only
    SelectedMicrophoneIndex = None  // Use default microphone
}

/// Get the typing speed from settings
let getTypingSpeed (settings: AppSettings) : TypingSpeed =
    parseTypingSpeed settings.TypingSpeedStr

/// Get the settings directory path
let private getSettingsDirectory () =
    let appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    Path.Combine(appData, "VocalFold")

/// Get the settings file path
let private getSettingsFilePath () =
    Path.Combine(getSettingsDirectory(), "settings.json")

/// Get the default keywords file path
let getDefaultKeywordsFilePath () =
    Path.Combine(getSettingsDirectory(), "keywords.json")

/// Get the keywords file path from settings, or default if not specified
let getKeywordsFilePath (settings: AppSettings) : string =
    match settings.KeywordsFilePath with
    | Some path when not (String.IsNullOrWhiteSpace(path)) -> path
    | _ -> getDefaultKeywordsFilePath()

/// Get the default open commands file path
let getDefaultOpenCommandsFilePath () =
    Path.Combine(getSettingsDirectory(), "open-commands.json")

/// Get the open commands file path from settings, or default if not specified
let getOpenCommandsFilePath (settings: AppSettings) : string =
    match settings.OpenCommandsFilePath with
    | Some path when not (String.IsNullOrWhiteSpace(path)) -> path
    | _ -> getDefaultOpenCommandsFilePath()

/// JSON serializer options
let private jsonOptions =
    let options = JsonSerializerOptions()
    options.WriteIndented <- true
    // Add F# converter to properly handle option types, lists, etc.
    options.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.Default))
    options

/// Load settings from file, or return default settings if file doesn't exist
let load () : AppSettings =
    let settingsPath = getSettingsFilePath()

    try
        if File.Exists(settingsPath) then
            let json = File.ReadAllText(settingsPath)

            // Try loading with the new format first (with F# converter)
            let settings =
                try
                    JsonSerializer.Deserialize<AppSettings>(json, jsonOptions)
                with
                | :? JsonException as ex ->
                    // If it fails, try loading with old format (without F# converter) for migration
                    Logger.info "Settings file appears to be in old format, attempting migration..."
                    let oldOptions = JsonSerializerOptions(WriteIndented = true)
                    let migrated = JsonSerializer.Deserialize<AppSettings>(json, oldOptions)
                    // Resave immediately with new format
                    try
                        let newJson = JsonSerializer.Serialize(migrated, jsonOptions)
                        File.WriteAllText(settingsPath, newJson)
                        Logger.info "Settings migrated to new format"
                    with
                    | ex -> Logger.warning (sprintf "Could not resave migrated settings: %s" ex.Message)
                    migrated

            // Validate settings
            if settings.HotkeyKey = 0u then
                Logger.warning "Invalid hotkey key in settings, using defaults"
                defaultSettings
            else
                // Ensure TypingSpeedStr has a valid value (handle null from old settings files)
                let normalizedSettings =
                    if String.IsNullOrEmpty(settings.TypingSpeedStr) then
                        Logger.info "TypingSpeed not set in settings file, defaulting to 'normal'"
                        { settings with TypingSpeedStr = "normal" }
                    else
                        settings

                Logger.info (sprintf "Settings loaded from: %s" settingsPath)
                normalizedSettings
        else
            Logger.info "Settings file not found, using defaults"
            defaultSettings
    with
    | ex ->
        Logger.error (sprintf "Error loading settings: %s - Using default settings" ex.Message)
        defaultSettings

/// Load settings from file and return whether this is the first run
/// Returns (settings, isFirstRun) where isFirstRun is true if settings file didn't exist
let loadWithFirstRunCheck () : AppSettings * bool =
    let settingsPath = getSettingsFilePath()
    let isFirstRun = not (File.Exists(settingsPath))
    let settings = load()
    (settings, isFirstRun)

/// Save settings to file
let save (settings: AppSettings) : bool =
    let settingsDir = getSettingsDirectory()
    let settingsPath = getSettingsFilePath()

    try
        // Create directory if it doesn't exist
        if not (Directory.Exists(settingsDir)) then
            Directory.CreateDirectory(settingsDir) |> ignore
            Logger.info (sprintf "Created settings directory: %s" settingsDir)

        // Serialize and save
        let json = JsonSerializer.Serialize(settings, jsonOptions)
        File.WriteAllText(settingsPath, json)
        Logger.info (sprintf "Settings saved to: %s" settingsPath)
        true
    with
    | ex ->
        Logger.error (sprintf "Error saving settings: %s" ex.Message)
        false

/// Load keyword data from external file
let loadKeywordData (filePath: string) : KeywordData =
    try
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            let data = JsonSerializer.Deserialize<KeywordData>(json, jsonOptions)
            Logger.info (sprintf "Keywords loaded from: %s" filePath)
            Logger.info (sprintf "Loaded %d keyword replacements" data.KeywordReplacements.Length)
            Logger.info (sprintf "Loaded %d categories" data.Categories.Length)
            data
        else
            Logger.info (sprintf "Keywords file not found at %s, using defaults" filePath)
            defaultKeywordData
    with
    | ex ->
        Logger.error (sprintf "Error loading keywords from %s: %s - Using default keywords" filePath ex.Message)
        defaultKeywordData

/// Save keyword data to external file
let saveKeywordData (filePath: string) (data: KeywordData) : bool =
    try
        // Create directory if it doesn't exist
        let directory = Path.GetDirectoryName(filePath)
        if not (String.IsNullOrEmpty(directory)) && not (Directory.Exists(directory)) then
            Directory.CreateDirectory(directory) |> ignore
            Logger.info (sprintf "Created keywords directory: %s" directory)

        // Serialize and save
        let json = JsonSerializer.Serialize(data, jsonOptions)
        File.WriteAllText(filePath, json)
        Logger.info (sprintf "Keywords saved to: %s" filePath)
        Logger.info (sprintf "Saved %d keyword replacements" data.KeywordReplacements.Length)
        Logger.info (sprintf "Saved %d categories" data.Categories.Length)
        true
    with
    | ex ->
        Logger.error (sprintf "Error saving keywords to %s: %s" filePath ex.Message)
        false

/// Validate that a keywords file path is valid
let validateKeywordsFilePath (path: string) : Result<string, string> =
    try
        if String.IsNullOrWhiteSpace(path) then
            Error "Keywords file path cannot be empty"
        else
            // Check if path is absolute
            if not (Path.IsPathRooted(path)) then
                Error "Keywords file path must be an absolute path"
            else
                // Check if directory exists or can be created
                let directory = Path.GetDirectoryName(path)
                if String.IsNullOrEmpty(directory) then
                    Error "Invalid keywords file path"
                else
                    // Try to ensure directory exists
                    if not (Directory.Exists(directory)) then
                        // Check if parent directory exists (for cloud storage paths)
                        let parentDir = Directory.GetParent(directory)
                        if parentDir = null || not parentDir.Exists then
                            Error (sprintf "Directory does not exist: %s" directory)
                        else
                            Ok path
                    else
                        Ok path
    with
    | ex ->
        Error (sprintf "Invalid path: %s" ex.Message)

/// Migrate keywords from old settings format to external file
/// Returns updated settings with KeywordsFilePath set
let migrateKeywordsToExternalFile (settings: AppSettings) : AppSettings =
    try
        // Check if migration is needed (old settings have keywords embedded)
        match settings.KeywordReplacements, settings.Categories with
        | Some keywords, Some categories when keywords.Length > 0 || categories.Length > 0 ->
            Logger.info "Migrating keywords from settings.json to external keywords.json file..."

            // Create keyword data from old format
            let keywordData = {
                KeywordReplacements = keywords
                Categories = categories
                Version = "1.0"
            }

            // Save to default keywords file location
            let keywordsPath = getDefaultKeywordsFilePath()
            if saveKeywordData keywordsPath keywordData then
                Logger.info (sprintf "Successfully migrated %d keywords and %d categories to %s" keywords.Length categories.Length keywordsPath)

                // Update settings to remove old fields and set new path
                let migratedSettings = {
                    settings with
                        KeywordsFilePath = None  // Use default location
                        KeywordReplacements = None  // Clear deprecated field
                        Categories = None  // Clear deprecated field
                }

                // Save updated settings
                if save migratedSettings then
                    Logger.info "Settings updated after keyword migration"
                    migratedSettings
                else
                    Logger.warning "Failed to save updated settings after migration, but keywords file was created"
                    migratedSettings
            else
                Logger.error "Failed to save keywords during migration, keeping old format"
                settings

        | Some keywords, None when keywords.Length > 0 ->
            // Only keywords, no categories - migrate with default categories
            Logger.info "Migrating keywords (without categories) from settings.json..."

            let keywordData = {
                KeywordReplacements = keywords
                Categories = defaultCategories
                Version = "1.0"
            }

            let keywordsPath = getDefaultKeywordsFilePath()
            if saveKeywordData keywordsPath keywordData then
                Logger.info (sprintf "Successfully migrated %d keywords to %s" keywords.Length keywordsPath)
                { settings with KeywordsFilePath = None; KeywordReplacements = None; Categories = None }
            else
                settings

        | None, Some categories when categories.Length > 0 ->
            // Only categories, no keywords - migrate anyway
            Logger.info "Migrating categories (without keywords) from settings.json..."

            let keywordData = {
                KeywordReplacements = []
                Categories = categories
                Version = "1.0"
            }

            let keywordsPath = getDefaultKeywordsFilePath()
            if saveKeywordData keywordsPath keywordData then
                Logger.info (sprintf "Successfully migrated %d categories to %s" categories.Length keywordsPath)
                { settings with KeywordsFilePath = None; KeywordReplacements = None; Categories = None }
            else
                settings

        | _ ->
            // No migration needed - either already migrated or no keywords to migrate
            settings
    with
    | ex ->
        Logger.error (sprintf "Error during keyword migration: %s" ex.Message)
        settings

/// Get the display name for modifiers
let getModifierDisplayName (modifiers: uint32) : string =
    let parts = ResizeArray<string>()

    if (modifiers &&& 0x0002u) <> 0u then parts.Add("Ctrl")
    if (modifiers &&& 0x0004u) <> 0u then parts.Add("Shift")
    if (modifiers &&& 0x0001u) <> 0u then parts.Add("Alt")
    if (modifiers &&& 0x0008u) <> 0u then parts.Add("Win")

    String.Join("+", parts)

/// Get the display name for a virtual key
let getKeyDisplayName (vkCode: uint32) : string =
    match vkCode with
    | 0x20u -> "Space"
    | 0x0Du -> "Enter"
    | 0x09u -> "Tab"
    | 0x1Bu -> "Esc"
    | 0x08u -> "Backspace"
    | 0x2Eu -> "Delete"
    | 0x21u -> "Page Up"
    | 0x22u -> "Page Down"
    | 0x23u -> "End"
    | 0x24u -> "Home"
    | 0x25u -> "Left"
    | 0x26u -> "Up"
    | 0x27u -> "Right"
    | 0x28u -> "Down"
    | 0x70u -> "F1"
    | 0x71u -> "F2"
    | 0x72u -> "F3"
    | 0x73u -> "F4"
    | 0x74u -> "F5"
    | 0x75u -> "F6"
    | 0x76u -> "F7"
    | 0x77u -> "F8"
    | 0x78u -> "F9"
    | 0x79u -> "F10"
    | 0x7Au -> "F11"
    | 0x7Bu -> "F12"
    | 0x5Bu -> "Left Win"
    | 0x5Cu -> "Right Win"
    | vk when vk >= 0x30u && vk <= 0x39u -> // 0-9
        char(vk) |> string
    | vk when vk >= 0x41u && vk <= 0x5Au -> // A-Z
        char(vk) |> string
    | _ -> sprintf "0x%X" vkCode

/// Get the full hotkey display name
let getHotkeyDisplayName (settings: AppSettings) : string =
    let modifiers = getModifierDisplayName settings.HotkeyModifiers
    let key = getKeyDisplayName settings.HotkeyKey

    if String.IsNullOrEmpty(modifiers) then
        key
    else
        sprintf "%s+%s" modifiers key

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
    else
        // Validate all targets (if any)
        // Note: Allow commands without targets (user can add them later via UI)
        command.Targets
        |> List.tryPick validateLaunchTarget

/// Load open commands from file
let loadOpenCommands (filePath: string) : OpenCommandsData =
    try
        if File.Exists(filePath) then
            let json = File.ReadAllText(filePath)
            let data = JsonSerializer.Deserialize<OpenCommandsData>(json, jsonOptions)
            Logger.info (sprintf "Open commands loaded from: %s" filePath)
            Logger.info (sprintf "Loaded %d open command(s)" data.OpenCommands.Length)
            data
        else
            Logger.info (sprintf "Open commands file not found at %s, using defaults" filePath)
            defaultOpenCommandsData
    with
    | ex ->
        Logger.error (sprintf "Error loading open commands from %s: %s - Using default open commands" filePath ex.Message)
        defaultOpenCommandsData

/// Save open commands to file
let saveOpenCommands (filePath: string) (data: OpenCommandsData) : Result<unit, string> =
    try
        let directory = Path.GetDirectoryName(filePath)
        if not (Directory.Exists(directory)) then
            Directory.CreateDirectory(directory) |> ignore
            Logger.info (sprintf "Created open commands directory: %s" directory)

        let json = JsonSerializer.Serialize(data, jsonOptions)
        File.WriteAllText(filePath, json)
        Logger.info (sprintf "Saved %d open command(s) to %s" data.OpenCommands.Length filePath)
        Ok ()
    with
    | ex ->
        let errorMsg = sprintf "Failed to save open commands: %s" ex.Message
        Logger.error errorMsg
        Error errorMsg
