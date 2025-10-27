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
    | Recording
    | Transcribing
    | Error

type OverlayWindow() as this =
    inherit Window()

    let mutable currentState = Hidden
    let mutable currentLevel = 0.0

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
        let centerY = 30.0  // Vertical center

        for i in 0 .. 4 do
            let bar = waveformBars.[i]
            bar.Width <- barWidth
            bar.Height <- 15.0  // Start with modest height
            bar.Fill <- Brushes.DodgerBlue
            bar.RadiusX <- 3.0
            bar.RadiusY <- 3.0

            // Position bars centered, growing from center
            let xPos = startX + float i * (barWidth + barSpacing)
            Canvas.SetLeft(bar, xPos)
            Canvas.SetBottom(bar, centerY - (bar.Height / 2.0))

            canvas.Children.Add(bar) |> ignore

    member private this.updateAnimation() =
        match currentState with
        | Recording ->
            // Animate waveform bars based on audio level
            let random = Random()
            for i in 0 .. 4 do
                let bar = waveformBars.[i]
                // Vary heights to create wave effect, with center bars taller
                let centerBoost = if i = 2 then 1.3 elif i = 1 || i = 3 then 1.15 else 1.0
                let targetHeight = (10.0 + (currentLevel * 35.0) + float (random.Next(5))) * centerBoost

                // Smooth animation
                let anim = DoubleAnimation(
                    To = Nullable targetHeight,
                    Duration = Duration(TimeSpan.FromMilliseconds(100.0)),
                    EasingFunction = QuadraticEase(EasingMode = EasingMode.EaseOut)
                )
                bar.BeginAnimation(Rectangle.HeightProperty, anim)

        | Transcribing ->
            // Gentle pulsing animation for all bars
            for i in 0 .. 4 do
                let bar = waveformBars.[i]
                let pulseAnim = DoubleAnimation(
                    From = Nullable 15.0,
                    To = Nullable 25.0,
                    Duration = Duration(TimeSpan.FromMilliseconds(600.0)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                )
                bar.BeginAnimation(Rectangle.HeightProperty, pulseAnim)

        | _ -> ()

    member this.ShowOverlay(state: OverlayState, ?audioLevel: float) =
        currentState <- state
        currentLevel <- defaultArg audioLevel 0.0

        // Update colors based on state
        match state with
        | Recording ->
            // Blue for recording
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.DodgerBlue)
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 30uy, 60uy, 120uy)) // Blue-tinted background

        | Transcribing ->
            // Green for transcribing
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.LimeGreen)
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 30uy, 120uy, 60uy)) // Green-tinted background

        | Error ->
            // Red for error
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.Red)
            backgroundRect.Fill <- new SolidColorBrush(Color.FromArgb(200uy, 120uy, 30uy, 30uy)) // Red-tinted background

        | Hidden ->
            ()

        // Position at cursor
        this.positionAtCursor()

        // Show window if not visible
        if not this.IsVisible then
            this.Visibility <- Visibility.Visible

        // Fade in
        this.Opacity <- 0.0
        let fadeIn = DoubleAnimation(
            From = Nullable 0.0,
            To = Nullable 1.0,
            Duration = Duration(TimeSpan.FromMilliseconds(150.0))
        )
        this.BeginAnimation(Window.OpacityProperty, fadeIn)

        // Start animation timer
        if state = Recording || state = Transcribing then
            animationTimer.Start()

    member this.UpdateLevel(level: float) =
        currentLevel <- level

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

    member this.ShowRecording() =
        this.Initialize()
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.ShowOverlay(Recording, 0.0)
            )
        | None -> ()

    member this.UpdateLevel(level: float) =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.UpdateLevel(level)
            )
        | None -> ()

    member this.ShowTranscribing() =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.ShowOverlay(Transcribing)
            )
        | None -> ()

    member this.ShowError() =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.ShowOverlay(Error)
            )
        | None -> ()

    member this.Hide() =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.HideOverlay()
            )
        | None -> ()

    member this.Cleanup() =
        match overlayWindow with
        | Some window ->
            window.Dispatcher.Invoke(fun () ->
                window.Close()
            )
            overlayWindow <- None
        | None -> ()
