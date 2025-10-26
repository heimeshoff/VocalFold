module Logger

open System
open System.IO
open System.Threading

/// Log levels
type LogLevel =
    | Debug
    | Info
    | Warning
    | Error
    | Critical

/// Get the log directory path
let private getLogDirectory () =
    let appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    Path.Combine(appData, "VocalFold", "logs")

/// Get the current log file path
let private getLogFilePath () =
    let logDir = getLogDirectory()
    let timestamp = DateTime.Now.ToString("yyyy-MM-dd")
    Path.Combine(logDir, sprintf "vocalfold-%s.log" timestamp)

/// Lock object for thread-safe logging
let private logLock = obj()

/// Ensure log directory exists
let private ensureLogDirectory () =
    let logDir = getLogDirectory()
    if not (Directory.Exists(logDir)) then
        Directory.CreateDirectory(logDir) |> ignore

/// Format log level for display
let private formatLogLevel (level: LogLevel) =
    match level with
    | Debug -> "DEBUG"
    | Info -> "INFO "
    | Warning -> "WARN "
    | Error -> "ERROR"
    | Critical -> "FATAL"

/// Write a log message to file and console
let private writeLog (level: LogLevel) (message: string) =
    lock logLock (fun () ->
        try
            ensureLogDirectory()
            let timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            let levelStr = formatLogLevel level
            let logMessage = sprintf "[%s] [%s] %s" timestamp levelStr message

            // Write to file
            let logPath = getLogFilePath()
            File.AppendAllText(logPath, logMessage + Environment.NewLine)

            // Also write to console (visible in debug mode or dotnet run)
            match level with
            | Error | Critical -> eprintfn "%s" logMessage
            | _ -> printfn "%s" logMessage
        with
        | ex ->
            // Last resort: write to console if file logging fails
            eprintfn "LOGGER ERROR: Failed to write log: %s" ex.Message
            eprintfn "Original message: %s" message
    )

/// Log a debug message
let debug message = writeLog Debug message

/// Log an info message
let info message = writeLog Info message

/// Log a warning message
let warning message = writeLog Warning message

/// Log an error message
let error message = writeLog Error message

/// Log a critical error message
let critical message = writeLog Critical message

/// Log an exception with stack trace
let logException (ex: Exception) (context: string) =
    let message = sprintf "%s - Exception: %s\nStack trace: %s" context ex.Message ex.StackTrace
    error message

/// Log an exception and show error dialog
let logAndShowError (ex: Exception) (context: string) =
    logException ex context

    try
        let errorMessage = sprintf "%s\n\nError: %s\n\nLog file: %s" context ex.Message (getLogFilePath())
        System.Windows.Forms.MessageBox.Show(
            errorMessage,
            "VocalFold Error",
            System.Windows.Forms.MessageBoxButtons.OK,
            System.Windows.Forms.MessageBoxIcon.Error
        ) |> ignore
    with
    | _ -> () // Silently fail if MessageBox fails

/// Get the path to the latest log file (for display/troubleshooting)
let getLatestLogPath () = getLogFilePath()

/// Clean up old log files (keep last 7 days)
let cleanupOldLogs () =
    try
        let logDir = getLogDirectory()
        if Directory.Exists(logDir) then
            let cutoffDate = DateTime.Now.AddDays(-7.0)
            let files = Directory.GetFiles(logDir, "vocalfold-*.log")

            for file in files do
                let fileInfo = FileInfo(file)
                if fileInfo.LastWriteTime < cutoffDate then
                    File.Delete(file)
                    info (sprintf "Deleted old log file: %s" file)
    with
    | ex ->
        warning (sprintf "Failed to cleanup old logs: %s" ex.Message)
