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

// P/Invoke for getting tray icon position and making window click-through
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
    let canvas = new Canvas(Width = 200.0, Height = 200.0)
    let microphoneGroup = new Canvas()
    let waveformBars = Array.init 5 (fun _ -> new Rectangle())
    let statusText = new TextBlock(
        FontSize = 12.0,
        Foreground = Brushes.White,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = Thickness(0.0, 0.0, 0.0, 10.0)
    )

    // Animation timer
    let animationTimer = new DispatcherTimer(Interval = TimeSpan.FromMilliseconds(33.0)) // ~30 fps

    do
        // Configure window
        this.Width <- 200.0
        this.Height <- 200.0
        this.WindowStyle <- WindowStyle.None
        this.ResizeMode <- ResizeMode.NoResize
        this.AllowsTransparency <- true
        this.Background <- Brushes.Transparent
        this.Topmost <- true
        this.ShowInTaskbar <- false
        this.Title <- "VocalFold Overlay"

        // Set initial position (will be updated to near tray)
        this.Left <- SystemParameters.PrimaryScreenWidth - 250.0
        this.Top <- SystemParameters.PrimaryScreenHeight - 250.0

        // Create microphone icon
        this.createMicrophoneIcon()

        // Create waveform bars
        this.createWaveformBars()

        // Add status text
        Canvas.SetLeft(statusText, 100.0 - 30.0)
        Canvas.SetTop(statusText, 170.0)
        canvas.Children.Add(statusText) |> ignore

        // Set content
        this.Content <- canvas

        // Animation timer tick
        animationTimer.Tick.Add(fun _ -> this.updateAnimation())

    member private this.createMicrophoneIcon() =
        // Create a stylized microphone icon
        let micBody = new Ellipse(
            Width = 40.0,
            Height = 60.0,
            Fill = Brushes.White,
            Stroke = Brushes.White,
            StrokeThickness = 3.0
        )

        let micStand = new Rectangle(
            Width = 4.0,
            Height = 30.0,
            Fill = Brushes.White
        )

        let micBase = new Rectangle(
            Width = 40.0,
            Height = 6.0,
            Fill = Brushes.White,
            RadiusX = 3.0,
            RadiusY = 3.0
        )

        // Position elements
        Canvas.SetLeft(micBody, 80.0)
        Canvas.SetTop(micBody, 40.0)

        Canvas.SetLeft(micStand, 98.0)
        Canvas.SetTop(micStand, 100.0)

        Canvas.SetLeft(micBase, 80.0)
        Canvas.SetTop(micBase, 130.0)

        microphoneGroup.Children.Add(micBody) |> ignore
        microphoneGroup.Children.Add(micStand) |> ignore
        microphoneGroup.Children.Add(micBase) |> ignore

        canvas.Children.Add(microphoneGroup) |> ignore

    member private this.createWaveformBars() =
        // Create 5 vertical bars for audio visualization
        let barWidth = 8.0
        let barSpacing = 12.0
        let startX = 60.0

        for i in 0 .. 4 do
            let bar = waveformBars.[i]
            bar.Width <- barWidth
            bar.Height <- 10.0
            bar.Fill <- Brushes.White
            bar.RadiusX <- 2.0
            bar.RadiusY <- 2.0

            Canvas.SetLeft(bar, startX + float i * barSpacing)
            Canvas.SetTop(bar, 150.0)

            canvas.Children.Add(bar) |> ignore

    member private this.updateAnimation() =
        match currentState with
        | Recording ->
            // Animate waveform bars based on audio level
            let random = Random()
            for i in 0 .. 4 do
                let bar = waveformBars.[i]
                let targetHeight = 10.0 + (currentLevel * 80.0) + float (random.Next(10))

                // Smooth animation
                let anim = DoubleAnimation(
                    To = Nullable targetHeight,
                    Duration = Duration(TimeSpan.FromMilliseconds(100.0)),
                    EasingFunction = QuadraticEase(EasingMode = EasingMode.EaseOut)
                )
                bar.BeginAnimation(Rectangle.HeightProperty, anim)

            // Pulse microphone
            let pulseScale = 1.0 + (currentLevel * 0.2)
            let scaleAnim = DoubleAnimation(
                To = Nullable pulseScale,
                Duration = Duration(TimeSpan.FromMilliseconds(200.0)),
                AutoReverse = true
            )
            microphoneGroup.RenderTransform <- ScaleTransform(1.0, 1.0, 100.0, 100.0)
            microphoneGroup.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim)
            microphoneGroup.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim)

        | Transcribing ->
            // Gentle pulsing animation
            let pulseAnim = DoubleAnimation(
                From = Nullable 0.8,
                To = Nullable 1.0,
                Duration = Duration(TimeSpan.FromMilliseconds(800.0)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            )
            this.BeginAnimation(Window.OpacityProperty, pulseAnim)

        | _ -> ()

    member this.ShowOverlay(state: OverlayState, ?audioLevel: float) =
        currentState <- state
        currentLevel <- defaultArg audioLevel 0.0

        // Update colors based on state
        match state with
        | Recording ->
            // Blue/white for recording
            microphoneGroup.Children |> Seq.cast<Shape> |> Seq.iter (fun shape ->
                shape.Fill <- Brushes.DodgerBlue
                shape.Stroke <- Brushes.DodgerBlue
            )
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.DodgerBlue)
            statusText.Text <- "Recording..."
            statusText.Foreground <- Brushes.DodgerBlue

        | Transcribing ->
            // Green for transcribing
            microphoneGroup.Children |> Seq.cast<Shape> |> Seq.iter (fun shape ->
                shape.Fill <- Brushes.LimeGreen
                shape.Stroke <- Brushes.LimeGreen
            )
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.LimeGreen)
            statusText.Text <- "Transcribing..."
            statusText.Foreground <- Brushes.LimeGreen

        | Error ->
            // Red for error
            microphoneGroup.Children |> Seq.cast<Shape> |> Seq.iter (fun shape ->
                shape.Fill <- Brushes.Red
                shape.Stroke <- Brushes.Red
            )
            waveformBars |> Array.iter (fun bar -> bar.Fill <- Brushes.Red)
            statusText.Text <- "Error"
            statusText.Foreground <- Brushes.Red

        | Hidden ->
            ()

        // Position near tray (bottom-right of screen)
        this.positionNearTray()

        // Show window if not visible
        if not this.IsVisible then
            this.Visibility <- Visibility.Visible

        // Fade in
        this.Opacity <- 0.0
        let fadeIn = DoubleAnimation(
            From = Nullable 0.0,
            To = Nullable 1.0,
            Duration = Duration(TimeSpan.FromMilliseconds(200.0))
        )
        this.BeginAnimation(Window.OpacityProperty, fadeIn)

        // Start animation timer
        if state = Recording then
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

    member private this.positionNearTray() =
        // Try to find the actual tray icon position
        // The system tray is part of the taskbar, usually at the bottom-right
        // We'll try to find the taskbar and position near it

        let mutable rect = RECT()

        // Try to find the taskbar window
        let taskbarHandle = FindWindow("Shell_TrayWnd", null)

        if taskbarHandle <> IntPtr.Zero && GetWindowRect(taskbarHandle, &rect) then
            // Calculate taskbar position and dimensions
            let taskbarLeft = float rect.Left
            let taskbarTop = float rect.Top
            let taskbarRight = float rect.Right
            let taskbarBottom = float rect.Bottom
            let taskbarWidth = taskbarRight - taskbarLeft
            let taskbarHeight = taskbarBottom - taskbarTop

            // Determine taskbar position (bottom, top, left, or right)
            let workingArea = SystemParameters.WorkArea
            let primaryWidth = SystemParameters.PrimaryScreenWidth
            let primaryHeight = SystemParameters.PrimaryScreenHeight

            // Get DPI scaling factor
            let dpiScale = System.Windows.Media.VisualTreeHelper.GetDpi(this).DpiScaleX

            if taskbarHeight < taskbarWidth then
                // Horizontal taskbar (bottom or top)
                if taskbarTop > workingArea.Top then
                    // Taskbar at bottom
                    this.Left <- workingArea.Right - this.Width - 20.0
                    this.Top <- taskbarTop - this.Height - 10.0
                else
                    // Taskbar at top
                    this.Left <- workingArea.Right - this.Width - 20.0
                    this.Top <- taskbarBottom + 10.0
            else
                // Vertical taskbar (left or right)
                if taskbarLeft > workingArea.Left then
                    // Taskbar at right
                    this.Left <- taskbarLeft - this.Width - 10.0
                    this.Top <- workingArea.Bottom - this.Height - 20.0
                else
                    // Taskbar at left
                    this.Left <- taskbarRight + 10.0
                    this.Top <- workingArea.Bottom - this.Height - 20.0
        else
            // Fallback to default bottom-right position if we can't find taskbar
            let workingArea = SystemParameters.WorkArea
            this.Left <- workingArea.Right - this.Width - 20.0
            this.Top <- workingArea.Bottom - this.Height - 20.0

        // Ensure window is within screen bounds (handle multi-monitor edge cases)
        let screenLeft = SystemParameters.VirtualScreenLeft
        let screenTop = SystemParameters.VirtualScreenTop
        let screenWidth = SystemParameters.VirtualScreenWidth
        let screenHeight = SystemParameters.VirtualScreenHeight

        // Clamp position to screen bounds
        if this.Left < screenLeft then
            this.Left <- screenLeft
        if this.Top < screenTop then
            this.Top <- screenTop
        if this.Left + this.Width > screenLeft + screenWidth then
            this.Left <- screenLeft + screenWidth - this.Width
        if this.Top + this.Height > screenTop + screenHeight then
            this.Top <- screenTop + screenHeight - this.Height

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
