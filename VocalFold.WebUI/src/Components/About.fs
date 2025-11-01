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

            // Profile and Contact Card
            card {
                Title = ""
                ClassName = Some "bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20"
                Children = [
                    Html.div [
                        prop.className "space-y-8"
                        prop.children [
                            // Profile Section
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
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // Divider
                            Html.div [
                                prop.className "border-t border-primary/20"
                            ]

                            // Contact Section
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    Html.h3 [
                                        prop.className "text-xl font-semibold text-text-primary"
                                        prop.text "Get in Touch"
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

                                            // Bluesky
                                            Html.div [
                                                prop.className "flex items-center space-x-3"
                                                prop.children [
                                                    Html.svg [
                                                        prop.className "w-6 h-6"
                                                        prop.style [ style.color "#1185fe" ]
                                                        prop.custom("viewBox", "0 0 24 24")
                                                        prop.custom("fill", "currentColor")
                                                        prop.children [
                                                            Html.path [
                                                                prop.custom("d", "M12 10.8c-1.087-2.114-4.046-6.053-6.798-7.995C2.566.944 1.561 1.266.902 1.565.139 1.908 0 3.08 0 3.768c0 .69.378 5.65.624 6.479.815 2.736 3.713 3.66 6.383 3.364.136-.02.275-.039.415-.056-.138.022-.276.04-.415.056-3.912.58-7.387 2.005-2.83 7.078 5.013 5.19 6.87-1.113 7.823-4.308.953 3.195 2.05 9.271 7.733 4.308 4.267-4.308 1.172-6.498-2.74-7.078a8.741 8.741 0 0 1-.415-.056c.14.017.279.036.415.056 2.67.297 5.568-.628 6.383-3.364.246-.828.624-5.79.624-6.478 0-.69-.139-1.861-.902-2.206-.659-.298-1.664-.62-4.3 1.24C16.046 4.748 13.087 8.687 12 10.8z")
                                                            ]
                                                        ]
                                                    ]
                                                    Html.a [
                                                        prop.href "https://bsky.app/profile/heimeshoff.de"
                                                        prop.target "_blank"
                                                        prop.rel "noopener noreferrer"
                                                        prop.className "text-primary hover:text-primary-600 transition-colors font-medium"
                                                        prop.text "@Heimeshoff.de"
                                                    ]
                                                ]
                                            ]

                                            // LinkedIn
                                            Html.div [
                                                prop.className "flex items-center space-x-3"
                                                prop.children [
                                                    Html.svg [
                                                        prop.className "w-6 h-6"
                                                        prop.style [ style.color "#0A66C2" ]
                                                        prop.custom("viewBox", "0 0 24 24")
                                                        prop.custom("fill", "currentColor")
                                                        prop.children [
                                                            Html.path [
                                                                prop.custom("d", "M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z")
                                                            ]
                                                        ]
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
                        prop.className "space-y-4 text-text-secondary text-center"
                        prop.children [
                            Html.p [
                                prop.children [
                                    Html.text "If you enjoy "
                                    Html.em [
                                        prop.text "VocalFold"
                                    ]
                                    Html.text " and want to support my open-source work, you can buy me a coffee!"
                                ]
                            ]
                            Html.div [
                                prop.className "flex justify-center py-2"
                                prop.children [
                                    Html.a [
                                        prop.href "https://ko-fi.com/heimeshoff"
                                        prop.target "_blank"
                                        prop.rel "noopener noreferrer"
                                        prop.className "inline-flex items-center justify-center gap-2 px-8 py-4 text-lg font-bold text-white bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 rounded-lg shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
                                        prop.children [
                                            Html.span [
                                                prop.text "☕"
                                            ]
                                            Html.span [
                                                prop.text "Buy me a coffee on Ko-fi"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.p [
                                prop.children [
                                    Html.text "Otherwise, just "
                                    Html.strong [
                                        prop.text "enjoy using VocalFold for free"
                                    ]
                                    Html.text " — and thanks for giving it a try! 😊"
                                ]
                            ]
                            Html.div [
                                prop.className "pt-2 flex justify-center"
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
