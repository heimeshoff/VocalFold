module SettingsDialog

open System
open System.Windows.Forms
open System.Drawing

/// Result of the settings dialog
type DialogResult =
    | Accepted of Settings.AppSettings
    | Cancelled

/// Create and show the settings dialog
let show (currentSettings: Settings.AppSettings) : DialogResult =
    // Create the form
    let form = new Form(
        Text = "VocalFold Settings",
        Width = 500,
        Height = 300,
        StartPosition = FormStartPosition.CenterScreen,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox = false,
        MinimizeBox = false
    )

    // Track the new settings
    let mutable newSettings = currentSettings
    let mutable isRecording = false
    let mutable recordedModifiers = 0u
    let mutable recordedKey = 0u

    // === Hotkey Section ===
    let hotkeyGroupBox = new GroupBox(
        Text = "Global Hotkey",
        Location = Point(20, 20),
        Size = Size(440, 120)
    )

    let hotkeyLabel = new Label(
        Text = "Current hotkey:",
        Location = Point(15, 25),
        AutoSize = true
    )

    let hotkeyDisplay = new Label(
        Text = Settings.getHotkeyDisplayName currentSettings,
        Location = Point(15, 50),
        Font = new Font(FontFamily.GenericMonospace, 12.0f, FontStyle.Bold),
        AutoSize = true,
        ForeColor = Color.DarkBlue
    )

    let recordButton = new Button(
        Text = "Record New Hotkey",
        Location = Point(15, 80),
        Size = Size(150, 25)
    )

    let instructionLabel = new Label(
        Text = "Press the record button, then press your desired hotkey combination.",
        Location = Point(175, 80),
        Size = Size(250, 30),
        ForeColor = Color.Gray
    )

    hotkeyGroupBox.Controls.Add(hotkeyLabel)
    hotkeyGroupBox.Controls.Add(hotkeyDisplay)
    hotkeyGroupBox.Controls.Add(recordButton)
    hotkeyGroupBox.Controls.Add(instructionLabel)

    // === Model Size Section ===
    let modelGroupBox = new GroupBox(
        Text = "Whisper Model",
        Location = Point(20, 150),
        Size = Size(440, 60)
    )

    let modelLabel = new Label(
        Text = "Model size:",
        Location = Point(15, 25),
        AutoSize = true
    )

    let modelCombo = new ComboBox(
        Location = Point(100, 22),
        Size = Size(100, 25),
        DropDownStyle = ComboBoxStyle.DropDownList
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

    let modelNote = new Label(
        Text = "(Requires restart to take effect)",
        Location = Point(210, 25),
        AutoSize = true,
        ForeColor = Color.Gray,
        Font = new Font(FontFamily.GenericSansSerif, 8.0f)
    )

    modelGroupBox.Controls.Add(modelLabel)
    modelGroupBox.Controls.Add(modelCombo)
    modelGroupBox.Controls.Add(modelNote)

    // === Buttons ===
    let okButton = new Button(
        Text = "Apply",
        Location = Point(280, 220),
        Size = Size(80, 30),
        DialogResult = DialogResult.OK
    )

    let cancelButton = new Button(
        Text = "Cancel",
        Location = Point(370, 220),
        Size = Size(80, 30),
        DialogResult = DialogResult.Cancel
    )

    form.AcceptButton <- okButton
    form.CancelButton <- cancelButton

    // === Event Handlers ===

    // Record button click
    recordButton.Click.Add(fun _ ->
        if not isRecording then
            isRecording <- true
            recordedModifiers <- 0u
            recordedKey <- 0u
            recordButton.Text <- "Recording... (Press any key)"
            recordButton.BackColor <- Color.LightCoral
            hotkeyDisplay.Text <- "..."
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
                    recordButton.Text <- "Record New Hotkey"
                    recordButton.BackColor <- Control.DefaultBackColor
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

        newSettings <-
            { newSettings with
                ModelSize = selectedModel
            }
    )

    // Add controls to form
    form.Controls.Add(hotkeyGroupBox)
    form.Controls.Add(modelGroupBox)
    form.Controls.Add(okButton)
    form.Controls.Add(cancelButton)

    // Show dialog and return result
    let result = form.ShowDialog()

    if result = DialogResult.OK then
        Accepted newSettings
    else
        Cancelled
