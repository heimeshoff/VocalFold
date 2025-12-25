module WebServer

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe

// ============================================================================
// Types
// ============================================================================

type ServerConfig = {
    OnSettingsChanged: Settings.AppSettings -> unit
    OnKeywordsChanged: Settings.KeywordData -> unit
    RestartFileWatcher: string -> unit
    GetWhisperService: unit -> TranscriptionService.WhisperService
}

type ServerState = {
    Port: int
    Host: IHost
    CancellationTokenSource: CancellationTokenSource
    FileWatcherRef: FileSystemWatcher option ref
}

// ============================================================================
// Port Discovery
// ============================================================================

/// Find an available port on localhost
let findAvailablePort() : int =
    use listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port

// ============================================================================
// Web API Handlers
// ============================================================================

/// Handler for GET /api/status
let getStatusHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let status = {|
                Version = "1.0.0"
                CurrentHotkey = Settings.getHotkeyDisplayName settings
            |}
            return! json status next ctx
        }

/// Handler for GET /api/settings
let getSettingsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            // Load keywords from external file and merge into response for frontend compatibility
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            // Create a response that includes keywords for frontend compatibility
            let responseSettings = {
                settings with
                    KeywordReplacements = Some keywordData.KeywordReplacements
                    Categories = Some keywordData.Categories
            }

            return! json responseSettings next ctx
        }

/// Handler for PUT /api/settings
let updateSettingsHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! newSettings = ctx.BindJsonAsync<Settings.AppSettings>()

                // Save settings to disk
                let saveResult = Settings.save newSettings

                if saveResult then
                    // Notify the application of settings change
                    config.OnSettingsChanged newSettings
                    return! json {| success = true |} next ctx
                else
                    ctx.SetStatusCode 500
                    return! json {| error = "Failed to save settings to disk" |} next ctx
            with
            | ex ->
                Logger.error (sprintf "Error in updateSettingsHandler: %s\n%s" ex.Message ex.StackTrace)
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error updating settings: %s" ex.Message |} next ctx
        }

/// Handler for GET /api/keywords
let getKeywordsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath
            return! json keywordData.KeywordReplacements next ctx
        }

/// Handler for POST /api/keywords
let addKeywordHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! keyword = ctx.BindJsonAsync<Settings.KeywordReplacement>()
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath
            let updatedData = { keywordData with KeywordReplacements = keywordData.KeywordReplacements @ [keyword] }
            Settings.saveKeywordData keywordsPath updatedData |> ignore
            config.OnSettingsChanged settings
            return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/keywords/:index
let updateKeywordHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! keyword = ctx.BindJsonAsync<Settings.KeywordReplacement>()
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            if index >= 0 && index < keywordData.KeywordReplacements.Length then
                let updatedKeywords =
                    keywordData.KeywordReplacements
                    |> List.mapi (fun i k -> if i = index then keyword else k)
                let updatedData = { keywordData with KeywordReplacements = updatedKeywords }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
            else
                ctx.SetStatusCode 404
                return! json {| error = "Keyword not found" |} next ctx
        }

/// Handler for DELETE /api/keywords/:index
let deleteKeywordHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            if index >= 0 && index < keywordData.KeywordReplacements.Length then
                let updatedKeywords =
                    keywordData.KeywordReplacements
                    |> List.mapi (fun i k -> (i, k))
                    |> List.filter (fun (i, _) -> i <> index)
                    |> List.map snd
                let updatedData = { keywordData with KeywordReplacements = updatedKeywords }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
            else
                ctx.SetStatusCode 404
                return! json {| error = "Keyword not found" |} next ctx
        }

/// Handler for POST /api/keywords/examples
let addExampleKeywordsHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath
            let examples = TextProcessor.getExampleReplacements()
            let updatedData = { keywordData with KeywordReplacements = keywordData.KeywordReplacements @ examples }
            Settings.saveKeywordData keywordsPath updatedData |> ignore
            config.OnSettingsChanged settings
            return! json {| success = true; added = examples.Length |} next ctx
        }

