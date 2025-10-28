module ModernTheme

open System
open System.Drawing
open System.Drawing.Drawing2D
open System.Drawing.Text
open System.Windows.Forms
open System.Runtime.InteropServices

// Windows DWM API for acrylic effects
module private DwmApi =
    [<Struct>]
    [<StructLayout(LayoutKind.Sequential)>]
    type DWM_BLURBEHIND =
        val mutable dwFlags: int
        val mutable fEnable: bool
        val mutable hRgnBlur: nativeint
        val mutable fTransitionOnMaximized: bool

    [<DllImport("dwmapi.dll")>]
    extern int DwmExtendFrameIntoClientArea(nativeint hwnd, int& margins)

    [<DllImport("dwmapi.dll", PreserveSig = false)>]
    extern void DwmEnableBlurBehindWindow(nativeint hwnd, DWM_BLURBEHIND& blurBehind)

    [<DllImport("dwmapi.dll", PreserveSig = true)>]
    extern int DwmSetWindowAttribute(nativeint hwnd, int attr, int& attrValue, int attrSize)

/// Modern color palette inspired by PowerToys
module Colors =
    // Background colors (Forms don't support transparent backgrounds, Mica handles the effect)
    let DarkBackground = Color.FromArgb(30, 30, 30)           // #1E1E1E
    let CardBackground = Color.FromArgb(42, 42, 42)           // #2A2A2A
    let SidebarBackground = Color.FromArgb(25, 25, 25)        // #191919
    let HoverBackground = Color.FromArgb(50, 50, 50)          // #323232

    // Foreground colors
    let PrimaryText = Color.FromArgb(255, 255, 255)      // #FFFFFF
    let SecondaryText = Color.FromArgb(180, 180, 180)    // #B4B4B4
    let DisabledText = Color.FromArgb(120, 120, 120)     // #787878

    // Accent colors
    let AccentBlue = Color.FromArgb(96, 205, 255)        // #60CDFF - PowerToys blue
    let AccentBlueDark = Color.FromArgb(70, 180, 230)    // Darker shade
    let AccentGreen = Color.FromArgb(16, 185, 129)       // Success/enabled

    // Border colors
    let BorderColor = Color.FromArgb(60, 60, 60)         // #3C3C3C
    let FocusBorder = AccentBlue

    // Special colors
    let ErrorColor = Color.FromArgb(242, 80, 80)         // #F25050
    let WarningColor = Color.FromArgb(255, 185, 0)       // #FFB900

