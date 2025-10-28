module OverlayWindow

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Media.Animation
open System.Windows.Shapes
open System.Windows.Threading
open System.Runtime.InteropServices

// Define RECT structure first (must come before P/Invoke declarations)
[<StructLayout(LayoutKind.Sequential)>]
type RECT =
    struct
        val mutable Left: int
        val mutable Top: int
        val mutable Right: int
        val mutable Bottom: int
    end

// Define POINT structure for cursor position
[<StructLayout(LayoutKind.Sequential)>]
type POINT =
    struct
        val mutable X: int
        val mutable Y: int
    end

// P/Invoke for getting cursor position and making window click-through
[<DllImport("user32.dll", SetLastError = true)>]
extern bool GetCursorPos(POINT& lpPoint)

[<DllImport("user32.dll", SetLastError = true)>]
extern bool GetWindowRect(IntPtr hWnd, RECT& lpRect)

[<DllImport("user32.dll", SetLastError = true)>]
extern IntPtr FindWindow(string lpClassName, string lpWindowName)

[<DllImport("user32.dll")>]
extern int GetWindowLong(IntPtr hWnd, int nIndex)

[<DllImport("user32.dll")>]
extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)

// Window styles
let WS_EX_TRANSPARENT = 0x00000020
let WS_EX_LAYERED = 0x00080000
let GWL_EXSTYLE = -20

// Overlay state
type OverlayState =
    | Hidden
    | Ready          // Overlay visible, waiting for recording to start
    | Recording      // Recording and detecting voice
    | Transcribing   // Processing transcription
    | Error

