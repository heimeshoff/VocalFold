module TranscriptionService

open System
open System.IO
open Whisper.net
open Whisper.net.Ggml

// Whisper model configuration
type ModelConfig = {
    ModelType: GgmlType
    ModelPath: string
}

// Get the model directory path
let private getModelDirectory () =
    let userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    Path.Combine(userProfile, ".whisper-models")

// Get the model file path
let private getModelPath (modelType: GgmlType) =
    let modelDir = getModelDirectory()
    let modelName =
        match modelType with
        | GgmlType.Tiny -> "ggml-tiny.bin"
        | GgmlType.Base -> "ggml-base.bin"
        | GgmlType.Small -> "ggml-small.bin"
        | GgmlType.Medium -> "ggml-medium.bin"
        | GgmlType.LargeV1 -> "ggml-large-v1.bin"
        | _ -> "ggml-base.bin"
    Path.Combine(modelDir, modelName)

// Download model if it doesn't exist
let ensureModelDownloaded (modelType: GgmlType) : Async<string> = async {
    let modelPath = getModelPath modelType
    let modelDir = getModelDirectory()

    // Create directory if it doesn't exist
    if not (Directory.Exists(modelDir)) then
        Directory.CreateDirectory(modelDir) |> ignore
        printfn "📁 Created model directory: %s" modelDir

    // Check if model already exists
    if File.Exists(modelPath) then
        printfn "✓ Model found: %s" modelPath
        return modelPath
    else
        printfn "⬇️  Downloading Whisper.NET model (%A)..." modelType
        printfn "   This may take a few minutes on first run..."

        try
            // Download the model
            use! downloader = WhisperGgmlDownloader.GetGgmlModelAsync(modelType) |> Async.AwaitTask

            // Save to file
            use fileStream = File.Create(modelPath)
            do! downloader.CopyToAsync(fileStream) |> Async.AwaitTask

            printfn "✓ Model downloaded successfully: %s" modelPath
            return modelPath
        with
        | ex ->
            eprintfn "✗ Failed to download model: %s" ex.Message
            return raise ex
}

// Whisper transcription service
type WhisperService(modelPath: string) =
    let whisperFactory = WhisperFactory.FromPath(modelPath)

    // Transcribe audio samples
    member this.Transcribe(audioSamples: float32[]) : Async<string> = async {
        try
            printfn "🤖 Transcribing audio with Whisper.NET..."
            let startTime = DateTime.Now

            use processor = whisperFactory.CreateBuilder()
                                .WithLanguage("en")
                                .Build()

            let mutable transcription = ""

            // Process the audio and collect segments
            let segments = processor.ProcessAsync(audioSamples)

            // Iterate through async enumerable segments
            let enumerator = segments.GetAsyncEnumerator()
            let mutable hasMore = true

            while hasMore do
                let! moveResult = enumerator.MoveNextAsync().AsTask() |> Async.AwaitTask
                hasMore <- moveResult
                if hasMore then
                    let segment = enumerator.Current
                    transcription <- transcription + segment.Text

            // Dispose enumerator
            do! enumerator.DisposeAsync().AsTask() |> Async.AwaitTask

            let elapsed = (DateTime.Now - startTime).TotalSeconds
            printfn "✓ Transcription complete (%.2f seconds)" elapsed

            return transcription.Trim()
        with
        | ex ->
            eprintfn "✗ Transcription error: %s" ex.Message
            return ""
    }

    interface IDisposable with
        member this.Dispose() =
            whisperFactory.Dispose()

// Initialize and get transcription service
let createService (modelType: GgmlType) : Async<WhisperService> = async {
    let! modelPath = ensureModelDownloaded modelType
    return new WhisperService(modelPath)
}
