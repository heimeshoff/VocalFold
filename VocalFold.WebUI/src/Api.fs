module Api

open Fable.Core
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Thoth.Json
open Types

// ============================================================================
// JSON Encoders/Decoders
// ============================================================================

let keywordCategoryDecoder: Decoder<KeywordCategory> =
    Decode.object (fun get -> {
        Name = get.Required.Field "name" Decode.string
        IsExpanded = get.Required.Field "isExpanded" Decode.bool
        Color = get.Optional.Field "color" Decode.string
    })

let keywordCategoryEncoder (kc: KeywordCategory) =
    Encode.object [
        "name", Encode.string kc.Name
        "isExpanded", Encode.bool kc.IsExpanded
        "color", (match kc.Color with | Some c -> Encode.string c | None -> Encode.nil)
    ]

let keywordReplacementDecoder: Decoder<KeywordReplacement> =
    Decode.object (fun get -> {
        Keyword = get.Required.Field "keyword" Decode.string
        Replacement = get.Required.Field "replacement" Decode.string
        Category = get.Optional.Field "category" Decode.string
    })

let keywordReplacementEncoder (kr: KeywordReplacement) =
    Encode.object [
        "keyword", Encode.string kr.Keyword
        "replacement", Encode.string kr.Replacement
        "category", (match kr.Category with | Some c -> Encode.string c | None -> Encode.nil)
    ]

let appSettingsDecoder: Decoder<AppSettings> =
    Decode.object (fun get -> {
        HotkeyKey = get.Required.Field "hotkeyKey" Decode.uint32
        HotkeyModifiers = get.Required.Field "hotkeyModifiers" Decode.uint32
        ModelSize = get.Required.Field "modelSize" Decode.string
        RecordingDuration = get.Required.Field "recordingDuration" Decode.int
        TypingSpeed = get.Required.Field "typingSpeed" Decode.string
        StartWithWindows = get.Optional.Field "startWithWindows" Decode.bool |> Option.defaultValue false
        // Phase 15: Keywords are now in external file, these are deprecated but kept for backward compatibility
        KeywordReplacements = get.Optional.Field "keywordReplacements" (Decode.list keywordReplacementDecoder) |> Option.defaultValue []
        Categories = get.Optional.Field "categories" (Decode.list keywordCategoryDecoder) |> Option.defaultValue []
    })

let appSettingsEncoder (settings: AppSettings) =
    Encode.object [
        "hotkeyKey", Encode.uint32 settings.HotkeyKey
        "hotkeyModifiers", Encode.uint32 settings.HotkeyModifiers
        "modelSize", Encode.string settings.ModelSize
        "recordingDuration", Encode.int settings.RecordingDuration
        "typingSpeed", Encode.string settings.TypingSpeed
        "startWithWindows", Encode.bool settings.StartWithWindows
        "keywordReplacements", Encode.list (List.map keywordReplacementEncoder settings.KeywordReplacements)
        "categories", Encode.list (List.map keywordCategoryEncoder settings.Categories)
    ]

let appStatusDecoder: Decoder<AppStatus> =
    Decode.object (fun get -> {
        Version = get.Required.Field "version" Decode.string
        CurrentHotkey = get.Required.Field "currentHotkey" Decode.string
    })

// ============================================================================
// API Base URL
// ============================================================================

let private baseUrl = "" // Empty means same origin (will use current host)

// ============================================================================
// API Functions
// ============================================================================

/// Get current application status
let getStatus () : JS.Promise<Result<AppStatus, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/status") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString appStatusDecoder response.responseText with
                | Result.Ok status -> return Result.Ok status
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get status: %s" ex.Message)
    } |> Async.StartAsPromise

/// Get current settings
let getSettings () : JS.Promise<Result<AppSettings, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/settings") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString appSettingsDecoder response.responseText with
                | Result.Ok settings -> return Result.Ok settings
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get settings: %s" ex.Message)
    } |> Async.StartAsPromise

/// Update settings
let updateSettings (settings: AppSettings) : JS.Promise<Result<unit, string>> =
    async {
        try
            let json = Encode.toString 0 (appSettingsEncoder settings)
            let! response =
                Http.request (baseUrl + "/api/settings")
                |> Http.method PUT
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to update settings: %s" ex.Message)
    } |> Async.StartAsPromise

/// Get all keywords
let getKeywords () : JS.Promise<Result<KeywordReplacement list, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/keywords") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString (Decode.list keywordReplacementDecoder) response.responseText with
                | Result.Ok keywords -> return Result.Ok keywords
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get keywords: %s" ex.Message)
    } |> Async.StartAsPromise

/// Add a new keyword
let addKeyword (keyword: KeywordReplacement) : JS.Promise<Result<unit, string>> =
    async {
        try
            let json = Encode.toString 0 (keywordReplacementEncoder keyword)
            let! response =
                Http.request (baseUrl + "/api/keywords")
                |> Http.method POST
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to add keyword: %s" ex.Message)
    } |> Async.StartAsPromise

/// Update a keyword at the given index
let updateKeyword (index: int) (keyword: KeywordReplacement) : JS.Promise<Result<unit, string>> =
    async {
        try
            let json = Encode.toString 0 (keywordReplacementEncoder keyword)
            let! response =
                Http.request (baseUrl + sprintf "/api/keywords/%d" index)
                |> Http.method PUT
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to update keyword: %s" ex.Message)
    } |> Async.StartAsPromise

/// Delete a keyword at the given index
let deleteKeyword (index: int) : JS.Promise<Result<unit, string>> =
    async {
        try
            let! response =
                Http.request (baseUrl + sprintf "/api/keywords/%d" index)
                |> Http.method DELETE
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to delete keyword: %s" ex.Message)
    } |> Async.StartAsPromise