/// Custom toggle switch control (like PowerToys)
type ModernToggleSwitch() as this =
    inherit Control()

    let mutable isChecked = false
    let mutable isHovered = false
    let mutable isAnimating = false
    let mutable animationProgress = 0.0

    let switchWidth = 44
    let switchHeight = 24
    let thumbSize = 18
    let padding = 3

    do
        this.Size <- Size(switchWidth, switchHeight)
        this.Cursor <- Cursors.Hand
        this.DoubleBuffered <- true
        this.SetStyle(ControlStyles.UserPaint, true)
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true)
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true)

    let checkedChangedEvent = new Event<EventHandler, EventArgs>()

    member this.Checked
        with get() = isChecked
        and set(value) =
            if isChecked <> value then
                isChecked <- value
                this.Invalidate()
                checkedChangedEvent.Trigger(this, EventArgs.Empty)

    [<CLIEvent>]
    member this.CheckedChanged = checkedChangedEvent.Publish

    override this.OnPaint(e: PaintEventArgs) =
        let g = e.Graphics
        g.SmoothingMode <- SmoothingMode.AntiAlias

        // Background track
        let trackColor = if isChecked then Colors.AccentBlue else Colors.BorderColor
        let trackRect = Rectangle(0, 0, switchWidth - 1, switchHeight - 1)
        use trackBrush = new SolidBrush(trackColor)
        use trackPath = this.GetRoundedRect(trackRect, switchHeight / 2)
        g.FillPath(trackBrush, trackPath)

        // Thumb position
        let thumbX =
            if isChecked then
                switchWidth - thumbSize - padding
            else
                padding

        let thumbY = (switchHeight - thumbSize) / 2
        let thumbRect = Rectangle(thumbX, thumbY, thumbSize, thumbSize)

        // Draw thumb with shadow
        let shadowRect = Rectangle(thumbX + 1, thumbY + 1, thumbSize, thumbSize)
        use shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0))
        g.FillEllipse(shadowBrush, shadowRect)

        use thumbBrush = new SolidBrush(Color.White)
        g.FillEllipse(thumbBrush, thumbRect)

        base.OnPaint(e)

    override this.OnMouseEnter(e: EventArgs) =
        isHovered <- true
        this.Invalidate()
        base.OnMouseEnter(e)

    override this.OnMouseLeave(e: EventArgs) =
        isHovered <- false
        this.Invalidate()
        base.OnMouseLeave(e)

    override this.OnClick(e: EventArgs) =
        this.Checked <- not this.Checked
        base.OnClick(e)

    member private this.GetRoundedRect(rect: Rectangle, radius: int) =
        let path = new GraphicsPath()
        let diameter = radius * 2
        let size = Size(diameter, diameter)
        let mutable arc = Rectangle(rect.Location, size)

        // Top left arc
        path.AddArc(arc, 180.0f, 90.0f)

        // Top right arc
        arc.X <- rect.Right - diameter
        path.AddArc(arc, 270.0f, 90.0f)

        // Bottom right arc
        arc.Y <- rect.Bottom - diameter
        path.AddArc(arc, 0.0f, 90.0f)

        // Bottom left arc
        arc.X <- rect.Left
        path.AddArc(arc, 90.0f, 90.0f)

        path.CloseFigure()
        path

/// Modern card panel with rounded corners
type ModernCard() as this =
    inherit Panel()

    do
        this.BackColor <- Colors.CardBackground
        this.Padding <- Padding(24, 20, 24, 20)  // Left, Top, Right, Bottom padding
        this.DoubleBuffered <- true
        this.SetStyle(ControlStyles.UserPaint, true)
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true)
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true)
        this.SetStyle(ControlStyles.ResizeRedraw, true)

    override this.OnPaint(e: PaintEventArgs) =
        let g = e.Graphics
        g.SmoothingMode <- SmoothingMode.AntiAlias

        // Draw rounded rectangle background
        let rect = Rectangle(0, 0, this.Width - 1, this.Height - 1)
        use bgBrush = new SolidBrush(this.BackColor)
        use borderPen = new Pen(Colors.BorderColor, 1.0f)
        use path = this.GetRoundedRect(rect, 8)

        g.FillPath(bgBrush, path)
        g.DrawPath(borderPen, path)

        // Paint child controls manually respecting padding
        base.OnPaint(e)

    override this.DisplayRectangle =
        let rect = base.DisplayRectangle
        Rectangle(
            rect.X + this.Padding.Left,
            rect.Y + this.Padding.Top,
            rect.Width - this.Padding.Left - this.Padding.Right,
            rect.Height - this.Padding.Top - this.Padding.Bottom
        )

    member private this.GetRoundedRect(rect: Rectangle, radius: int) =
        let path = new GraphicsPath()
        let diameter = radius * 2
        let size = Size(diameter, diameter)
        let mutable arc = Rectangle(rect.Location, size)

        // Top left arc
        path.AddArc(arc, 180.0f, 90.0f)

        // Top right arc
        arc.X <- rect.Right - diameter
        path.AddArc(arc, 270.0f, 90.0f)

        // Bottom right arc
        arc.Y <- rect.Bottom - diameter
        path.AddArc(arc, 0.0f, 90.0f)

        // Bottom left arc
        arc.X <- rect.Left
        path.AddArc(arc, 90.0f, 90.0f)

        path.CloseFigure()
        path

