module ApplicationLauncher

open System
open System.Diagnostics
open Settings

/// Result of launching an application
type LaunchResult = {
    Target: LaunchTarget
    Success: bool
    ErrorMessage: string option
    ProcessId: int option
}

/// Launch a single URL in the default browser
let private launchUrl (url: string) : LaunchResult =
    try
        Logger.info (sprintf "Launching URL: %s" url)

        // Use Process.Start with UseShellExecute to open URL in default browser
        let psi = new ProcessStartInfo()
        psi.FileName <- url
        psi.UseShellExecute <- true

        let proc = Process.Start(psi)

        {
            Target = { Name = url; Type = "url"; Path = url; Arguments = None; WorkingDirectory = None }
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to launch URL: %s" url)
        {
            Target = { Name = url; Type = "url"; Path = url; Arguments = None; WorkingDirectory = None }
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Launch a single executable
let private launchExecutable (target: LaunchTarget) : LaunchResult =
    try
        Logger.info (sprintf "Launching executable: %s" target.Path)

        let psi = new ProcessStartInfo()
        psi.FileName <- target.Path
        psi.UseShellExecute <- true

        // Set arguments if provided
        match target.Arguments with
        | Some args when not (String.IsNullOrWhiteSpace(args)) ->
            psi.Arguments <- args
            Logger.debug (sprintf "  Arguments: %s" args)
        | _ -> ()

        // Set working directory if provided
        match target.WorkingDirectory with
        | Some workDir when not (String.IsNullOrWhiteSpace(workDir)) ->
            psi.WorkingDirectory <- workDir
            Logger.debug (sprintf "  Working directory: %s" workDir)
        | _ -> ()

        let proc = Process.Start(psi)

        {
            Target = target
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to launch executable: %s" target.Path)
        {
            Target = target
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Open a folder in Windows Explorer
let private openFolder (path: string) : LaunchResult =
    try
        Logger.info (sprintf "Opening folder: %s" path)

        let psi = new ProcessStartInfo()
        psi.FileName <- "explorer.exe"
        psi.Arguments <- sprintf "\"%s\"" path
        psi.UseShellExecute <- true

        let proc = Process.Start(psi)

        {
            Target = { Name = path; Type = "folder"; Path = path; Arguments = None; WorkingDirectory = None }
            Success = true
            ErrorMessage = None
            ProcessId = if proc <> null then Some proc.Id else None
        }
    with
    | ex ->
        Logger.logException ex (sprintf "Failed to open folder: %s" path)
        {
            Target = { Name = path; Type = "folder"; Path = path; Arguments = None; WorkingDirectory = None }
            Success = false
            ErrorMessage = Some ex.Message
            ProcessId = None
        }

/// Launch a single target (URL, executable, or folder)
let launchTarget (target: LaunchTarget) : LaunchResult =
    Logger.info (sprintf "Launching target: %s (type: %s)" target.Name target.Type)

    match target.Type.ToLowerInvariant() with
    | "url" -> launchUrl target.Path
    | "executable" -> launchExecutable target
    | "folder" -> openFolder target.Path
    | unknown ->
        Logger.warning (sprintf "Unknown target type: %s" unknown)
        {
            Target = target
            Success = false
            ErrorMessage = Some (sprintf "Unknown target type: %s" unknown)
            ProcessId = None
        }

/// Launch multiple targets with optional delay between launches
let launchTargets (targets: LaunchTarget list) (delayMs: int option) : LaunchResult list =
    Logger.info (sprintf "Launching %d target(s)" targets.Length)

    let delay = defaultArg delayMs 500  // Default 500ms delay between launches

    targets
    |> List.mapi (fun index target ->
        // Add delay before launching (except first one)
        if index > 0 && delay > 0 then
            Logger.debug (sprintf "Waiting %dms before next launch..." delay)
            System.Threading.Thread.Sleep(delay)

        launchTarget target
    )

/// Launch all targets for an open command
let executeOpenCommand (command: OpenCommand) : LaunchResult list =
    Logger.info (sprintf "Executing open command: %s" command.Keyword)
    Logger.debug (sprintf "  Description: %s" (defaultArg command.Description "N/A"))
    Logger.debug (sprintf "  Targets: %d" command.Targets.Length)

    let results = launchTargets command.Targets command.LaunchDelay

    // Log summary
    let successCount = results |> List.filter (fun r -> r.Success) |> List.length
    let failCount = results.Length - successCount

    if failCount = 0 then
        Logger.info (sprintf "✓ Successfully launched all %d target(s)" successCount)
    else
        Logger.warning (sprintf "⚠ Launched %d/%d targets (%d failed)" successCount results.Length failCount)

    results
