module TextProcessor

open System
open System.Text.RegularExpressions

/// Result of processing transcribed text
type ProcessingResult =
    | TypeText of string        // Text should be typed
    | OpenSettings             // Open settings dialog
    | OpenApplication of Settings.OpenCommand  // Execute custom open command

/// Mutable storage for the last transcribed message
let mutable private lastTranscription: string option = None

/// Mutable storage for open commands cache
let mutable private openCommandsCache: Settings.OpenCommandsData option = None
let mutable private openCommandsFilePath: string option = None

/// Reload open commands from file
let reloadOpenCommands (filePath: string) : unit =
    try
        let data = Settings.loadOpenCommands filePath
        openCommandsCache <- Some data
        openCommandsFilePath <- Some filePath
        Logger.info (sprintf "Loaded %d open command(s) from %s" data.OpenCommands.Length filePath)
    with
    | ex ->
        Logger.logException ex "Failed to reload open commands"
        openCommandsCache <- Some Settings.defaultOpenCommandsData

/// Get current open commands
let getOpenCommands () : Settings.OpenCommand list =
    match openCommandsCache with
    | Some data -> data.OpenCommands
    | None -> []

/// Find an open command by keyword (case-insensitive)
let findOpenCommand (keyword: string) : Settings.OpenCommand option =
    let normalizedKeyword = keyword.Trim().ToLowerInvariant()
    getOpenCommands()
    |> List.tryFind (fun cmd -> cmd.Keyword.ToLowerInvariant() = normalizedKeyword)

/// Check if text matches "open [keyword]" pattern and find the command
let private tryMatchOpenCommand (normalizedText: string) : Settings.OpenCommand option =
    // Pattern: "open <keyword>"
    if normalizedText.StartsWith("open ") && normalizedText.Length > 5 then
        let keyword = normalizedText.Substring(5).Trim()

        // Don't match "open settings" (built-in command)
        if keyword <> "settings" then
            Logger.debug (sprintf "Checking for open command with keyword: %s" keyword)
            findOpenCommand keyword
        else
            None
    else
        None

/// Store the last transcription for "repeat message" command
let storeLastTranscription (text: string) : unit =
    if not (String.IsNullOrWhiteSpace(text)) then
        lastTranscription <- Some text
        Logger.debug (sprintf "Stored last transcription: \"%s\""
            (if text.Length > 50 then text.Substring(0, 47) + "..." else text))

/// Get the last stored transcription (for debugging/testing)
let getLastTranscription () : string option = lastTranscription

/// Process transcribed text and apply keyword replacements
/// Returns tuple of (processed text, list of used keywords)
let processTranscription (text: string) (replacements: Settings.KeywordReplacement list) : string * string list =
    try
        // If no replacements or empty text, return original
        if List.isEmpty replacements || String.IsNullOrEmpty(text) then
            (text, [])
        else
            Logger.debug (sprintf "Processing text with %d keyword replacements" replacements.Length)

            // Sort replacements by keyword length (longest first) to handle overlapping keywords
            // This ensures "German email footer" is matched before "email" or "footer"
            let sortedReplacements =
                replacements
                |> List.sortByDescending (fun r -> r.Keyword.Length)

            // Apply each replacement and track used keywords
            let mutable processedText = text
            let mutable replacementCount = 0
            let mutable usedKeywords = []

            for replacement in sortedReplacements do
                if not (String.IsNullOrEmpty(replacement.Keyword)) then
                    try
                        // Always match whole phrases (word boundaries) and case-insensitively
                        let pattern = sprintf @"\b%s\b" (Regex.Escape(replacement.Keyword))
                        let regex = new Regex(pattern, RegexOptions.IgnoreCase)
                        let matches = regex.Matches(processedText)

                        if matches.Count > 0 then
                            processedText <- regex.Replace(processedText, replacement.Replacement)
                            replacementCount <- replacementCount + matches.Count
                            usedKeywords <- replacement.Keyword :: usedKeywords  // Track used keyword
                            Logger.debug (sprintf "Replaced %d occurrence(s) of '%s' with '%s'"
                                matches.Count
                                replacement.Keyword
                                (if replacement.Replacement.Length > 50 then
                                    replacement.Replacement.Substring(0, 47) + "..."
                                 else
                                    replacement.Replacement))
                    with
                    | ex ->
                        Logger.warning (sprintf "Error applying replacement for keyword '%s': %s"
                            replacement.Keyword ex.Message)
                        // Continue with other replacements

            if replacementCount > 0 then
                Logger.info (sprintf "Applied %d keyword replacement(s) to transcription" replacementCount)
            else
                Logger.debug "No keyword replacements applied (no matches found)"

            (processedText, List.rev usedKeywords)
    with
    | ex ->
        Logger.logException ex "Error in processTranscription, returning original text"
        (text, [])  // Return original text on error