// ============================================================================
// Category API Handlers
// ============================================================================

/// Handler for GET /api/categories
let getCategoriesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath
            return! json keywordData.Categories next ctx
        }

/// Handler for POST /api/categories
let createCategoryHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! category = ctx.BindJsonAsync<Settings.KeywordCategory>()
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            // Check if category name already exists
            let nameExists = keywordData.Categories |> List.exists (fun c -> c.Name = category.Name)

            if nameExists then
                ctx.SetStatusCode 400
                return! json {| error = "Category name already exists" |} next ctx
            else
                let updatedData = { keywordData with Categories = keywordData.Categories @ [category] }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/categories/:name
let updateCategoryHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! updatedCategory = ctx.BindJsonAsync<Settings.KeywordCategory>()
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            // Find the category to update
            let categoryExists = keywordData.Categories |> List.exists (fun c -> c.Name = name)

            if not categoryExists then
                ctx.SetStatusCode 404
                return! json {| error = "Category not found" |} next ctx
            else
                // Update the category
                let updatedCategories =
                    keywordData.Categories
                    |> List.map (fun c -> if c.Name = name then updatedCategory else c)

                // Also update keywords that reference the old category name (if renamed)
                let updatedKeywords =
                    if name <> updatedCategory.Name then
                        keywordData.KeywordReplacements
                        |> List.map (fun k ->
                            match k.Category with
                            | Some cat when cat = name -> { k with Category = Some updatedCategory.Name }
                            | _ -> k
                        )
                    else
                        keywordData.KeywordReplacements

                let updatedData = { keywordData with Categories = updatedCategories; KeywordReplacements = updatedKeywords }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
        }

/// Handler for DELETE /api/categories/:name
let deleteCategoryHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            // Prevent deletion of "Uncategorized"
            if name = "Uncategorized" then
                ctx.SetStatusCode 400
                return! json {| error = "Cannot delete Uncategorized category" |} next ctx
            else
                // Remove the category
                let updatedCategories = keywordData.Categories |> List.filter (fun c -> c.Name <> name)

                // Move all keywords from this category to "Uncategorized"
                let updatedKeywords =
                    keywordData.KeywordReplacements
                    |> List.map (fun k ->
                        match k.Category with
                        | Some cat when cat = name -> { k with Category = Some "Uncategorized" }
                        | _ -> k
                    )

                let updatedData = { keywordData with Categories = updatedCategories; KeywordReplacements = updatedKeywords }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/categories/:name/state
let toggleCategoryStateHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            // Toggle the IsExpanded state
            let updatedCategories =
                keywordData.Categories
                |> List.map (fun c -> if c.Name = name then { c with IsExpanded = not c.IsExpanded } else c)

            let updatedData = { keywordData with Categories = updatedCategories }
            Settings.saveKeywordData keywordsPath updatedData |> ignore
            config.OnSettingsChanged settings
            return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/keywords/:index/category
let moveKeywordToCategoryHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.BindJsonAsync<{| category: string option |}>()
            let settings = Settings.load()
            let keywordsPath = Settings.getKeywordsFilePath settings
            let keywordData = Settings.loadKeywordData keywordsPath

            if index >= 0 && index < keywordData.KeywordReplacements.Length then
                let updatedKeywords =
                    keywordData.KeywordReplacements
                    |> List.mapi (fun i k -> if i = index then { k with Category = body.category } else k)
                let updatedData = { keywordData with KeywordReplacements = updatedKeywords }
                Settings.saveKeywordData keywordsPath updatedData |> ignore
                config.OnSettingsChanged settings
                return! json {| success = true |} next ctx
            else
                ctx.SetStatusCode 404
                return! json {| error = "Keyword not found" |} next ctx
        }

// ============================================================================
// Keywords File Path API Handlers
// ============================================================================

/// Handler for GET /api/settings/keywords-path
let getKeywordsPathHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let currentPath = Settings.getKeywordsFilePath settings
            let defaultPath = Settings.getDefaultKeywordsFilePath()
            let isDefault = currentPath = defaultPath

            return! json {|
                currentPath = currentPath
                defaultPath = defaultPath
                isDefault = isDefault
            |} next ctx
        }