/// Add example keywords
let addExampleKeywords () : JS.Promise<Result<int, string>> =
    async {
        try
            let! response =
                Http.request (baseUrl + "/api/keywords/examples")
                |> Http.method POST
                |> Http.send

            match response.statusCode with
            | 200 ->
                let decoder = Decode.object (fun get -> get.Required.Field "added" Decode.int)
                match Decode.fromString decoder response.responseText with
                | Result.Ok count -> return Result.Ok count
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to add example keywords: %s" ex.Message)
    } |> Async.StartAsPromise

// ============================================================================
// Category API Functions
// ============================================================================

/// Move a keyword to a different category
let moveKeywordToCategory (index: int) (category: string option) : JS.Promise<Result<unit, string>> =
    async {
        try
            let categoryValue = match category with | Some c -> Encode.string c | None -> Encode.nil
            let json = Encode.toString 0 (Encode.object [ "category", categoryValue ])
            let! response =
                Http.request (baseUrl + sprintf "/api/keywords/%d/category" index)
                |> Http.method PUT
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to move keyword: %s" ex.Message)
    } |> Async.StartAsPromise

/// Get all categories
let getCategories () : JS.Promise<Result<KeywordCategory list, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/categories") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString (Decode.list keywordCategoryDecoder) response.responseText with
                | Result.Ok categories -> return Result.Ok categories
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get categories: %s" ex.Message)
    } |> Async.StartAsPromise

/// Create a new category
let createCategory (category: KeywordCategory) : JS.Promise<Result<unit, string>> =
    async {
        try
            let json = Encode.toString 0 (keywordCategoryEncoder category)
            let! response =
                Http.request (baseUrl + "/api/categories")
                |> Http.method POST
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to create category: %s" ex.Message)
    } |> Async.StartAsPromise

/// Update a category
let updateCategory (name: string) (category: KeywordCategory) : JS.Promise<Result<unit, string>> =
    async {
        try
            let json = Encode.toString 0 (keywordCategoryEncoder category)
            let! response =
                Http.request (baseUrl + sprintf "/api/categories/%s" name)
                |> Http.method PUT
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to update category: %s" ex.Message)
    } |> Async.StartAsPromise

/// Delete a category
let deleteCategory (name: string) : JS.Promise<Result<unit, string>> =
    async {
        try
            let! response =
                Http.request (baseUrl + sprintf "/api/categories/%s" name)
                |> Http.method DELETE
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to delete category: %s" ex.Message)
    } |> Async.StartAsPromise

/// Toggle category expanded state
let toggleCategoryState (name: string) : JS.Promise<Result<unit, string>> =
    async {
        try
            let! response =
                Http.request (baseUrl + sprintf "/api/categories/%s/state" name)
                |> Http.method PUT
                |> Http.send

            match response.statusCode with
            | 200 -> return Result.Ok ()
            | code -> return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to toggle category state: %s" ex.Message)
    } |> Async.StartAsPromise

// ============================================================================
// Keywords File Path API Functions
// ============================================================================

let keywordsPathInfoDecoder: Decoder<Types.KeywordsPathInfo> =
    Decode.object (fun get -> {
        CurrentPath = get.Required.Field "currentPath" Decode.string
        DefaultPath = get.Required.Field "defaultPath" Decode.string
        IsDefault = get.Required.Field "isDefault" Decode.bool
    })

/// Get the current keywords file path
let getKeywordsPath () : JS.Promise<Result<Types.KeywordsPathInfo, string>> =
    async {
        try
            let! response = Http.request (baseUrl + "/api/settings/keywords-path") |> Http.method GET |> Http.send

            match response.statusCode with
            | 200 ->
                match Decode.fromString keywordsPathInfoDecoder response.responseText with
                | Result.Ok pathInfo -> return Result.Ok pathInfo
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to get keywords path: %s" ex.Message)
    } |> Async.StartAsPromise

/// Update the keywords file path (or reset to default if None)
let updateKeywordsPath (path: string option) : JS.Promise<Result<string, string>> =
    async {
        try
            let pathValue = match path with | Some p -> Encode.string p | None -> Encode.nil
            let json = Encode.toString 0 (Encode.object [ "path", pathValue ])
            let! response =
                Http.request (baseUrl + "/api/settings/keywords-path")
                |> Http.method PUT
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 ->
                let decoder = Decode.object (fun get -> get.Required.Field "path" Decode.string)
                match Decode.fromString decoder response.responseText with
                | Result.Ok newPath -> return Result.Ok newPath
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to update keywords path: %s" ex.Message)
    } |> Async.StartAsPromise

/// Export keywords to a file
let exportKeywordsToFile (targetPath: string) (setAsActive: bool) : JS.Promise<Result<string, string>> =
    async {
        try
            let json = Encode.toString 0 (Encode.object [
                "targetPath", Encode.string targetPath
                "setAsActive", Encode.bool setAsActive
            ])
            let! response =
                Http.request (baseUrl + "/api/keywords/export-to-file")
                |> Http.method POST
                |> Http.content (BodyContent.Text json)
                |> Http.header (Headers.contentType "application/json")
                |> Http.send

            match response.statusCode with
            | 200 ->
                let decoder = Decode.object (fun get -> get.Required.Field "path" Decode.string)
                match Decode.fromString decoder response.responseText with
                | Result.Ok newPath -> return Result.Ok newPath
                | Result.Error err -> return Result.Error err
            | code ->
                return Result.Error (sprintf "HTTP %d: %s" code response.responseText)
        with ex ->
            return Result.Error (sprintf "Failed to export keywords: %s" ex.Message)
    } |> Async.StartAsPromise

