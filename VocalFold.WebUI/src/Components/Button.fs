module Components.Button

open Feliz

type ButtonVariant =
    | Primary
    | Secondary
    | Success
    | Danger
    | Ghost

type ButtonSize =
    | Small
    | Medium
    | Large

type ButtonProps = {
    Text: string
    OnClick: unit -> unit
    Variant: ButtonVariant
    Size: ButtonSize
    Disabled: bool
    ClassName: string option
}

let private getVariantClasses variant =
    match variant with
    | Primary -> "bg-primary hover:bg-primary-600 text-white"
    | Secondary -> "bg-secondary hover:bg-secondary-600 text-white"
    | Success -> "bg-green-500 hover:bg-green-600 text-white"
    | Danger -> "bg-red-500 hover:bg-red-600 text-white"
    | Ghost -> "bg-transparent hover:bg-white/10 text-text-primary border border-text-secondary"

let private getSizeClasses size =
    match size with
    | Small -> "px-3 py-1.5 text-sm"
    | Medium -> "px-4 py-2 text-base"
    | Large -> "px-6 py-3 text-lg"

let button (props: ButtonProps) =
    let className = sprintf "rounded font-medium transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed %s %s %s" (getVariantClasses props.Variant) (getSizeClasses props.Size) (props.ClassName |> Option.defaultValue "")
    Html.button [
        prop.className className
        prop.onClick (fun _ -> if not props.Disabled then props.OnClick())
        prop.disabled props.Disabled
        prop.text props.Text
    ]

let primaryButton text onClick =
    button {
        Text = text
        OnClick = onClick
        Variant = Primary
        Size = Medium
        Disabled = false
        ClassName = None
    }

let secondaryButton text onClick =
    button {
        Text = text
        OnClick = onClick
        Variant = Secondary
        Size = Medium
        Disabled = false
        ClassName = None
    }

let dangerButton text onClick =
    button {
        Text = text
        OnClick = onClick
        Variant = Danger
        Size = Medium
        Disabled = false
        ClassName = None
    }