type OverlayWindow() as this =
    inherit Window()

    let mutable currentState = Hidden
    let mutable currentLevel = 0.0
    let mutable hasVoiceActivity = false  // Track if voice has been detected
    let mutable spectrumData = Array.zeroCreate<float> 5  // Frequency spectrum for 5 bars

    // Visual elements
    let canvas = new Canvas(Width = 120.0, Height = 60.0)
    let backgroundRect = new Rectangle(
        Width = 120.0,
        Height = 60.0,
        RadiusX = 8.0,
        RadiusY = 8.0,
        Fill = new SolidColorBrush(Color.FromArgb(180uy, 30uy, 30uy, 30uy)) // Semi-transparent dark
    )
    let waveformBars = Array.init 5 (fun _ -> new Rectangle())
    let transcribingDots = Array.init 3 (fun _ -> new Ellipse())

    // Animation timer
    let animationTimer = new DispatcherTimer(Interval = TimeSpan.FromMilliseconds(33.0)) // ~30 fps

    do
        // Configure window
        this.Width <- 120.0
        this.Height <- 60.0
        this.WindowStyle <- WindowStyle.None
        this.ResizeMode <- ResizeMode.NoResize
        this.AllowsTransparency <- true
        this.Background <- Brushes.Transparent
        this.Topmost <- true
        this.ShowInTaskbar <- false
        this.Title <- "VocalFold Overlay"

        // Set initial position (will be updated to cursor position)
        this.Left <- SystemParameters.PrimaryScreenWidth / 2.0
        this.Top <- SystemParameters.PrimaryScreenHeight / 2.0

        // Add background first
        canvas.Children.Add(backgroundRect) |> ignore

        // Create waveform bars
        this.createWaveformBars()

        // Create transcribing dots (initially hidden)
        this.createTranscribingDots()

        // Set content
        this.Content <- canvas

        // Animation timer tick
        animationTimer.Tick.Add(fun _ -> this.updateAnimation())

    member private this.createWaveformBars() =
        // Create 5 vertical bars for audio visualization - centered in the compact window
        let barWidth = 6.0
        let barSpacing = 10.0
        let totalWidth = (5.0 * barWidth) + (4.0 * barSpacing)
        let startX = (120.0 - totalWidth) / 2.0  // Center horizontally
        let centerY = 30.0  // Vertical center of 60px window

        for i in 0 .. 4 do
            let bar = waveformBars.[i]
            bar.Width <- barWidth
            bar.Height <- 2.0  // Start with minimal height (slim line)
            bar.Fill <- Brushes.DodgerBlue
            bar.RadiusX <- 3.0
            bar.RadiusY <- 3.0

            // Position bars centered vertically, they will grow up and down from center
            let xPos = startX + float i * (barWidth + barSpacing)
            Canvas.SetLeft(bar, xPos)
            Canvas.SetTop(bar, centerY - (bar.Height / 2.0))  // Center vertically

            canvas.Children.Add(bar) |> ignore

    member private this.createTranscribingDots() =
        // Create 3 dots for transcribing animation - centered in the window
        let dotSize = 8.0
        let dotSpacing = 16.0
        let totalWidth = (3.0 * dotSize) + (2.0 * dotSpacing)
        let startX = (120.0 - totalWidth) / 2.0
        let centerY = 30.0

        for i in 0 .. 2 do
            let dot = transcribingDots.[i]
            dot.Width <- dotSize
            dot.Height <- dotSize
            dot.Fill <- Brushes.LimeGreen
            dot.Visibility <- Visibility.Collapsed  // Hidden by default

            let xPos = startX + float i * (dotSize + dotSpacing)
            Canvas.SetLeft(dot, xPos)
            Canvas.SetTop(dot, centerY - dotSize / 2.0)

            canvas.Children.Add(dot) |> ignore

    member private this.updateAnimation() =
        match currentState with
        | Recording ->
            // Animate bars based on spectrum data
            // Use 90% of window height (60px), leaving 5% padding top and bottom
            let windowHeight = 60.0
            let maxHeight = windowHeight * 0.9  // 54px max
            let minHeight = 2.0  // Slim line when silent
            let centerY = windowHeight / 2.0  // 30px

            for i in 0 .. 4 do
                let bar = waveformBars.[i]
                // Use spectrum data for each bar
                let spectrumValue = spectrumData.[i]

                // Map spectrum to height
                let targetHeight = minHeight + (spectrumValue * (maxHeight - minHeight))

                // Animate both height and position to keep centered
                let heightAnim = DoubleAnimation(
                    To = Nullable targetHeight,
                    Duration = Duration(TimeSpan.FromMilliseconds(50.0)),
                    EasingFunction = QuadraticEase(EasingMode = EasingMode.EaseOut)
                )

                let topAnim = DoubleAnimation(
                    To = Nullable (centerY - targetHeight / 2.0),
                    Duration = Duration(TimeSpan.FromMilliseconds(50.0)),
                    EasingFunction = QuadraticEase(EasingMode = EasingMode.EaseOut)
                )

                bar.BeginAnimation(Rectangle.HeightProperty, heightAnim)
                bar.BeginAnimation(Canvas.TopProperty, topAnim)

        | Transcribing ->
            // Animate dots moving up and down in a wave pattern
            let time = DateTime.Now.TimeOfDay.TotalMilliseconds
            for i in 0 .. 2 do
                let dot = transcribingDots.[i]
                let phase = float i * Math.PI / 3.0  // Offset each dot by 60 degrees
                let offset = Math.Sin((time / 300.0) + phase) * 8.0  // Move up/down by 8 pixels
                let centerY = 30.0

                Canvas.SetTop(dot, centerY - 4.0 + offset)

        | _ -> ()

    member this.ShowOverlay(state: OverlayState, ?audioLevel: float) =
        currentState <- state
        currentLevel <- defaultArg audioLevel 0.0

        // Reset voice activity flag when showing ready/recording state
        if state = Ready || state = Recording then
            hasVoiceActivity <- false

        // Update colors and visibility based on state
        match state with
        | Ready ->
            // Blue background appears immediately, but NO bars yet
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 30uy, 60uy, 120uy)) // Blue-tinted background
            waveformBars |> Array.iter (fun bar ->
                bar.Fill <- Brushes.DodgerBlue
                bar.Visibility <- Visibility.Collapsed  // Hidden until voice detected
                bar.Height <- 2.0  // Start with minimal height (slim line)
                bar.BeginAnimation(Rectangle.HeightProperty, null)  // Stop any animation
            )
            transcribingDots |> Array.iter (fun dot -> dot.Visibility <- Visibility.Collapsed)
            // Start animation timer in Ready state so it's running when bars appear
            animationTimer.Start()

        | Recording ->
            // Bars become visible and animate when voice is detected
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 30uy, 60uy, 120uy)) // Blue-tinted background
            waveformBars |> Array.iter (fun bar ->
                bar.Fill <- Brushes.DodgerBlue
                bar.Visibility <- Visibility.Visible  // Now visible
            )
            transcribingDots |> Array.iter (fun dot -> dot.Visibility <- Visibility.Collapsed)

        | Transcribing ->
            // Green background, hide bars and show animated dots
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 30uy, 120uy, 60uy)) // Green-tinted background
            waveformBars |> Array.iter (fun bar ->
                bar.Visibility <- Visibility.Collapsed
                bar.BeginAnimation(Rectangle.HeightProperty, null)  // Stop any animation
            )
            transcribingDots |> Array.iter (fun dot -> dot.Visibility <- Visibility.Visible)

        | Error ->
            // Red for error
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 120uy, 30uy, 30uy)) // Red-tinted background
            waveformBars |> Array.iter (fun bar ->
                bar.Fill <- Brushes.Red
                bar.Visibility <- Visibility.Visible
            )
            transcribingDots |> Array.iter (fun dot -> dot.Visibility <- Visibility.Collapsed)

        | Hidden ->
            ()

        // Position at cursor
        this.positionAtCursor()

        // Show window if not visible
        if not this.IsVisible then
            this.Visibility <- Visibility.Visible

        // Immediate show for Ready state, fade in for others
        if state = Ready then
            this.Opacity <- 1.0
            this.BeginAnimation(Window.OpacityProperty, null)  // Cancel any fade animation
        else
            this.Opacity <- 0.0
            let fadeIn = DoubleAnimation(
                From = Nullable 0.0,
                To = Nullable 1.0,
                Duration = Duration(TimeSpan.FromMilliseconds(150.0))
            )
            this.BeginAnimation(Window.OpacityProperty, fadeIn)

        // Start animation timer for Recording and Transcribing states
        if state = Recording || state = Transcribing then
            animationTimer.Start()
        else
            animationTimer.Stop()

    member this.UpdateLevel(level: float) =
        currentLevel <- level

        // Detect voice activity (threshold-based)
        if level > 0.02 then
            if not hasVoiceActivity then
                hasVoiceActivity <- true
                // Show bars when voice is detected in Ready state
                if currentState = Ready then
                    // Make bars visible without changing state
                    waveformBars |> Array.iter (fun bar -> bar.Visibility <- Visibility.Visible)
                    currentState <- Recording
                    animationTimer.Start()

    member this.UpdateSpectrum(spectrum: float32[]) =
        // Update spectrum data for visualization
        if spectrum.Length = 5 then
            for i in 0 .. 4 do
                spectrumData.[i] <- float spectrum.[i]
            // Debug: Log spectrum values to see if we're receiving data
            // Uncomment for debugging:
            // Logger.debug (sprintf "Spectrum: [%.3f, %.3f, %.3f, %.3f, %.3f]" spectrum.[0] spectrum.[1] spectrum.[2] spectrum.[3] spectrum.[4])

    member this.HideOverlay() =
        // Stop animation
        animationTimer.Stop()

        // Fade out
        let fadeOut = DoubleAnimation(
            From = Nullable 1.0,
            To = Nullable 0.0,
            Duration = Duration(TimeSpan.FromMilliseconds(300.0))
        )

        fadeOut.Completed.Add(fun _ ->
            this.Visibility <- Visibility.Hidden
            currentState <- Hidden
        )

        this.BeginAnimation(Window.OpacityProperty, fadeOut)

    member private this.positionAtCursor() =
        // Position window centered horizontally and slightly above the taskbar
        let screenWidth = SystemParameters.PrimaryScreenWidth
        let workArea = SystemParameters.WorkArea

        // Center horizontally
        this.Left <- (screenWidth - this.Width) / 2.0

        // Position slightly above the taskbar
        // WorkArea.Bottom gives us the bottom of the usable area (top of taskbar)
        let offsetFromTaskbar = 10.0 // Pixels above the taskbar
        this.Top <- workArea.Bottom - this.Height - offsetFromTaskbar

    override this.OnSourceInitialized(e: EventArgs) =
        base.OnSourceInitialized(e)

        // Make window click-through
        let hwnd = (new System.Windows.Interop.WindowInteropHelper(this)).Handle
        let extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle ||| WS_EX_TRANSPARENT ||| WS_EX_LAYERED) |> ignore