/// Handler for PUT /api/settings/keywords-path
let updateKeywordsPathHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.BindJsonAsync<{| path: string option |}>()
            let settings = Settings.load()

            // Validate the path if provided
            match body.path with
            | Some path when not (String.IsNullOrWhiteSpace(path)) ->
                match Settings.validateKeywordsFilePath path with
                | Ok validPath ->
                    // Update settings with new path
                    let updatedSettings = { settings with KeywordsFilePath = Some validPath }

                    // Save settings
                    if Settings.save updatedSettings then
                        config.OnSettingsChanged updatedSettings
                        // Restart file watcher with new path
                        config.RestartFileWatcher validPath
                        return! json {| success = true; path = validPath |} next ctx
                    else
                        ctx.SetStatusCode 500
                        return! json {| error = "Failed to save settings" |} next ctx

                | Error errorMsg ->
                    ctx.SetStatusCode 400
                    return! json {| error = errorMsg |} next ctx

            | _ ->
                // Reset to default path
                let updatedSettings = { settings with KeywordsFilePath = None }

                if Settings.save updatedSettings then
                    config.OnSettingsChanged updatedSettings
                    let defaultPath = Settings.getDefaultKeywordsFilePath()
                    // Restart file watcher with default path
                    config.RestartFileWatcher defaultPath
                    return! json {| success = true; path = defaultPath |} next ctx
                else
                    ctx.SetStatusCode 500
                    return! json {| error = "Failed to save settings" |} next ctx
        }

/// Handler for POST /api/keywords/export-to-file
let exportKeywordsToFileHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.BindJsonAsync<{| targetPath: string; setAsActive: bool |}>()
            let settings = Settings.load()

            // Validate target path
            match Settings.validateKeywordsFilePath body.targetPath with
            | Ok validPath ->
                // Load current keywords
                let currentPath = Settings.getKeywordsFilePath settings
                let keywordData = Settings.loadKeywordData currentPath

                // Save to new location
                if Settings.saveKeywordData validPath keywordData then
                    // If requested, update settings to use new path
                    if body.setAsActive then
                        let updatedSettings = { settings with KeywordsFilePath = Some validPath }
                        Settings.save updatedSettings |> ignore
                        config.OnSettingsChanged updatedSettings
                        // Restart file watcher with new path
                        config.RestartFileWatcher validPath

                    return! json {| success = true; path = validPath |} next ctx
                else
                    ctx.SetStatusCode 500
                    return! json {| error = "Failed to export keywords file" |} next ctx

            | Error errorMsg ->
                ctx.SetStatusCode 400
                return! json {| error = errorMsg |} next ctx
        }

// ============================================================================
// Open Commands File Path API Handlers
// ============================================================================

/// Handler for GET /api/settings/open-commands-path
let getOpenCommandsPathHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            let currentPath = Settings.getOpenCommandsFilePath settings
            let defaultPath = Settings.getDefaultOpenCommandsFilePath()
            let isDefault = currentPath = defaultPath

            return! json {|
                currentPath = currentPath
                defaultPath = defaultPath
                isDefault = isDefault
            |} next ctx
        }

/// Handler for PUT /api/settings/open-commands-path
let updateOpenCommandsPathHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.BindJsonAsync<{| path: string option |}>()
            let settings = Settings.load()

            // Validate the path if provided
            match body.path with
            | Some path when not (String.IsNullOrWhiteSpace(path)) ->
                match Settings.validateOpenCommandsFilePath path with
                | Ok validPath ->
                    // Update settings with new path
                    let updatedSettings = { settings with OpenCommandsFilePath = Some validPath }

                    // Save settings
                    if Settings.save updatedSettings then
                        config.OnSettingsChanged updatedSettings
                        // Reload open commands from new path
                        TextProcessor.reloadOpenCommands validPath
                        return! json {| success = true; path = validPath |} next ctx
                    else
                        ctx.SetStatusCode 500
                        return! json {| error = "Failed to save settings" |} next ctx

                | Error errorMsg ->
                    ctx.SetStatusCode 400
                    return! json {| error = errorMsg |} next ctx

            | _ ->
                // Reset to default path
                let updatedSettings = { settings with OpenCommandsFilePath = None }

                if Settings.save updatedSettings then
                    config.OnSettingsChanged updatedSettings
                    let defaultPath = Settings.getDefaultOpenCommandsFilePath()
                    // Reload open commands from default path
                    TextProcessor.reloadOpenCommands defaultPath
                    return! json {| success = true; path = defaultPath |} next ctx
                else
                    ctx.SetStatusCode 500
                    return! json {| error = "Failed to save settings" |} next ctx
        }

