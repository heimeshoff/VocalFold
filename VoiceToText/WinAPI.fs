module WinAPI

open System
open System.Runtime.InteropServices

// Windows message constants
let WM_HOTKEY = 0x0312u
let WM_KEYDOWN = 0x0100u
let WM_KEYUP = 0x0101u
let WM_SYSKEYDOWN = 0x0104u
let WM_SYSKEYUP = 0x0105u

// Hook constants
let WH_KEYBOARD_LL = 13

// Modifier key constants
let MOD_CONTROL = 0x0002u
let MOD_SHIFT = 0x0004u
let MOD_ALT = 0x0001u
let MOD_WIN = 0x0008u

// Virtual key codes
let VK_SPACE = 0x20u
let VK_CONTROL = 0x11u
let VK_SHIFT = 0x10u
let VK_MENU = 0x12u  // Alt key
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

[<StructLayout(LayoutKind.Sequential)>]
type KBDLLHOOKSTRUCT =
    struct
        val vkCode: uint32
        val scanCode: uint32
        val flags: uint32
        val time: uint32
        val dwExtraInfo: UIntPtr
    end

// Keyboard hook callback delegate
type LowLevelKeyboardProc = delegate of int * IntPtr * IntPtr -> IntPtr

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

[<DllImport("user32.dll", SetLastError = true)>]
extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint32 dwThreadId)

[<DllImport("user32.dll", SetLastError = true)>]
extern bool UnhookWindowsHookEx(IntPtr hhk)

[<DllImport("user32.dll")>]
extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)

[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr GetModuleHandle(string lpModuleName)

// Hotkey callback storage
let mutable private hotkeyCallback: (unit -> unit) option = None

// Register a hotkey with a callback
let registerHotkey (id: int) (modifiers: uint32) (vk: uint32) (callback: unit -> unit) =
    hotkeyCallback <- Some callback
    let result = RegisterHotKey(IntPtr.Zero, id, modifiers, vk)
    if result then
        printfn "✓ Hotkey registered (ID: %d)" id
    else
        let errorCode = Marshal.GetLastWin32Error()
        eprintfn "✗ Failed to register hotkey (ID: %d, Error: %d)" id errorCode
        eprintfn "  Common causes:"
        eprintfn "  - Hotkey already registered by another application"
        eprintfn "  - Administrator privileges required"
        eprintfn "  - Invalid key combination"
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

// ===== Keyboard Hook Implementation =====

// Callback storage for keyboard hook
let mutable private keyDownCallback: (unit -> unit) option = None
let mutable private keyUpCallback: (unit -> unit) option = None
let mutable private hookHandle: IntPtr = IntPtr.Zero
let mutable private hookProc: LowLevelKeyboardProc option = None

// Track modifier key states
let mutable private ctrlPressed = false
let mutable private shiftPressed = false
let mutable private targetKeyPressed = false

// Check if GetAsyncKeyState reports a key is pressed
[<DllImport("user32.dll")>]
extern int16 GetAsyncKeyState(int vKey)

let isKeyPressed (vKey: int) =
    (GetAsyncKeyState(vKey) &&& 0x8000s) <> 0s

// Keyboard hook callback
let private keyboardHookCallback (nCode: int) (wParam: IntPtr) (lParam: IntPtr) : IntPtr =
    if nCode >= 0 then
        let wParamValue = uint32 wParam
        let kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)
        let vkCode = kbd.vkCode

        // Check for key down events (WM_KEYDOWN or WM_SYSKEYDOWN)
        if wParamValue = WM_KEYDOWN || wParamValue = WM_SYSKEYDOWN then
            // Track modifier keys
            if vkCode = VK_CONTROL then ctrlPressed <- true
            if vkCode = VK_SHIFT then shiftPressed <- true

            // Check if target key (Space) is pressed with modifiers
            if vkCode = VK_SPACE then
                let ctrlDown = isKeyPressed(int VK_CONTROL)
                let shiftDown = isKeyPressed(int VK_SHIFT)

                if ctrlDown && shiftDown && not targetKeyPressed then
                    targetKeyPressed <- true
                    printfn "🔔 Hotkey pressed (Ctrl+Shift+Space)"
                    match keyDownCallback with
                    | Some callback ->
                        try
                            callback()
                        with
                        | ex -> eprintfn "✗ Error in key down callback: %s" ex.Message
                    | None -> ()

        // Check for key up events (WM_KEYUP or WM_SYSKEYUP)
        elif wParamValue = WM_KEYUP || wParamValue = WM_SYSKEYUP then
            // Track modifier keys
            if vkCode = VK_CONTROL then ctrlPressed <- false
            if vkCode = VK_SHIFT then shiftPressed <- false

            // Check if target key (Space) is released
            if vkCode = VK_SPACE && targetKeyPressed then
                targetKeyPressed <- false
                printfn "🔔 Hotkey released"
                match keyUpCallback with
                | Some callback ->
                    try
                        callback()
                    with
                    | ex -> eprintfn "✗ Error in key up callback: %s" ex.Message
                | None -> ()

    CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)

// Install keyboard hook with callbacks
let installKeyboardHook (onKeyDown: unit -> unit) (onKeyUp: unit -> unit) =
    keyDownCallback <- Some onKeyDown
    keyUpCallback <- Some onKeyUp

    // Create the hook procedure delegate and keep it alive
    let proc = LowLevelKeyboardProc(keyboardHookCallback)
    hookProc <- Some proc

    // Install the hook
    use curModule = System.Diagnostics.Process.GetCurrentProcess().MainModule
    let moduleHandle = GetModuleHandle(curModule.ModuleName)

    hookHandle <- SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0u)

    if hookHandle <> IntPtr.Zero then
        printfn "✓ Keyboard hook installed"
        printfn "  Listening for: Ctrl+Shift+Space"
        true
    else
        let errorCode = Marshal.GetLastWin32Error()
        eprintfn "✗ Failed to install keyboard hook (Error: %d)" errorCode
        hookProc <- None
        false

// Uninstall keyboard hook
let uninstallKeyboardHook () =
    if hookHandle <> IntPtr.Zero then
        let result = UnhookWindowsHookEx(hookHandle)
        if result then
            printfn "✓ Keyboard hook uninstalled"
        else
            eprintfn "✗ Failed to uninstall keyboard hook"
        hookHandle <- IntPtr.Zero
        hookProc <- None
        keyDownCallback <- None
        keyUpCallback <- None
        result
    else
        false
