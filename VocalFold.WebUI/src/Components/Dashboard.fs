module Components.Dashboard

open Feliz
open Types
open Components.Card
open Components.Button

let private statusCard status dispatch =
    Html.div [
        prop.className "bg-gradient-to-br from-primary/20 to-secondary/20 rounded-lg shadow-lg p-6 border border-primary/30"
        prop.children [
            Html.h3 [
                prop.className "text-xl font-semibold mb-4 text-text-primary"
                prop.text "Status"
            ]
            Html.div [
                prop.className "space-y-4"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-between mb-2"
                        prop.children [
                            Html.span [
                                prop.className "text-text-secondary text-sm"
                                prop.text "Global Hotkey"
                            ]
                            Html.span [
                                prop.className "text-primary font-mono font-semibold"
                                prop.text status.CurrentHotkey
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "flex items-center justify-between"
                        prop.children [
                            Html.span [
                                prop.className "text-text-secondary text-sm"
                                prop.text "Version"
                            ]
                            Html.span [
                                prop.className "text-text-primary font-semibold"
                                prop.text status.Version
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let private quickStatsCard settings =
    card {
        Title = "Quick Stats"
        ClassName = None
        Children = [
            Html.div [
                prop.className "grid grid-cols-2 gap-4"
                prop.children [
                    Html.div [
                        Html.p [
                            prop.className "text-text-secondary text-sm"
                            prop.text "Model"
                        ]
                        Html.p [
                            prop.className "text-primary font-semibold text-lg"
                            prop.text settings.ModelSize
                        ]
                    ]
                    Html.div [
                        Html.p [
                            prop.className "text-text-secondary text-sm"
                            prop.text "Typing Speed"
                        ]
                        Html.p [
                            prop.className "text-secondary font-semibold text-lg"
                            prop.text (settings.TypingSpeed.ToUpper())
                        ]
                    ]
                    Html.div [
                        Html.p [
                            prop.className "text-text-secondary text-sm"
                            prop.text "Keywords"
                        ]
                        Html.p [
                            prop.className "text-primary font-semibold text-lg"
                            prop.text (sprintf "%d configured" settings.KeywordReplacements.Length)
                        ]
                    ]
                    Html.div [
                        Html.p [
                            prop.className "text-text-secondary text-sm"
                            prop.text "Recording Duration"
                        ]
                        Html.p [
                            prop.className "text-secondary font-semibold text-lg"
                            prop.text (if settings.RecordingDuration = 0 then "Hold to speak" else sprintf "%ds" settings.RecordingDuration)
                        ]
                    ]
                ]
            ]
        ]
    }

let private quickActionsCard dispatch =
    card {
        Title = "Quick Actions"
        ClassName = None
        Children = [
            Html.div [
                prop.className "grid grid-cols-2 gap-3"
                prop.children [
                    Html.button [
                        prop.className "px-4 py-3 bg-primary hover:bg-primary-600 text-white rounded-lg font-medium transition-all duration-200 hover:shadow-lg"
                        prop.text "Configure Hotkey"
                        prop.onClick (fun _ -> dispatch (NavigateToPage SystemSettings))
                    ]
                    Html.button [
                        prop.className "px-4 py-3 bg-secondary hover:bg-secondary-600 text-white rounded-lg font-medium transition-all duration-200 hover:shadow-lg"
                        prop.text "Manage Keywords"
                        prop.onClick (fun _ -> dispatch (NavigateToPage KeywordSettings))
                    ]
                ]
            ]
        ]
    }

let private loadingCard title =
    card {
        Title = title
        ClassName = None
        Children = [
            Html.div [
                prop.className "flex items-center justify-center py-8"
                prop.children [
                    Html.div [
                        prop.className "animate-spin rounded-full h-8 w-8 border-b-2 border-primary"
                    ]
                    Html.span [
                        prop.className "ml-3 text-text-secondary"
                        prop.text "Loading..."
                    ]
                ]
            ]
        ]
    }

let private errorCard title error =
    card {
        Title = title
        ClassName = None
        Children = [
            Html.div [
                prop.className "bg-red-500/10 border border-red-500/30 rounded p-4"
                prop.children [
                    Html.p [
                        prop.className "text-red-500 text-sm"
                        prop.text (sprintf "Error: %s" error)
                    ]
                ]
            ]
        ]
    }

let view (status: LoadingState<AppStatus>) (settings: LoadingState<AppSettings>) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "Dashboard"
            ]

            // Status Card
            match status with
            | LoadingState.Loaded s -> statusCard s dispatch
            | LoadingState.Loading -> loadingCard "Status"
            | LoadingState.Error err -> errorCard "Status" err
            | LoadingState.NotStarted -> loadingCard "Status"

            // Grid for Quick Stats and Quick Actions
            Html.div [
                prop.className "grid grid-cols-1 md:grid-cols-2 gap-6"
                prop.children [
                    match settings with
                    | LoadingState.Loaded s -> quickStatsCard s
                    | LoadingState.Loading -> loadingCard "Quick Stats"
                    | LoadingState.Error err -> errorCard "Quick Stats" err
                    | LoadingState.NotStarted -> loadingCard "Quick Stats"

                    quickActionsCard dispatch
                ]
            ]

            // Info Box
            Html.div [
                prop.className "bg-primary/10 border border-primary/30 rounded-lg p-4"
                prop.children [
                    Html.div [
                        prop.className "flex items-start space-x-3"
                        prop.children [
                            Html.div [
                                prop.className "text-primary text-xl"
                                prop.text "ℹ️"
                            ]
                            Html.div [
                                Html.p [
                                    prop.className "text-text-primary font-medium mb-1"
                                    prop.text "How to use VocalFold"
                                ]
                                Html.p [
                                    prop.className "text-text-secondary text-sm"
                                    prop.text "Press your global hotkey to start recording. Speak your text, and it will be typed automatically at your cursor position. Use keywords to quickly insert frequently used text."
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
