module Components.OpenCommandsSettings

open Feliz
open Feliz.UseElmish
open Elmish
open Types

// ============================================================================
// Open Command Modal Component
// ============================================================================

let private getTypeFriendlyName (targetType: string) =
    match targetType with
    | "executable" -> "Application"
    | "url" -> "URL"
    | "folder" -> "Folder"
    | _ -> targetType

[<ReactComponent>]
let private OpenCommandModal (command: OpenCommand option) (dispatch: Msg -> unit) (onClose: unit -> unit) =
    let keywordInput, setKeywordInput = React.useState(command |> Option.map (fun c -> c.Keyword) |> Option.defaultValue "")
    let descriptionInput, setDescriptionInput = React.useState(command |> Option.bind (fun c -> c.Description) |> Option.defaultValue "")
    let targets, setTargets = React.useState(command |> Option.map (fun c -> c.Targets) |> Option.defaultValue [])
    let editingTarget, setEditingTarget = React.useState(None: int option)

    // Target form state
    let targetName, setTargetName = React.useState("")
    let targetType, setTargetType = React.useState("executable")
    let targetPath, setTargetPath = React.useState("")
    let targetArgs, setTargetArgs = React.useState("")
    let targetWorkingDir, setTargetWorkingDir = React.useState("")

    let isValid = not (System.String.IsNullOrWhiteSpace(keywordInput))

    let clearTargetForm () =
        setTargetName ""
        setTargetType "executable"
        setTargetPath ""
        setTargetArgs ""
        setTargetWorkingDir ""
        setEditingTarget None

    let handleAddTarget () =
        if not (System.String.IsNullOrWhiteSpace(targetName)) && not (System.String.IsNullOrWhiteSpace(targetPath)) then
            let newTarget: LaunchTarget = {
                Name = targetName.Trim()
                Type = targetType
                Path = targetPath.Trim()
                Arguments = if System.String.IsNullOrWhiteSpace(targetArgs) then None else Some (targetArgs.Trim())
                WorkingDirectory = if System.String.IsNullOrWhiteSpace(targetWorkingDir) then None else Some (targetWorkingDir.Trim())
            }

            match editingTarget with
            | Some idx ->
                // Update existing target
                let updatedTargets =
                    targets
                    |> List.mapi (fun i t -> if i = idx then newTarget else t)
                setTargets updatedTargets
            | None ->
                // Add new target
                setTargets (targets @ [newTarget])

            clearTargetForm()

    let handleEditTarget idx =
        let target = targets.[idx]
        setTargetName target.Name
        setTargetType target.Type
        setTargetPath target.Path
        setTargetArgs (target.Arguments |> Option.defaultValue "")
        setTargetWorkingDir (target.WorkingDirectory |> Option.defaultValue "")
        setEditingTarget (Some idx)

    let handleDeleteTarget idx =
        setTargets (targets |> List.mapi (fun i t -> (i, t)) |> List.filter (fun (i, _) -> i <> idx) |> List.map snd)
        if editingTarget = Some idx then
            clearTargetForm()

    let handleSave () =
        if isValid then
            let newCommand = {
                Keyword = keywordInput.Trim()
                Description = if System.String.IsNullOrWhiteSpace(descriptionInput) then None else Some (descriptionInput.Trim())
                Targets = targets
                LaunchDelay = None
                UsageCount = command |> Option.bind (fun c -> c.UsageCount)  // Preserve usage count
            }
            dispatch (SaveOpenCommand newCommand)
            onClose()

    Html.div [
        prop.className "fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50"
        prop.onClick (fun e ->
            if e.target = e.currentTarget then
                onClose()
        )
        prop.children [
            Html.div [
                prop.className "bg-background-card rounded-lg shadow-2xl w-full max-w-4xl mx-4 animate-slide-up"
                prop.onClick (fun e -> e.stopPropagation())
                prop.children [
                    // Header
                    Html.div [
                        prop.className "flex items-center justify-between p-6 border-b border-white/10"
                        prop.children [
                            Html.h3 [
                                prop.className "text-2xl font-bold text-text-primary"
                                prop.text (if command.IsSome then "Edit Command" else "Add Command")
                            ]
                            Html.button [
                                prop.className "text-text-secondary hover:text-text-primary transition-colors text-2xl"
                                prop.text "×"
                                prop.onClick (fun _ -> onClose())
                            ]
                        ]
                    ]

                    // Body - Scrollable
                    Html.div [
                        prop.className "p-6 space-y-6 max-h-[70vh] overflow-y-auto"
                        prop.children [
                            // Command Info Section
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    Html.div [
                                        Html.label [
                                            prop.className "block text-sm font-medium text-text-primary mb-2"
                                            prop.text "Keyword (what you say)"
                                        ]
                                        Html.input [
                                            prop.type'.text
                                            prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                                            prop.placeholder "e.g., 'browser', 'workspace', 'email'"
                                            prop.value keywordInput
                                            prop.onChange setKeywordInput
                                            prop.autoFocus true
                                        ]
                                    ]

                                    Html.div [
                                        Html.label [
                                            prop.className "block text-sm font-medium text-text-primary mb-2"
                                            prop.text "Description (optional)"
                                        ]
                                        Html.input [
                                            prop.type'.text
                                            prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-4 py-2 text-text-primary focus:outline-none focus:border-primary transition-colors"
                                            prop.placeholder "e.g., 'Launch Chrome and open Google'"
                                            prop.value descriptionInput
                                            prop.onChange setDescriptionInput
                                        ]
                                    ]
                                ]
                            ]

                            // Divider
                            Html.hr [ prop.className "border-white/10" ]

                            // Launch Targets Section
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-center justify-between"
                                        prop.children [
                                            Html.h4 [
                                                prop.className "text-lg font-semibold text-text-primary"
                                                prop.text "Launch Targets"
                                            ]
                                            Html.span [
                                                prop.className "text-sm text-text-secondary"
                                                prop.text (sprintf "%d target(s)" targets.Length)
                                            ]
                                        ]
                                    ]

                                    // Existing Targets List
                                    if targets.Length > 0 then
                                        Html.div [
                                            prop.className "space-y-2"
                                            prop.children (
                                                targets
                                                |> List.mapi (fun idx target ->
                                                    Html.div [
                                                        prop.key (string idx)
                                                        prop.className "border border-text-secondary/30 rounded-lg p-3 bg-background-sidebar/50"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-start justify-between"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex-1"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "font-medium text-text-primary"
                                                                                prop.text target.Name
                                                                            ]
                                                                            Html.div [
                                                                                prop.className "text-sm text-text-secondary mt-1"
                                                                                prop.text (sprintf "%s: %s" (getTypeFriendlyName target.Type) target.Path)
                                                                            ]
                                                                        ]
                                                                    ]
                                                                    Html.div [
                                                                        prop.className "flex items-center space-x-2"
                                                                        prop.children [
                                                                            Html.button [
                                                                                prop.className "text-primary hover:text-primary-600 text-sm"
                                                                                prop.text "Edit"
                                                                                prop.onClick (fun _ -> handleEditTarget idx)
                                                                            ]
                                                                            Html.button [
                                                                                prop.className "text-accent hover:text-accent-hover text-sm"
                                                                                prop.text "Delete"
                                                                                prop.onClick (fun _ -> handleDeleteTarget idx)
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                )
                                            )
                                        ]

                                    // Add/Edit Target Form
                                    Html.div [
                                        prop.className "border-2 border-dashed border-text-secondary/30 rounded-lg p-4 space-y-3"
                                        prop.children [
                                            Html.h5 [
                                                prop.className "font-medium text-text-primary mb-3"
                                                prop.text (if editingTarget.IsSome then "Edit Target" else "Add New Target")
                                            ]

                                            Html.div [
                                                prop.className "grid grid-cols-2 gap-3"
                                                prop.children [
                                                    Html.div [
                                                        Html.label [
                                                            prop.className "block text-xs font-medium text-text-primary mb-1"
                                                            prop.text "Name"
                                                        ]
                                                        Html.input [
                                                            prop.type'.text
                                                            prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-primary transition-colors"
                                                            prop.placeholder "e.g., Chrome, VS Code"
                                                            prop.value targetName
                                                            prop.onChange setTargetName
                                                        ]
                                                    ]

                                                    Html.div [
                                                        Html.label [
                                                            prop.className "block text-xs font-medium text-text-primary mb-1"
                                                            prop.text "Type"
                                                        ]
                                                        Html.select [
                                                            prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-primary transition-colors"
                                                            prop.value targetType
                                                            prop.onChange setTargetType
                                                            prop.children [
                                                                Html.option [ prop.value "executable"; prop.text "Application" ]
                                                                Html.option [ prop.value "url"; prop.text "URL" ]
                                                                Html.option [ prop.value "folder"; prop.text "Folder" ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-xs font-medium text-text-primary mb-1"
                                                    prop.text "Path"
                                                ]
                                                Html.input [
                                                    prop.type'.text
                                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-3 py-2 text-sm text-text-primary font-mono focus:outline-none focus:border-primary transition-colors"
                                                    prop.placeholder "e.g., C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe or https://google.com"
                                                    prop.value targetPath
                                                    prop.onChange setTargetPath
                                                ]
                                            ]

                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-xs font-medium text-text-primary mb-1"
                                                    prop.text "Arguments (optional)"
                                                ]
                                                Html.input [
                                                    prop.type'.text
                                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-3 py-2 text-sm text-text-primary font-mono focus:outline-none focus:border-primary transition-colors"
                                                    prop.placeholder "e.g., --incognito or /select,C:\\path"
                                                    prop.value targetArgs
                                                    prop.onChange setTargetArgs
                                                ]
                                            ]

                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-xs font-medium text-text-primary mb-1"
                                                    prop.text "Working Directory (optional)"
                                                ]
                                                Html.input [
                                                    prop.type'.text
                                                    prop.className "w-full bg-background-sidebar border border-text-secondary/30 rounded px-3 py-2 text-sm text-text-primary font-mono focus:outline-none focus:border-primary transition-colors"
                                                    prop.placeholder "e.g., C:\\Projects\\MyProject"
                                                    prop.value targetWorkingDir
                                                    prop.onChange setTargetWorkingDir
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "flex justify-end space-x-2"
                                                prop.children [
                                                    if editingTarget.IsSome then
                                                        Html.button [
                                                            prop.className "px-3 py-1.5 text-sm bg-transparent hover:bg-white/10 text-text-primary rounded transition-all"
                                                            prop.text "Cancel"
                                                            prop.onClick (fun _ -> clearTargetForm())
                                                        ]
                                                    let addButtonClass =
                                                        sprintf "px-3 py-1.5 text-sm rounded transition-all %s"
                                                            (if System.String.IsNullOrWhiteSpace(targetName) || System.String.IsNullOrWhiteSpace(targetPath)
                                                             then "bg-gray-500 text-gray-300 cursor-not-allowed"
                                                             else "bg-primary hover:bg-primary-600 text-white")
                                                    Html.button [
                                                        prop.className addButtonClass
                                                        prop.text (if editingTarget.IsSome then "Update Target" else "Add Target")
                                                        prop.disabled (System.String.IsNullOrWhiteSpace(targetName) || System.String.IsNullOrWhiteSpace(targetPath))
                                                        prop.onClick (fun _ -> handleAddTarget())
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Footer
                    Html.div [
                        prop.className "flex items-center justify-end space-x-3 p-6 border-t border-white/10"
                        prop.children [
                            Html.button [
                                prop.className "px-6 py-2 bg-transparent hover:bg-white/10 text-text-primary rounded-lg font-medium transition-all"
                                prop.text "Cancel"
                                prop.onClick (fun _ -> onClose())
                            ]
                            let saveButtonClass =
                                sprintf "px-6 py-2 rounded-lg font-medium transition-all %s"
                                    (if isValid then "bg-primary hover:bg-primary-600 text-white" else "bg-gray-500 text-gray-300 cursor-not-allowed")
                            Html.button [
                                prop.className saveButtonClass
                                prop.text "Save"
                                prop.disabled (not isValid)
                                prop.onClick (fun _ -> handleSave())
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ============================================================================
// Main Component
// ============================================================================

[<ReactComponent>]
let private OpenCommandsCard (commands: OpenCommand list) (onEdit: int -> unit) =
    Html.div [
        prop.className "bg-background-card rounded-lg shadow-sm p-6"
        prop.children [
            // Header
            Html.div [
                prop.className "flex items-center justify-between mb-4"
                prop.children [
                    Html.div [
                        prop.children [
                            Html.h2 [
                                prop.className "text-lg font-semibold text-text-primary"
                                prop.text "Open Commands"
                            ]
                            Html.p [
                                prop.className "text-sm text-text-secondary mt-1"
                                prop.text "Create voice commands to launch applications, URLs, and folders"
                            ]
                        ]
                    ]
                    Html.button [
                        prop.className "px-4 py-2 bg-primary text-white rounded-md hover:bg-primary-600 transition-colors text-sm font-medium"
                        prop.text "+ Add Command"
                        prop.title "Add a new open command"
                        prop.onClick (fun _ -> onEdit -1)
                    ]
                ]
            ]

            // Info box
            Html.div [
                prop.className "bg-primary/10 border border-primary/30 rounded-md p-4 mb-6"
                prop.children [
                    Html.div [
                        prop.className "flex items-start"
                        prop.children [
                            Html.div [
                                prop.className "flex-shrink-0"
                                prop.children [
                                    Html.svg [
                                        prop.className "h-5 w-5 text-primary"
                                        prop.custom("fill", "currentColor")
                                        prop.custom("viewBox", "0 0 20 20")
                                        prop.children [
                                            Html.path [
                                                prop.custom("fillRule", "evenodd")
                                                prop.custom("d", "M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z")
                                                prop.custom("clipRule", "evenodd")
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.className "ml-3"
                                prop.children [
                                    Html.h3 [
                                        prop.className "text-sm font-medium text-text-primary"
                                        prop.text "How it works"
                                    ]
                                    Html.div [
                                        prop.className "mt-2 text-sm text-text-secondary"
                                        prop.children [
                                            Html.p "Say \"open [keyword]\" to launch your configured applications."
                                            Html.p [
                                                prop.className "mt-1"
                                                prop.text "Example: \"open workspace\" can launch VS Code, Terminal, and your browser."
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // Command list or empty state
            if commands.Length = 0 then
                Html.div [
                    prop.className "text-center py-12"
                    prop.children [
                        Html.svg [
                            prop.className "mx-auto h-12 w-12 text-text-secondary"
                            prop.custom("fill", "none")
                            prop.custom("viewBox", "0 0 24 24")
                            prop.custom("stroke", "currentColor")
                            prop.children [
                                Html.path [
                                    prop.custom("strokeLinecap", "round")
                                    prop.custom("strokeLinejoin", "round")
                                    prop.custom("strokeWidth", "2")
                                    prop.custom("d", "M12 6v6m0 0v6m0-6h6m-6 0H6")
                                ]
                            ]
                        ]
                        Html.h3 [
                            prop.className "mt-2 text-sm font-medium text-text-primary"
                            prop.text "No open commands yet"
                        ]
                        Html.p [
                            prop.className "mt-1 text-sm text-text-secondary"
                            prop.text "Get started by creating your first voice-activated command."
                        ]
                    ]
                ]
            else
                Html.div [
                    prop.className "space-y-3"
                    prop.children (
                        commands
                        |> List.mapi (fun i cmd ->
                            Html.div [
                                prop.key (string i)
                                prop.className "border border-text-secondary/30 rounded-lg p-4 hover:border-primary/50 transition-colors"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-center justify-between"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex-1"
                                                prop.children [
                                                    Html.h3 [
                                                        prop.className "font-medium text-text-primary"
                                                        prop.text ("\"open " + cmd.Keyword + "\"")
                                                    ]
                                                    match cmd.Description with
                                                    | Some desc ->
                                                        Html.p [
                                                            prop.className "text-sm text-text-secondary mt-1"
                                                            prop.text desc
                                                        ]
                                                    | None -> Html.none
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "flex items-center space-x-2"
                                                prop.children [
                                                    // Usage Count Badge
                                                    match cmd.UsageCount with
                                                    | Some count when count > 0 ->
                                                        Html.span [
                                                            prop.className "px-2 py-1 text-xs font-medium bg-primary/10 text-primary rounded"
                                                            prop.title (sprintf "Used %d time%s" count (if count = 1 then "" else "s"))
                                                            prop.text (sprintf "×%d" count)
                                                        ]
                                                    | _ ->
                                                        Html.span [
                                                            prop.className "px-2 py-1 text-xs font-medium text-text-secondary/50"
                                                            prop.text "×0"
                                                        ]
                                                    Html.button [
                                                        prop.className "px-3 py-1 text-sm text-primary hover:bg-primary/10 rounded transition-colors"
                                                        prop.text "Edit"
                                                        prop.onClick (fun _ -> onEdit i)
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        )
                    )
                ]
        ]
    ]

let view (openCommands: LoadingState<OpenCommand list>) (editingOpenCommand: (int * OpenCommand) option) (dispatch: Msg -> unit) =
    let showModal = editingOpenCommand.IsSome
    let editCommand = editingOpenCommand |> Option.map snd

    let handleEdit (index: int) =
        match openCommands with
        | LoadingState.Loaded cmds when index >= 0 && index < cmds.Length ->
            dispatch (EditOpenCommand index)
        | LoadingState.Loaded _ when index = -1 ->
            dispatch AddOpenCommand
        | _ -> ()

    let handleCloseModal () =
        dispatch CancelEditOpenCommand

    Html.div [
        prop.className "max-w-4xl mx-auto"
        prop.children [
            // Modal
            if showModal then
                OpenCommandModal editCommand dispatch handleCloseModal

            // Main card
            match openCommands with
            | LoadingState.Loaded cmds ->
                OpenCommandsCard cmds handleEdit
            | LoadingState.Loading ->
                Html.div [
                    prop.className "bg-background-card rounded-lg shadow-sm p-6 text-center py-12 text-text-secondary"
                    prop.text "Loading..."
                ]
            | LoadingState.Error err ->
                Html.div [
                    prop.className "bg-background-card rounded-lg shadow-sm p-6 text-center py-12 text-accent"
                    prop.text ("Error: " + err)
                ]
            | LoadingState.NotStarted ->
                OpenCommandsCard [] handleEdit

            // Examples card
            Html.div [
                prop.className "bg-background-card rounded-lg shadow-sm p-6 mt-6"
                prop.children [
                    Html.h3 [
                        prop.className "text-lg font-semibold text-text-primary mb-4"
                        prop.text "Example Commands"
                    ]
                    Html.div [
                        prop.className "space-y-4"
                        prop.children [
                            // Example 1
                            Html.div [
                                prop.className "border-l-4 border-primary pl-4 py-2"
                                prop.children [
                                    Html.div [
                                        prop.className "font-medium text-text-primary"
                                        prop.text "\"open browser\""
                                    ]
                                    Html.p [
                                        prop.className "text-sm text-text-secondary mt-1"
                                        prop.text "Launches Chrome at https://google.com"
                                    ]
                                ]
                            ]

                            // Example 2
                            Html.div [
                                prop.className "border-l-4 border-primary pl-4 py-2"
                                prop.children [
                                    Html.div [
                                        prop.className "font-medium text-text-primary"
                                        prop.text "\"open workspace\""
                                    ]
                                    Html.p [
                                        prop.className "text-sm text-text-secondary mt-1"
                                        prop.text "Launches VS Code, Terminal, and development server"
                                    ]
                                ]
                            ]

                            // Example 3
                            Html.div [
                                prop.className "border-l-4 border-accent pl-4 py-2"
                                prop.children [
                                    Html.div [
                                        prop.className "font-medium text-text-primary"
                                        prop.text "\"open email\""
                                    ]
                                    Html.p [
                                        prop.className "text-sm text-text-secondary mt-1"
                                        prop.text "Opens Outlook and Gmail in browser"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
