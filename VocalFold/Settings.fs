module Settings

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

/// Application settings
type AppSettings = {
    /// Virtual key code for the hotkey (e.g., 0x20 for Space)
    [<JsonPropertyName("hotkeyKey")>]
    HotkeyKey: uint32

    /// Modifiers for the hotkey (combination of MOD_CONTROL, MOD_SHIFT, etc.)
    [<JsonPropertyName("hotkeyModifiers")>]
    HotkeyModifiers: uint32

    /// Whether voice input is enabled
    [<JsonPropertyName("isEnabled")>]
    IsEnabled: bool

    /// Whisper model size (Tiny, Base, Small, Medium, Large)
    [<JsonPropertyName("modelSize")>]
    ModelSize: string

    /// Maximum recording duration in seconds (0 = no limit)
    [<JsonPropertyName("recordingDuration")>]
    RecordingDuration: int
}

/// Default settings
let defaultSettings = {
    HotkeyKey = 0x20u  // Space key
    HotkeyModifiers = 0x0002u ||| 0x0004u  // Ctrl + Shift
    IsEnabled = true
    ModelSize = "Base"
    RecordingDuration = 0  // No limit (press and hold)
}

/// Get the settings directory path
let private getSettingsDirectory () =
    let appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    Path.Combine(appData, "VocalFold")

/// Get the settings file path
let private getSettingsFilePath () =
    Path.Combine(getSettingsDirectory(), "settings.json")

/// JSON serializer options
let private jsonOptions =
    let options = JsonSerializerOptions()
    options.WriteIndented <- true
    options

/// Load settings from file, or return default settings if file doesn't exist
let load () : AppSettings =
    let settingsPath = getSettingsFilePath()

    try
        if File.Exists(settingsPath) then
            let json = File.ReadAllText(settingsPath)
            let settings = JsonSerializer.Deserialize<AppSettings>(json, jsonOptions)

            // Validate settings
            if settings.HotkeyKey = 0u then
                Logger.warning "Invalid hotkey key in settings, using defaults"
                defaultSettings
            else
                Logger.info (sprintf "Settings loaded from: %s" settingsPath)
                settings
        else
            Logger.info "Settings file not found, using defaults"
            defaultSettings
    with
    | ex ->
        Logger.error (sprintf "Error loading settings: %s - Using default settings" ex.Message)
        defaultSettings

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