// Global overlay instance management
type OverlayManager() =
    let mutable overlayWindow: OverlayWindow option = None

    member this.Initialize() =
        match overlayWindow with
        | Some _ -> () // Already initialized
        | None ->
            let window = OverlayWindow()
            overlayWindow <- Some window

    member this.ShowReady() =
        this.Initialize()
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.ShowOverlay(Ready, 0.0)
            ) |> ignore
        | None -> ()

    member this.ShowRecording() =
        this.Initialize()
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.ShowOverlay(Recording, 0.0)
            ) |> ignore
        | None -> ()

    member this.UpdateLevel(level: float) =
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.UpdateLevel(level)
            ) |> ignore
        | None -> ()

    member this.UpdateSpectrum(spectrum: float32[]) =
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.UpdateSpectrum(spectrum)
            ) |> ignore
        | None -> ()

    member this.ShowTranscribing() =
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.ShowOverlay(Transcribing)
            ) |> ignore
        | None -> ()

    member this.ShowError() =
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.ShowOverlay(Error)
            ) |> ignore
        | None -> ()

    member this.Hide() =
        match overlayWindow with
        | Some window ->
            // Use BeginInvoke for non-blocking UI update
            window.Dispatcher.BeginInvoke(fun () ->
                window.HideOverlay()
            ) |> ignore
        | None -> ()

    member this.Cleanup() =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.Close()
            )
            overlayWindow <- None
        | None -> ()
