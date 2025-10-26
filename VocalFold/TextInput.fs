module TextInput

open System
open System.Windows.Forms
open WindowsInput
open WindowsInput.Native

// Type text at the current cursor position using clipboard paste
let typeText (text: string) : unit =
    try
        // Handle empty or whitespace-only strings
        if String.IsNullOrWhiteSpace(text) then
            Logger.warning "No text to type (empty string)"
        else
            Logger.info (sprintf "Typing text: \"%s\" (%d characters)" text text.Length)

            // Small delay before pasting (100ms) to allow window focus to stabilize
            Threading.Thread.Sleep(100)

            // Copy text to clipboard
            Logger.debug "Setting clipboard text..."
            Clipboard.SetText(text)
            Logger.debug "Clipboard text set successfully"

            // Simulate Ctrl+V to paste
            Logger.debug "Simulating Ctrl+V keystroke..."
            let simulator = new InputSimulator()
            simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V) |> ignore

            Logger.info (sprintf "Successfully pasted %d characters" text.Length)
    with
    | ex ->
        Logger.logException ex "Error in typeText"
        reraise()
