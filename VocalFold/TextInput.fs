module TextInput

open System
open System.Threading
open WindowsInput
open WindowsInput.Native

/// Type text character-by-character at the current cursor position
/// This implementation does NOT use the clipboard, preserving user's copied content
let typeText (text: string) (delayMs: int) : unit =
    try
        // Handle empty or whitespace-only strings
        if String.IsNullOrEmpty(text) then
            Logger.warning "No text to type (empty string)"
        else
            Logger.info (sprintf "Typing text character-by-character: \"%s\" (%d characters, %dms delay)" text text.Length delayMs)

            // Small delay before typing (100ms) to allow window focus to stabilize
            Thread.Sleep(100)

            let simulator = new InputSimulator()
            let mutable successCount = 0
            let mutable failCount = 0
            let startTime = DateTime.Now

            // Type each character individually
            for i = 0 to text.Length - 1 do
                let c = text.[i]

                try
                    // Use TextEntry for each character - handles Unicode properly
                    simulator.Keyboard.TextEntry(c) |> ignore
                    successCount <- successCount + 1

                    // Add delay between characters (except after the last one)
                    if i < text.Length - 1 && delayMs > 0 then
                        Thread.Sleep(delayMs)
                with
                | ex ->
                    Logger.warning (sprintf "Failed to type character '%c' (U+%04X): %s" c (int c) ex.Message)
                    failCount <- failCount + 1
                    // Continue trying to type remaining characters

            let elapsed = (DateTime.Now - startTime).TotalMilliseconds
            let charsPerSecond = if elapsed > 0.0 then float successCount / (elapsed / 1000.0) else 0.0

            if failCount = 0 then
                Logger.info (sprintf "Successfully typed %d characters in %.0fms (%.1f chars/sec)" successCount elapsed charsPerSecond)
            else
                Logger.warning (sprintf "Typed %d/%d characters (%d failed) in %.0fms" successCount text.Length failCount elapsed)
    with
    | ex ->
        Logger.logException ex "Error in typeText"
        reraise()

/// Type text using settings-based configuration
let typeTextWithSettings (text: string) (settings: Settings.AppSettings) : unit =
    let typingSpeed = Settings.getTypingSpeed settings
    let delayMs = Settings.getTypingDelay typingSpeed
    typeText text delayMs
