module SettingsDialog

open System
open System.Windows.Forms
open System.Drawing
open ModernTheme

/// Result of the settings dialog
type DialogResult =
    | Accepted of Settings.AppSettings
    | Cancelled

/// Create and show the settings dialog
let show (currentSettings: Settings.AppSettings) : DialogResult =
    // Create the form with modern dark theme
    let form = new Form(
        Text = "VocalFold Settings",
        Width = 950,
        Height = 750,
        MinimumSize = Size(900, 700),
        StartPosition = FormStartPosition.CenterScreen,
        FormBorderStyle = FormBorderStyle.Sizable,
        MaximizeBox = true,
        MinimizeBox = true,
        BackColor = Colors.DarkBackground,
        ForeColor = Colors.PrimaryText,
        Font = new Font("Segoe UI", 10.0f)
    )

    // Track the new settings
    let mutable newSettings = currentSettings
    let mutable isRecording = false
    let mutable recordedModifiers = 0u
    let mutable recordedKey = 0u

    // === Sidebar Navigation ===
    let sidebar = new Panel(
        Width = 200,
        Dock = DockStyle.Left,
        BackColor = Colors.SidebarBackground
    )

    // Add General item first (will be pushed down by title)
    let generalItem = new SidebarItem("General", "")
    generalItem.Dock <- DockStyle.Top
    generalItem.Selected <- true
    sidebar.Controls.Add(generalItem)

    // Add title at the top (will push other items down)
    let sidebarTitle = new Label(
        Text = "VocalFold",
        Font = new Font("Segoe UI Semibold", 14.0f, FontStyle.Bold),
        ForeColor = Colors.PrimaryText,
        AutoSize = false,
        Height = 60,
        Dock = DockStyle.Top,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = Padding(20, 20, 0, 0),
        BackColor = Color.Transparent
    )
    sidebar.Controls.Add(sidebarTitle)

    // === Main Content Area ===
    let mainPanel = new Panel(
        Dock = DockStyle.Fill,
        BackColor = Colors.DarkBackground,
        AutoScroll = true,
        Padding = Padding(30, 20, 30, 20)
    )

    // Page title
    let pageTitle = Helpers.createSectionHeader "General Settings"
    pageTitle.Location <- Point(0, 0)
    mainPanel.Controls.Add(pageTitle)

    // === Hotkey Card ===
    let hotkeyCard = new ModernCard()
    hotkeyCard.Location <- Point(0, 50)
    hotkeyCard.Size <- Size(600, 180)
    hotkeyCard.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let hotkeyCardTitle = new ModernLabel("Global Hotkey")
    hotkeyCardTitle.Font <- new Font("Segoe UI", 12.0f, FontStyle.Bold)
    hotkeyCardTitle.Location <- Point(0, 0)
    hotkeyCard.Controls.Add(hotkeyCardTitle)

    let hotkeyLabel = new ModernLabelSecondary("Current hotkey:")
    hotkeyLabel.Location <- Point(0, 35)
    hotkeyCard.Controls.Add(hotkeyLabel)

    let hotkeyDisplay = new Label(
        Text = Settings.getHotkeyDisplayName currentSettings,
        Location = Point(0, 58),
        Font = new Font("Consolas", 13.0f, FontStyle.Bold),
        AutoSize = true,
        ForeColor = Colors.AccentBlue,
        BackColor = Color.Transparent
    )
    hotkeyCard.Controls.Add(hotkeyDisplay)

    let recordButton = new ModernButton(
        Text = "Record New Hotkey",
        Location = Point(0, 100),
        Width = 180
    )
    hotkeyCard.Controls.Add(recordButton)

    let instructionLabel = new ModernLabelSecondary(
        "Press the record button, then press your desired hotkey combination."
    )
    instructionLabel.Location <- Point(190, 106)
    instructionLabel.MaximumSize <- Size(360, 0)
    hotkeyCard.Controls.Add(instructionLabel)

    mainPanel.Controls.Add(hotkeyCard)

    // === Model Size Card ===
    let modelCard = new ModernCard()
    modelCard.Location <- Point(0, 250)
    modelCard.Size <- Size(600, 130)
    modelCard.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let modelCardTitle = new ModernLabel("Whisper Model")
    modelCardTitle.Font <- new Font("Segoe UI", 12.0f, FontStyle.Bold)
    modelCardTitle.Location <- Point(0, 0)
    modelCard.Controls.Add(modelCardTitle)

    let modelLabel = new ModernLabel("Model size:")
    modelLabel.Location <- Point(0, 42)
    modelCard.Controls.Add(modelLabel)

    let modelCombo = new ComboBox(
        Location = Point(100, 40),
        Size = Size(130, 30),
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor = Colors.CardBackground,
        ForeColor = Colors.PrimaryText,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10.0f)
    )

    // Add model options as objects
    modelCombo.Items.Add("Tiny") |> ignore
    modelCombo.Items.Add("Base") |> ignore
    modelCombo.Items.Add("Small") |> ignore
    modelCombo.Items.Add("Medium") |> ignore
    modelCombo.Items.Add("Large") |> ignore

    // Find and select the current model, default to Base if not found
    let modelOptions = [| "Tiny"; "Base"; "Small"; "Medium"; "Large" |]
    let modelIndex = Array.tryFindIndex (fun m -> m = currentSettings.ModelSize) modelOptions
    modelCombo.SelectedIndex <-
        match modelIndex with
        | Some idx -> idx
        | None -> 1  // Default to "Base"

    modelCard.Controls.Add(modelCombo)

    let modelNote = new ModernLabelSecondary(
        "Requires restart to take effect"
    )
    modelNote.Location <- Point(0, 78)
    modelCard.Controls.Add(modelNote)

    mainPanel.Controls.Add(modelCard)

    // === Typing Speed Card ===
    let typingCard = new ModernCard()
    typingCard.Location <- Point(0, 400)
    typingCard.Size <- Size(600, 190)
    typingCard.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let typingCardTitle = new ModernLabel("Typing Speed")
    typingCardTitle.Font <- new Font("Segoe UI", 12.0f, FontStyle.Bold)
    typingCardTitle.Location <- Point(0, 0)
    typingCard.Controls.Add(typingCardTitle)

    let typingLabel = new ModernLabelSecondary("Select the character typing speed:")
    typingLabel.Location <- Point(0, 35)
    typingCard.Controls.Add(typingLabel)

    let fastRadio = new RadioButton(
        Text = "Fast (5ms delay)",
        Location = Point(0, 65),
        AutoSize = true,
        Tag = "fast",
        ForeColor = Colors.PrimaryText,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", 10.0f)
    )
    typingCard.Controls.Add(fastRadio)

    let normalRadio = new RadioButton(
        Text = "Normal (10ms delay) - Recommended",
        Location = Point(0, 95),
        AutoSize = true,
        Tag = "normal",
        ForeColor = Colors.PrimaryText,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", 10.0f)
    )
    typingCard.Controls.Add(normalRadio)

    let slowRadio = new RadioButton(
        Text = "Slow (20ms delay)",
        Location = Point(0, 125),
        AutoSize = true,
        Tag = "slow",
        ForeColor = Colors.PrimaryText,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", 10.0f)
    )
    typingCard.Controls.Add(slowRadio)

    // Set current selection based on settings
    let currentTypingSpeed = Settings.getTypingSpeed currentSettings
    match currentTypingSpeed with
    | Settings.Fast -> fastRadio.Checked <- true
    | Settings.Normal -> normalRadio.Checked <- true
    | Settings.Slow -> slowRadio.Checked <- true
    | Settings.Custom _ -> normalRadio.Checked <- true  // Default to normal for custom

    mainPanel.Controls.Add(typingCard)

    // === Bottom Button Bar ===
    let buttonBar = new Panel(
        Height = 60,
        Dock = DockStyle.Bottom,
        BackColor = Colors.CardBackground,
        Padding = Padding(30, 15, 30, 15)
    )

    let okButton = new ModernButton(
        Text = "Apply",
        Width = 100,
        Dock = DockStyle.Right
    )
    okButton.DialogResult <- DialogResult.OK
    buttonBar.Controls.Add(okButton)

    let cancelButton = new Button(
        Text = "Cancel",
        Width = 100,
        Height = 36,
        Dock = DockStyle.Right,
        BackColor = Color.Transparent,
        ForeColor = Colors.PrimaryText,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10.0f, FontStyle.Regular),
        Margin = Padding(0, 0, 15, 0),
        UseCompatibleTextRendering = false,
        TextAlign = ContentAlignment.MiddleCenter
    )
    cancelButton.FlatAppearance.BorderColor <- Colors.BorderColor
    cancelButton.FlatAppearance.BorderSize <- 1
    cancelButton.DialogResult <- DialogResult.Cancel
    buttonBar.Controls.Add(cancelButton)

    form.AcceptButton <- okButton
    form.CancelButton <- cancelButton

    // Add all main panels to form
    form.Controls.Add(mainPanel)
    form.Controls.Add(sidebar)
    form.Controls.Add(buttonBar)

    // Enable Mica effect after form handle is created
    form.Load.Add(fun _ ->
        Helpers.enableMicaEffect form
    )

    // === Event Handlers ===

    // Record button click
    recordButton.Click.Add(fun _ ->
        if not isRecording then
            isRecording <- true
            recordedModifiers <- 0u
            recordedKey <- 0u
            recordButton.Text <- "Recording... (Press any key)"
            recordButton.BackColor <- Colors.ErrorColor
            hotkeyDisplay.Text <- "..."
            hotkeyDisplay.ForeColor <- Colors.WarningColor
            form.KeyPreview <- true
    )

    // Key down event for recording
    form.KeyDown.Add(fun e ->
        if isRecording then
            // Track modifiers
            let mutable modifiers = 0u
            if e.Control then modifiers <- modifiers ||| 0x0002u  // MOD_CONTROL
            if e.Shift then modifiers <- modifiers ||| 0x0004u    // MOD_SHIFT
            if e.Alt then modifiers <- modifiers ||| 0x0001u      // MOD_ALT

            // Get the key code (excluding modifier keys themselves)
            let keyCode = uint32 e.KeyCode

            // Ignore if only modifier keys are pressed
            if keyCode <> 0x10u && keyCode <> 0x11u && keyCode <> 0x12u then  // Not Shift, Ctrl, or Alt
                // Validate: must have at least one modifier
                if modifiers = 0u then
                    MessageBox.Show(
                        "Hotkey must include at least one modifier key (Ctrl, Shift, or Alt).",
                        "Invalid Hotkey",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    ) |> ignore
                else
                    recordedModifiers <- modifiers
                    recordedKey <- keyCode

                    // Update new settings
                    newSettings <-
                        { newSettings with
                            HotkeyModifiers = recordedModifiers
                            HotkeyKey = recordedKey
                        }

                    // Update display
                    hotkeyDisplay.Text <- Settings.getHotkeyDisplayName newSettings
                    hotkeyDisplay.ForeColor <- Colors.AccentBlue
                    recordButton.Text <- "Record New Hotkey"
                    recordButton.BackColor <- Colors.AccentBlue
                    isRecording <- false
                    form.KeyPreview <- false

                    e.Handled <- true
                    e.SuppressKeyPress <- true
    )

    // OK button click
    okButton.Click.Add(fun _ ->
        // Update model size (with null check)
        let selectedModel =
            if modelCombo.SelectedItem <> null then
                modelCombo.SelectedItem.ToString()
            else
                "Base"  // Default fallback

        // Determine selected typing speed
        let selectedTypingSpeed =
            if fastRadio.Checked then "fast"
            elif slowRadio.Checked then "slow"
            else "normal"

        newSettings <-
            { newSettings with
                ModelSize = selectedModel
                TypingSpeedStr = selectedTypingSpeed
            }
    )

    // Show dialog and return result
    let result = form.ShowDialog()

    if result = DialogResult.OK then
        Accepted newSettings
    else
        Cancelled
