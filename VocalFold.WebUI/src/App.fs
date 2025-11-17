module App

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Feliz.UseElmish
open Elmish
open Types

// ============================================================================
// Init
// ============================================================================

let init () =
    let model = {
        CurrentPage = Dashboard
        Settings = NotStarted
        Status = NotStarted
        IsRecordingHotkey = false
        PendingHotkey = None
        EditingKeyword = None
        EditingCategory = None
        ExpandedCategories = Set.empty
        Toasts = []
        KeywordsPath = NotStarted
        EditingKeywordsPath = None
        OpenCommandsPath = NotStarted
        EditingOpenCommandsPath = None
        OpenCommands = NotStarted
        EditingOpenCommand = None
    }
    // Load initial data
    model, Cmd.batch [
        Cmd.ofMsg LoadSettings
        Cmd.ofMsg LoadStatus
        Cmd.ofMsg LoadKeywordsPath
        Cmd.ofMsg LoadOpenCommandsPath
        Cmd.ofMsg LoadOpenCommands
    ]

// ============================================================================
// Update
// ============================================================================

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | NavigateToPage page ->
        { model with CurrentPage = page }, Cmd.none

    | LoadSettings ->
        { model with Settings = LoadingState.Loading },
        Cmd.OfPromise.either
            Api.getSettings
            ()
            SettingsLoaded
            (fun ex -> SettingsLoaded (Result.Error ex.Message))

    | SettingsLoaded (Result.Ok settings) ->
        { model with Settings = LoadingState.Loaded settings },
        Cmd.none

    | SettingsLoaded (Result.Error err) ->
        { model with Settings = LoadingState.Error err },
        Cmd.ofMsg (ShowToast (sprintf "Failed to load settings: %s" err, ToastType.Error))

    | UpdateSettings settings ->
        model,
        Cmd.OfPromise.either
            Api.updateSettings
            settings
            SettingsSaved
            (fun ex -> SettingsSaved (Result.Error ex.Message))

    | SettingsSaved (Result.Ok ()) ->
        model,
        Cmd.ofMsg LoadSettings

    | SettingsSaved (Result.Error err) ->
        model,
        Cmd.ofMsg (ShowToast (sprintf "Failed to save settings: %s" err, ToastType.Error))

    | LoadStatus ->
        { model with Status = LoadingState.Loading },
        Cmd.OfPromise.either
            Api.getStatus
            ()
            StatusLoaded
            (fun ex -> StatusLoaded (Result.Error ex.Message))

    | StatusLoaded (Result.Ok status) ->
        { model with Status = LoadingState.Loaded status }, Cmd.none

    | StatusLoaded (Result.Error err) ->
        { model with Status = LoadingState.Error err }, Cmd.none

    | StartRecordingHotkey ->
        { model with IsRecordingHotkey = true; PendingHotkey = None }, Cmd.none

    | HotkeyRecorded (modifiers, key) ->
        // Just store the pending hotkey, don't apply it yet
        // Validate that at least one modifier is present
        if modifiers = 0u then
            { model with IsRecordingHotkey = false; PendingHotkey = None },
            Cmd.ofMsg (ShowToast ("Hotkey must include at least one modifier key (Ctrl, Shift, Alt, or Win)", ToastType.Warning))
        else
            { model with IsRecordingHotkey = false; PendingHotkey = Some (modifiers, key) },
            Cmd.none

    | ApplyHotkey ->
        match model.PendingHotkey, model.Settings with
        | Some (modifiers, key), LoadingState.Loaded settings ->
            let updatedSettings = {
                settings with
                    HotkeyModifiers = modifiers
                    HotkeyKey = key
            }
            { model with PendingHotkey = None },
            Cmd.ofMsg (UpdateSettings updatedSettings)
        | _ ->
            model, Cmd.none

    | CancelRecordingHotkey ->
        { model with IsRecordingHotkey = false; PendingHotkey = None }, Cmd.none

    | AddKeyword ->
        { model with EditingKeyword = Some (-1, { Keyword = ""; Replacement = ""; Category = None }) }, Cmd.none

    | EditKeyword index ->
        match model.Settings with
        | LoadingState.Loaded settings when index >= 0 && index < settings.KeywordReplacements.Length ->
            { model with EditingKeyword = Some (index, settings.KeywordReplacements.[index]) }, Cmd.none
        | _ ->
            model, Cmd.none

    | SaveKeyword keyword ->
        match model.Settings, model.EditingKeyword with
        | LoadingState.Loaded settings, Some (index, _) when index >= 0 ->
            // Update existing keyword
            model,
            Cmd.OfPromise.either
                (Api.updateKeyword index)
                keyword
                (fun _ -> SettingsSaved (Result.Ok ()))
                (fun ex -> SettingsSaved (Result.Error ex.Message))
        | LoadingState.Loaded settings, Some (-1, _) ->
            // Add new keyword
            model,
            Cmd.OfPromise.either
                Api.addKeyword
                keyword
                (fun _ -> SettingsSaved (Result.Ok ()))
                (fun ex -> SettingsSaved (Result.Error ex.Message))
        | _ ->
            model, Cmd.none

    | DeleteKeyword index ->
        model,
        Cmd.OfPromise.either
            Api.deleteKeyword
            index
            (fun _ -> SettingsSaved (Result.Ok ()))
            (fun ex -> SettingsSaved (Result.Error ex.Message))

    | CancelEditKeyword ->
        { model with EditingKeyword = None }, Cmd.none

    | AddExampleKeywords ->
        model,
        Cmd.OfPromise.either
            Api.addExampleKeywords
            ()
            ExampleKeywordsAdded
            (fun ex -> ExampleKeywordsAdded (Result.Error ex.Message))

    | ExampleKeywordsAdded (Result.Ok count) ->
        model,
        Cmd.batch [
            Cmd.ofMsg LoadSettings
            Cmd.ofMsg (ShowToast (sprintf "Added %d example keywords" count, ToastType.Success))
        ]

    | ExampleKeywordsAdded (Result.Error err) ->
        model,
        Cmd.ofMsg (ShowToast (sprintf "Failed to add examples: %s" err, ToastType.Error))

    | ShowToast (message, toastType) ->
        let toast = {
            Id = System.Guid.NewGuid().ToString()
            Message = message
            Type = toastType
        }
        { model with Toasts = toast :: model.Toasts }, Cmd.none

    | DismissToast id ->
        { model with Toasts = model.Toasts |> List.filter (fun t -> t.Id <> id) }, Cmd.none

    | LoadOpenCommands ->
        model,
        Cmd.OfPromise.either
            Api.getOpenCommands
            ()
            OpenCommandsLoaded
            (fun ex -> OpenCommandsLoaded (Result.Error ex.Message))

    | OpenCommandsLoaded result ->
        match result with
        | Result.Ok commands ->
            { model with OpenCommands = Loaded commands }, Cmd.none
        | Result.Error err ->
            { model with OpenCommands = LoadingState.Error err }, Cmd.none

    | AddOpenCommand ->
        { model with EditingOpenCommand = Some (-1, { Keyword = ""; Description = None; Targets = []; LaunchDelay = None }) }, Cmd.none

    | EditOpenCommand index ->
        match model.OpenCommands with
        | LoadingState.Loaded commands when index >= 0 && index < commands.Length ->
            { model with EditingOpenCommand = Some (index, commands.[index]) }, Cmd.none
        | _ ->
            model, Cmd.none

    | UpdateEditingOpenCommand command ->
        match model.EditingOpenCommand with
        | Some (index, _) ->
            { model with EditingOpenCommand = Some (index, command) }, Cmd.none
        | None ->
            model, Cmd.none

    | SaveOpenCommand command ->
        match model.OpenCommands, model.EditingOpenCommand with
        | LoadingState.Loaded commands, Some (index, _) when index >= 0 ->
            // Update existing command
            { model with EditingOpenCommand = None },
            Cmd.OfPromise.either
                (Api.updateOpenCommand index)
                command
                (fun _ -> LoadOpenCommands)
                (fun ex -> ShowToast (sprintf "Failed to save command: %s" ex.Message, ToastType.Error))
        | LoadingState.Loaded commands, Some (-1, _) ->
            // Add new command
            { model with EditingOpenCommand = None },
            Cmd.OfPromise.either
                Api.createOpenCommand
                command
                (fun _ -> LoadOpenCommands)
                (fun ex -> ShowToast (sprintf "Failed to create command: %s" ex.Message, ToastType.Error))
        | _ ->
            model, Cmd.none

    | DeleteOpenCommand index ->
        model,
        Cmd.OfPromise.either
            Api.deleteOpenCommand
            index
            (fun _ -> LoadOpenCommands)
            (fun ex -> ShowToast (sprintf "Failed to delete command: %s" ex.Message, ToastType.Error))

    | CancelEditOpenCommand ->
        { model with EditingOpenCommand = None }, Cmd.none

    | TestOpenCommand command ->
        model, Cmd.none

    | OpenCommandTested result ->
        model, Cmd.none

    | LoadKeywords ->
        model, Cmd.ofMsg LoadSettings

    | KeywordsLoaded _ ->
        model, Cmd.none

    | MoveKeywordToCategory (index, category) ->
        model,
        Cmd.OfPromise.either
            (Api.moveKeywordToCategory index)
            category
            (fun _ -> SettingsSaved (Result.Ok ()))
            (fun ex -> SettingsSaved (Result.Error ex.Message))

    | ToggleCategory name ->
        let expandedCategories =
            if model.ExpandedCategories.Contains name then
                model.ExpandedCategories.Remove name
            else
                model.ExpandedCategories.Add name
        { model with ExpandedCategories = expandedCategories }, Cmd.none

    | AddCategory ->
        { model with EditingCategory = Some { Name = ""; IsExpanded = true; Color = None } }, Cmd.none

    | EditCategory name ->
        match model.Settings with
        | LoadingState.Loaded settings ->
            let category = settings.Categories |> List.tryFind (fun c -> c.Name = name)
            { model with EditingCategory = category }, Cmd.none
        | _ ->
            model, Cmd.none

    | SaveCategory category ->
        model,
        Cmd.OfPromise.either
            Api.createCategory
            category
            (fun _ -> SettingsSaved (Result.Ok ()))
            (fun ex -> SettingsSaved (Result.Error ex.Message))

    | DeleteCategory name ->
        model,
        Cmd.OfPromise.either
            Api.deleteCategory
            name
            (fun _ -> SettingsSaved (Result.Ok ()))
            (fun ex -> SettingsSaved (Result.Error ex.Message))

    | CancelEditCategory ->
        { model with EditingCategory = None }, Cmd.none

    | LoadKeywordsPath ->
        { model with KeywordsPath = LoadingState.Loading },
        Cmd.OfPromise.either
            Api.getKeywordsPath
            ()
            KeywordsPathLoaded
            (fun ex -> KeywordsPathLoaded (Result.Error ex.Message))

    | KeywordsPathLoaded (Result.Ok pathInfo) ->
        { model with KeywordsPath = LoadingState.Loaded pathInfo }, Cmd.none

    | KeywordsPathLoaded (Result.Error err) ->
        { model with KeywordsPath = LoadingState.Error err },
        Cmd.ofMsg (ShowToast (sprintf "Failed to load keywords path: %s" err, ToastType.Error))

    | StartEditingKeywordsPath ->
        match model.KeywordsPath with
        | LoadingState.Loaded pathInfo ->
            { model with EditingKeywordsPath = Some pathInfo.CurrentPath }, Cmd.none
        | _ ->
            model, Cmd.none

    | UpdateEditingKeywordsPath path ->
        { model with EditingKeywordsPath = Some path }, Cmd.none

    | SaveKeywordsPath ->
        match model.EditingKeywordsPath with
        | Some path when path.Trim() <> "" ->
            { model with EditingKeywordsPath = None },
            Cmd.OfPromise.either
                Api.updateKeywordsPath
                (Some path)
                KeywordsPathSaved
                (fun ex -> KeywordsPathSaved (Result.Error ex.Message))
        | _ ->
            model, Cmd.none

    | KeywordsPathSaved (Result.Ok newPath) ->
        model,
        Cmd.batch [
            Cmd.ofMsg LoadKeywordsPath
            Cmd.ofMsg (ShowToast (sprintf "Keywords file path updated to: %s" newPath, ToastType.Success))
        ]

    | KeywordsPathSaved (Result.Error err) ->
        model,
        Cmd.ofMsg (ShowToast (sprintf "Failed to update keywords path: %s" err, ToastType.Error))

    | ResetKeywordsPathToDefault ->
        { model with EditingKeywordsPath = None },
        Cmd.OfPromise.either
            Api.updateKeywordsPath
            None
            KeywordsPathSaved
            (fun ex -> KeywordsPathSaved (Result.Error ex.Message))

    | CancelEditingKeywordsPath ->
        { model with EditingKeywordsPath = None }, Cmd.none

    | ExportKeywords (targetPath, setAsActive) ->
        model,
        Cmd.OfPromise.either
            (fun (p, a) -> Api.exportKeywordsToFile p a)
            (targetPath, setAsActive)
            KeywordsExported
            (fun ex -> KeywordsExported (Result.Error ex.Message))

    | KeywordsExported (Result.Ok newPath) ->
        model,
        Cmd.batch [
            Cmd.ofMsg LoadKeywordsPath
            Cmd.ofMsg (ShowToast (sprintf "Keywords exported to: %s" newPath, ToastType.Success))
        ]

    | KeywordsExported (Result.Error err) ->
        model,
        Cmd.ofMsg (ShowToast (sprintf "Failed to export keywords: %s" err, ToastType.Error))

    | LoadOpenCommandsPath ->
        { model with OpenCommandsPath = LoadingState.Loading },
        Cmd.OfPromise.either
            Api.getOpenCommandsPath
            ()
            OpenCommandsPathLoaded
            (fun ex -> OpenCommandsPathLoaded (Result.Error ex.Message))

    | OpenCommandsPathLoaded (Result.Ok pathInfo) ->
        { model with OpenCommandsPath = LoadingState.Loaded pathInfo }, Cmd.none

    | OpenCommandsPathLoaded (Result.Error err) ->
        { model with OpenCommandsPath = LoadingState.Error err },
        Cmd.ofMsg (ShowToast (sprintf "Failed to load open commands path: %s" err, ToastType.Error))

    | StartEditingOpenCommandsPath ->
        match model.OpenCommandsPath with
        | LoadingState.Loaded pathInfo ->
            { model with EditingOpenCommandsPath = Some pathInfo.CurrentPath }, Cmd.none
        | _ ->
            model, Cmd.none

    | UpdateEditingOpenCommandsPath path ->
        { model with EditingOpenCommandsPath = Some path }, Cmd.none

    | SaveOpenCommandsPath ->
        match model.EditingOpenCommandsPath with
        | Some path when path.Trim() <> "" ->
            { model with EditingOpenCommandsPath = None },
            Cmd.OfPromise.either
                Api.updateOpenCommandsPath
                (Some path)
                OpenCommandsPathSaved
                (fun ex -> OpenCommandsPathSaved (Result.Error ex.Message))
        | _ ->
            model, Cmd.none

    | OpenCommandsPathSaved (Result.Ok newPath) ->
        model,
        Cmd.batch [
            Cmd.ofMsg LoadOpenCommandsPath
            Cmd.ofMsg LoadOpenCommands  // Reload commands from the new path
            Cmd.ofMsg (ShowToast (sprintf "Open commands file path updated to: %s" newPath, ToastType.Success))
        ]

    | OpenCommandsPathSaved (Result.Error err) ->
        model,
        Cmd.ofMsg (ShowToast (sprintf "Failed to update open commands path: %s" err, ToastType.Error))

    | ResetOpenCommandsPathToDefault ->
        { model with EditingOpenCommandsPath = None },
        Cmd.OfPromise.either
            Api.updateOpenCommandsPath
            None
            OpenCommandsPathSaved
            (fun ex -> OpenCommandsPathSaved (Result.Error ex.Message))

    | CancelEditingOpenCommandsPath ->
        { model with EditingOpenCommandsPath = None }, Cmd.none