// ============================================================================
// Microphone API Handlers
// ============================================================================

// Mutable state to hold active test recording (only one test at a time)
let mutable currentTestState: AudioRecorder.TestRecordingState option = None

// Global mutable state for server config (needed to access whisper service)
let mutable serverConfig: ServerConfig option = None

/// Handler for GET /api/microphones - List all available microphones
let getMicrophonesHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
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

                return! json devicesJson next ctx
            with
            | ex ->
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error listing microphones: %s" ex.Message |} next ctx
        }

/// Handler for POST /api/microphones/test/start - Start microphone test
let startMicrophoneTestHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! body = ctx.BindJsonAsync<{| deviceIndex: int option |}>()

                // Stop any existing test
                match currentTestState with
                | Some state ->
                    try
                        AudioRecorder.stopTestRecording state |> ignore
                    with
                    | _ -> ()
                | None -> ()

                // Start new test
                let state = AudioRecorder.startTestRecording body.deviceIndex None
                currentTestState <- Some state

                return! json {| status = "started"; deviceIndex = body.deviceIndex |} next ctx
            with
            | ex ->
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error starting test: %s" ex.Message |} next ctx
        }

/// Handler for POST /api/microphones/test/stop - Stop microphone test and get results
let stopMicrophoneTestHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                match currentTestState with
                | Some state ->
                    let (current, max, avg) = AudioRecorder.stopTestRecording state
                    currentTestState <- None

                    return! json {|
                        status = "stopped"
                        currentLevel = current
                        maxLevel = max
                        avgLevel = avg
                    |} next ctx
                | None ->
                    ctx.SetStatusCode 400
                    return! json {| error = "No active test recording" |} next ctx
            with
            | ex ->
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error stopping test: %s" ex.Message |} next ctx
        }

/// Handler for GET /api/microphones/test/levels - Get current audio levels
let getMicrophoneLevelsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                match currentTestState with
                | Some state ->
                    return! json {|
                        currentLevel = state.CurrentLevel
                        maxLevel = state.MaxLevel
                        avgLevel = if state.SampleCount > 0 then state.AvgLevel / float32 state.SampleCount else 0.0f
                    |} next ctx
                | None ->
                    return! json {|
                        currentLevel = 0.0f
                        maxLevel = 0.0f
                        avgLevel = 0.0f
                    |} next ctx
            with
            | ex ->
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error getting levels: %s" ex.Message |} next ctx
        }

/// Handler for POST /api/microphones/test/transcribe - Test microphone with transcription
let transcribeMicrophoneTestHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! body = ctx.BindJsonAsync<{| deviceIndex: int option; durationSeconds: int option |}>()
                let duration = body.durationSeconds |> Option.defaultValue 5

                Logger.info (sprintf "Starting transcription test (duration: %d seconds)" duration)

                // Record audio
                let recording = AudioRecorder.recordAudio duration body.deviceIndex
                Logger.info (sprintf "Recorded %d samples" recording.Samples.Length)

                // Check if muted
                if recording.IsMuted then
                    Logger.warning "Microphone is muted or audio level too low"
                    return! json {|
                        status = "error"
                        error = "Microphone is muted or audio level too low"
                        transcription = ""
                    |} next ctx
                elif recording.Samples.Length = 0 then
                    Logger.warning "No audio captured"
                    return! json {|
                        status = "error"
                        error = "No audio captured"
                        transcription = ""
                    |} next ctx
                else
                    // Transcribe
                    match serverConfig with
                    | Some config ->
                        Logger.info "Transcribing audio..."
                        let whisperService = config.GetWhisperService()
                        let! transcription = whisperService.Transcribe(recording.Samples) |> Async.StartAsTask
                        Logger.info (sprintf "Transcription: %s" transcription)

                        return! json {|
                            status = "success"
                            transcription = transcription
                            sampleCount = recording.Samples.Length
                        |} next ctx
                    | None ->
                        ctx.SetStatusCode 500
                        return! json {| error = "Whisper service not available" |} next ctx
            with
            | ex ->
                Logger.logException ex "Error during transcription test"
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error during transcription test: %s" ex.Message |} next ctx
        }

