module WinAPI

open System
open System.Runtime.InteropServices

// Windows message constants
let WM_HOTKEY = 0x0312u

// Modifier key constants
let MOD_CONTROL = 0x0002u
let MOD_SHIFT = 0x0004u
let MOD_ALT = 0x0001u
let MOD_WIN = 0x0008u

// Virtual key codes
let VK_SPACE = 0x20u
let VK_LWIN = 0x5Bu  // Left Windows key
let VK_RWIN = 0x5Cu  // Right Windows key

// Windows API structures
[<StructLayout(LayoutKind.Sequential)>]
type MSG =
    struct
        val mutable hwnd: IntPtr
        val mutable message: uint32
        val mutable wParam: IntPtr
        val mutable lParam: IntPtr
        val mutable time: uint32
        val mutable pt_x: int32
        val mutable pt_y: int32
    end

// P/Invoke declarations
[<DllImport("user32.dll", SetLastError = true)>]
extern bool RegisterHotKey(IntPtr hWnd, int id, uint32 fsModifiers, uint32 vk)

[<DllImport("user32.dll", SetLastError = true)>]
extern bool UnregisterHotKey(IntPtr hWnd, int id)

[<DllImport("user32.dll")>]
extern int GetMessage(MSG& lpMsg, IntPtr hWnd, uint32 wMsgFilterMin, uint32 wMsgFilterMax)

[<DllImport("user32.dll")>]
extern bool TranslateMessage(MSG& lpMsg)

[<DllImport("user32.dll")>]
extern IntPtr DispatchMessage(MSG& lpMsg)

// Hotkey callback storage
let mutable private hotkeyCallback: (unit -> unit) option = None

// Register a hotkey with a callback
let registerHotkey (id: int) (modifiers: uint32) (vk: uint32) (callback: unit -> unit) =
    hotkeyCallback <- Some callback
    let result = RegisterHotKey(IntPtr.Zero, id, modifiers, vk)
    if result then
        printfn "✓ Hotkey registered (ID: %d)" id
    else
        eprintfn "✗ Failed to register hotkey (ID: %d)" id
    result

// Unregister a hotkey
let unregisterHotkey (id: int) =
    let result = UnregisterHotKey(IntPtr.Zero, id)
    if result then
        printfn "✓ Hotkey unregistered (ID: %d)" id
    else
        eprintfn "✗ Failed to unregister hotkey (ID: %d)" id
    hotkeyCallback <- None
    result

// Windows message loop
let messageLoop () =
    printfn "🔄 Entering message loop (Press Ctrl+C to exit)..."
    let mutable msg = MSG()
    let mutable running = true

    // Setup Ctrl+C handler for clean exit
    Console.CancelKeyPress.Add(fun args ->
        args.Cancel <- true
        running <- false
        printfn "\n⚠️ Shutting down..."
    )

    while running do
        let result = GetMessage(&msg, IntPtr.Zero, 0u, 0u)

        if result > 0 then
            // Check if this is a hotkey message
            if msg.message = WM_HOTKEY then
                printfn "🔔 Hotkey detected!"
                match hotkeyCallback with
                | Some callback ->
                    try
                        callback()
                    with
                    | ex -> eprintfn "✗ Error in hotkey callback: %s" ex.Message
                | None -> ()

            // Standard message processing
            TranslateMessage(&msg) |> ignore
            DispatchMessage(&msg) |> ignore
        elif result = 0 then
            // WM_QUIT received
            running <- false
        else
            // Error occurred
            eprintfn "✗ GetMessage error"
            running <- false

    printfn "✓ Message loop exited"