// ============================================================================
// View
// ============================================================================

let private view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "min-h-screen bg-background-dark text-text-primary"
        prop.children [
            Html.div [
                prop.className "flex"
                prop.children [
                    // Sidebar (placeholder)
                    Html.aside [
                        prop.className "w-64 h-screen bg-background-sidebar p-6 flex flex-col"
                        prop.children [
                            Html.div [
                                prop.className "flex items-center gap-3 mb-8"
                                prop.children [
                                    Html.img [
                                        prop.src "/logo.png"
                                        prop.alt "VocalFold Logo"
                                        prop.className "w-10 h-10"
                                    ]
                                    Html.h1 [
                                        prop.className "text-2xl font-bold text-primary"
                                        prop.text "VocalFold"
                                    ]
                                ]
                            ]
                            Html.nav [
                                prop.className "flex flex-col flex-1 overflow-hidden"
                                prop.children [
                                    Html.div [
                                        prop.className "flex-1 overflow-y-auto"
                                        prop.children [
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth"
                                                prop.text "Dashboard"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage Dashboard))
                                            ]
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth"
                                                prop.text "System"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage SystemSettings))
                                            ]
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth"
                                                prop.text "General"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage GeneralSettings))
                                            ]
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth"
                                                prop.text "Keywords"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage KeywordSettings))
                                            ]
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth"
                                                prop.text "Open Commands"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage OpenCommandsSettings))
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "pt-4 border-t border-text-secondary/20"
                                        prop.children [
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth text-secondary font-bold"
                                                prop.text "Changelog"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage Changelog))
                                            ]
                                            Html.button [
                                                prop.className "w-full text-left px-4 py-2 rounded hover:bg-primary/20 transition-smooth text-secondary font-bold"
                                                prop.text "About"
                                                prop.onClick (fun _ -> dispatch (NavigateToPage About))
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Main content
                    Html.main [
                        prop.className "flex-1 p-8"
                        prop.children [
                            match model.CurrentPage with
                            | Dashboard ->
                                Components.Dashboard.view model.Status model.Settings dispatch
                            | SystemSettings ->
                                Components.SystemSettings.view model.Settings model.IsRecordingHotkey model.PendingHotkey dispatch
                            | GeneralSettings ->
                                Components.GeneralSettings.view model.Settings model.IsRecordingHotkey model.PendingHotkey model.KeywordsPath model.EditingKeywordsPath model.OpenCommandsPath model.EditingOpenCommandsPath dispatch
                            | KeywordSettings ->
                                Components.KeywordManager.view model.Settings model.EditingKeyword model.EditingCategory model.ExpandedCategories dispatch
                            | OpenCommandsSettings ->
                                Components.OpenCommandsSettings.view model.OpenCommands model.EditingOpenCommand dispatch
                            | Changelog ->
                                Components.Changelog.view dispatch
                            | About ->
                                Components.About.view dispatch
                        ]
                    ]
                ]
            ]

            // Toast notifications (placeholder)
            Html.div [
                prop.className "fixed top-4 right-4 space-y-2"
                prop.children (
                    model.Toasts
                    |> List.map (fun toast ->
                        let toastBorderColor =
                            match toast.Type with
                            | ToastType.Success -> "border-green-500"
                            | ToastType.Error -> "border-red-500"
                            | ToastType.Info -> "border-blue-500"
                            | ToastType.Warning -> "border-yellow-500"
                        let toastClassName = sprintf "bg-background-card p-4 rounded shadow-lg border-l-4 %s" toastBorderColor
                        Html.div [
                            prop.key toast.Id
                            prop.className toastClassName
                            prop.children [
                                Html.p toast.Message
                                Html.button [
                                    prop.className "text-sm text-text-secondary hover:text-text-primary"
                                    prop.text "Dismiss"
                                    prop.onClick (fun _ -> dispatch (DismissToast toast.Id))
                                ]
                            ]
                        ]
                    )
                )
            ]
        ]
    ]

// ============================================================================
// App
// ============================================================================

[<ReactComponent>]
let App () =
    let model, dispatch = React.useElmish(init, update, [||])
    view model dispatch

// ============================================================================
// Mount
// ============================================================================

open Browser.Dom

let root = document.getElementById "root"
let app = ReactDOM.createRoot root
app.render(App())
