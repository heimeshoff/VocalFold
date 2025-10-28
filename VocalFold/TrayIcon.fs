module TrayIcon

open System
open System.Windows.Forms
open System.Drawing
open System.Runtime.InteropServices
open Microsoft.Win32

/// P/Invoke for setting foreground window (needed for proper context menu behavior)
[<DllImport("user32.dll")>]
extern bool SetForegroundWindow(IntPtr hWnd)

/// Configuration for the tray icon
type TrayConfig = {
    ApplicationName: string
    InitialEnabled: bool
    OnExit: unit -> unit
    OnToggleEnabled: bool -> unit
    OnSettings: unit -> unit
}

/// Tray icon state
type TrayState = {
    Icon: NotifyIcon
    mutable IsEnabled: bool
}

/// Load icon from logo.png file with larger visual size
let loadIconFromLogo () =
    try
        // Try to load logo.png from the application directory
        let appPath = AppDomain.CurrentDomain.BaseDirectory
        let logoPath = System.IO.Path.Combine(appPath, "logo.png")

        if System.IO.File.Exists(logoPath) then
            use originalBitmap = new Bitmap(logoPath)
            // Use 64x64 for maximum visibility in tray (Windows will scale appropriately)
            let resizedBitmap = new Bitmap(64, 64)
            use g = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.HighQuality
            g.PixelOffsetMode <- System.Drawing.Drawing2D.PixelOffsetMode.HighQuality
            g.CompositingQuality <- System.Drawing.Drawing2D.CompositingQuality.HighQuality
            g.DrawImage(originalBitmap, 0, 0, 64, 64)
            Icon.FromHandle(resizedBitmap.GetHicon())
        else
            // Fallback to simple icon if logo not found
            let bitmap = new Bitmap(64, 64)
            use g = Graphics.FromImage(bitmap)
            g.Clear(Color.Transparent)
            use brush = new SolidBrush(Color.White)
            g.FillEllipse(brush, 24, 12, 16, 24)
            g.FillRectangle(brush, 28, 36, 8, 12)
            g.FillRectangle(brush, 20, 48, 24, 8)
            Icon.FromHandle(bitmap.GetHicon())
    with
    | ex ->
        eprintfn "Error loading logo: %s" ex.Message
        // Fallback to simple icon
        let bitmap = new Bitmap(64, 64)
        use g = Graphics.FromImage(bitmap)
        g.Clear(Color.Transparent)
        use brush = new SolidBrush(Color.White)
        g.FillEllipse(brush, 24, 12, 16, 24)
        g.FillRectangle(brush, 28, 36, 8, 12)
        g.FillRectangle(brush, 20, 48, 24, 8)
        Icon.FromHandle(bitmap.GetHicon())

/// Create a grayed-out version for disabled state with larger visual size
let createDisabledIcon () =
    try
        // Try to load logo.png and make it grayscale
        let appPath = AppDomain.CurrentDomain.BaseDirectory
        let logoPath = System.IO.Path.Combine(appPath, "logo.png")

        if System.IO.File.Exists(logoPath) then
            use originalBitmap = new Bitmap(logoPath)
            // Use 64x64 for maximum visibility in tray
            let resizedBitmap = new Bitmap(64, 64)
            use g = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.HighQuality
            g.PixelOffsetMode <- System.Drawing.Drawing2D.PixelOffsetMode.HighQuality
            g.CompositingQuality <- System.Drawing.Drawing2D.CompositingQuality.HighQuality

            // Create grayscale color matrix
            let colorMatrix = new System.Drawing.Imaging.ColorMatrix(
                [| [| 0.3f; 0.3f; 0.3f; 0.0f; 0.0f |]
                   [| 0.59f; 0.59f; 0.59f; 0.0f; 0.0f |]
                   [| 0.11f; 0.11f; 0.11f; 0.0f; 0.0f |]
                   [| 0.0f; 0.0f; 0.0f; 0.5f; 0.0f |]  // Reduced alpha for disabled look
                   [| 0.0f; 0.0f; 0.0f; 0.0f; 1.0f |] |]
            )

            use attributes = new System.Drawing.Imaging.ImageAttributes()
            attributes.SetColorMatrix(colorMatrix)

            g.DrawImage(originalBitmap,
                        new Rectangle(0, 0, 64, 64),
                        0, 0, originalBitmap.Width, originalBitmap.Height,
                        GraphicsUnit.Pixel, attributes)
            Icon.FromHandle(resizedBitmap.GetHicon())
        else
            // Fallback
            let bitmap = new Bitmap(64, 64)
            use g = Graphics.FromImage(bitmap)
            g.Clear(Color.Transparent)
            use brush = new SolidBrush(Color.Gray)
            g.FillEllipse(brush, 24, 12, 16, 24)
            g.FillRectangle(brush, 28, 36, 8, 12)
            g.FillRectangle(brush, 20, 48, 24, 8)
            Icon.FromHandle(bitmap.GetHicon())
    with
    | ex ->
        eprintfn "Error loading disabled logo: %s" ex.Message
        // Fallback
        let bitmap = new Bitmap(64, 64)
        use g = Graphics.FromImage(bitmap)
        g.Clear(Color.Transparent)
        use brush = new SolidBrush(Color.Gray)
        g.FillEllipse(brush, 24, 12, 16, 24)
        g.FillRectangle(brush, 28, 36, 8, 12)
        g.FillRectangle(brush, 20, 48, 24, 8)
        Icon.FromHandle(bitmap.GetHicon())

