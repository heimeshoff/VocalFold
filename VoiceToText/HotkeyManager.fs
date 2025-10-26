module HotkeyManager

open System

// Type alias for hotkey callback function
type HotkeyAction = unit -> unit

// Internal storage for the callback
let mutable private hotkeyCallback: HotkeyAction option = None

// Register a global hotkey with a callback
let registerHotkey (hotkeyId: int) (modifiers: uint32) (virtualKey: uint32) (callback: HotkeyAction) : bool =
    // Store the callback
    hotkeyCallback <- Some callback

    // Register with Windows API
    let registered = WinAPI.registerHotkey hotkeyId modifiers virtualKey (fun () ->
        match hotkeyCallback with
        | Some action -> action()
        | None -> ()
    )

    if registered then
        printfn "✓ Hotkey registered successfully (ID: %d)" hotkeyId
    else
        eprintfn "✗ Failed to register hotkey (ID: %d)" hotkeyId

    registered

// Unregister a global hotkey
let unregisterHotkey (hotkeyId: int) : bool =
    let unregistered = WinAPI.unregisterHotkey hotkeyId

    if unregistered then
        printfn "✓ Hotkey unregistered (ID: %d)" hotkeyId

    // Clear the callback
    hotkeyCallback <- None

    unregistered

// Start the Windows message loop
let messageLoop () : unit =
    WinAPI.messageLoop()
