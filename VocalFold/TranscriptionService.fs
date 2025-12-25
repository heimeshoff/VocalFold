module TranscriptionService

open System
open System.IO
open System.Diagnostics
open System.Runtime.InteropServices
open Whisper.net
open Whisper.net.Ggml
open Whisper.net.LibraryLoader

// GPU Runtime detection types
type RuntimeType =
    | CUDA
    | Vulkan
    | CPU
    | Unknown

type RuntimeInfo = {
    RuntimeType: RuntimeType
    Description: string
    NativeLibrary: string option
}

// Mutable state to track runtime info and last transcription metrics
let mutable private detectedRuntime: RuntimeInfo option = None
let mutable private lastTranscriptionMetrics: (float * float * float) option = None // (audioDuration, transcriptionTime, rtf)

// Detect which Whisper.NET runtime is loaded
let detectRuntime () =
    try
        // Check loaded modules for Whisper native libraries
        let currentProcess = Process.GetCurrentProcess()
        let modules = currentProcess.Modules

        let mutable foundRuntime = None

        for i in 0 .. modules.Count - 1 do
            let m = modules.[i]
            let name = m.ModuleName.ToLowerInvariant()
            // Also check full path for runtime detection (module name alone may not contain "cuda"/"vulkan")
            let fullPath = m.FileName.ToLowerInvariant()

            if name.Contains("whisper") then
                Logger.debug (sprintf "Found Whisper module: %s" m.FileName)

                // Check both module name AND full path for CUDA/Vulkan indicators
                if name.Contains("cuda") || fullPath.Contains("\\cuda\\") || fullPath.Contains("/cuda/") then
                    foundRuntime <- Some {
                        RuntimeType = CUDA
                        Description = "NVIDIA CUDA (GPU accelerated)"
                        NativeLibrary = Some m.FileName
                    }
                elif name.Contains("vulkan") || fullPath.Contains("\\vulkan\\") || fullPath.Contains("/vulkan/") then
                    foundRuntime <- Some {
                        RuntimeType = Vulkan
                        Description = "AMD/Intel Vulkan (GPU accelerated)"
                        NativeLibrary = Some m.FileName
                    }
                elif name.Contains("ggml") || name = "whisper.dll" then
                    if foundRuntime.IsNone then
                        foundRuntime <- Some {
                            RuntimeType = CPU
                            Description = "CPU (no GPU acceleration)"
                            NativeLibrary = Some m.FileName
                        }

        match foundRuntime with
        | Some runtime ->
            detectedRuntime <- Some runtime
            runtime
        | None ->
            let unknown = {
                RuntimeType = Unknown
                Description = "Unknown (could not detect loaded runtime)"
                NativeLibrary = None
            }
            detectedRuntime <- Some unknown
            unknown
    with
    | ex ->
        Logger.warning (sprintf "Failed to detect runtime: %s" ex.Message)
        let unknown = {
            RuntimeType = Unknown
            Description = sprintf "Detection failed: %s" ex.Message
            NativeLibrary = None
        }
        detectedRuntime <- Some unknown
        unknown

// Get the currently detected runtime (for API exposure)
let getDetectedRuntime () = detectedRuntime

// Get the last transcription metrics (audioDuration, transcriptionTime, realTimeFactor)
let getLastTranscriptionMetrics () = lastTranscriptionMetrics

// Check for suspicious transcription output patterns
let private checkSuspiciousOutput (text: string) =
    if String.IsNullOrWhiteSpace(text) then
        None
    else
        // Check if output is mostly repeated punctuation
        let punctuationChars = ['!'; '.'; '?'; ','; ';'; ':']
        let punctCount = text |> Seq.filter (fun c -> List.contains c punctuationChars) |> Seq.length
        let totalChars = text.Length

        if totalChars > 0 && float punctCount / float totalChars > 0.8 then
            Some (sprintf "Output is %.0f%% punctuation: '%s'" (float punctCount / float totalChars * 100.0) text)
        // Check if single character repeated many times
        elif totalChars > 3 then
            let distinctChars = text |> Seq.distinct |> Seq.length
            if distinctChars <= 2 then
                Some (sprintf "Output contains only %d distinct character(s): '%s'" distinctChars text)
            else
                None
        else
            None

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
        Logger.info (sprintf "Created model directory: %s" modelDir)

    // Check if model already exists
    if File.Exists(modelPath) then
        Logger.info (sprintf "Model found: %s" modelPath)
        return modelPath
    else
        Logger.info (sprintf "Downloading Whisper.NET model (%A)..." modelType)
        Logger.info "This may take a few minutes on first run..."

        try
            // Download the model
            use! downloader = WhisperGgmlDownloader.GetGgmlModelAsync(modelType) |> Async.AwaitTask

            // Save to file
            use fileStream = File.Create(modelPath)
            do! downloader.CopyToAsync(fileStream) |> Async.AwaitTask

            Logger.info (sprintf "Model downloaded successfully: %s" modelPath)
            return modelPath
        with
        | ex ->
            Logger.logException ex "Failed to download Whisper model"
            return raise ex
}

