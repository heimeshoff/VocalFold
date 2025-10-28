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
    let mutable currentPage = "General"

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

    // Add Keywords item
    let keywordsItem = new SidebarItem("Keywords", "")
    keywordsItem.Dock <- DockStyle.Top
    keywordsItem.Selected <- false
    sidebar.Controls.Add(keywordsItem)

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

    // === Keywords Panel (initially hidden) ===
    let keywordsPanel = new Panel(
        Dock = DockStyle.Fill,
        BackColor = Colors.DarkBackground,
        AutoScroll = true,
        Padding = Padding(30, 20, 30, 20),
        Visible = false
    )

    // Keywords page title
    let keywordsTitle = Helpers.createSectionHeader "Keyword Replacements"
    keywordsTitle.Location <- Point(0, 0)
    keywordsPanel.Controls.Add(keywordsTitle)

    // Description card
    let descCard = new ModernCard()
    descCard.Location <- Point(0, 50)
    descCard.Size <- Size(600, 90)
    descCard.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let descLabel = new ModernLabelSecondary(
        "Configure keywords that will be automatically replaced in transcriptions.\n" +
        "Example: Say \"comma\" and it will be replaced with \",\""
    )
    descLabel.Location <- Point(0, 0)
    descLabel.MaximumSize <- Size(580, 0)
    descCard.Controls.Add(descLabel)
    keywordsPanel.Controls.Add(descCard)

    // Keywords list card
    let keywordsListCard = new ModernCard()
    keywordsListCard.Location <- Point(0, 160)
    keywordsListCard.Size <- Size(600, 400)
    keywordsListCard.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right ||| AnchorStyles.Bottom

    let keywordsListTitle = new ModernLabel("Configured Keywords")
    keywordsListTitle.Font <- new Font("Segoe UI", 12.0f, FontStyle.Bold)
    keywordsListTitle.Location <- Point(0, 0)
    keywordsListCard.Controls.Add(keywordsListTitle)

    // DataGridView for keywords list
    let keywordsGrid = new DataGridView(
        Location = Point(0, 35),
        Size = Size(580, 280),
        Anchor = AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right ||| AnchorStyles.Bottom,
        BackgroundColor = Colors.DarkBackground,
        ForeColor = Colors.PrimaryText,
        GridColor = Colors.BorderColor,
        BorderStyle = BorderStyle.None,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
        ColumnHeadersHeight = 32,
        RowTemplate = new DataGridViewRow(Height = 28),
        EnableHeadersVisualStyles = false,
        Font = new Font("Segoe UI", 9.5f)
    )

    // Configure DataGridView style
    keywordsGrid.DefaultCellStyle.BackColor <- Colors.CardBackground
    keywordsGrid.DefaultCellStyle.ForeColor <- Colors.PrimaryText
    keywordsGrid.DefaultCellStyle.SelectionBackColor <- Colors.AccentBlue
    keywordsGrid.DefaultCellStyle.SelectionForeColor <- Color.White
    keywordsGrid.DefaultCellStyle.Padding <- Padding(5, 2, 5, 2)
    keywordsGrid.ColumnHeadersDefaultCellStyle.BackColor <- Colors.DarkBackground
    keywordsGrid.ColumnHeadersDefaultCellStyle.ForeColor <- Colors.SecondaryText
    keywordsGrid.ColumnHeadersDefaultCellStyle.Font <- new Font("Segoe UI", 9.5f, FontStyle.Bold)
    keywordsGrid.ColumnHeadersDefaultCellStyle.Padding <- Padding(5, 4, 5, 4)

    // Add columns
    let keywordCol = new DataGridViewTextBoxColumn(
        HeaderText = "Keyword",
        Name = "Keyword",
        ReadOnly = true,
        FillWeight = 25.0f
    )
    keywordsGrid.Columns.Add(keywordCol) |> ignore

    let replacementCol = new DataGridViewTextBoxColumn(
        HeaderText = "Replacement",
        Name = "Replacement",
        ReadOnly = true,
        FillWeight = 40.0f
    )
    keywordsGrid.Columns.Add(replacementCol) |> ignore

    let caseSensitiveCol = new DataGridViewCheckBoxColumn(
        HeaderText = "Case Sensitive",
        Name = "CaseSensitive",
        ReadOnly = true,
        FillWeight = 17.0f
    )
    keywordsGrid.Columns.Add(caseSensitiveCol) |> ignore

    let wholePhraseCol = new DataGridViewCheckBoxColumn(
        HeaderText = "Whole Phrase",
        Name = "WholePhrase",
        ReadOnly = true,
        FillWeight = 18.0f
    )
    keywordsGrid.Columns.Add(wholePhraseCol) |> ignore

    // Function to refresh keywords grid
    let refreshKeywordsGrid () =
        keywordsGrid.Rows.Clear()
        for kw in newSettings.KeywordReplacements do
            let displayReplacement =
                if kw.Replacement.Length > 50 then
                    kw.Replacement.Substring(0, 47) + "..."
                else
                    kw.Replacement.Replace("\n", "\\n").Replace("\r", "")
            keywordsGrid.Rows.Add(kw.Keyword, displayReplacement, kw.CaseSensitive, kw.WholePhrase) |> ignore

    // Initial load
    refreshKeywordsGrid()

    keywordsListCard.Controls.Add(keywordsGrid)

    // Buttons for keyword management
    let addKeywordBtn = new ModernButton(
        Text = "Add",
        Location = Point(0, 325),
        Width = 90,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left
    )
    keywordsListCard.Controls.Add(addKeywordBtn)

    let editKeywordBtn = new ModernButton(
        Text = "Edit",
        Location = Point(100, 325),
        Width = 90,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left
    )
    keywordsListCard.Controls.Add(editKeywordBtn)

    let deleteKeywordBtn = new ModernButton(
        Text = "Delete",
        Location = Point(200, 325),
        Width = 90,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left
    )
    keywordsListCard.Controls.Add(deleteKeywordBtn)

    let importKeywordBtn = new ModernButton(
        Text = "Import",
        Location = Point(300, 325),
        Width = 90,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left
    )
    keywordsListCard.Controls.Add(importKeywordBtn)

    let exportKeywordBtn = new ModernButton(
        Text = "Export",
        Location = Point(400, 325),
        Width = 90,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left
    )
    keywordsListCard.Controls.Add(exportKeywordBtn)

    let addExamplesBtn = new Button(
        Text = "Add Examples",
        Location = Point(500, 325),
        Width = 80,
        Height = 36,
        Anchor = AnchorStyles.Bottom ||| AnchorStyles.Left,
        BackColor = Color.Transparent,
        ForeColor = Colors.PrimaryText,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9.0f, FontStyle.Regular),
        UseCompatibleTextRendering = false,
        TextAlign = ContentAlignment.MiddleCenter
    )
    addExamplesBtn.FlatAppearance.BorderColor <- Colors.BorderColor
    addExamplesBtn.FlatAppearance.BorderSize <- 1
    keywordsListCard.Controls.Add(addExamplesBtn)

    keywordsPanel.Controls.Add(keywordsListCard)

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
    form.Controls.Add(keywordsPanel)
    form.Controls.Add(sidebar)
    form.Controls.Add(buttonBar)

    // Enable Mica effect after form handle is created
    form.Load.Add(fun _ ->
        Helpers.enableMicaEffect form
    )

    // === Event Handlers ===

    // Helper function to switch pages
    let switchToPage (pageName: string) =
        currentPage <- pageName
        generalItem.Selected <- (pageName = "General")
        keywordsItem.Selected <- (pageName = "Keywords")
        mainPanel.Visible <- (pageName = "General")
        keywordsPanel.Visible <- (pageName = "Keywords")

    // Navigation event handlers
    generalItem.Click.Add(fun _ -> switchToPage "General")
    keywordsItem.Click.Add(fun _ -> switchToPage "Keywords")

    // Helper function to show keyword edit dialog
    let showKeywordDialog (title: string) (keyword: Settings.KeywordReplacement option) : Settings.KeywordReplacement option =
        use dlg = new Form(
            Text = title,
            Width = 500,
            Height = 380,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Colors.DarkBackground,
            ForeColor = Colors.PrimaryText,
            Font = new Font("Segoe UI", 10.0f)
        )

        let mutable result: Settings.KeywordReplacement option = None

        // Keyword input
        let keywordLabel = new ModernLabel("Keyword (what you say):")
        keywordLabel.Location <- Point(20, 20)
        dlg.Controls.Add(keywordLabel)

        let keywordTextBox = new TextBox(
            Location = Point(20, 48),
            Size = Size(440, 25),
            BackColor = Colors.CardBackground,
            ForeColor = Colors.PrimaryText,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10.0f)
        )
        dlg.Controls.Add(keywordTextBox)

        // Replacement input
        let replacementLabel = new ModernLabel("Replacement (what to type):")
        replacementLabel.Location <- Point(20, 85)
        dlg.Controls.Add(replacementLabel)

        let replacementTextBox = new TextBox(
            Location = Point(20, 113),
            Size = Size(440, 80),
            BackColor = Colors.CardBackground,
            ForeColor = Colors.PrimaryText,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10.0f),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            AcceptsReturn = true
        )
        dlg.Controls.Add(replacementTextBox)

        // Case sensitive checkbox
        let caseSensitiveCheck = new CheckBox(
            Text = "Case Sensitive",
            Location = Point(20, 205),
            AutoSize = true,
            ForeColor = Colors.PrimaryText,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 10.0f)
        )
        dlg.Controls.Add(caseSensitiveCheck)

        // Whole phrase checkbox
        let wholePhraseCheck = new CheckBox(
            Text = "Match Whole Phrase Only",
            Location = Point(20, 235),
            AutoSize = true,
            ForeColor = Colors.PrimaryText,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 10.0f),
            Checked = true
        )
        dlg.Controls.Add(wholePhraseCheck)

        // Load existing keyword data if editing
        match keyword with
        | Some kw ->
            keywordTextBox.Text <- kw.Keyword
            replacementTextBox.Text <- kw.Replacement
            caseSensitiveCheck.Checked <- kw.CaseSensitive
            wholePhraseCheck.Checked <- kw.WholePhrase
        | None -> ()

        // OK button
        let okBtn = new ModernButton(
            Text = "OK",
            Location = Point(360, 280),
            Width = 100
        )
        okBtn.Click.Add(fun _ ->
            if String.IsNullOrWhiteSpace(keywordTextBox.Text) then
                MessageBox.Show("Keyword cannot be empty", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            else
                result <- Some {
                    Keyword = keywordTextBox.Text.Trim()
                    Replacement = replacementTextBox.Text
                    CaseSensitive = caseSensitiveCheck.Checked
                    WholePhrase = wholePhraseCheck.Checked
                }
                dlg.DialogResult <- DialogResult.OK
        )
        dlg.Controls.Add(okBtn)

        // Cancel button
        let cancelBtn = new Button(
            Text = "Cancel",
            Location = Point(250, 280),
            Width = 100,
            Height = 36,
            BackColor = Color.Transparent,
            ForeColor = Colors.PrimaryText,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10.0f)
        )
        cancelBtn.FlatAppearance.BorderColor <- Colors.BorderColor
        cancelBtn.FlatAppearance.BorderSize <- 1
        cancelBtn.Click.Add(fun _ -> dlg.DialogResult <- DialogResult.Cancel)
        dlg.Controls.Add(cancelBtn)

        dlg.AcceptButton <- okBtn
        dlg.CancelButton <- cancelBtn

        if dlg.ShowDialog() = DialogResult.OK then
            result
        else
            None

    // Add keyword button
    addKeywordBtn.Click.Add(fun _ ->
        match showKeywordDialog "Add Keyword" None with
        | Some kw ->
            newSettings <- { newSettings with KeywordReplacements = newSettings.KeywordReplacements @ [kw] }
            refreshKeywordsGrid()
        | None -> ()
    )

    // Edit keyword button
    editKeywordBtn.Click.Add(fun _ ->
        if keywordsGrid.SelectedRows.Count > 0 then
            let index = keywordsGrid.SelectedRows.[0].Index
            let currentKw = newSettings.KeywordReplacements.[index]
            match showKeywordDialog "Edit Keyword" (Some currentKw) with
            | Some kw ->
                let updatedList =
                    newSettings.KeywordReplacements
                    |> List.mapi (fun i k -> if i = index then kw else k)
                newSettings <- { newSettings with KeywordReplacements = updatedList }
                refreshKeywordsGrid()
            | None -> ()
        else
            MessageBox.Show("Please select a keyword to edit", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    )

    // Delete keyword button
    deleteKeywordBtn.Click.Add(fun _ ->
        if keywordsGrid.SelectedRows.Count > 0 then
            let index = keywordsGrid.SelectedRows.[0].Index
            let kw = newSettings.KeywordReplacements.[index]
            let confirmResult = MessageBox.Show(
                sprintf "Are you sure you want to delete the keyword \"%s\"?" kw.Keyword,
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )
            if confirmResult = DialogResult.Yes then
                let updatedList =
                    newSettings.KeywordReplacements
                    |> List.mapi (fun i k -> (i, k))
                    |> List.filter (fun (i, _) -> i <> index)
                    |> List.map snd
                newSettings <- { newSettings with KeywordReplacements = updatedList }
                refreshKeywordsGrid()
        else
            MessageBox.Show("Please select a keyword to delete", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    )

    // Import keywords button
    importKeywordBtn.Click.Add(fun _ ->
        use openDialog = new OpenFileDialog(
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Keywords"
        )
        if openDialog.ShowDialog() = DialogResult.OK then
            try
                let json = System.IO.File.ReadAllText(openDialog.FileName)
                let imported = System.Text.Json.JsonSerializer.Deserialize<Settings.KeywordReplacement list>(json)
                newSettings <- { newSettings with KeywordReplacements = newSettings.KeywordReplacements @ imported }
                refreshKeywordsGrid()
                MessageBox.Show(sprintf "Imported %d keywords" imported.Length, "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
            with
            | ex ->
                MessageBox.Show(sprintf "Error importing keywords: %s" ex.Message, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
    )

    // Export keywords button
    exportKeywordBtn.Click.Add(fun _ ->
        if newSettings.KeywordReplacements.IsEmpty then
            MessageBox.Show("No keywords to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
        else
            use saveDialog = new SaveFileDialog(
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Keywords",
                FileName = "keywords.json"
            )
            if saveDialog.ShowDialog() = DialogResult.OK then
                try
                    let jsonOptions = System.Text.Json.JsonSerializerOptions()
                    jsonOptions.WriteIndented <- true
                    let json = System.Text.Json.JsonSerializer.Serialize(newSettings.KeywordReplacements, jsonOptions)
                    System.IO.File.WriteAllText(saveDialog.FileName, json)
                    MessageBox.Show(sprintf "Exported %d keywords" newSettings.KeywordReplacements.Length, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                with
                | ex ->
                    MessageBox.Show(sprintf "Error exporting keywords: %s" ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
    )

    // Add examples button
    addExamplesBtn.Click.Add(fun _ ->
        let examples = TextProcessor.getExampleReplacements()
        let confirmResult = MessageBox.Show(
            sprintf "This will add %d example keywords. Continue?" examples.Length,
            "Add Examples",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )
        if confirmResult = DialogResult.Yes then
            newSettings <- { newSettings with KeywordReplacements = newSettings.KeywordReplacements @ examples }
            refreshKeywordsGrid()
            MessageBox.Show(sprintf "Added %d example keywords" examples.Length, "Examples Added", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    )

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
