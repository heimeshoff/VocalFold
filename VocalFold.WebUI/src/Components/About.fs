module Components.About

open Feliz
open Types
open Components.Card

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "space-y-6 max-w-4xl mx-auto"
        prop.children [
            Html.h2 [
                prop.className "text-3xl font-bold text-text-primary mb-6"
                prop.text "About VocalFold"
            ]

            // Profile Card
            card {
                Title = ""
                ClassName = Some "bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20"
                Children = [
                    Html.div [
                        prop.className "flex flex-col md:flex-row items-center md:items-start gap-6"
                        prop.children [
                            // Profile Image
                            Html.img [
                                prop.src "/heimeshoff.jpg"
                                prop.alt "Marco Heimeshoff"
                                prop.className "w-48 h-48 rounded-full object-cover shadow-lg border-4 border-primary"
                            ]

                            // Bio Text
                            Html.div [
                                prop.className "flex-1 text-center md:text-left space-y-4"
                                prop.children [
                                    Html.div [
                                        prop.className "text-text-primary space-y-3"
                                        prop.children [
                                            Html.p [
                                                prop.className "text-lg"
                                                prop.children [
                                                    Html.text "👋 Hi, I'm "
                                                    Html.strong [
                                                        prop.className "text-primary"
                                                        prop.text "Marco Heimeshoff"
                                                    ]
                                                    Html.text " - trainer, consultant, and conference organiser focused on "
                                                    Html.strong [
                                                        prop.className "text-primary"
                                                        prop.text "Domain-Driven Design"
                                                    ]
                                                    Html.text " and "
                                                    Html.strong [
                                                        prop.className "text-primary"
                                                        prop.text "collaborative modeling"
                                                    ]
                                                    Html.text "."
                                                ]
                                            ]

                                            Html.p [
                                                prop.className "text-text-secondary"
                                                prop.children [
                                                    Html.text "DDD is all about creating a "
                                                    Html.em [
                                                        prop.text "ubiquitous language"
                                                    ]
                                                    Html.text " within "
                                                    Html.em [
                                                        prop.text "bounded contexts"
                                                    ]
                                                    Html.text " — and with "
                                                    Html.em [
                                                        prop.text "VocalFold"
                                                    ]
                                                    Html.text ", I figured it's time to let your own language flow directly into words on screen. 😉"
                                                ]
                                            ]

                                            Html.p [
                                                prop.className "text-text-secondary"
                                                prop.text "When I'm not helping teams design meaningful software, I enjoy building open-source tools like this one to make life a little smoother."
                                            ]

                                            Html.p [
                                                prop.className "text-text-primary"
                                                prop.children [
                                                    Html.text "If you'd like to "
                                                    Html.strong [
                                                        prop.text "learn more about my trainings or consulting work"
                                                    ]
                                                    Html.text ", feel free to reach out:"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            }

            // Contact Card
            card {
                Title = "Get in Touch"
                ClassName = None
                Children = [
                    Html.div [
                        prop.className "space-y-4"
                        prop.children [
                            // Website
                            Html.div [
                                prop.className "flex items-center space-x-3"
                                prop.children [
                                    Html.span [
                                        prop.className "text-2xl"
                                        prop.text "🌐"
                                    ]
                                    Html.a [
                                        prop.href "https://heimeshoff.de"
                                        prop.target "_blank"
                                        prop.rel "noopener noreferrer"
                                        prop.className "text-primary hover:text-primary-600 transition-colors font-medium"
                                        prop.text "heimeshoff.de"
                                    ]
                                ]
                            ]

                            // Email
                            Html.div [
                                prop.className "flex items-center space-x-3"
                                prop.children [
                                    Html.span [
                                        prop.className "text-2xl"
                                        prop.text "✉️"
                                    ]
                                    Html.a [
                                        prop.href "mailto:marco@heimeshoff.de"
                                        prop.className "text-primary hover:text-primary-600 transition-colors font-medium"
                                        prop.text "marco@heimeshoff.de"
                                    ]
                                ]
                            ]

                            // Twitter/X
                            Html.div [
                                prop.className "flex items-center space-x-3"
                                prop.children [
                                    Html.span [
                                        prop.className "text-2xl"
                                        prop.text "𝕏"
                                    ]
                                    Html.a [
                                        prop.href "https://twitter.com/Heimeshoff"
                                        prop.target "_blank"
                                        prop.rel "noopener noreferrer"
                                        prop.className "text-primary hover:text-primary-600 transition-colors font-medium"
                                        prop.text "@Heimeshoff"
                                    ]
                                ]
                            ]

                            // LinkedIn
                            Html.div [
                                prop.className "flex items-center space-x-3"
                                prop.children [
                                    Html.span [
                                        prop.className "text-2xl"
                                        prop.text "💼"
                                    ]
                                    Html.a [
                                        prop.href "https://linkedin.com/in/heimeshoff"
                                        prop.target "_blank"
                                        prop.rel "noopener noreferrer"
                                        prop.className "text-primary hover:text-primary-600 transition-colors font-medium"
                                        prop.text "linkedin.com/in/heimeshoff"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            }

            // VocalFold Info Card
            card {
                Title = "About VocalFold"
                ClassName = Some "bg-gradient-to-br from-secondary/10 to-primary/10 border border-secondary/20"
                Children = [
                    Html.div [
                        prop.className "space-y-3 text-text-secondary"
                        prop.children [
                            Html.p [
                                prop.text "VocalFold is an open-source voice-to-text application that transcribes your speech directly at your cursor position using AI."
                            ]
                            Html.p [
                                prop.text "Built with F#, Whisper.NET, and modern web technologies, it provides fast, accurate, and privacy-focused speech recognition that runs entirely on your local machine."
                            ]
                            Html.div [
                                prop.className "pt-2"
                                prop.children [
                                    Html.a [
                                        prop.href "https://github.com/heimeshoff/VocalFold"
                                        prop.target "_blank"
                                        prop.rel "noopener noreferrer"
                                        prop.className "inline-flex items-center space-x-2 text-primary hover:text-primary-600 transition-colors font-medium"
                                        prop.children [
                                            Html.span [
                                                prop.text "⭐"
                                            ]
                                            Html.span [
                                                prop.text "View on GitHub"
                                            ]
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