// Whisper transcription service
type WhisperService(modelPath: string, factoryOverride: WhisperFactory option) =
    let whisperFactory =
        match factoryOverride with
        | Some f -> f
        | None -> WhisperFactory.FromPath(modelPath)

    // Constructor for normal usage
    new(modelPath: string) = new WhisperService(modelPath, None)

    // Transcribe audio samples
    member this.Transcribe(audioSamples: float32[]) : Async<string> = async {
        try
            Logger.info "Transcribing audio with Whisper.NET..."
            let startTime = DateTime.Now

            // Calculate audio duration (16kHz sample rate)
            let audioDuration = float audioSamples.Length / 16000.0
            Logger.info (sprintf "Audio duration: %.2f seconds (%d samples)" audioDuration audioSamples.Length)

            // Build processor without language specification to enable auto-detection
            // This allows Whisper to transcribe in the original spoken language
            use processor = whisperFactory.CreateBuilder()
                                .WithLanguage("auto")
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
            let realTimeFactor = if audioDuration > 0.0 then elapsed / audioDuration else 0.0

            // Store metrics for API exposure
            lastTranscriptionMetrics <- Some (audioDuration, elapsed, realTimeFactor)

            // Log detailed performance info
            Logger.info (sprintf "Transcription complete: %.2fs audio -> %.2fs processing (RTF: %.2fx)" audioDuration elapsed realTimeFactor)

            // Warn if transcription is very slow (likely CPU mode)
            if realTimeFactor > 3.0 then
                Logger.warning (sprintf "SLOW TRANSCRIPTION DETECTED (RTF: %.2fx) - GPU acceleration may not be working!" realTimeFactor)
                Logger.warning "Check the logs above for runtime detection info. Expected RTF < 1.0x with GPU."

            // Check for suspicious output patterns
            let trimmedResult = transcription.Trim()
            match checkSuspiciousOutput trimmedResult with
            | Some warning ->
                Logger.warning (sprintf "SUSPICIOUS OUTPUT DETECTED: %s" warning)
                Logger.warning "This may indicate Whisper model issues or audio problems."
            | None -> ()

            return trimmedResult
        with
        | ex ->
            Logger.logException ex "Transcription error"
            return ""
    }

    interface IDisposable with
        member this.Dispose() =
            whisperFactory.Dispose()

// Configure runtime library order to prioritize GPU runtimes
let private configureRuntimeOrder () =
    try
        Logger.info "Configuring Whisper.NET runtime selection..."

        // Set runtime order: CUDA first (NVIDIA), then Vulkan (AMD/Intel), then CPU fallback
        // CUDA is more reliable for NVIDIA GPUs than Vulkan acceleration
        let runtimeOrder = System.Collections.Generic.List<RuntimeLibrary>()
        runtimeOrder.Add(RuntimeLibrary.Cuda)     // NVIDIA GPUs (most common, most reliable)
        runtimeOrder.Add(RuntimeLibrary.Vulkan)   // AMD/Intel GPUs
        runtimeOrder.Add(RuntimeLibrary.Cpu)      // CPU fallback

        RuntimeOptions.Instance.SetRuntimeLibraryOrder(runtimeOrder)

        Logger.info "Runtime order set: CUDA -> Vulkan -> CPU"
        true
    with
    | ex ->
        Logger.warning (sprintf "Could not configure runtime order: %s" ex.Message)
        Logger.warning "Using default runtime selection"
        false

