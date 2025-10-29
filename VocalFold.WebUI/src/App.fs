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
        Toasts = []
    }
    // Load initial data
    model, Cmd.batch [
        Cmd.ofMsg LoadSettings
        Cmd.ofMsg LoadStatus
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
        { model with EditingKeyword = Some (-1, { Keyword = ""; Replacement = "" }) }, Cmd.none

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

    | _ ->
        // Placeholder for other messages
        model, Cmd.none

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
                        prop.className "w-64 h-screen bg-background-sidebar p-6"
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
                                Components.GeneralSettings.view model.Settings model.IsRecordingHotkey model.PendingHotkey dispatch
                            | KeywordSettings ->
                                Components.KeywordManager.view model.Settings model.EditingKeyword dispatch
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
