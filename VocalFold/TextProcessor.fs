module TextProcessor

open System
open System.Text.RegularExpressions

/// Result of processing transcribed text
type ProcessingResult =
    | TypeText of string        // Text should be typed
    | OpenSettings             // Open settings dialog

/// Process transcribed text and apply keyword replacements
/// Returns the processed text with all applicable keyword replacements applied
let processTranscription (text: string) (replacements: Settings.KeywordReplacement list) : string =
    try
        // If no replacements or empty text, return original
        if List.isEmpty replacements || String.IsNullOrEmpty(text) then
            text
        else
            Logger.debug (sprintf "Processing text with %d keyword replacements" replacements.Length)

            // Sort replacements by keyword length (longest first) to handle overlapping keywords
            // This ensures "German email footer" is matched before "email" or "footer"
            let sortedReplacements =
                replacements
                |> List.sortByDescending (fun r -> r.Keyword.Length)

            // Apply each replacement
            let mutable processedText = text
            let mutable replacementCount = 0

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

            processedText
    with
    | ex ->
        Logger.logException ex "Error in processTranscription, returning original text"
        text  // Return original text on error

/// Create a keyword replacement with default values
let createKeywordReplacement (keyword: string) (replacement: string) : Settings.KeywordReplacement =
    {
        Keyword = keyword
        Replacement = replacement
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
        { Keyword = "comma"; Replacement = "," }
        { Keyword = "period"; Replacement = "." }
        { Keyword = "question mark"; Replacement = "?" }
        { Keyword = "exclamation mark"; Replacement = "!" }
        { Keyword = "new line"; Replacement = "\n" }

        // Email signature example
        {
            Keyword = "German email footer"
            Replacement = "Mit freundlichen Grüßen\nIhr VocalFold Team"
        }

        // Code snippet example
        {
            Keyword = "main function"
            Replacement = "let main argv =\n    printfn \"Hello, World!\"\n    0"
        }
    ]

/// Process transcription and check for special commands
/// This is the main entry point that should be used instead of processTranscription directly
let processTranscriptionWithCommands (text: string) (replacements: Settings.KeywordReplacement list) : ProcessingResult =
    try
        // Check for special built-in commands (case-insensitive)
        let normalizedText = text.Trim().ToLowerInvariant()

        // Check for "open vocalfold settings" command
        if normalizedText.Contains("open vocalfold settings") ||
           normalizedText.Contains("open vocal fold settings") then
            Logger.info "Detected 'open vocalfold settings' command"
            OpenSettings
        else
            // No special command detected, process normally
            let processedText = processTranscription text replacements
            TypeText processedText
    with
    | ex ->
        Logger.logException ex "Error in processTranscriptionWithCommands"
        // On error, return the original text
        TypeText text