/// Modern button with hover effects and custom painting
type ModernButton() as this =
    inherit Button()

    let mutable isHovered = false

    do
        this.FlatStyle <- FlatStyle.Flat
        this.BackColor <- Colors.AccentBlue
        this.ForeColor <- Color.Black  // Black text
        this.Font <- new Font("Segoe UI", 10.0f, FontStyle.Bold)
        this.Cursor <- Cursors.Hand
        this.FlatAppearance.BorderSize <- 0
        this.Padding <- Padding(20, 8, 20, 8)
        this.AutoSize <- false
        this.Height <- 36
        this.TextAlign <- ContentAlignment.MiddleCenter
        this.SetStyle(ControlStyles.UserPaint, true)
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true)
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true)

    override this.OnPaint(e: PaintEventArgs) =
        let g = e.Graphics
        g.SmoothingMode <- SmoothingMode.AntiAlias
        g.TextRenderingHint <- Drawing.Text.TextRenderingHint.ClearTypeGridFit

        // Background
        let bgColor = if isHovered then Colors.AccentBlueDark else Colors.AccentBlue
        use bgBrush = new SolidBrush(bgColor)
        g.FillRectangle(bgBrush, this.ClientRectangle)

        // Text - force black color
        use textBrush = new SolidBrush(Color.Black)
        let format = new StringFormat(Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center)
        g.DrawString(this.Text, this.Font, textBrush, RectangleF(0.0f, 0.0f, float32 this.Width, float32 this.Height), format)

    override this.OnMouseEnter(e: EventArgs) =
        isHovered <- true
        this.Invalidate()
        base.OnMouseEnter(e)

    override this.OnMouseLeave(e: EventArgs) =
        isHovered <- false
        this.Invalidate()
        base.OnMouseLeave(e)

/// Modern text box with border
type ModernTextBox() as this =
    inherit TextBox()

    do
        this.BorderStyle <- BorderStyle.FixedSingle
        this.BackColor <- Colors.CardBackground
        this.ForeColor <- Colors.PrimaryText
        this.Font <- new Font("Segoe UI", 10.0f)
        this.Padding <- Padding(8)
        this.Height <- 32

/// Modern label
type ModernLabel(?text: string) as this =
    inherit Label()

    do
        this.AutoSize <- true
        this.ForeColor <- Colors.PrimaryText
        this.Font <- new Font("Segoe UI", 10.0f)
        this.BackColor <- Color.Transparent
        match text with
        | Some t -> this.Text <- t
        | None -> ()

/// Secondary label (for descriptions)
type ModernLabelSecondary(?text: string) as this =
    inherit Label()

    do
        this.AutoSize <- true
        this.ForeColor <- Colors.SecondaryText
        this.Font <- new Font("Segoe UI", 9.0f)
        this.BackColor <- Color.Transparent
        match text with
        | Some t -> this.Text <- t
        | None -> ()

/// Sidebar navigation item
type SidebarItem(text: string, icon: string) as this =
    inherit Panel()

    let mutable isSelected = false
    let mutable isHovered = false

    do
        this.Height <- 40
        this.Cursor <- Cursors.Hand
        this.Padding <- Padding(15, 0, 15, 0)
        this.DoubleBuffered <- true
        this.SetStyle(ControlStyles.UserPaint, true)

        let label = new Label(
            Text = text,
            ForeColor = Colors.PrimaryText,
            Font = new Font("Segoe UI", 10.0f),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        )

        // Forward click events from the label to the panel
        label.Click.Add(fun e -> this.RaiseClickEvent())

        this.Controls.Add(label)

    member private this.RaiseClickEvent() =
        this.OnClick(EventArgs.Empty)

    member this.Selected
        with get() = isSelected
        and set(value) =
            isSelected <- value
            this.Invalidate()

    override this.OnPaint(e: PaintEventArgs) =
        let g = e.Graphics

        // Background
        let bgColor =
            if isSelected then Colors.HoverBackground
            elif isHovered then Color.FromArgb(40, 40, 40)
            else Color.Transparent

        use bgBrush = new SolidBrush(bgColor)
        g.FillRectangle(bgBrush, this.ClientRectangle)

        // Left accent bar if selected
        if isSelected then
            use accentBrush = new SolidBrush(Colors.AccentBlue)
            g.FillRectangle(accentBrush, Rectangle(0, 0, 3, this.Height))

        base.OnPaint(e)

    override this.OnMouseEnter(e: EventArgs) =
        isHovered <- true
        this.Invalidate()
        base.OnMouseEnter(e)

    override this.OnMouseLeave(e: EventArgs) =
        isHovered <- false
        this.Invalidate()
        base.OnMouseLeave(e)

