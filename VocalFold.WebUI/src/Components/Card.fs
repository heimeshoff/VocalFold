module Components.Card

open Feliz

type CardProps = {
    Title: string
    Children: ReactElement list
    ClassName: string option
}

let card (props: CardProps) =
    let className = sprintf "bg-background-card rounded-lg shadow-lg p-6 %s" (props.ClassName |> Option.defaultValue "")
    Html.div [
        prop.className className
        prop.children [
            Html.h3 [
                prop.className "text-xl font-semibold mb-4 text-text-primary"
                prop.text props.Title
            ]
            Html.div [
                prop.className "space-y-3"
                prop.children props.Children
            ]
        ]
    ]

let statusCard (title: string) (value: string) (color: string) icon =
    Html.div [
        prop.className "bg-background-card rounded-lg shadow-lg p-6 hover:shadow-xl transition-all duration-200"
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    Html.div [
                        Html.h3 [
                            prop.className "text-sm font-medium text-text-secondary mb-1"
                            prop.text title
                        ]
                        Html.p [
                            prop.className (sprintf "text-2xl font-bold %s" color)
                            prop.text value
                        ]
                    ]
                    match icon with
                    | Some iconElement -> iconElement
                    | None -> Html.none
                ]
            ]
        ]
    ]