// ============================================================================
// Open Commands API Handlers
// ============================================================================

/// Handler for GET /api/open-commands
let getOpenCommandsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug "API: GET /api/open-commands"

                let settings = Settings.load()
                let filePath = Settings.getOpenCommandsFilePath settings
                let data = Settings.loadOpenCommands filePath

                Logger.info (sprintf "Returning %d open command(s)" data.OpenCommands.Length)
                return! json data.OpenCommands next ctx
            with
            | ex ->
                Logger.logException ex "Failed to get open commands"
                ctx.SetStatusCode 500
                return! json {| error = ex.Message |} next ctx
        }

/// Handler for POST /api/open-commands
let createOpenCommandHandler (config: ServerConfig) : HttpHandler =
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
                    let settings = Settings.load()
                    let filePath = Settings.getOpenCommandsFilePath settings
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

/// Handler for PUT /api/open-commands/{index}
let updateOpenCommandHandler (config: ServerConfig) (index: int) : HttpHandler =
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
                    let settings = Settings.load()
                    let filePath = Settings.getOpenCommandsFilePath settings
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

/// Handler for DELETE /api/open-commands/{index}
let deleteOpenCommandHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                Logger.debug (sprintf "API: DELETE /api/open-commands/%d" index)

                let settings = Settings.load()
                let filePath = Settings.getOpenCommandsFilePath settings
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

/// Handler for POST /api/open-commands/test
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

// ============================================================================
// GPU Status API Handler
// ============================================================================

/// Handler for GET /api/gpu-status - Get GPU runtime status and transcription metrics
let getGpuStatusHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let runtime = TranscriptionService.getDetectedRuntime()
                let metrics = TranscriptionService.getLastTranscriptionMetrics()

                let runtimeInfo =
                    match runtime with
                    | Some r ->
                        {|
                            runtimeType = r.RuntimeType.ToString()
                            description = r.Description
                            nativeLibrary = r.NativeLibrary |> Option.defaultValue ""
                            isGpuAccelerated =
                                match r.RuntimeType with
                                | TranscriptionService.CUDA | TranscriptionService.Vulkan -> true
                                | _ -> false
                        |}
                    | None ->
                        {|
                            runtimeType = "NotDetected"
                            description = "Runtime detection has not run yet"
                            nativeLibrary = ""
                            isGpuAccelerated = false
                        |}

                let metricsInfo =
                    match metrics with
                    | Some (audioDuration, transcriptionTime, rtf) ->
                        {|
                            hasMetrics = true
                            audioDurationSeconds = audioDuration
                            transcriptionTimeSeconds = transcriptionTime
                            realTimeFactor = rtf
                            performanceWarning =
                                if rtf > 3.0 then
                                    Some "Slow transcription detected - GPU acceleration may not be working"
                                else
                                    None
                        |}
                    | None ->
                        {|
                            hasMetrics = false
                            audioDurationSeconds = 0.0
                            transcriptionTimeSeconds = 0.0
                            realTimeFactor = 0.0
                            performanceWarning = None
                        |}

                return! json {|
                    runtime = runtimeInfo
                    lastTranscription = metricsInfo
                    troubleshooting = {|
                        nvidia = "For NVIDIA GPUs: Install CUDA Toolkit 12.x and latest Game Ready drivers"
                        amd = "For AMD GPUs: Install latest Adrenalin drivers (RX 6000+ recommended for Vulkan)"
                        intel = "For Intel Arc GPUs: Install latest drivers with Vulkan support"
                        cpu = "CPU mode is slower but works on any system"
                    |}
                |} next ctx
            with
            | ex ->
                Logger.logException ex "Error getting GPU status"
                ctx.SetStatusCode 500
                return! json {| error = sprintf "Error getting GPU status: %s" ex.Message |} next ctx
        }