/// Registry operations for Windows startup
module Startup =
    let private startupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run"
    let private appName = "VocalFold"

    /// Enable application to start with Windows
    let enable (exePath: string) =
        try
            use key = Registry.CurrentUser.OpenSubKey(startupKeyPath, true)
            if key <> null then
                key.SetValue(appName, exePath)
                true
            else
                false
        with
        | ex ->
            eprintfn "Error enabling startup: %s" ex.Message
            false

    /// Disable application from starting with Windows
    let disable () =
        try
            use key = Registry.CurrentUser.OpenSubKey(startupKeyPath, true)
            if key <> null then
                let value = key.GetValue(appName)
                if value <> null then
                    key.DeleteValue(appName)
            true
        with
        | ex ->
            eprintfn "Error disabling startup: %s" ex.Message
            false

    /// Check if application is set to start with Windows
    let isEnabled () =
        try
            use key = Registry.CurrentUser.OpenSubKey(startupKeyPath, false)
            if key <> null then
                let value = key.GetValue(appName)
                value <> null
            else
                false
        with
        | _ -> false

/// Create and configure the tray icon
let create (config: TrayConfig) : TrayState =
    let notifyIcon = new NotifyIcon()

    // Set up the icon
    let enabledIcon = loadIconFromLogo()
    let disabledIcon = createDisabledIcon()

    notifyIcon.Icon <- enabledIcon
    notifyIcon.Visible <- true
    notifyIcon.Text <- config.ApplicationName

    // Create context menu
    let contextMenu = new ContextMenuStrip()

    // Enabled/Disabled toggle menu item
    let toggleText = if config.InitialEnabled then "Enabled ✓" else "Disabled"
    let toggleItem = new ToolStripMenuItem(toggleText)

    // Settings menu item
    let settingsItem = new ToolStripMenuItem("Settings...")

    // Start with Windows menu item
    let startupItem = new ToolStripMenuItem("Start with Windows")
    startupItem.Checked <- Startup.isEnabled()

    // Separator
    let separator1 = new ToolStripSeparator()
    let separator2 = new ToolStripSeparator()

    // Exit menu item
    let exitItem = new ToolStripMenuItem("Exit")

    // Create state
    let state = { Icon = notifyIcon; IsEnabled = config.InitialEnabled }

    // Set initial icon based on enabled state
    if config.InitialEnabled then
        notifyIcon.Icon <- enabledIcon
    else
        notifyIcon.Icon <- disabledIcon

    // Toggle handler
    toggleItem.Click.Add(fun _ ->
        state.IsEnabled <- not state.IsEnabled

        if state.IsEnabled then
            toggleItem.Text <- "Enabled ✓"
            notifyIcon.Icon <- enabledIcon
        else
            toggleItem.Text <- "Disabled"
            notifyIcon.Icon <- disabledIcon

        config.OnToggleEnabled state.IsEnabled
    )

    // Settings handler
    settingsItem.Click.Add(fun _ ->
        config.OnSettings()
    )

    // Startup handler
    startupItem.Click.Add(fun _ ->
        let exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
        let exePath =
            if exePath.EndsWith(".dll") then
                // Running via dotnet, use the actual exe path
                System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            else
                exePath

        if startupItem.Checked then
            if Startup.disable() then
                startupItem.Checked <- false
        else
            if Startup.enable exePath then
                startupItem.Checked <- true
    )

    // Exit handler
    exitItem.Click.Add(fun _ ->
        config.OnExit()
    )

    // Add items to menu
    contextMenu.Items.Add(toggleItem) |> ignore
    contextMenu.Items.Add(separator1) |> ignore
    contextMenu.Items.Add(settingsItem) |> ignore
    contextMenu.Items.Add(startupItem) |> ignore
    contextMenu.Items.Add(separator2) |> ignore
    contextMenu.Items.Add(exitItem) |> ignore

    // Handle right-click manually for proper positioning
    notifyIcon.MouseClick.Add(fun e ->
        if e.Button = MouseButtons.Right then
            // Set foreground window BEFORE showing menu
            // This is required for proper menu dismissal behavior
            SetForegroundWindow(contextMenu.Handle) |> ignore

            // Get cursor position
            let cursorPosition = Cursor.Position

            // Show the menu at cursor position
            // This ensures the menu appears where the user clicked, with proper shadow alignment
            contextMenu.Show(cursorPosition)
    )

    // Double-click to toggle enabled state
    notifyIcon.DoubleClick.Add(fun _ ->
        toggleItem.PerformClick()
    )

    state

/// Show error notification
let notifyError (state: TrayState) (message: string) =
    state.Icon.ShowBalloonTip(2000, "VocalFold Error", message, ToolTipIcon.Error)

/// Show warning notification (for no speech detected, no audio, etc.)
let notifyWarning (state: TrayState) (message: string) =
    state.Icon.ShowBalloonTip(2000, "VocalFold", message, ToolTipIcon.Warning)

/// Show info notification (for settings changes, etc.)
let notifyInfo (state: TrayState) (message: string) =
    state.Icon.ShowBalloonTip(2000, "VocalFold", message, ToolTipIcon.Info)

/// Cleanup the tray icon
let dispose (state: TrayState) =
    state.Icon.Visible <- false
    state.Icon.Dispose()
