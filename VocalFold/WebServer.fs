module WebServer

open System
open System.Net
open System.Net.Sockets
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
}

type ServerState = {
    Port: int
    Host: IHost
    CancellationTokenSource: CancellationTokenSource
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
            return! json settings next ctx
        }

/// Handler for PUT /api/settings
let updateSettingsHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! newSettings = ctx.BindJsonAsync<Settings.AppSettings>()

            // Save settings to disk
            Settings.save newSettings

            // Notify the application of settings change
            config.OnSettingsChanged newSettings

            return! json {| success = true |} next ctx
        }

/// Handler for GET /api/keywords
let getKeywordsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()
            return! json settings.KeywordReplacements next ctx
        }

/// Handler for POST /api/keywords
let addKeywordHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! keyword = ctx.BindJsonAsync<Settings.KeywordReplacement>()
            let settings = Settings.load()
            let updatedSettings = { settings with KeywordReplacements = settings.KeywordReplacements @ [keyword] }
            Settings.save updatedSettings
            config.OnSettingsChanged updatedSettings
            return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/keywords/:index
let updateKeywordHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! keyword = ctx.BindJsonAsync<Settings.KeywordReplacement>()
            let settings = Settings.load()

            if index >= 0 && index < settings.KeywordReplacements.Length then
                let updatedKeywords =
                    settings.KeywordReplacements
                    |> List.mapi (fun i k -> if i = index then keyword else k)
                let updatedSettings = { settings with KeywordReplacements = updatedKeywords }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
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

            if index >= 0 && index < settings.KeywordReplacements.Length then
                let updatedKeywords =
                    settings.KeywordReplacements
                    |> List.mapi (fun i k -> (i, k))
                    |> List.filter (fun (i, _) -> i <> index)
                    |> List.map snd
                let updatedSettings = { settings with KeywordReplacements = updatedKeywords }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
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
            let examples = TextProcessor.getExampleReplacements()
            let updatedSettings = { settings with KeywordReplacements = settings.KeywordReplacements @ examples }
            Settings.save updatedSettings
            config.OnSettingsChanged updatedSettings
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
            return! json settings.Categories next ctx
        }

/// Handler for POST /api/categories
let createCategoryHandler (config: ServerConfig) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! category = ctx.BindJsonAsync<Settings.KeywordCategory>()
            let settings = Settings.load()

            // Check if category name already exists
            let nameExists = settings.Categories |> List.exists (fun c -> c.Name = category.Name)

            if nameExists then
                ctx.SetStatusCode 400
                return! json {| error = "Category name already exists" |} next ctx
            else
                let updatedSettings = { settings with Categories = settings.Categories @ [category] }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
                return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/categories/:name
let updateCategoryHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! updatedCategory = ctx.BindJsonAsync<Settings.KeywordCategory>()
            let settings = Settings.load()

            // Find the category to update
            let categoryExists = settings.Categories |> List.exists (fun c -> c.Name = name)

            if not categoryExists then
                ctx.SetStatusCode 404
                return! json {| error = "Category not found" |} next ctx
            else
                // Update the category
                let updatedCategories =
                    settings.Categories
                    |> List.map (fun c -> if c.Name = name then updatedCategory else c)

                // Also update keywords that reference the old category name (if renamed)
                let updatedKeywords =
                    if name <> updatedCategory.Name then
                        settings.KeywordReplacements
                        |> List.map (fun k ->
                            match k.Category with
                            | Some cat when cat = name -> { k with Category = Some updatedCategory.Name }
                            | _ -> k
                        )
                    else
                        settings.KeywordReplacements

                let updatedSettings = { settings with Categories = updatedCategories; KeywordReplacements = updatedKeywords }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
                return! json {| success = true |} next ctx
        }

/// Handler for DELETE /api/categories/:name
let deleteCategoryHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()

            // Prevent deletion of "Uncategorized"
            if name = "Uncategorized" then
                ctx.SetStatusCode 400
                return! json {| error = "Cannot delete Uncategorized category" |} next ctx
            else
                // Remove the category
                let updatedCategories = settings.Categories |> List.filter (fun c -> c.Name <> name)

                // Move all keywords from this category to "Uncategorized"
                let updatedKeywords =
                    settings.KeywordReplacements
                    |> List.map (fun k ->
                        match k.Category with
                        | Some cat when cat = name -> { k with Category = Some "Uncategorized" }
                        | _ -> k
                    )

                let updatedSettings = { settings with Categories = updatedCategories; KeywordReplacements = updatedKeywords }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
                return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/categories/:name/state
let toggleCategoryStateHandler (config: ServerConfig) (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let settings = Settings.load()

            // Toggle the IsExpanded state
            let updatedCategories =
                settings.Categories
                |> List.map (fun c -> if c.Name = name then { c with IsExpanded = not c.IsExpanded } else c)

            let updatedSettings = { settings with Categories = updatedCategories }
            Settings.save updatedSettings
            config.OnSettingsChanged updatedSettings
            return! json {| success = true |} next ctx
        }

/// Handler for PUT /api/keywords/:index/category
let moveKeywordToCategoryHandler (config: ServerConfig) (index: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.BindJsonAsync<{| category: string option |}>()
            let settings = Settings.load()

            if index >= 0 && index < settings.KeywordReplacements.Length then
                let updatedKeywords =
                    settings.KeywordReplacements
                    |> List.mapi (fun i k -> if i = index then { k with Category = body.category } else k)
                let updatedSettings = { settings with KeywordReplacements = updatedKeywords }
                Settings.save updatedSettings
                config.OnSettingsChanged updatedSettings
                return! json {| success = true |} next ctx
            else
                ctx.SetStatusCode 404
                return! json {| error = "Keyword not found" |} next ctx
        }

// ============================================================================
// Routing
// ============================================================================

let webApp (config: ServerConfig) : HttpHandler =
    choose [
        subRoute "/api" (
            choose [
                GET  >=> route "/status" >=> getStatusHandler
                GET  >=> route "/settings" >=> getSettingsHandler
                PUT  >=> route "/settings" >=> updateSettingsHandler config
                GET  >=> route "/keywords" >=> getKeywordsHandler
                POST >=> route "/keywords" >=> addKeywordHandler config
                POST >=> route "/keywords/examples" >=> addExampleKeywordsHandler config
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

        return {
            Port = port
            Host = host
            CancellationTokenSource = cts
        }
    }

/// Stop the web server
let stop (state: ServerState) : Async<unit> =
    async {
        try
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