// ============================================================================
// Routing
// ============================================================================

let webApp (config: ServerConfig) : HttpHandler =
    choose [
        subRoute "/api" (
            choose [
                GET  >=> route "/status" >=> getStatusHandler
                GET  >=> route "/gpu-status" >=> getGpuStatusHandler
                GET  >=> route "/settings" >=> getSettingsHandler
                PUT  >=> route "/settings" >=> updateSettingsHandler config
                GET  >=> route "/settings/keywords-path" >=> getKeywordsPathHandler
                PUT  >=> route "/settings/keywords-path" >=> updateKeywordsPathHandler config
                GET  >=> route "/settings/open-commands-path" >=> getOpenCommandsPathHandler
                PUT  >=> route "/settings/open-commands-path" >=> updateOpenCommandsPathHandler config
                GET  >=> route "/keywords" >=> getKeywordsHandler
                POST >=> route "/keywords" >=> addKeywordHandler config
                POST >=> route "/keywords/examples" >=> addExampleKeywordsHandler config
                POST >=> route "/keywords/export-to-file" >=> exportKeywordsToFileHandler config
                routef "/keywords/%i" (fun index ->
                    choose [
                        PUT    >=> updateKeywordHandler config index
                        DELETE >=> deleteKeywordHandler config index
                    ]
                )
                routef "/keywords/%i/category" (fun index ->
                    PUT >=> moveKeywordToCategoryHandler config index
                )
                GET  >=> route "/categories" >=> getCategoriesHandler
                POST >=> route "/categories" >=> createCategoryHandler config
                routef "/categories/%s" (fun name ->
                    choose [
                        PUT    >=> updateCategoryHandler config name
                        DELETE >=> deleteCategoryHandler config name
                    ]
                )
                routef "/categories/%s/state" (fun name ->
                    PUT >=> toggleCategoryStateHandler config name
                )

                // Microphone endpoints
                GET  >=> route "/microphones" >=> getMicrophonesHandler
                POST >=> route "/microphones/test/start" >=> startMicrophoneTestHandler
                POST >=> route "/microphones/test/stop" >=> stopMicrophoneTestHandler
                GET  >=> route "/microphones/test/levels" >=> getMicrophoneLevelsHandler
                POST >=> route "/microphones/test/transcribe" >=> transcribeMicrophoneTestHandler

                // Open Commands endpoints
                GET  >=> route "/open-commands" >=> getOpenCommandsHandler
                POST >=> route "/open-commands" >=> createOpenCommandHandler config
                POST >=> route "/open-commands/test" >=> testOpenCommandHandler
                routef "/open-commands/%i" (fun index ->
                    choose [
                        PUT    >=> updateOpenCommandHandler config index
                        DELETE >=> deleteOpenCommandHandler config index
                    ]
                )
            ]
        )

        // Let static file middleware handle everything else (index.html, JS, CSS, etc.)
        setStatusCode 404 >=> text "Not Found"
    ]

// ============================================================================
// Web Host Configuration
// ============================================================================

