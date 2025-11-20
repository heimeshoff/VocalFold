module Components.Changelog

open Feliz
open Types
open Components.Card

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6 max-w-4xl mx-auto"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "Changelog"
            ]

            card {
                Title = ""
                ClassName = Some "bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20"
                Children = [
                    Html.div [
                        prop.className "prose prose-invert max-w-none"
                        prop.children [
                            // Version 1.3.0
                            Html.div [
                                prop.className "mb-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-baseline gap-3 mb-4"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-2xl font-bold text-primary"
                                                prop.text "[1.3.0]"
                                            ]
                                            Html.span [
                                                prop.className "text-text-secondary"
                                                prop.text "- 2025-11-20"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Added"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "Executable Commands feature with application launcher functionality"
                                                            Html.li "Usage tracking for commands and keyword replacements"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]


                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // Version 1.2.3
                            Html.div [
                                prop.className "mb-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-baseline gap-3 mb-4"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-2xl font-bold text-primary"
                                                prop.text "[1.2.3]"
                                            ]
                                            Html.span [
                                                prop.className "text-text-secondary"
                                                prop.text "- 2025-11-06"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Added"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "Automatic Releases on GitHub when a new Version is tagged"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]


                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // Version 1.1.0
                            Html.div [
                                prop.className "mb-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-baseline gap-3 mb-4"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-2xl font-bold text-primary"
                                                prop.text "[1.1.0]"
                                            ]
                                            Html.span [
                                                prop.className "text-text-secondary"
                                                prop.text "- 2025-11-03"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Added"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "Microphone Settings page with device selection and testing capabilities"
                                                            Html.li "Real-time audio level monitoring and visualization"
                                                            Html.li "Microphone test functionality with transcription testing"
                                                            Html.li "Changelog page to track version history"
                                                            Html.li "External configuration file support for keywords (for GDrive/OneDrive/etc. sync)"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Changed"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "Improved settings UI with file selection for keywords path"
                                                            Html.li "Enhanced AudioRecorder with level and spectrum callbacks"
                                                            Html.li "Updated SystemSettings to integrate microphone configuration"
                                                            Html.li "Reduced verbosity of Fast Fourier Transform log output"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Fixed"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "HTTP 500 error when saving settings changes"
                                                            Html.li "Settings API error handling and response serialization"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]


                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // Version 1.0.0
                            Html.div [
                                prop.className "mb-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-baseline gap-3 mb-4"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-2xl font-bold text-primary"
                                                prop.text "[1.0.0]"
                                            ]
                                            Html.span [
                                                prop.className "text-text-secondary"
                                                prop.text "- 2025-11-01"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-3"
                                        prop.children [
                                            Html.div [
                                                prop.children [
                                                    Html.h4 [
                                                        prop.className "text-lg font-semibold text-text-primary mb-2"
                                                        prop.text "Added"
                                                    ]
                                                    Html.ul [
                                                        prop.className "list-disc list-inside text-text-secondary space-y-1"
                                                        prop.children [
                                                            Html.li "Initial public release"
                                                            Html.li "Voice-to-text transcription with Whisper"
                                                            Html.li "Keyword replacement system with categories"
                                                            Html.li "Hotkey support for quick voice input"
                                                            Html.li "System tray integration"
                                                            Html.li "Auto-start with Windows option"
                                                            Html.li "General and system settings management"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            Html.hr [
                                prop.className "border-text-secondary/20 my-6"
                            ]

                            // Footer info
                            Html.div [
                                prop.className "text-sm text-text-secondary/70 space-y-2"
                                prop.children [
                                    Html.p [
                                        prop.children [
                                            Html.text "The format is based on "
                                            Html.a [
                                                prop.href "https://keepachangelog.com/en/1.1.0/"
                                                prop.target "_blank"
                                                prop.rel "noopener noreferrer"
                                                prop.className "text-primary hover:text-primary-600 transition-colors underline"
                                                prop.text "Keep a Changelog"
                                            ]
                                            Html.text ", and this project adheres to "
                                            Html.a [
                                                prop.href "https://semver.org/spec/v2.0.0.html"
                                                prop.target "_blank"
                                                prop.rel "noopener noreferrer"
                                                prop.className "text-primary hover:text-primary-600 transition-colors underline"
                                                prop.text "Semantic Versioning"
                                            ]
                                            Html.text "."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            }
        ]
    ]