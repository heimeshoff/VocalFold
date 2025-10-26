module TextInput

open System
open System.Windows.Forms
open WindowsInput
open WindowsInput.Native

// Type text at the current cursor position using clipboard paste
let typeText (text: string) : unit =
    // Handle empty or whitespace-only strings
    if String.IsNullOrWhiteSpace(text) then
        printfn "  ⚠️  No text to type (empty string)"
    else
        printfn "  ⌨️  Pasting text: \"%s\"" text

        // Small delay before pasting (100ms) to allow window focus to stabilize
        Threading.Thread.Sleep(100)

        // Copy text to clipboard
        Clipboard.SetText(text)

        // Simulate Ctrl+V to paste
        let simulator = new InputSimulator()
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V) |> ignore

        printfn "  ✓ Pasted %d characters successfully" text.Length