// Try to create a WhisperService with optional CPU fallback
let private tryCreateServiceWithFallback (modelPath: string) : WhisperService =
    // First, verify the model file exists and is readable
    if not (IO.File.Exists(modelPath)) then
        let msg = sprintf "Model file not found: %s" modelPath
        Logger.error msg
        failwith msg

    let fileInfo = IO.FileInfo(modelPath)
    Logger.info (sprintf "Model file: %s (%.2f MB)" modelPath (float fileInfo.Length / 1024.0 / 1024.0))

    // Check if the model file looks valid (should start with GGML magic bytes)
    try
        use fs = IO.File.OpenRead(modelPath)
        let header = Array.zeroCreate<byte> 4
        let bytesRead = fs.Read(header, 0, 4)
        if bytesRead >= 4 then
            let headerStr = System.Text.Encoding.ASCII.GetString(header)
            Logger.debug (sprintf "Model header bytes: %02X %02X %02X %02X (%s)" header.[0] header.[1] header.[2] header.[3] headerStr)
    with
    | ex -> Logger.warning (sprintf "Could not read model header: %s" ex.Message)

    try
        // First attempt - use configured runtime order (CUDA -> Vulkan -> CPU)
        Logger.info "Creating WhisperFactory..."
        let factory = WhisperFactory.FromPath(modelPath)
        Logger.info "WhisperFactory created, testing model loading..."

        // Test that we can actually create a builder (this is where CUDA failures happen)
        try
            use testProcessor = factory.CreateBuilder().WithLanguage("auto").Build()
            Logger.info "Model loading test successful!"
            new WhisperService(modelPath, Some factory)
        with
        | ex ->
            Logger.warning (sprintf "Model loading failed with configured runtime: %s" ex.Message)
            if ex.InnerException <> null then
                Logger.warning (sprintf "  Inner exception: %s" ex.InnerException.Message)
            Logger.warning "Attempting fallback to CPU-only mode..."

            // Dispose the failed factory
            factory.Dispose()

            // Try CPU-only mode
            try
                Logger.info "Reconfiguring for CPU-only runtime..."
                let cpuOrder = System.Collections.Generic.List<RuntimeLibrary>()
                cpuOrder.Add(RuntimeLibrary.Cpu)
                RuntimeOptions.Instance.SetRuntimeLibraryOrder(cpuOrder)

                Logger.info "Creating new WhisperFactory with CPU runtime..."
                let cpuFactory = WhisperFactory.FromPath(modelPath)

                // Test CPU mode
                use cpuTestProcessor = cpuFactory.CreateBuilder().WithLanguage("auto").Build()
                Logger.info "CPU fallback successful!"
                new WhisperService(modelPath, Some cpuFactory)
            with
            | cpuEx ->
                Logger.error (sprintf "CPU fallback also failed: %s" cpuEx.Message)
                if cpuEx.InnerException <> null then
                    Logger.error (sprintf "  Inner exception: %s" cpuEx.InnerException.Message)
                raise ex // Re-raise original exception
    with
    | ex ->
        Logger.logException ex "Failed to create WhisperService"
        raise ex

// Initialize and get transcription service
let createService (modelType: GgmlType) : Async<WhisperService> = async {
    try
        Logger.info (sprintf "Initializing Whisper service with model: %A" modelType)

        // Configure runtime order BEFORE loading the model
        Logger.info "========================================="
        Logger.info "GPU RUNTIME CONFIGURATION"
        Logger.info "========================================="
        let configured = configureRuntimeOrder()
        if not configured then
            Logger.warning "Runtime configuration failed - will use automatic selection"
        Logger.info "========================================="

        let! modelPath = ensureModelDownloaded modelType

        Logger.info "Creating WhisperService instance..."
        let service = tryCreateServiceWithFallback modelPath
        Logger.info "Whisper service initialized successfully"

        // Detect and log which runtime is actually loaded
        Logger.info "========================================="
        Logger.info "GPU RUNTIME DETECTION"
        Logger.info "========================================="

        let runtime = detectRuntime()

        match runtime.RuntimeType with
        | CUDA ->
            Logger.info "RUNTIME: NVIDIA CUDA (GPU accelerated)"
            Logger.info "  Status: Optimal performance expected"
        | Vulkan ->
            Logger.info "RUNTIME: Vulkan (AMD/Intel GPU accelerated)"
            Logger.info "  Status: GPU acceleration active"
        | CPU ->
            Logger.warning "RUNTIME: CPU ONLY (no GPU acceleration)"
            Logger.warning "  Performance will be significantly slower!"
            Logger.warning "  For NVIDIA: Install CUDA Toolkit 12.x"
            Logger.warning "  For AMD: Ensure latest Adrenalin drivers with Vulkan support"
            Logger.warning "  Note: The Vulkan runtime requires:"
            Logger.warning "    - Vulkan SDK >= 1.4 installed"
            Logger.warning "    - GPU drivers with Vulkan support"
        | Unknown ->
            Logger.warning "RUNTIME: Could not detect which runtime is loaded"
            Logger.warning "  This may indicate a configuration issue"

        match runtime.NativeLibrary with
        | Some lib -> Logger.info (sprintf "  Native library: %s" lib)
        | None -> ()

        Logger.info "========================================="

        return service
    with
    | ex ->
        Logger.logException ex "Failed to initialize Whisper service"
        return raise ex
}