/// Helper functions for styling
module Helpers =
    /// Apply dark theme to a form
    let applyDarkTheme (form: Form) =
        form.BackColor <- Colors.DarkBackground
        form.ForeColor <- Colors.PrimaryText

    /// Enable Mica effect on a form (Windows 11+)
    let enableMicaEffect (form: Form) =
        try
            if Environment.OSVersion.Version.Major >= 10 then
                let osVersion = Environment.OSVersion.Version

                // Enable dark mode for title bar (Windows 10 1809+)
                // DWMWA_USE_IMMERSIVE_DARK_MODE = 20
                let mutable useDarkMode = 1
                DwmApi.DwmSetWindowAttribute(form.Handle, 20, &useDarkMode, sizeof<int>) |> ignore

                // Windows 11 build 22000 or higher supports Mica
                if osVersion.Build >= 22000 then
                    // Try to enable Mica Alt effect (Windows 11 22H2+ - Build 22621+)
                    // DWMWA_SYSTEMBACKDROP_TYPE = 38
                    // DWMSBT_MAINWINDOW = 2 (Mica)
                    // DWMSBT_TABBEDWINDOW = 4 (Mica Alt - responds to desktop wallpaper)

                    // Try Mica Alt first (build 22621+)
                    if osVersion.Build >= 22621 then
                        let mutable backdropType = 4  // Mica Alt
                        let result = DwmApi.DwmSetWindowAttribute(form.Handle, 38, &backdropType, sizeof<int>)

                        if result <> 0 then
                            // Fallback to regular Mica
                            backdropType <- 2
                            DwmApi.DwmSetWindowAttribute(form.Handle, 38, &backdropType, sizeof<int>) |> ignore
                    else
                        // Use regular Mica for older Windows 11
                        let mutable backdropType = 2
                        DwmApi.DwmSetWindowAttribute(form.Handle, 38, &backdropType, sizeof<int>) |> ignore

                    // Enable rounded corners (Windows 11)
                    // DWMWA_WINDOW_CORNER_PREFERENCE = 33
                    // DWMWCP_ROUND = 2
                    let mutable cornerPref = 2
                    DwmApi.DwmSetWindowAttribute(form.Handle, 33, &cornerPref, sizeof<int>) |> ignore

                    printfn "✓ Mica effect enabled on Windows 11 build %d" osVersion.Build
                else
                    printfn "✗ Mica not supported on Windows build %d (need 22000+)" osVersion.Build
        with
        | ex ->
            // Log but don't crash if not supported
            printfn "✗ Mica effect error: %s" ex.Message

    /// Create a section header
    let createSectionHeader (text: string) =
        new Label(
            Text = text,
            Font = new Font("Segoe UI Semibold", 14.0f, FontStyle.Bold),
            ForeColor = Colors.PrimaryText,
            AutoSize = true,
            BackColor = Color.Transparent
        )

    /// Create a setting row with label and control
    let createSettingRow (labelText: string) (control: Control) (description: string option) =
        let panel = new Panel(
            Height = (if description.IsSome then 60 else 40),
            Dock = DockStyle.Top,
            BackColor = Color.Transparent
        )

        let label = new ModernLabel(labelText)
        label.Location <- Point(0, 10)
        panel.Controls.Add(label)

        control.Location <- Point(panel.Width - control.Width - 20, 8)
        control.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
        panel.Controls.Add(control)

        match description with
        | Some desc ->
            let descLabel = new ModernLabelSecondary(desc)
            descLabel.Location <- Point(0, 32)
            descLabel.MaximumSize <- Size(panel.Width - control.Width - 40, 0)
            panel.Controls.Add(descLabel)
        | None -> ()

        panel