let configureApp (config: ServerConfig) (app: IApplicationBuilder) =
    // Try to find the WebUI dist folder
    let baseDir = System.AppDomain.CurrentDomain.BaseDirectory

    // Try multiple possible paths
    let possiblePaths = [
        System.IO.Path.Combine(baseDir, "..", "..", "..", "..", "VocalFold.WebUI", "dist")  // Development
        System.IO.Path.Combine(baseDir, "VocalFold.WebUI", "dist")  // Published
        System.IO.Path.Combine(baseDir, "..", "VocalFold.WebUI", "dist")  // Alternative
    ]

    let distFullPath =
        possiblePaths
        |> List.map System.IO.Path.GetFullPath
        |> List.tryFind System.IO.Directory.Exists
        |> Option.defaultWith (fun () ->
            Logger.error "Could not find VocalFold.WebUI/dist folder!"
            System.IO.Path.GetFullPath(possiblePaths.[0])
        )

    Logger.info $"Serving static files from: {distFullPath}"

    let fileProvider = new PhysicalFileProvider(distFullPath)

    app
        .UseDefaultFiles(DefaultFilesOptions(
            FileProvider = fileProvider
        ))
        .UseStaticFiles(StaticFileOptions(
            FileProvider = fileProvider
        ))
        .UseGiraffe (webApp config)
    |> ignore

let configureServices (services: IServiceCollection) =
    // Configure JSON serialization with F# support
    let jsonOptions = JsonSerializerOptions()
    jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonOptions.Converters.Add(System.Text.Json.Serialization.JsonFSharpConverter(System.Text.Json.Serialization.JsonUnionEncoding.Default))

    services
        .AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(jsonOptions))
    |> ignore

    services
        .AddGiraffe()
        .AddCors(fun options ->
            options.AddDefaultPolicy(fun builder ->
                builder
                    .WithOrigins("http://localhost:*")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                |> ignore
            ) |> ignore
        )
    |> ignore

// ============================================================================
// Server Lifecycle
// ============================================================================

/// Start the web server on a random available port
let start (config: ServerConfig) : Async<ServerState> =
    async {
        // Store config globally so handlers can access it
        serverConfig <- Some config

        let port = findAvailablePort()
        let cts = new CancellationTokenSource()

        let host =
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(fun webBuilder ->
                    webBuilder
                        .UseKestrel(fun options ->
                            options.Listen(IPAddress.Loopback, port)
                        )
                        .ConfigureServices(configureServices)
                        .Configure(Action<IApplicationBuilder>(configureApp config))
                        .ConfigureLogging(fun logging ->
                            logging.SetMinimumLevel(LogLevel.Warning) |> ignore
                        )
                    |> ignore
                )
                .Build()

        // Start the host in the background
        do! host.StartAsync(cts.Token) |> Async.AwaitTask

        Logger.info $"Web server started on http://localhost:{port}"

        // Set up file watcher for keywords file
        let settings = Settings.load()
        let keywordsPath = Settings.getKeywordsFilePath settings

        let reloadCallback = fun () ->
            try
                Logger.info "🔄 Reloading keywords from external file..."
                let keywordData = Settings.loadKeywordData keywordsPath
                config.OnKeywordsChanged keywordData
                Logger.info "✅ Keywords reloaded successfully"
            with
            | ex ->
                Logger.error (sprintf "Failed to reload keywords: %s" ex.Message)

        let fileWatcher =
            try
                if File.Exists(keywordsPath) then
                    Some (FileWatcher.createWatcher keywordsPath reloadCallback)
                else
                    Logger.info (sprintf "Keywords file not found at startup: %s" keywordsPath)
                    None
            with
            | ex ->
                Logger.warning (sprintf "Could not start file watcher: %s" ex.Message)
                None

        return {
            Port = port
            Host = host
            CancellationTokenSource = cts
            FileWatcherRef = ref fileWatcher
        }
    }

/// Stop the web server
let stop (state: ServerState) : Async<unit> =
    async {
        try
            // Stop file watcher first
            match !state.FileWatcherRef with
            | Some watcher -> FileWatcher.stopWatcher watcher
            | None -> ()

            state.CancellationTokenSource.Cancel()
            do! state.Host.StopAsync() |> Async.AwaitTask
            state.Host.Dispose()
            state.CancellationTokenSource.Dispose()
            Logger.info "Web server stopped"
        with ex ->
            Logger.error $"Error stopping web server: {ex.Message}"
    }

/// Get the URL of the running server
let getUrl (state: ServerState) : string =
    $"http://localhost:{state.Port}"
