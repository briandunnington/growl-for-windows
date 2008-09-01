namespace Growl.WindowsClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuSeperator1 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.currentAssemblyVersionLabel = new System.Windows.Forms.Label();
            this.autoCheckUpdatedCheckBox = new System.Windows.Forms.CheckBox();
            this.currentVersionInfoLabel = new System.Windows.Forms.Label();
            this.currentVersionLabel = new System.Windows.Forms.Label();
            this.idleCheckBox = new System.Windows.Forms.CheckBox();
            this.idleLabel = new System.Windows.Forms.Label();
            this.statusItemCheckBox = new System.Windows.Forms.CheckBox();
            this.statusItemLabel = new System.Windows.Forms.Label();
            this.loggingOptionsPanel = new System.Windows.Forms.Panel();
            this.logOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.customLoggingFileTextBox = new System.Windows.Forms.TextBox();
            this.consoleLogRadioButton = new System.Windows.Forms.RadioButton();
            this.customFileRadioButton = new System.Windows.Forms.RadioButton();
            this.enableLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.logginLabel = new System.Windows.Forms.Label();
            this.defaultDisplayStyleComboBox = new System.Windows.Forms.ComboBox();
            this.defaultDisplayStyleLabel = new System.Windows.Forms.Label();
            this.autoStartCheckbox = new System.Windows.Forms.CheckBox();
            this.runningStatusLabel = new System.Windows.Forms.Label();
            this.startStopButton = new System.Windows.Forms.Button();
            this.launchOptionsLabel = new System.Windows.Forms.Label();
            this.tabPageApplications = new System.Windows.Forms.TabPage();
            this.notificationStickyComboBox = new System.Windows.Forms.ComboBox();
            this.notificationPriorityComboBox = new System.Windows.Forms.ComboBox();
            this.notificationDisplayComboBox = new System.Windows.Forms.ComboBox();
            this.applicationDisplayComboBox = new System.Windows.Forms.ComboBox();
            this.notificationsListView = new Growl.WindowsClient.ListViewEx();
            this.notificationListEnableColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.notificationListNotificationNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.notificationListDisplayColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.notificationListPriorityColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.notificationListStickyColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.applicationListView = new Growl.WindowsClient.ListViewEx();
            this.appListEnableColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.appListAppNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.appListDisplayColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.appListClickColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.tabPageDisplayOptions = new System.Windows.Forms.TabPage();
            this.displayOptionsContainerPanel = new System.Windows.Forms.Panel();
            this.displayStyleDescriptionLabel = new System.Windows.Forms.Label();
            this.displayStyleVersionLabel = new System.Windows.Forms.Label();
            this.displayStyleAuthorLabel = new System.Windows.Forms.Label();
            this.displayStyleNameLabel = new System.Windows.Forms.Label();
            this.displaySettingsPanel = new System.Windows.Forms.Panel();
            this.displayOptionsListBox = new System.Windows.Forms.ListBox();
            this.tabPageNetwork = new System.Windows.Forms.TabPage();
            this.autoEnableWebNotificationsCheckBox = new System.Windows.Forms.CheckBox();
            this.listenForWebNotificationsCheckBox = new System.Windows.Forms.CheckBox();
            this.removeComputerButton = new System.Windows.Forms.Button();
            this.addForwardButton = new System.Windows.Forms.Button();
            this.forwardToComputersListView = new System.Windows.Forms.ListView();
            this.forwardComputerListUseColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.forwardComputerListComputerColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.forwardComputerListPasswordColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.forwardNotificationsCheckBox = new System.Windows.Forms.CheckBox();
            this.serverPasswordTextBox = new System.Windows.Forms.TextBox();
            this.serverPasswordLabel = new System.Windows.Forms.Label();
            this.allowRemoteRegistrationCheckBox = new System.Windows.Forms.CheckBox();
            this.listenForNotificationsCheckBox = new System.Windows.Forms.CheckBox();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.aboutTextBox = new System.Windows.Forms.TextBox();
            this.previewDisplayButton = new System.Windows.Forms.Button();
            this.contextMenuStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.loggingOptionsPanel.SuspendLayout();
            this.tabPageApplications.SuspendLayout();
            this.tabPageDisplayOptions.SuspendLayout();
            this.displayOptionsContainerPanel.SuspendLayout();
            this.tabPageNetwork.SuspendLayout();
            this.tabPageAbout.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Growl";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItemSettings,
            this.contextMenuSeperator1,
            this.contextMenuItemQuit});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.ShowImageMargin = false;
            this.contextMenuStrip.Size = new System.Drawing.Size(100, 54);
            // 
            // contextMenuItemSettings
            // 
            this.contextMenuItemSettings.Name = "contextMenuItemSettings";
            this.contextMenuItemSettings.Size = new System.Drawing.Size(99, 22);
            this.contextMenuItemSettings.Text = "Settings";
            this.contextMenuItemSettings.Click += new System.EventHandler(this.contextMenuItemSettings_Click);
            // 
            // contextMenuSeperator1
            // 
            this.contextMenuSeperator1.Name = "contextMenuSeperator1";
            this.contextMenuSeperator1.Size = new System.Drawing.Size(96, 6);
            // 
            // contextMenuItemQuit
            // 
            this.contextMenuItemQuit.Name = "contextMenuItemQuit";
            this.contextMenuItemQuit.Size = new System.Drawing.Size(99, 22);
            this.contextMenuItemQuit.Text = "Quit";
            this.contextMenuItemQuit.Click += new System.EventHandler(this.contextMenuItemQuit_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageGeneral);
            this.tabControl.Controls.Add(this.tabPageApplications);
            this.tabControl.Controls.Add(this.tabPageDisplayOptions);
            this.tabControl.Controls.Add(this.tabPageNetwork);
            this.tabControl.Controls.Add(this.tabPageAbout);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(484, 350);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 3;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.currentAssemblyVersionLabel);
            this.tabPageGeneral.Controls.Add(this.autoCheckUpdatedCheckBox);
            this.tabPageGeneral.Controls.Add(this.currentVersionInfoLabel);
            this.tabPageGeneral.Controls.Add(this.currentVersionLabel);
            this.tabPageGeneral.Controls.Add(this.idleCheckBox);
            this.tabPageGeneral.Controls.Add(this.idleLabel);
            this.tabPageGeneral.Controls.Add(this.statusItemCheckBox);
            this.tabPageGeneral.Controls.Add(this.statusItemLabel);
            this.tabPageGeneral.Controls.Add(this.loggingOptionsPanel);
            this.tabPageGeneral.Controls.Add(this.enableLoggingCheckBox);
            this.tabPageGeneral.Controls.Add(this.logginLabel);
            this.tabPageGeneral.Controls.Add(this.defaultDisplayStyleComboBox);
            this.tabPageGeneral.Controls.Add(this.defaultDisplayStyleLabel);
            this.tabPageGeneral.Controls.Add(this.autoStartCheckbox);
            this.tabPageGeneral.Controls.Add(this.runningStatusLabel);
            this.tabPageGeneral.Controls.Add(this.startStopButton);
            this.tabPageGeneral.Controls.Add(this.launchOptionsLabel);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(476, 324);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // currentAssemblyVersionLabel
            // 
            this.currentAssemblyVersionLabel.AutoSize = true;
            this.currentAssemblyVersionLabel.Location = new System.Drawing.Point(255, 283);
            this.currentAssemblyVersionLabel.Name = "currentAssemblyVersionLabel";
            this.currentAssemblyVersionLabel.Size = new System.Drawing.Size(40, 13);
            this.currentAssemblyVersionLabel.TabIndex = 18;
            this.currentAssemblyVersionLabel.Text = "0.0.0.0";
            // 
            // autoCheckUpdatedCheckBox
            // 
            this.autoCheckUpdatedCheckBox.AutoSize = true;
            this.autoCheckUpdatedCheckBox.Location = new System.Drawing.Point(206, 301);
            this.autoCheckUpdatedCheckBox.Name = "autoCheckUpdatedCheckBox";
            this.autoCheckUpdatedCheckBox.Size = new System.Drawing.Size(207, 17);
            this.autoCheckUpdatedCheckBox.TabIndex = 17;
            this.autoCheckUpdatedCheckBox.Text = "Automatically check for updates (daily)";
            this.autoCheckUpdatedCheckBox.UseVisualStyleBackColor = true;
            // 
            // currentVersionInfoLabel
            // 
            this.currentVersionInfoLabel.AutoSize = true;
            this.currentVersionInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentVersionInfoLabel.Location = new System.Drawing.Point(203, 279);
            this.currentVersionInfoLabel.Name = "currentVersionInfoLabel";
            this.currentVersionInfoLabel.Size = new System.Drawing.Size(31, 18);
            this.currentVersionInfoLabel.TabIndex = 16;
            this.currentVersionInfoLabel.Text = "0.0";
            // 
            // currentVersionLabel
            // 
            this.currentVersionLabel.AutoSize = true;
            this.currentVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentVersionLabel.Location = new System.Drawing.Point(69, 279);
            this.currentVersionLabel.Name = "currentVersionLabel";
            this.currentVersionLabel.Size = new System.Drawing.Size(131, 18);
            this.currentVersionLabel.TabIndex = 15;
            this.currentVersionLabel.Text = "Current Version:";
            this.currentVersionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // idleCheckBox
            // 
            this.idleCheckBox.AutoSize = true;
            this.idleCheckBox.Location = new System.Drawing.Point(206, 253);
            this.idleCheckBox.Name = "idleCheckBox";
            this.idleCheckBox.Size = new System.Drawing.Size(217, 17);
            this.idleCheckBox.TabIndex = 14;
            this.idleCheckBox.Text = "Sticky notifications after 30 s of inactivity";
            this.idleCheckBox.UseVisualStyleBackColor = true;
            // 
            // idleLabel
            // 
            this.idleLabel.AutoSize = true;
            this.idleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.idleLabel.Location = new System.Drawing.Point(161, 252);
            this.idleLabel.Name = "idleLabel";
            this.idleLabel.Size = new System.Drawing.Size(39, 18);
            this.idleLabel.TabIndex = 13;
            this.idleLabel.Text = "Idle:";
            this.idleLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // statusItemCheckBox
            // 
            this.statusItemCheckBox.AutoSize = true;
            this.statusItemCheckBox.Location = new System.Drawing.Point(206, 226);
            this.statusItemCheckBox.Name = "statusItemCheckBox";
            this.statusItemCheckBox.Size = new System.Drawing.Size(197, 17);
            this.statusItemCheckBox.TabIndex = 12;
            this.statusItemCheckBox.Text = "Display Growl status in the menu bar";
            this.statusItemCheckBox.UseVisualStyleBackColor = true;
            // 
            // statusItemLabel
            // 
            this.statusItemLabel.AutoSize = true;
            this.statusItemLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusItemLabel.Location = new System.Drawing.Point(102, 225);
            this.statusItemLabel.Name = "statusItemLabel";
            this.statusItemLabel.Size = new System.Drawing.Size(98, 18);
            this.statusItemLabel.TabIndex = 11;
            this.statusItemLabel.Text = "Status Item:";
            this.statusItemLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // loggingOptionsPanel
            // 
            this.loggingOptionsPanel.Controls.Add(this.logOnlyCheckBox);
            this.loggingOptionsPanel.Controls.Add(this.customLoggingFileTextBox);
            this.loggingOptionsPanel.Controls.Add(this.consoleLogRadioButton);
            this.loggingOptionsPanel.Controls.Add(this.customFileRadioButton);
            this.loggingOptionsPanel.Location = new System.Drawing.Point(206, 141);
            this.loggingOptionsPanel.Name = "loggingOptionsPanel";
            this.loggingOptionsPanel.Size = new System.Drawing.Size(264, 76);
            this.loggingOptionsPanel.TabIndex = 10;
            // 
            // logOnlyCheckBox
            // 
            this.logOnlyCheckBox.AutoSize = true;
            this.logOnlyCheckBox.Location = new System.Drawing.Point(4, 50);
            this.logOnlyCheckBox.Name = "logOnlyCheckBox";
            this.logOnlyCheckBox.Size = new System.Drawing.Size(130, 17);
            this.logOnlyCheckBox.TabIndex = 11;
            this.logOnlyCheckBox.Text = "Log only, don\'t display";
            this.logOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // customLoggingFileTextBox
            // 
            this.customLoggingFileTextBox.Location = new System.Drawing.Point(86, 26);
            this.customLoggingFileTextBox.Name = "customLoggingFileTextBox";
            this.customLoggingFileTextBox.Size = new System.Drawing.Size(175, 20);
            this.customLoggingFileTextBox.TabIndex = 10;
            // 
            // consoleLogRadioButton
            // 
            this.consoleLogRadioButton.AutoSize = true;
            this.consoleLogRadioButton.Location = new System.Drawing.Point(3, 3);
            this.consoleLogRadioButton.Name = "consoleLogRadioButton";
            this.consoleLogRadioButton.Size = new System.Drawing.Size(79, 17);
            this.consoleLogRadioButton.TabIndex = 8;
            this.consoleLogRadioButton.TabStop = true;
            this.consoleLogRadioButton.Text = "console.log";
            this.consoleLogRadioButton.UseVisualStyleBackColor = true;
            // 
            // customFileRadioButton
            // 
            this.customFileRadioButton.AutoSize = true;
            this.customFileRadioButton.Location = new System.Drawing.Point(3, 26);
            this.customFileRadioButton.Name = "customFileRadioButton";
            this.customFileRadioButton.Size = new System.Drawing.Size(79, 17);
            this.customFileRadioButton.TabIndex = 9;
            this.customFileRadioButton.TabStop = true;
            this.customFileRadioButton.Text = "Custom file:";
            this.customFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // enableLoggingCheckBox
            // 
            this.enableLoggingCheckBox.AutoSize = true;
            this.enableLoggingCheckBox.Location = new System.Drawing.Point(206, 119);
            this.enableLoggingCheckBox.Name = "enableLoggingCheckBox";
            this.enableLoggingCheckBox.Size = new System.Drawing.Size(100, 17);
            this.enableLoggingCheckBox.TabIndex = 7;
            this.enableLoggingCheckBox.Text = "Enable Logging";
            this.enableLoggingCheckBox.UseVisualStyleBackColor = true;
            // 
            // logginLabel
            // 
            this.logginLabel.AutoSize = true;
            this.logginLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logginLabel.Location = new System.Drawing.Point(128, 118);
            this.logginLabel.Name = "logginLabel";
            this.logginLabel.Size = new System.Drawing.Size(72, 18);
            this.logginLabel.TabIndex = 6;
            this.logginLabel.Text = "Logging:";
            this.logginLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defaultDisplayStyleComboBox
            // 
            this.defaultDisplayStyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultDisplayStyleComboBox.FormattingEnabled = true;
            this.defaultDisplayStyleComboBox.Location = new System.Drawing.Point(206, 80);
            this.defaultDisplayStyleComboBox.Name = "defaultDisplayStyleComboBox";
            this.defaultDisplayStyleComboBox.Size = new System.Drawing.Size(121, 21);
            this.defaultDisplayStyleComboBox.TabIndex = 5;
            this.defaultDisplayStyleComboBox.SelectionChangeCommitted += new System.EventHandler(this.defaultDisplayStyleComboBox_SelectionChangeCommitted);
            // 
            // defaultDisplayStyleLabel
            // 
            this.defaultDisplayStyleLabel.AutoSize = true;
            this.defaultDisplayStyleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defaultDisplayStyleLabel.Location = new System.Drawing.Point(90, 84);
            this.defaultDisplayStyleLabel.Name = "defaultDisplayStyleLabel";
            this.defaultDisplayStyleLabel.Size = new System.Drawing.Size(110, 18);
            this.defaultDisplayStyleLabel.TabIndex = 4;
            this.defaultDisplayStyleLabel.Text = "Display Style:";
            this.defaultDisplayStyleLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // autoStartCheckbox
            // 
            this.autoStartCheckbox.AutoSize = true;
            this.autoStartCheckbox.Location = new System.Drawing.Point(206, 52);
            this.autoStartCheckbox.Name = "autoStartCheckbox";
            this.autoStartCheckbox.Size = new System.Drawing.Size(89, 17);
            this.autoStartCheckbox.TabIndex = 3;
            this.autoStartCheckbox.Text = "Start at Login";
            this.autoStartCheckbox.UseVisualStyleBackColor = true;
            this.autoStartCheckbox.CheckedChanged += new System.EventHandler(this.autoStartCheckbox_CheckedChanged);
            // 
            // runningStatusLabel
            // 
            this.runningStatusLabel.AutoSize = true;
            this.runningStatusLabel.Location = new System.Drawing.Point(289, 28);
            this.runningStatusLabel.Name = "runningStatusLabel";
            this.runningStatusLabel.Size = new System.Drawing.Size(85, 13);
            this.runningStatusLabel.TabIndex = 2;
            this.runningStatusLabel.Text = "Growl is stopped";
            // 
            // startStopButton
            // 
            this.startStopButton.AutoSize = true;
            this.startStopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startStopButton.Location = new System.Drawing.Point(206, 21);
            this.startStopButton.Name = "startStopButton";
            this.startStopButton.Size = new System.Drawing.Size(77, 25);
            this.startStopButton.TabIndex = 1;
            this.startStopButton.Text = "Start Growl";
            this.startStopButton.UseVisualStyleBackColor = true;
            this.startStopButton.Click += new System.EventHandler(this.startStopButton_Click);
            // 
            // launchOptionsLabel
            // 
            this.launchOptionsLabel.AutoSize = true;
            this.launchOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.launchOptionsLabel.Location = new System.Drawing.Point(69, 26);
            this.launchOptionsLabel.Name = "launchOptionsLabel";
            this.launchOptionsLabel.Size = new System.Drawing.Size(131, 18);
            this.launchOptionsLabel.TabIndex = 0;
            this.launchOptionsLabel.Text = "Launch Options:";
            this.launchOptionsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tabPageApplications
            // 
            this.tabPageApplications.Controls.Add(this.notificationStickyComboBox);
            this.tabPageApplications.Controls.Add(this.notificationPriorityComboBox);
            this.tabPageApplications.Controls.Add(this.notificationDisplayComboBox);
            this.tabPageApplications.Controls.Add(this.applicationDisplayComboBox);
            this.tabPageApplications.Controls.Add(this.notificationsListView);
            this.tabPageApplications.Controls.Add(this.applicationListView);
            this.tabPageApplications.Location = new System.Drawing.Point(4, 22);
            this.tabPageApplications.Name = "tabPageApplications";
            this.tabPageApplications.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageApplications.Size = new System.Drawing.Size(476, 324);
            this.tabPageApplications.TabIndex = 1;
            this.tabPageApplications.Text = "Applications";
            this.tabPageApplications.UseVisualStyleBackColor = true;
            // 
            // notificationStickyComboBox
            // 
            this.notificationStickyComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.notificationStickyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationStickyComboBox.FormattingEnabled = true;
            this.notificationStickyComboBox.Location = new System.Drawing.Point(23, 256);
            this.notificationStickyComboBox.Name = "notificationStickyComboBox";
            this.notificationStickyComboBox.Size = new System.Drawing.Size(121, 21);
            this.notificationStickyComboBox.TabIndex = 5;
            this.notificationStickyComboBox.Visible = false;
            this.notificationStickyComboBox.Leave += new System.EventHandler(this.notificationStickyComboBox_Leave);
            this.notificationStickyComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.notificationStickyComboBox_KeyPress);
            this.notificationStickyComboBox.SelectedValueChanged += new System.EventHandler(this.notificationStickyComboBox_SelectedValueChanged);
            // 
            // notificationPriorityComboBox
            // 
            this.notificationPriorityComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.notificationPriorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationPriorityComboBox.FormattingEnabled = true;
            this.notificationPriorityComboBox.Location = new System.Drawing.Point(23, 228);
            this.notificationPriorityComboBox.Name = "notificationPriorityComboBox";
            this.notificationPriorityComboBox.Size = new System.Drawing.Size(121, 21);
            this.notificationPriorityComboBox.TabIndex = 4;
            this.notificationPriorityComboBox.Visible = false;
            this.notificationPriorityComboBox.Leave += new System.EventHandler(this.notificationPriorityComboBox_Leave);
            this.notificationPriorityComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.notificationPriorityComboBox_KeyPress);
            this.notificationPriorityComboBox.SelectedValueChanged += new System.EventHandler(this.notificationPriorityComboBox_SelectedValueChanged);
            // 
            // notificationDisplayComboBox
            // 
            this.notificationDisplayComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.notificationDisplayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notificationDisplayComboBox.FormattingEnabled = true;
            this.notificationDisplayComboBox.Location = new System.Drawing.Point(23, 200);
            this.notificationDisplayComboBox.Name = "notificationDisplayComboBox";
            this.notificationDisplayComboBox.Size = new System.Drawing.Size(121, 21);
            this.notificationDisplayComboBox.TabIndex = 3;
            this.notificationDisplayComboBox.Visible = false;
            this.notificationDisplayComboBox.Leave += new System.EventHandler(this.notificationDisplayComboBox_Leave);
            this.notificationDisplayComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.notificationDisplayComboBox_KeyPress);
            this.notificationDisplayComboBox.SelectedValueChanged += new System.EventHandler(this.notificationDisplayComboBox_SelectedValueChanged);
            // 
            // applicationDisplayComboBox
            // 
            this.applicationDisplayComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.applicationDisplayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.applicationDisplayComboBox.FormattingEnabled = true;
            this.applicationDisplayComboBox.Location = new System.Drawing.Point(23, 35);
            this.applicationDisplayComboBox.Name = "applicationDisplayComboBox";
            this.applicationDisplayComboBox.Size = new System.Drawing.Size(121, 21);
            this.applicationDisplayComboBox.TabIndex = 2;
            this.applicationDisplayComboBox.Visible = false;
            this.applicationDisplayComboBox.Leave += new System.EventHandler(this.applicationDisplayComboBox_Leave);
            this.applicationDisplayComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.applicationDisplayComboBox_KeyPress);
            this.applicationDisplayComboBox.SelectedValueChanged += new System.EventHandler(this.applicationDisplayComboBox_SelectedValueChanged);
            // 
            // notificationsListView
            // 
            this.notificationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.notificationsListView.CheckBoxes = true;
            this.notificationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.notificationListEnableColumnHeader,
            this.notificationListNotificationNameColumnHeader,
            this.notificationListDisplayColumnHeader,
            this.notificationListPriorityColumnHeader,
            this.notificationListStickyColumnHeader});
            this.notificationsListView.FullRowSelect = true;
            this.notificationsListView.Location = new System.Drawing.Point(7, 170);
            this.notificationsListView.Name = "notificationsListView";
            this.notificationsListView.Size = new System.Drawing.Size(463, 148);
            this.notificationsListView.TabIndex = 1;
            this.notificationsListView.UseCompatibleStateImageBehavior = false;
            this.notificationsListView.View = System.Windows.Forms.View.Details;
            this.notificationsListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.notificationsListView_ItemChecked);
            this.notificationsListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.notificationsListView_MouseUp);
            // 
            // notificationListEnableColumnHeader
            // 
            this.notificationListEnableColumnHeader.Text = "Enable";
            // 
            // notificationListNotificationNameColumnHeader
            // 
            this.notificationListNotificationNameColumnHeader.Text = "Notification";
            // 
            // notificationListDisplayColumnHeader
            // 
            this.notificationListDisplayColumnHeader.Text = "Display";
            // 
            // notificationListPriorityColumnHeader
            // 
            this.notificationListPriorityColumnHeader.Text = "Priority";
            // 
            // notificationListStickyColumnHeader
            // 
            this.notificationListStickyColumnHeader.Text = "Sticky";
            // 
            // applicationListView
            // 
            this.applicationListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.applicationListView.CheckBoxes = true;
            this.applicationListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.appListEnableColumnHeader,
            this.appListAppNameColumnHeader,
            this.appListDisplayColumnHeader,
            this.appListClickColumnHeader});
            this.applicationListView.FullRowSelect = true;
            this.applicationListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.applicationListView.Location = new System.Drawing.Point(7, 7);
            this.applicationListView.MultiSelect = false;
            this.applicationListView.Name = "applicationListView";
            this.applicationListView.Size = new System.Drawing.Size(463, 157);
            this.applicationListView.TabIndex = 0;
            this.applicationListView.UseCompatibleStateImageBehavior = false;
            this.applicationListView.View = System.Windows.Forms.View.Details;
            this.applicationListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.applicationListView_ItemChecked);
            this.applicationListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.applicationListView_MouseUp);
            this.applicationListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.applicationListView_ItemSelectionChanged);
            // 
            // appListEnableColumnHeader
            // 
            this.appListEnableColumnHeader.Text = "Enable";
            // 
            // appListAppNameColumnHeader
            // 
            this.appListAppNameColumnHeader.Text = "Application";
            this.appListAppNameColumnHeader.Width = 200;
            // 
            // appListDisplayColumnHeader
            // 
            this.appListDisplayColumnHeader.Text = "Display";
            // 
            // appListClickColumnHeader
            // 
            this.appListClickColumnHeader.Text = "Click";
            // 
            // tabPageDisplayOptions
            // 
            this.tabPageDisplayOptions.Controls.Add(this.displayOptionsContainerPanel);
            this.tabPageDisplayOptions.Controls.Add(this.displayOptionsListBox);
            this.tabPageDisplayOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageDisplayOptions.Name = "tabPageDisplayOptions";
            this.tabPageDisplayOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDisplayOptions.Size = new System.Drawing.Size(476, 324);
            this.tabPageDisplayOptions.TabIndex = 2;
            this.tabPageDisplayOptions.Text = "Display Options";
            this.tabPageDisplayOptions.UseVisualStyleBackColor = true;
            // 
            // displayOptionsContainerPanel
            // 
            this.displayOptionsContainerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.displayOptionsContainerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.displayOptionsContainerPanel.Controls.Add(this.previewDisplayButton);
            this.displayOptionsContainerPanel.Controls.Add(this.displayStyleDescriptionLabel);
            this.displayOptionsContainerPanel.Controls.Add(this.displayStyleVersionLabel);
            this.displayOptionsContainerPanel.Controls.Add(this.displayStyleAuthorLabel);
            this.displayOptionsContainerPanel.Controls.Add(this.displayStyleNameLabel);
            this.displayOptionsContainerPanel.Controls.Add(this.displaySettingsPanel);
            this.displayOptionsContainerPanel.Location = new System.Drawing.Point(134, 7);
            this.displayOptionsContainerPanel.Name = "displayOptionsContainerPanel";
            this.displayOptionsContainerPanel.Size = new System.Drawing.Size(336, 303);
            this.displayOptionsContainerPanel.TabIndex = 2;
            // 
            // displayStyleDescriptionLabel
            // 
            this.displayStyleDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.displayStyleDescriptionLabel.Location = new System.Drawing.Point(3, 21);
            this.displayStyleDescriptionLabel.Name = "displayStyleDescriptionLabel";
            this.displayStyleDescriptionLabel.Size = new System.Drawing.Size(328, 32);
            this.displayStyleDescriptionLabel.TabIndex = 6;
            this.displayStyleDescriptionLabel.Text = "[description]";
            // 
            // displayStyleVersionLabel
            // 
            this.displayStyleVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.displayStyleVersionLabel.AutoSize = true;
            this.displayStyleVersionLabel.Location = new System.Drawing.Point(3, 281);
            this.displayStyleVersionLabel.Name = "displayStyleVersionLabel";
            this.displayStyleVersionLabel.Size = new System.Drawing.Size(47, 13);
            this.displayStyleVersionLabel.TabIndex = 5;
            this.displayStyleVersionLabel.Text = "[version]";
            this.displayStyleVersionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // displayStyleAuthorLabel
            // 
            this.displayStyleAuthorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.displayStyleAuthorLabel.AutoSize = true;
            this.displayStyleAuthorLabel.Location = new System.Drawing.Point(3, 268);
            this.displayStyleAuthorLabel.Name = "displayStyleAuthorLabel";
            this.displayStyleAuthorLabel.Size = new System.Drawing.Size(43, 13);
            this.displayStyleAuthorLabel.TabIndex = 4;
            this.displayStyleAuthorLabel.Text = "[author]";
            // 
            // displayStyleNameLabel
            // 
            this.displayStyleNameLabel.AutoSize = true;
            this.displayStyleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayStyleNameLabel.Location = new System.Drawing.Point(3, 5);
            this.displayStyleNameLabel.Name = "displayStyleNameLabel";
            this.displayStyleNameLabel.Size = new System.Drawing.Size(56, 16);
            this.displayStyleNameLabel.TabIndex = 2;
            this.displayStyleNameLabel.Text = "[name]";
            // 
            // displaySettingsPanel
            // 
            this.displaySettingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.displaySettingsPanel.BackColor = System.Drawing.Color.Transparent;
            this.displaySettingsPanel.Location = new System.Drawing.Point(3, 56);
            this.displaySettingsPanel.Name = "displaySettingsPanel";
            this.displaySettingsPanel.Size = new System.Drawing.Size(328, 207);
            this.displaySettingsPanel.TabIndex = 1;
            // 
            // displayOptionsListBox
            // 
            this.displayOptionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.displayOptionsListBox.FormattingEnabled = true;
            this.displayOptionsListBox.Location = new System.Drawing.Point(7, 7);
            this.displayOptionsListBox.Name = "displayOptionsListBox";
            this.displayOptionsListBox.Size = new System.Drawing.Size(120, 303);
            this.displayOptionsListBox.TabIndex = 0;
            this.displayOptionsListBox.SelectedValueChanged += new System.EventHandler(this.displayOptionsListBox_SelectedValueChanged);
            // 
            // tabPageNetwork
            // 
            this.tabPageNetwork.Controls.Add(this.autoEnableWebNotificationsCheckBox);
            this.tabPageNetwork.Controls.Add(this.listenForWebNotificationsCheckBox);
            this.tabPageNetwork.Controls.Add(this.removeComputerButton);
            this.tabPageNetwork.Controls.Add(this.addForwardButton);
            this.tabPageNetwork.Controls.Add(this.forwardToComputersListView);
            this.tabPageNetwork.Controls.Add(this.forwardNotificationsCheckBox);
            this.tabPageNetwork.Controls.Add(this.serverPasswordTextBox);
            this.tabPageNetwork.Controls.Add(this.serverPasswordLabel);
            this.tabPageNetwork.Controls.Add(this.allowRemoteRegistrationCheckBox);
            this.tabPageNetwork.Controls.Add(this.listenForNotificationsCheckBox);
            this.tabPageNetwork.Location = new System.Drawing.Point(4, 22);
            this.tabPageNetwork.Name = "tabPageNetwork";
            this.tabPageNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNetwork.Size = new System.Drawing.Size(476, 324);
            this.tabPageNetwork.TabIndex = 3;
            this.tabPageNetwork.Text = "Network";
            this.tabPageNetwork.UseVisualStyleBackColor = true;
            // 
            // autoEnableWebNotificationsCheckBox
            // 
            this.autoEnableWebNotificationsCheckBox.AutoSize = true;
            this.autoEnableWebNotificationsCheckBox.Location = new System.Drawing.Point(29, 143);
            this.autoEnableWebNotificationsCheckBox.Name = "autoEnableWebNotificationsCheckBox";
            this.autoEnableWebNotificationsCheckBox.Size = new System.Drawing.Size(205, 17);
            this.autoEnableWebNotificationsCheckBox.TabIndex = 13;
            this.autoEnableWebNotificationsCheckBox.Text = "Automatically enable web notifications";
            this.autoEnableWebNotificationsCheckBox.UseVisualStyleBackColor = true;
            this.autoEnableWebNotificationsCheckBox.CheckedChanged += new System.EventHandler(this.autoEnableWebNotificationsCheckBox_CheckedChanged);
            // 
            // listenForWebNotificationsCheckBox
            // 
            this.listenForWebNotificationsCheckBox.AutoSize = true;
            this.listenForWebNotificationsCheckBox.Location = new System.Drawing.Point(29, 119);
            this.listenForWebNotificationsCheckBox.Name = "listenForWebNotificationsCheckBox";
            this.listenForWebNotificationsCheckBox.Size = new System.Drawing.Size(206, 17);
            this.listenForWebNotificationsCheckBox.TabIndex = 12;
            this.listenForWebNotificationsCheckBox.Text = "Listen for notifications from web pages";
            this.listenForWebNotificationsCheckBox.UseVisualStyleBackColor = true;
            this.listenForWebNotificationsCheckBox.CheckedChanged += new System.EventHandler(this.listenForWebNotificationsCheckBox_CheckedChanged);
            // 
            // removeComputerButton
            // 
            this.removeComputerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removeComputerButton.Location = new System.Drawing.Point(122, 285);
            this.removeComputerButton.Name = "removeComputerButton";
            this.removeComputerButton.Size = new System.Drawing.Size(111, 23);
            this.removeComputerButton.TabIndex = 11;
            this.removeComputerButton.Text = "Remove Computer";
            this.removeComputerButton.UseVisualStyleBackColor = true;
            this.removeComputerButton.Click += new System.EventHandler(this.removeComputerButton_Click);
            // 
            // addForwardButton
            // 
            this.addForwardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addForwardButton.Location = new System.Drawing.Point(29, 285);
            this.addForwardButton.Name = "addForwardButton";
            this.addForwardButton.Size = new System.Drawing.Size(87, 23);
            this.addForwardButton.TabIndex = 10;
            this.addForwardButton.Text = "Add Computer";
            this.addForwardButton.UseVisualStyleBackColor = true;
            this.addForwardButton.Click += new System.EventHandler(this.AddForwardButton_Click);
            // 
            // forwardToComputersListView
            // 
            this.forwardToComputersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.forwardToComputersListView.CheckBoxes = true;
            this.forwardToComputersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.forwardComputerListUseColumnHeader,
            this.forwardComputerListComputerColumnHeader,
            this.forwardComputerListPasswordColumnHeader});
            this.forwardToComputersListView.FullRowSelect = true;
            this.forwardToComputersListView.Location = new System.Drawing.Point(29, 203);
            this.forwardToComputersListView.MultiSelect = false;
            this.forwardToComputersListView.Name = "forwardToComputersListView";
            this.forwardToComputersListView.Size = new System.Drawing.Size(427, 75);
            this.forwardToComputersListView.TabIndex = 9;
            this.forwardToComputersListView.UseCompatibleStateImageBehavior = false;
            this.forwardToComputersListView.View = System.Windows.Forms.View.Details;
            this.forwardToComputersListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.forwardToComputersListView_ItemChecked);
            this.forwardToComputersListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.forwardToComputersListView_ItemSelectionChanged);
            // 
            // forwardComputerListUseColumnHeader
            // 
            this.forwardComputerListUseColumnHeader.Text = "Use";
            this.forwardComputerListUseColumnHeader.Width = 40;
            // 
            // forwardComputerListComputerColumnHeader
            // 
            this.forwardComputerListComputerColumnHeader.Text = "Computer";
            this.forwardComputerListComputerColumnHeader.Width = 260;
            // 
            // forwardComputerListPasswordColumnHeader
            // 
            this.forwardComputerListPasswordColumnHeader.Text = "Password";
            this.forwardComputerListPasswordColumnHeader.Width = 100;
            // 
            // forwardNotificationsCheckBox
            // 
            this.forwardNotificationsCheckBox.AutoSize = true;
            this.forwardNotificationsCheckBox.Location = new System.Drawing.Point(29, 179);
            this.forwardNotificationsCheckBox.Name = "forwardNotificationsCheckBox";
            this.forwardNotificationsCheckBox.Size = new System.Drawing.Size(214, 17);
            this.forwardNotificationsCheckBox.TabIndex = 8;
            this.forwardNotificationsCheckBox.Text = "Forward notifications to other computers";
            this.forwardNotificationsCheckBox.UseVisualStyleBackColor = true;
            this.forwardNotificationsCheckBox.CheckedChanged += new System.EventHandler(this.forwardNotificationsCheckBox_CheckedChanged);
            // 
            // serverPasswordTextBox
            // 
            this.serverPasswordTextBox.Location = new System.Drawing.Point(122, 76);
            this.serverPasswordTextBox.Name = "serverPasswordTextBox";
            this.serverPasswordTextBox.Size = new System.Drawing.Size(154, 20);
            this.serverPasswordTextBox.TabIndex = 7;
            this.serverPasswordTextBox.UseSystemPasswordChar = true;
            this.serverPasswordTextBox.TextChanged += new System.EventHandler(this.serverPasswordTextBox_TextChanged);
            // 
            // serverPasswordLabel
            // 
            this.serverPasswordLabel.AutoSize = true;
            this.serverPasswordLabel.Location = new System.Drawing.Point(26, 79);
            this.serverPasswordLabel.Name = "serverPasswordLabel";
            this.serverPasswordLabel.Size = new System.Drawing.Size(90, 13);
            this.serverPasswordLabel.TabIndex = 6;
            this.serverPasswordLabel.Text = "Server Password:";
            // 
            // allowRemoteRegistrationCheckBox
            // 
            this.allowRemoteRegistrationCheckBox.AutoSize = true;
            this.allowRemoteRegistrationCheckBox.Location = new System.Drawing.Point(29, 53);
            this.allowRemoteRegistrationCheckBox.Name = "allowRemoteRegistrationCheckBox";
            this.allowRemoteRegistrationCheckBox.Size = new System.Drawing.Size(194, 17);
            this.allowRemoteRegistrationCheckBox.TabIndex = 5;
            this.allowRemoteRegistrationCheckBox.Text = "Allow remote application registration";
            this.allowRemoteRegistrationCheckBox.UseVisualStyleBackColor = true;
            this.allowRemoteRegistrationCheckBox.CheckedChanged += new System.EventHandler(this.allowRemoteRegistrationCheckBox_CheckedChanged);
            // 
            // listenForNotificationsCheckBox
            // 
            this.listenForNotificationsCheckBox.AutoSize = true;
            this.listenForNotificationsCheckBox.Location = new System.Drawing.Point(29, 29);
            this.listenForNotificationsCheckBox.Name = "listenForNotificationsCheckBox";
            this.listenForNotificationsCheckBox.Size = new System.Drawing.Size(245, 17);
            this.listenForNotificationsCheckBox.TabIndex = 4;
            this.listenForNotificationsCheckBox.Text = "Listen for notifications from remote applications";
            this.listenForNotificationsCheckBox.UseVisualStyleBackColor = true;
            this.listenForNotificationsCheckBox.CheckedChanged += new System.EventHandler(this.listenForNotificationsCheckBox_CheckedChanged);
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.Controls.Add(this.aboutTextBox);
            this.tabPageAbout.Location = new System.Drawing.Point(4, 22);
            this.tabPageAbout.Name = "tabPageAbout";
            this.tabPageAbout.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAbout.Size = new System.Drawing.Size(476, 324);
            this.tabPageAbout.TabIndex = 4;
            this.tabPageAbout.Text = "About Growl";
            this.tabPageAbout.UseVisualStyleBackColor = true;
            // 
            // aboutTextBox
            // 
            this.aboutTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutTextBox.BackColor = System.Drawing.Color.White;
            this.aboutTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aboutTextBox.Location = new System.Drawing.Point(6, 6);
            this.aboutTextBox.Multiline = true;
            this.aboutTextBox.Name = "aboutTextBox";
            this.aboutTextBox.ReadOnly = true;
            this.aboutTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.aboutTextBox.Size = new System.Drawing.Size(464, 295);
            this.aboutTextBox.TabIndex = 0;
            // 
            // previewDisplayButton
            // 
            this.previewDisplayButton.Location = new System.Drawing.Point(269, 270);
            this.previewDisplayButton.Name = "previewDisplayButton";
            this.previewDisplayButton.Size = new System.Drawing.Size(61, 23);
            this.previewDisplayButton.TabIndex = 7;
            this.previewDisplayButton.Text = "Preview";
            this.previewDisplayButton.UseVisualStyleBackColor = true;
            this.previewDisplayButton.Click += new System.EventHandler(this.previewDisplayButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 374);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(516, 408);
            this.Name = "MainForm";
            this.Text = "Growl";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.loggingOptionsPanel.ResumeLayout(false);
            this.loggingOptionsPanel.PerformLayout();
            this.tabPageApplications.ResumeLayout(false);
            this.tabPageDisplayOptions.ResumeLayout(false);
            this.displayOptionsContainerPanel.ResumeLayout(false);
            this.displayOptionsContainerPanel.PerformLayout();
            this.tabPageNetwork.ResumeLayout(false);
            this.tabPageNetwork.PerformLayout();
            this.tabPageAbout.ResumeLayout(false);
            this.tabPageAbout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem contextMenuItemQuit;
        private System.Windows.Forms.ToolStripMenuItem contextMenuItemSettings;
        private System.Windows.Forms.ToolStripSeparator contextMenuSeperator1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageApplications;
        private System.Windows.Forms.TabPage tabPageDisplayOptions;
        private System.Windows.Forms.TabPage tabPageNetwork;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.TextBox aboutTextBox;
        private System.Windows.Forms.Label launchOptionsLabel;
        private System.Windows.Forms.Label runningStatusLabel;
        private System.Windows.Forms.Button startStopButton;
        private System.Windows.Forms.CheckBox autoStartCheckbox;
        private System.Windows.Forms.Label logginLabel;
        private System.Windows.Forms.ComboBox defaultDisplayStyleComboBox;
        private System.Windows.Forms.Label defaultDisplayStyleLabel;
        private System.Windows.Forms.CheckBox enableLoggingCheckBox;
        private System.Windows.Forms.RadioButton customFileRadioButton;
        private System.Windows.Forms.RadioButton consoleLogRadioButton;
        private System.Windows.Forms.Panel loggingOptionsPanel;
        private System.Windows.Forms.TextBox customLoggingFileTextBox;
        private System.Windows.Forms.Label currentVersionLabel;
        private System.Windows.Forms.CheckBox idleCheckBox;
        private System.Windows.Forms.Label idleLabel;
        private System.Windows.Forms.CheckBox statusItemCheckBox;
        private System.Windows.Forms.Label statusItemLabel;
        private System.Windows.Forms.CheckBox logOnlyCheckBox;
        private System.Windows.Forms.CheckBox autoCheckUpdatedCheckBox;
        private System.Windows.Forms.Label currentVersionInfoLabel;
        private ListViewEx applicationListView;
        private ListViewEx notificationsListView;
        private System.Windows.Forms.ColumnHeader appListEnableColumnHeader;
        private System.Windows.Forms.ColumnHeader appListAppNameColumnHeader;
        private System.Windows.Forms.ColumnHeader appListDisplayColumnHeader;
        private System.Windows.Forms.ColumnHeader appListClickColumnHeader;
        private System.Windows.Forms.ColumnHeader notificationListEnableColumnHeader;
        private System.Windows.Forms.ColumnHeader notificationListNotificationNameColumnHeader;
        private System.Windows.Forms.ColumnHeader notificationListDisplayColumnHeader;
        private System.Windows.Forms.ColumnHeader notificationListPriorityColumnHeader;
        private System.Windows.Forms.ColumnHeader notificationListStickyColumnHeader;
        private System.Windows.Forms.TextBox serverPasswordTextBox;
        private System.Windows.Forms.Label serverPasswordLabel;
        private System.Windows.Forms.CheckBox allowRemoteRegistrationCheckBox;
        private System.Windows.Forms.CheckBox listenForNotificationsCheckBox;
        private System.Windows.Forms.ListView forwardToComputersListView;
        private System.Windows.Forms.CheckBox forwardNotificationsCheckBox;
        private System.Windows.Forms.Button addForwardButton;
        private System.Windows.Forms.ColumnHeader forwardComputerListUseColumnHeader;
        private System.Windows.Forms.ColumnHeader forwardComputerListComputerColumnHeader;
        private System.Windows.Forms.ColumnHeader forwardComputerListPasswordColumnHeader;
        private System.Windows.Forms.Button removeComputerButton;
        private System.Windows.Forms.ComboBox applicationDisplayComboBox;
        private System.Windows.Forms.ComboBox notificationDisplayComboBox;
        private System.Windows.Forms.ComboBox notificationPriorityComboBox;
        private System.Windows.Forms.ComboBox notificationStickyComboBox;
        private System.Windows.Forms.Label currentAssemblyVersionLabel;
        private System.Windows.Forms.Panel displaySettingsPanel;
        private System.Windows.Forms.ListBox displayOptionsListBox;
        private System.Windows.Forms.Panel displayOptionsContainerPanel;
        private System.Windows.Forms.Label displayStyleAuthorLabel;
        private System.Windows.Forms.Label displayStyleNameLabel;
        private System.Windows.Forms.Label displayStyleVersionLabel;
        private System.Windows.Forms.Label displayStyleDescriptionLabel;
        private System.Windows.Forms.CheckBox listenForWebNotificationsCheckBox;
        private System.Windows.Forms.CheckBox autoEnableWebNotificationsCheckBox;
        private System.Windows.Forms.Button previewDisplayButton;
    }
}