/// Create a keyword replacement with default values
let createKeywordReplacement (keyword: string) (replacement: string) : Settings.KeywordReplacement =
    {
        Keyword = keyword
        Replacement = replacement
        Category = None  // Default to no category
        UsageCount = None  // Default to no usage
    }

/// Validate a keyword replacement
let validateKeywordReplacement (replacement: Settings.KeywordReplacement) : string option =
    if String.IsNullOrWhiteSpace(replacement.Keyword) then
        Some "Keyword cannot be empty"
    elif String.IsNullOrEmpty(replacement.Replacement) then
        Some "Replacement cannot be null (but can be empty for deletion)"
    else
        None

/// Get example keyword replacements for documentation/testing
let getExampleReplacements () : Settings.KeywordReplacement list =
    [
        // Punctuation shortcuts
        { Keyword = "comma"; Replacement = ","; Category = Some "Punctuation"; UsageCount = None }
        { Keyword = "period"; Replacement = "."; Category = Some "Punctuation"; UsageCount = None }
        { Keyword = "question mark"; Replacement = "?"; Category = Some "Punctuation"; UsageCount = None }
        { Keyword = "exclamation mark"; Replacement = "!"; Category = Some "Punctuation"; UsageCount = None }
        { Keyword = "new line"; Replacement = "\n"; Category = Some "Punctuation"; UsageCount = None }

        // Email signature example
        {
            Keyword = "German email footer"
            Replacement = "Mit freundlichen Grüßen\nIhr VocalFold Team"
            Category = Some "Email Templates"
            UsageCount = None
        }

        // Code snippet example
        {
            Keyword = "main function"
            Replacement = "let main argv =\n    printfn \"Hello, World!\"\n    0"
            Category = Some "Code Snippets"
            UsageCount = None
        }
    ]

/// Track keyword usage for analytics
let trackKeywordUsage (usedKeywords: string list) (keywordsFilePath: string) : unit =
    // Increment usage count for each keyword that was used
    for keyword in usedKeywords do
        Settings.incrementKeywordUsage keyword keywordsFilePath

/// Process transcription and check for special commands
/// This is the main entry point that should be used instead of processTranscription directly
/// Returns tuple of (ProcessingResult, list of used keywords)
let processTranscriptionWithCommands (text: string) (replacements: Settings.KeywordReplacement list) : ProcessingResult * string list =
    try
        // Check for special built-in commands (case-insensitive, punctuation removed)
        let normalizedText =
            let lowered = text.Trim().ToLowerInvariant()
            // Remove all punctuation, keeping only letters, numbers, and spaces
            let noPunctuation = Regex.Replace(lowered, @"[^\w\s]", "")
            // Collapse multiple spaces into single space
            let singleSpaced = Regex.Replace(noPunctuation, @"\s+", " ")
            singleSpaced.Trim()

        Logger.debug (sprintf "Normalized text for command matching: \"%s\"" normalizedText)

        // Check for "open settings" command (exact match after normalization)
        if normalizedText = "open settings" then
            Logger.info "Detected 'open settings' command"
            (OpenSettings, [])
        // Check for custom "open [keyword]" commands
        elif normalizedText.StartsWith("open ") then
            match tryMatchOpenCommand normalizedText with
            | Some openCommand ->
                Logger.info (sprintf "Detected custom open command: %s" openCommand.Keyword)
                (OpenApplication openCommand, [])
            | None ->
                // No matching open command, process as normal text
                Logger.debug "No matching open command found, processing as text"
                let (processedText, usedKeywords) = processTranscription text replacements
                (TypeText processedText, usedKeywords)
        // Check for "repeat message" command (exact match after normalization)
        elif normalizedText = "repeat message" then
            Logger.info "Detected 'repeat message' command"
            match lastTranscription with
            | Some lastText ->
                Logger.info (sprintf "Repeating last transcription: \"%s\""
                    (if lastText.Length > 50 then lastText.Substring(0, 47) + "..." else lastText))
                // Replace "repeat message" with the actual last transcription
                let pattern = @"\brepeat message\b"
                let regex = new Regex(pattern, RegexOptions.IgnoreCase)
                let resultText = regex.Replace(text, lastText)
                // Process the result text with keyword replacements
                let (processedText, usedKeywords) = processTranscription resultText replacements
                (TypeText processedText, usedKeywords)
            | None ->
                Logger.warning "No previous transcription available to repeat"
                (TypeText text, [])  // Just type the original text if no previous transcription
        else
            // No special command detected, process normally
            let (processedText, usedKeywords) = processTranscription text replacements
            (TypeText processedText, usedKeywords)
    with
    | ex ->
        Logger.logException ex "Error in processTranscriptionWithCommands"
        // On error, return the original text
        (TypeText text, [])
