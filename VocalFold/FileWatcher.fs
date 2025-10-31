module FileWatcher

open System
open System.IO

/// Callback function when file changes are detected
type FileChangeCallback = unit -> unit

/// Create a file watcher for a specific file path
/// Returns a FileSystemWatcher that monitors the file for changes
let createWatcher (filePath: string) (callback: FileChangeCallback) : FileSystemWatcher =
    try
        let directory = Path.GetDirectoryName(filePath)
        let fileName = Path.GetFileName(filePath)

        if String.IsNullOrEmpty(directory) || String.IsNullOrEmpty(fileName) then
            raise (ArgumentException("Invalid file path"))

        let watcher = new FileSystemWatcher()
        watcher.Path <- directory
        watcher.Filter <- fileName
        watcher.NotifyFilter <- NotifyFilters.LastWrite ||| NotifyFilters.FileName

        // Debounce changes (avoid multiple events for single change)
        let mutable lastEventTime = DateTime.MinValue
        let debounceMs = 500.0

        let onChanged (e: FileSystemEventArgs) =
            let now = DateTime.Now
            if (now - lastEventTime).TotalMilliseconds > debounceMs then
                lastEventTime <- now
                Logger.info (sprintf "📄 File changed externally: %s" e.Name)
                try
                    // Small delay to ensure file is fully written (especially for cloud sync)
                    Threading.Thread.Sleep(100)
                    callback()
                with
                | ex ->
                    Logger.error (sprintf "Error in file change callback: %s" ex.Message)

        // Subscribe to file system events
        watcher.Changed.Add(onChanged)
        watcher.Created.Add(onChanged)
        watcher.Renamed.Add(fun (e: RenamedEventArgs) ->
            if e.Name = fileName then
                onChanged(e)
        )

        watcher.EnableRaisingEvents <- true
        Logger.info (sprintf "📂 Started watching file: %s" filePath)
        watcher
    with
    | ex ->
        Logger.error (sprintf "Failed to create file watcher for %s: %s" filePath ex.Message)
        reraise()

/// Stop watching a file
let stopWatcher (watcher: FileSystemWatcher) =
    try
        watcher.EnableRaisingEvents <- false
        watcher.Dispose()
        Logger.info "📂 Stopped file watcher"
    with
    | ex ->
        Logger.warning (sprintf "Error stopping file watcher: %s" ex.Message)
