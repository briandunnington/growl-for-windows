using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.UI;
using Growl.Destinations;


namespace Growl
{
    public partial class MainForm : Form
    {
        private bool initialized;
        private bool disposed;
        private Controller controller;
        private List<Panel> panels = new List<Panel>();
        private ToolTip historyTrackBarToolTip = new ToolTip();
        private Timer historyTrackBarTimer = new Timer();
        private PastNotificationManager historyManager = new PastNotificationManager();


        public MainForm()
        {
            InitializeComponent();

            // localize text
            this.Text = Properties.Resources.SettingsForm_FormTitle;
            this.labelInitializationStage.Text = Properties.Resources.Loading_Initializing;
            this.toolbarButtonGeneral.Text = Properties.Resources.Toolbar_General;
            this.toolbarButtonApplications.Text = Properties.Resources.Toolbar_Applications;
            this.toolbarButtonDisplays.Text = Properties.Resources.Toolbar_Displays;
            this.toolbarButtonNetwork.Text = Properties.Resources.Toolbar_Network;
            this.toolbarButtonSecurity.Text = Properties.Resources.Toolbar_Security;
            this.toolbarButtonHistory.Text = Properties.Resources.Toolbar_History;
            this.toolbarButtonAbout.Text = Properties.Resources.Toolbar_About;

            this.groupBoxIdleSettings.Text = Properties.Resources.General_IdleSettingsTitle;
            this.radioButtonIdleNever.Text = Properties.Resources.General_IdleSettings_NeverIdle;
            this.groupBoxSoundSettings.Text = Properties.Resources.General_SoundSettingsTitle;
            this.labelDefaultSound.Text = Properties.Resources.General_SoundSettings_SoundLabel;
            this.checkBoxMuteAllSounds.Text = Properties.Resources.General_SoundSettings_MuteLabel;
            this.checkBoxAutoStart.Text = Properties.Resources.General_AutoStart;

            this.labelPrefSound.Text = Properties.Resources.Applications_Preferences_SoundLabel;
            this.labelPrefSticky.Text = Properties.Resources.Applications_Preferences_StickyLabel;
            this.labelPrefPriority.Text = Properties.Resources.Applications_Preferences_PriorityLabel;
            this.labelPrefForward.Text = Properties.Resources.Applications_Preferences_ForwardingLabel;
            this.labelPrefDisplay.Text = Properties.Resources.Applications_Preferences_DisplayLabel;
            this.labelPrefDuration.Text = Properties.Resources.Applications_Preferences_DurationLabel;
            this.labelPrefEnabled.Text = Properties.Resources.Applications_Preferences_EnabledLabel;
            this.labelNoApps.Text = Properties.Resources.Applications_NoAppsRegistered_Title;
            this.labelNoAppsDesc.Text = Properties.Resources.Applications_NoAppsRegistered_Description;
            this.removeApplicationToolStripMenuItem.Text = Properties.Resources.Applications_RemoveApplication;
            this.listControlApplications.HeaderText = Properties.Resources.Applications_ApplicationListHeader;
            this.listControlApplicationNotifications.HeaderText = Properties.Resources.Applications_NotificationListHeader;

            this.buttonPreviewDisplay.Text = Properties.Resources.Button_Preview;
            this.buttonSetAsDefault.Text = Properties.Resources.Button_SetAsDefault;
            this.listControlDisplays.HeaderText = Properties.Resources.Displays_DisplayListHeader;
            this.getDisplaysLabel.Text = Properties.Resources.Displays_FindMore;

            this.labelPasswordManager.Text = Properties.Resources.Security_PasswordManager_Title;
            this.checkBoxAllowSubscriptions.Text = Properties.Resources.Security_AllowSubscriptions;
            this.checkBoxAllowWebNotifications.Text = Properties.Resources.Security_AllowWebNotifications;
            this.checkBoxAllowNetworkNotifications.Text = Properties.Resources.Security_AllowNetworkNotifications;
            this.checkBoxRequireLocalPassword.Text = Properties.Resources.Security_RequirePasswordLocalApps;
            this.checkBoxRequireLANPassword.Text = Properties.Resources.Security_RequirePasswordLANApps;
            this.editToolStripMenuItem.Text = Properties.Resources.Network_EditComputer;
            this.removeComputerToolStripMenuItem.Text = Properties.Resources.Network_RemoveComputer;

            this.checkBoxEnableSubscriptions.Text = Properties.Resources.Network_SubscribeToNotifications;
            this.checkBoxEnableForwarding.Text = Properties.Resources.Network_ForwardNotifications;

            this.buttonClearHistory.Text = Properties.Resources.Button_Clear;
            this.historyDaysGroupBox.Text = Properties.Resources.History_NumberOfDaysTitle;
            this.historySortByGroupBox.Text = Properties.Resources.History_SortByTitle;
            this.historySortByDateRadioButton.Text = Properties.Resources.History_SortBy_Date;
            this.historySortByApplicationRadioButton.Text = Properties.Resources.History_SortBy_Application;

            this.labelAboutBuildNumber.Text = Properties.Resources.About_BuildNumber;
            this.labelAboutUs.Text = Properties.Resources.About_FindInformationLabel;
            this.labelAboutIcons.Text = Properties.Resources.About_IconInfoLabel;
            this.labelAboutIcons2.Text = Properties.Resources.About_IconInfoLabel2;
            this.labelAboutOriginal.Text = Properties.Resources.About_OriginalLabel;
            this.labelAboutOriginal2.Text = Properties.Resources.About_OriginalLabel2;
            this.labelAboutGrowlVersion.Text = Properties.Resources.About_GrowlVersion;

            // handle the 'consider me idle after X seconds' radio button
            // (since the textbox needs to be placed at the appropriate position within the label)
            string idleAfterText = Properties.Resources.General_IdleSettings_IdleAfter;
            if (idleAfterText.Contains("{0}"))
            {
                string before = idleAfterText.Substring(0, idleAfterText.IndexOf('{'));
                string after = idleAfterText.Substring(idleAfterText.IndexOf('}') + 1);
                this.radioButtonIdleAfter.Text = before;
                this.textBoxIdleAfterSeconds.Location = new Point(this.radioButtonIdleAfter.Bounds.Right - 6, this.radioButtonIdleAfter.Location.Y - 1);
                idleAfterText = String.Format(idleAfterText, "         "); // this leaves space for the textbox
            }
            this.radioButtonIdleAfter.Text = idleAfterText;
        }

        # region visual style

        void toolbarPanel_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.LightBlue);

            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;
            //Color c1 = Color.FromArgb(220, 220, 220);
            //Color c2 = Color.WhiteSmoke;
            Color c1 = Color.FromArgb(255, 255, 255);
            Color c2 = Color.FromArgb(255, 255, 255);

            w = this.toolbarPanel.Size.Width;
            h = this.toolbarPanel.Size.Height;
            Rectangle r1 = new Rectangle(x, y, w, h);
            System.Drawing.Drawing2D.LinearGradientBrush b1 = new System.Drawing.Drawing2D.LinearGradientBrush(r1, c1, c2, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            using (b1)
            {
                b1.SetSigmaBellShape(0.5f, 1.0f);
                e.Graphics.FillRectangle(b1, r1);
            }

            x = this.toolbarPanel.Location.X;
            y = this.toolbarPanel.Location.Y;
            w = this.toolbarPanel.Size.Width;
            h = 1;
            Rectangle r3 = new Rectangle(x, y, w, h);
            System.Drawing.SolidBrush b3 = new SolidBrush(Color.Black);
            using (b3)
            {
                e.Graphics.FillRectangle(b3, r3);
            }

            x = this.toolbarPanel.Location.X;
            y = this.toolbarPanel.Size.Height - 1;
            w = this.toolbarPanel.Size.Width;
            h = 1;
            Rectangle r4 = new Rectangle(x, y, w, h);
            System.Drawing.SolidBrush b4 = new SolidBrush(Color.Black);
            using (b4)
            {
                e.Graphics.FillRectangle(b4, r4);
            }
        }

        # endregion visual style

        private void MainForm_Load(object sender, EventArgs e)
        {
            // handle form style
            this.toolbarPanel.Paint += new PaintEventHandler(toolbarPanel_Paint);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // FORM
            this.Size = Properties.Settings.Default.FormSize;
            this.Location = Properties.Settings.Default.FormLocation;
            if (this.Location == new Point(-1, -1))
            {
                int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
                int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
                this.Location = new Point(x, y);
            }
        }

        internal void InitializePreferences()
        {
            this.controller = Controller.GetController();
            this.controller.ApplicationRegistered += new Controller.ApplicationRegisteredDelegate(controller_ApplicationRegistered);
            this.controller.NotificationReceived += new Controller.NotificationReceivedDelegate(controller_NotificationReceived);
            this.controller.NotificationPast += new Controller.NotificationPastDelegate(controller_NotificationPast);
            this.controller.BonjourServiceUpdate += new Controller.BonjourServiceUpdateDelegate(controller_BonjourServiceUpdate);
            this.controller.ForwardDestinationsUpdated += new EventHandler(controller_ForwardDestinationsUpdated);
            this.controller.SubscriptionsUpdated += new Controller.SubscriptionsUpdatedDelegate(controller_SubscriptionsUpdated);


            // GENERAL
            // start (default to running when launched - handled later in Form_Load)
            this.checkBoxAutoStart.Checked = Properties.Settings.Default.AutoStart;
            this.comboBoxPrefDisplay.Items.Add(Display.None);
            this.comboBoxDefaultSound.DataSource = PrefSound.GetList(false);
            this.comboBoxDefaultSound.SelectedItem = controller.DefaultSound;
            this.checkBoxMuteAllSounds.Checked = Properties.Settings.Default.MuteAllSounds;
            this.textBoxIdleAfterSeconds.Text = controller.IdleAfterSeconds.ToString();
            if (controller.CheckForIdle)
            {
                this.textBoxIdleAfterSeconds.Enabled = true;
                this.radioButtonIdleAfter.Checked = true;
            }

            // APPLICATIONS
            LoadRegisteredApplications();

            // DISPLAYS
            LoadAvailableDisplays();
            this.listControlDisplays.IsDefaultComparer = new Growl.UI.ListControl.IsDefaultComparerDelegate(defaultDisplayComparer);

            // SECURITY
            this.checkBoxRequireLocalPassword.Checked = this.controller.RequireLocalPassword;
            this.checkBoxRequireLANPassword.Checked = this.controller.RequireLANPassword;
            this.checkBoxAllowNetworkNotifications.Checked = this.controller.AllowNetworkNotifications;
            this.checkBoxAllowWebNotifications.Checked = this.controller.AllowWebNotifications;
            this.checkBoxAllowSubscriptions.Checked = this.controller.AllowSubscriptions;
            this.passwordManagerControl1.SetPasswordManager(this.controller.PasswordManager);
            this.passwordManagerControl1.Updated += new EventHandler(passwordManagerControl1_Updated);

            // NETWORK
            this.checkBoxEnableForwarding.Checked = Properties.Settings.Default.AllowForwarding;
            //this.forwardListView.Enabled = this.checkBoxEnableForwarding.Checked;
            //this.buttonAddComputer.Enabled = this.checkBoxEnableForwarding.Checked;
            LoadForwardDestinations();
            this.checkBoxEnableSubscriptions.Checked = Properties.Settings.Default.EnableSubscriptions;
            //this.subscribedListView.Enabled = this.checkBoxEnableSubscriptions.Checked;
            //this.buttonSubscribe.Enabled = this.checkBoxEnableSubscriptions.Checked;
            LoadSubscriptions();

            // HISTORY
            if (Properties.Settings.Default.HistoryView == View.Details.ToString())
                this.historyListView.View = View.Details;
            this.historyTrackBarTimer.Tick += new EventHandler(historyTrackBarTimer_Tick);
            this.historyDaysTrackBar.Minimum = HistoryListView.MIN_NUMBER_OF_DAYS;
            this.historyDaysTrackBar.Maximum = HistoryListView.MAX_NUMBER_OF_DAYS;
            this.historyDaysTrackBar.Value = Properties.Settings.Default.HistoryDays;
            this.historyListView.GroupBy = Properties.Settings.Default.HistorySortBy;
            this.historyListView.NumberOfDays = this.historyDaysTrackBar.Value;

            this.historyManager.LoadPastNotifications();
            this.historyListView.PastNotifications = this.historyManager.PastNotifications;
            if (this.historyListView.GroupBy == HistoryGroupItemsBy.Application)
                this.historySortByApplicationRadioButton.Checked = true;
            else
                this.historySortByDateRadioButton.Checked = true;
            //this.historyListView.Draw();
            this.historyListView.RedrawStarted += new EventHandler(historyListView_RedrawStarted);
            this.historyListView.RedrawFinished += new EventHandler(historyListView_RedrawFinished);

            // ABOUT
            this.labelAboutGrowlVersion.Text = String.Format(this.labelAboutGrowlVersion.Text, Utility.FileVersionInfo.ProductVersion);
            //this.labelAboutGrowlVersion.Text = String.Format(this.labelAboutGrowlVersion.Text, f.FileVersion);
            this.labelAboutBuildNumber.Text = String.Format(this.labelAboutBuildNumber.Text, Utility.FileVersionInfo.FileVersion);
            //this.labelAboutBuildNumber.Text = String.Format(this.labelAboutBuildNumber.Text, a.GetName().Version.ToString());
            this.labelAboutBuildNumber.Left = this.labelAboutGrowlVersion.Right + 6;
        }

        void historyListView_RedrawStarted(object sender, EventArgs args)
        {
            //this.historyListView.Visible = false;
        }

        void historyListView_RedrawFinished(object sender, EventArgs args)
        {
            //this.historyListView.Visible = true;
        }

        void controller_Stopped(object sender, EventArgs e)
        {
            // not used yet
        }

        void controller_Started(object sender, EventArgs e)
        {
            // not used yet
        }

        internal void DoneInitializing()
        {
            // enable toolbar actions
            InitToolbarButtonAndPanel(this.toolbarButtonGeneral, this.panelGeneral);
            InitToolbarButtonAndPanel(this.toolbarButtonApplications, this.panelApplications);
            InitToolbarButtonAndPanel(this.toolbarButtonDisplays, this.panelDisplays);
            InitToolbarButtonAndPanel(this.toolbarButtonNetwork, this.panelNetwork);
            InitToolbarButtonAndPanel(this.toolbarButtonSecurity, this.panelSecurity);
            InitToolbarButtonAndPanel(this.toolbarButtonHistory, this.panelHistory);
            InitToolbarButtonAndPanel(this.toolbarButtonAbout, this.panelAbout);
            this.panelInitializing.Visible = false;
            SwitchPanel(this.toolbarButtonGeneral);
            this.Refresh();

            this.initialized = true;
        }

        private void InitToolbarButtonAndPanel(ToolStripButton tb, Panel panel)
        {
            this.panels.Add(panel);
            tb.Tag = panel;
            tb.Click += new EventHandler(toolbarButton_Click);
        }

        internal void UpdateInitializationProgress(string text, int progress)
        {
            this.labelInitializationStage.Text = text;
            this.labelInitializationStage.Refresh();
            this.progressBarInitializing.Value = progress;
            Application.DoEvents();
        }

        internal void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private static void SwitchPanel(ToolStripButton c)
        {
            ToolStrip ts = c.GetCurrentParent();
            foreach (ToolStripButton tsi in ts.Items)
            {
                if (tsi.Name == c.Name)
                {
                    tsi.Checked = true;
                    (tsi.Tag as Panel).Visible = true;
                }
                else
                {
                    tsi.Checked = false;
                    (tsi.Tag as Panel).Visible = false;
                }
            }
        }

        private void LoadRegisteredApplications()
        {
            BindApplicationList();

            if(this.listControlApplications.Items.Count > 0)
                this.listControlApplications.SelectedIndex = 0;
        }

        private void LoadAvailableDisplays()
        {
            BindDisplayList();
        }

        private void LoadForwardDestinations()
        {
            BindForwardList();
        }

        private void LoadSubscriptions()
        {
            BindSubscriptionList();
        }

        private void BindApplicationList()
        {
            SortedDictionary<string, RegisteredApplication> list = new SortedDictionary<string, RegisteredApplication>(controller.RegisteredApplications);

            this.listControlApplications.SuspendLayout();
            this.listControlApplications.Items.Clear();
            foreach (RegisteredApplication app in list.Values)
            {
                ListControlItem lci = new ListControlItem(app.Name, app);
                this.listControlApplications.AddItem(lci);
            }
            this.listControlApplications.ResumeLayout();

            if (this.controller.RegisteredApplications.Count == 0)
                this.panelNoApps.Visible = true;
            else
                this.panelNoApps.Visible = false;
        }

        internal void BindDisplayList()
        {
            SortedDictionary<string, Display> list = new SortedDictionary<string, Display>(controller.AvailableDisplays);

            this.listControlDisplays.SuspendLayout();
            this.listControlDisplays.Items.Clear();
            bool selected = false;
            foreach (Display display in list.Values)
            {
                this.listControlDisplays.Items.Add(display);
                if (!selected && defaultDisplayComparer(display))
                {
                    this.listControlDisplays.SelectedIndex = this.listControlDisplays.Items.Count - 1;
                    selected = true;
                }
            }

            if(!selected && this.listControlDisplays.Items.Count > 0) this.listControlDisplays.SelectedIndex = 0;
            this.listControlDisplays.ResumeLayout();
        }

        private void BindForwardList()
        {
            this.forwardListView.SuspendLayout();
            this.forwardListView.Computers = GenericDictionaryToList<string, ForwardDestination, DestinationBase>(controller.ForwardDestinations);
            this.forwardListView.Draw();
            this.forwardListView.ResumeLayout();
            this.buttonRemoveComputer.Enabled = false;
        }

        private void BindSubscriptionList()
        {
            this.subscribedListView.SuspendLayout();
            this.subscribedListView.Computers = GenericDictionaryToList<string, Subscription, DestinationBase>(controller.Subscriptions);
            this.subscribedListView.Draw();
            this.subscribedListView.ResumeLayout();
            this.buttonUnsubscribe.Enabled = false;
        }

        private TReturn[] GenericDictionaryToList<TKey, TValue, TReturn>(Dictionary<TKey, TValue> gd) where TValue : TReturn
        {
            if (gd != null && gd.Values != null)
            {
                Dictionary<TKey, TValue>.ValueCollection vc = gd.Values;
                List<TValue> list = new List<TValue>(vc);
                TValue[] values = list.ToArray();
                TReturn[] array = new TReturn[values.Length];
                Array.Copy(values, array, values.Length);
                return array;
            }
            else
            {
                return new TReturn[0];
            }
        }

        internal void UpdateState(string text, Color color)
        {
            this.labelCurrentState.Text = text;
            this.labelCurrentState.ForeColor = color;
        }

        internal void Mute(bool mute)
        {
            this.checkBoxMuteAllSounds.Checked = mute;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // make sure to save any settings from an open settings panel
            if (this.panelDisplaySettings.Tag != null)
            {
                Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)this.panelDisplaySettings.Tag;
                sp.DeselectPanel();
            }
            this.panelDisplaySettingsContainer.Controls.Clear();

            // remember some form information for next time
            Properties.Settings.Default.FormSize = this.Size;
            Properties.Settings.Default.FormLocation = this.Location;
            Properties.Settings.Default.Save();

            /*
            // instead of closing, just hide the form (minimized to system tray)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            else
            {
                // make sure to save any settings from an open settings panel
                if (this.panelDisplaySettings.Tag != null)
                {
                    Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)this.panelDisplaySettings.Tag;
                    sp.DeselectPanel();
                }

                // remember some form information for next time
                Properties.Settings.Default.FormSize = this.Size;
                Properties.Settings.Default.FormLocation = this.Location;
                Properties.Settings.Default.Save();
            }
             * */
        }

        internal Controller Controller
        {
            get
            {
                return this.controller;
            }
        }

        internal ProgressBar ProgressBarInitializing
        {
            get
            {
                return this.progressBarInitializing;
            }
        }

        internal OnOffButton OnOffButton
        {
            get
            {
                return this.onOffButton1;
            }
        }

        private void toolbarButton_Click(object sender, EventArgs e)
        {
            ToolStripButton c = (ToolStripButton)sender;
            SwitchPanel(c);
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.checkBoxAutoStart.Checked)
            {
                controller.DisableAutoStart();
            }
            else
            {
                controller.EnableAutoStart();
            }
        }

        void controller_ApplicationRegistered(RegisteredApplication ra)
        {
            BindApplicationList();
        }

        void controller_NotificationReceived(Growl.DisplayStyle.Notification n)
        {
            // do nothing for now
        }

        void controller_NotificationPast(PastNotification pn)
        {
            this.historyListView.AddNotification(pn);
        }

        void controller_ForwardDestinationsUpdated(object sender, EventArgs e)
        {
            this.BindForwardList();
        }

        void controller_BonjourServiceUpdate(BonjourForwardDestination bfc)
        {
            //BindForwardList();
            this.forwardListView.Refresh();
        }

        void controller_SubscriptionsUpdated(bool countChanged)
        {
            if (countChanged)
            {
                BindSubscriptionList();
            }
            else
                this.subscribedListView.Refresh();
        }

        internal void OnSystemTimeChanged()
        {
            this.historyManager.ReloadPastNotifications();
            this.historyListView.PastNotifications = this.historyManager.PastNotifications;
            this.historyListView.Draw();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    // unregister event handlers
                    this.toolbarPanel.Paint -= new PaintEventHandler(toolbarPanel_Paint);
                    this.controller.ApplicationRegistered -= new Controller.ApplicationRegisteredDelegate(controller_ApplicationRegistered);
                    this.controller.NotificationReceived -= new Controller.NotificationReceivedDelegate(controller_NotificationReceived);
                    this.controller.NotificationPast -= new Controller.NotificationPastDelegate(controller_NotificationPast);
                    this.controller.BonjourServiceUpdate -= new Controller.BonjourServiceUpdateDelegate(controller_BonjourServiceUpdate);
                    this.controller.ForwardDestinationsUpdated -= new EventHandler(controller_ForwardDestinationsUpdated);
                    this.controller.SubscriptionsUpdated -= new Controller.SubscriptionsUpdatedDelegate(controller_SubscriptionsUpdated);
                    this.passwordManagerControl1.Updated -= new EventHandler(passwordManagerControl1_Updated);
                    this.historyTrackBarTimer.Tick -= new EventHandler(historyTrackBarTimer_Tick);

                    this.toolbarButtonGeneral.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonApplications.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonDisplays.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonNetwork.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonSecurity.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonHistory.Click -= new EventHandler(toolbarButton_Click);
                    this.toolbarButtonAbout.Click -= new EventHandler(toolbarButton_Click);

                    if (this.listControlDisplays != null)
                    {
                        foreach (Display display in this.listControlDisplays.Items)
                        {
                            if (display != null)
                            {
                                Growl.DisplayStyle.SettingsPanelBase panel = display.SettingsPanel;
                                if (panel != null)
                                {
                                    panel.SettingsChanged -= new EventHandler(panel_SettingsChanged);
                                }
                            }
                        }
                    }

                    if (components != null)
                    {
                        components.Dispose();
                    }

                    if (historyTrackBarToolTip != null)
                    {
                        historyTrackBarToolTip.RemoveAll();
                        historyTrackBarToolTip.Dispose();
                        historyTrackBarToolTip = null;
                    }

                    if (historyTrackBarTimer != null)
                    {
                        historyTrackBarTimer.Dispose();
                        historyTrackBarTimer = null;
                    }

                    if (this.Icon != null)
                    {
                        this.Icon.Dispose();
                        this.Icon = null;
                    }

                     Growl.FormResources.ResourceManager.ReleaseAllResources();

                    // deal with pictureboxes
                    if (this.pictureBox1 != null && this.pictureBox1.Image != null) this.pictureBox1.Image.Dispose();
                    if (this.pictureBoxApplication != null && this.pictureBoxApplication.Image != null) this.pictureBoxApplication.Image.Dispose();
                    if (this.pictureBoxApplicationNotification != null && this.pictureBoxApplicationNotification.Image != null) this.pictureBoxApplicationNotification.Image.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void historySortByDateRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.historySortByDateRadioButton.Checked)
            {
                this.historyListView.GroupBy = HistoryGroupItemsBy.Date;
                this.historyListView.Draw();
                Properties.Settings.Default.HistorySortBy = this.historyListView.GroupBy;
                Properties.Settings.Default.Save();
            }
        }

        private void historySortByApplicationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.historySortByApplicationRadioButton.Checked)
            {
                this.historyListView.GroupBy = HistoryGroupItemsBy.Application;
                this.historyListView.Draw();
                Properties.Settings.Default.HistorySortBy = this.historyListView.GroupBy;
                Properties.Settings.Default.Save();
            }
        }

        private void historyDaysTrackBar_Scroll(object sender, EventArgs e)
        {
            this.historyTrackBarTimer.Interval = 200;
            this.historyTrackBarTimer.Start();
        }

        void historyTrackBarTimer_Tick(object sender, EventArgs e)
        {
            this.historyTrackBarTimer.Stop();

            if (this.historyListView.NumberOfDays != this.historyDaysTrackBar.Value)
            {
                int val = this.historyDaysTrackBar.Value;

                if (this.historyTrackBarToolTip != null) this.historyTrackBarToolTip.Hide(this.historyDaysTrackBar);
                int interval = this.historyDaysTrackBar.Width / this.historyDaysTrackBar.Maximum;
                int x = (interval * val) - ((int)(0.70 * interval));
                int y = this.historyDaysTrackBar.Height - 18;
                this.historyTrackBarToolTip.Show(val.ToString(), this.historyDaysTrackBar, x, y, 750);

                this.historyListView.NumberOfDays = val;
                this.historyListView.Draw();

                Properties.Settings.Default.HistoryDays = val;
                Properties.Settings.Default.Save();
            }
        }

        private void labelAboutUsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void labelAboutOriginalLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void labelAboutIconsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(ll.Text);
            OpenLink(ll.Text);
        }

        private void OpenLink(string link)
        {
            System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(OpenLinkAsync);
            System.Threading.Thread t = new System.Threading.Thread(pts);
            t.Start(link);
        }

        private void OpenLinkAsync(object state)
        {
            string link = (string)state;
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(link);
            info.UseShellExecute = true;
            System.Diagnostics.Process.Start(info);
        }

        void panel_SettingsChanged(object sender, EventArgs e)
        {
            Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)sender;
            Display d = (Display)sp.Display;
            d.SettingsCollection = sp.GetSettings();
        }

        private void buttonPreviewDisplay_Click(object sender, EventArgs e)
        {
            panel_SettingsChanged(this.panelDisplaySettings.Tag, EventArgs.Empty);
            Display display = (Display)this.listControlDisplays.SelectedItem;
            this.controller.SendSystemNotification(Properties.Resources.SystemNotification_Preview_Title, String.Format(Properties.Resources.SystemNotification_Preview_Text, display.Name), display);
        }

        private void displayStyleWebsiteLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            System.Diagnostics.Process.Start(ll.Text);
        }

        private void buttonClearHistory_Click(object sender, EventArgs e)
        {
            this.historyManager.ClearHistory();
            this.historyListView.SuspendLayout();
            this.historyListView.PastNotifications = this.historyManager.PastNotifications;
            this.historyListView.Draw();
            this.historyListView.ResumeLayout();
            
            // Normally we shouldnt ever explicitly call GC.Collect(), but since the items in the History
            // view could have been taking up a lot of memory, and this is a user-initiated event that 
            // does not occur frequently, this is an OK place to force a collection.
            Utility.WriteDebugInfo("History cleared. Force GC to clean up LOH");
            ApplicationMain.ForceGC();
        }

        private void ShowPreferences(IRegisteredObject iro, NotificationPreferences prefs, string text)
        {
            if (this.pictureBoxApplicationNotification.Image != null) this.pictureBoxApplicationNotification.Image.Dispose();
            this.pictureBoxApplicationNotification.Image = iro.GetIcon();

            this.labelApplicationNotification.Text = text;

            this.comboBoxPrefEnabled.DataSource = PrefEnabled.GetList();
            this.comboBoxPrefEnabled.SelectedItem = prefs.PrefEnabled;
            this.comboBoxPrefEnabled.Tag = prefs;

            this.comboBoxPrefDisplay.BeginUpdate();
            this.comboBoxPrefDisplay.Items.Clear();
            this.comboBoxPrefDisplay.Items.Add(Display.Default);
            this.comboBoxPrefDisplay.Items.Add(Display.None);
            foreach (Display display in this.controller.AvailableDisplays.Values)
            {
                this.comboBoxPrefDisplay.Items.Add(display);
            }
            this.comboBoxPrefDisplay.SelectedItem = prefs.PrefDisplay;
            this.comboBoxPrefDisplay.Tag = prefs;
            this.comboBoxPrefDisplay.EndUpdate();

            this.comboBoxPrefDuration.DataSource = PrefDuration.GetList(true);
            this.comboBoxPrefDuration.SelectedItem = prefs.PrefDuration;
            this.comboBoxPrefDuration.Tag = prefs;

            this.comboBoxPrefSticky.DataSource = PrefSticky.GetList(true);
            this.comboBoxPrefSticky.SelectedItem = prefs.PrefSticky;
            this.comboBoxPrefSticky.Tag = prefs;

            this.comboBoxPrefForward.DataSource = PrefForward.GetList(true);
            this.comboBoxPrefForward.SelectedItem = prefs.PrefForward;
            this.comboBoxPrefForward.Tag = prefs;

            this.comboBoxPrefPriority.DataSource = PrefPriority.GetList(true);
            this.comboBoxPrefPriority.SelectedItem = prefs.PrefPriority;
            this.comboBoxPrefPriority.Tag = prefs;

            this.comboBoxPrefSound.DataSource = PrefSound.GetList(true);
            this.comboBoxPrefSound.SelectedItem = prefs.PrefSound;
            this.comboBoxPrefSound.Tag = prefs;
        }

        private void comboBoxPrefEnabled_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefEnabled.Tag;
            if (prefs != null)
            {
                PrefEnabled prefEnabled = (PrefEnabled) this.comboBoxPrefEnabled.SelectedItem;
                prefs.PrefEnabled = prefEnabled;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefDisplay_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefDisplay.Tag;
            if (prefs != null)
            {
                Display prefDisplay = (Display)this.comboBoxPrefDisplay.SelectedItem;
                prefs.PrefDisplay = prefDisplay;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void listControlApplications_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.listControlApplicationNotifications.SuspendLayout();
            this.listControlApplicationNotifications.Items.Clear();

            Growl.UI.ListControl lc = sender as Growl.UI.ListControl;
            ListControlItem selectedLCI = lc.SelectedItem as ListControlItem;
            if (selectedLCI != null)
            {
                RegisteredApplication app = (RegisteredApplication)selectedLCI.RegisteredObject;

                if (this.pictureBoxApplication.Image != null) this.pictureBoxApplication.Image.Dispose();
                this.pictureBoxApplication.Image = app.GetIcon();
                this.labelApplication.Text = app.Name;

                // add a default item to the list
                this.listControlApplicationNotifications.SuspendLayout();
                ListControlItem appLCI = new ListControlItem(Properties.Resources.Applications_AllNotifications, app);
                this.listControlApplicationNotifications.AddItem(appLCI);

                foreach (RegisteredNotification rn in app.Notifications.Values)
                {
                    ListControlItem lci = new ListControlItem(rn.Name, rn);
                    this.listControlApplicationNotifications.AddItem(lci);
                }
                this.listControlApplicationNotifications.ResumeLayout();

                this.listControlApplicationNotifications.SelectedIndex = 0;

                //ShowPreferences(appLCI.RegisteredObject, app.Preferences, appLCI.Text);
                this.panelSelectedApplication.Visible = true;
                this.listControlApplicationNotifications.Select();
            }
            else
            {
                this.panelSelectedApplication.Visible = false;
            }
            this.listControlApplicationNotifications.ResumeLayout();
        }

        private void listControlApplicationNotifications_SelectedIndexChanged(object sender, EventArgs e)
        {
            Growl.UI.ListControl lc = sender as Growl.UI.ListControl;
            ListControlItem selectedLCI = lc.SelectedItem as ListControlItem;
            if (selectedLCI != null)
            {
                // TODO: consider making .Preferences property on IRegisteredObject to avoid this statement
                if (selectedLCI.RegisteredObject is RegisteredApplication)
                {
                    ApplicationPreferences prefs = ((RegisteredApplication)selectedLCI.RegisteredObject).Preferences;
                    ShowPreferences(selectedLCI.RegisteredObject, prefs, selectedLCI.Text);
                }
                else
                {
                    NotificationPreferences prefs = ((RegisteredNotification)selectedLCI.RegisteredObject).Preferences;
                    ShowPreferences(selectedLCI.RegisteredObject, prefs, selectedLCI.Text);
                }

                this.panelPrefs.Visible = true;
            }
            else
            {
                this.panelPrefs.Visible = false;
            }
        }

        private void listControlDisplays_SelectedIndexChanged(object sender, EventArgs e)
        {
            // hide any current panel
            if (this.panelDisplaySettings.Tag != null && this.panelDisplaySettings.Tag is Growl.DisplayStyle.SettingsPanelBase)
            {
                Growl.DisplayStyle.SettingsPanelBase sp = (Growl.DisplayStyle.SettingsPanelBase)this.panelDisplaySettings.Tag;
                sp.SettingsChanged -= new EventHandler(panel_SettingsChanged);
                sp.DeselectPanel();
            }
            this.panelDisplaySettings.Tag = null;
            this.panelDisplaySettingsContainer.Controls.Clear();
            this.displayStyleNameLabel.Text = "";
            this.displayStyleDescriptionLabel.Text = "";
            this.displayStyleAuthorLabel.Text = "";
            this.displayStyleWebsiteLabel.Text = "";
            this.displayStyleVersionLabel.Text = "";
            this.panelDisplaySettings.Visible = false;

            // show the new panel
            if (this.listControlDisplays.SelectedItem != null)
            {
                Display display = this.listControlDisplays.SelectedItem as Display;
                if (display != null)
                {
                    Growl.DisplayStyle.SettingsPanelBase panel = display.SettingsPanel;
                    if (panel != null)
                    {
                        panel.Dock = DockStyle.Fill;
                        panel.Display = display;
                        panel.SettingsChanged += new EventHandler(panel_SettingsChanged);
                        panel.SelectPanel();
                        this.panelDisplaySettingsContainer.Controls.Add(panel);
                        this.panelDisplaySettings.Tag = panel;

                        this.displayStyleNameLabel.Text = display.Name;
                        this.displayStyleDescriptionLabel.Text = display.Description;
                        this.displayStyleAuthorLabel.Text = String.Format("{0} {1}", Properties.Resources.Displays_CreatedBy, display.Author);
                        this.displayStyleWebsiteLabel.Text = display.Website;
                        this.displayStyleVersionLabel.Text = display.Version;

                        // deal with multiple monitor support
                        this.pictureBoxMultipleMonitors.Tag = display;
                        this.pictureBoxMultipleMonitors.Visible = display.SupportsMultipleMonitors;

                        this.panelDisplaySettings.Visible = true;
                    }
                }
            }
        }

        void listControlDisplays_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.listControlDisplays.SelectedItem != null)
            {
                Display newDefault = (Display)this.listControlDisplays.SelectedItem;
                this.controller.DefaultDisplay = newDefault;
                this.listControlDisplays.Refresh();
            }
        }

        private void checkBoxEnableForwarding_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AllowForwarding = this.checkBoxEnableForwarding.Checked;
            Properties.Settings.Default.Save();

            this.forwardListView.AllDisabled = !Properties.Settings.Default.AllowForwarding;
            this.forwardListView.Refresh();

            //this.forwardListView.SelectedIndices.Clear();
            //this.forwardListView.Enabled = this.checkBoxEnableForwarding.Checked;
            //this.buttonAddComputer.Enabled = this.checkBoxEnableForwarding.Checked;
        }

        private void buttonAddComputer_Click(object sender, EventArgs e)
        {
            AddComputer f = new AddComputer();
            f.SetController(this.controller);
            f.ShowDialog(this);
        }

        private void buttonRemoveComputer_Click(object sender, EventArgs e)
        {
            if (this.forwardListView.SelectedItems.Count == 1)
            {
                ListViewItem lvi = this.forwardListView.SelectedItems[0];
                this.forwardListView.Items.Remove(lvi);

                ForwardDestination fc = (ForwardDestination)lvi.Tag;
                this.controller.RemoveForwardDestination(fc);
            }
        }

        private void comboBoxPrefForward_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences) this.comboBoxPrefForward.Tag;
            if (prefs != null)
            {
                PrefForward prefForward = (PrefForward)this.comboBoxPrefForward.SelectedItem;

                // if they chose 'Custom...', we need to let them pick which computers to forward to.
                if (prefForward.IsCustom)
                {
                    ChooseForwarding f = new ChooseForwarding();
                    f.SetController(this.controller);
                    f.SetPrefs(prefs);
                    DialogResult result = f.ShowDialog(this);

                    // if they didnt save their choices, revert their selection
                    if (result != DialogResult.OK)
                    {
                        this.comboBoxPrefForward.SelectedItem = prefs.PrefForward;
                        return;
                    }
                }

                // save their preference
                prefs.PrefForward = prefForward;

                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefPriority_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefPriority.Tag;
            if (prefs != null)
            {
                PrefPriority prefPriority = (PrefPriority)this.comboBoxPrefPriority.SelectedItem;
                prefs.PrefPriority = prefPriority;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefSticky_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences) this.comboBoxPrefSticky.Tag;
            if (prefs != null)
            {
                PrefSticky prefSticky = (PrefSticky)this.comboBoxPrefSticky.SelectedItem;
                prefs.PrefSticky = prefSticky;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void comboBoxPrefSound_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefSound.Tag;
            if (prefs != null)
            {
                PrefSound prefSound = (PrefSound)this.comboBoxPrefSound.SelectedItem;
                prefs.PrefSound = prefSound;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void checkBoxRequireLocalPassword_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.RequireLocalPassword = this.checkBoxRequireLocalPassword.Checked;
        }

        private void checkBoxRequireLANPassword_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.RequireLANPassword = this.checkBoxRequireLANPassword.Checked;
        }

        private void checkBoxAllowNetworkNotifications_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowNetworkNotifications = this.checkBoxAllowNetworkNotifications.Checked;
        }

        private void removeApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripApplications.Tag != null && this.contextMenuStripApplications.Tag is ListControlItem)
            {
                ListControlItem lci = (ListControlItem)this.contextMenuStripApplications.Tag;
                this.controller.RegisteredApplications.Remove(lci.RegisteredObject.Name);
                this.controller.SaveApplicationPrefs();
            }
            this.contextMenuStripApplications.Tag = null;
            this.contextMenuStripApplications.Hide();
            BindApplicationList();
            if (this.listControlApplications.Items.Count > 0)
                this.listControlApplications.SelectedIndex = 0;
            else
                this.panelSelectedApplication.Visible = false;
        }

        private void listControlApplications_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripApplications.Hide();

            if(e.Button == MouseButtons.Right)
            {
                int index = this.listControlApplications.IndexFromPoint(e.Location);
                if(index != ListBox.NoMatches)
                {
                    this.listControlApplications.SelectedIndex = index;
                    this.contextMenuStripApplications.Tag = this.listControlApplications.SelectedItem;
                    this.contextMenuStripApplications.Show(this.listControlApplications, e.Location);
                }
            }
        }

        private void comboBoxDefaultSound_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PrefSound ps = (PrefSound)this.comboBoxDefaultSound.SelectedItem;
            this.controller.DefaultSound = ps;
            try
            {
                if (ps.PlaySound.HasValue && ps.PlaySound.Value)
                {
                    System.Media.SoundPlayer sp = new System.Media.SoundPlayer(ps.SoundFile);
                    using (sp)
                    {
                        sp.Play();
                    }
                }
            }
            catch
            {
                // suppress - dont fail just because the sound could not play
            }
        }

        private void radioButtonIdleAfter_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButtonIdleAfter.Checked)
            {
                this.textBoxIdleAfterSeconds.Enabled = true;
                int idleAfterSeconds = Convert.ToInt32(this.textBoxIdleAfterSeconds.Text);
                this.controller.IdleAfterSeconds = idleAfterSeconds;
                this.controller.CheckForIdle = true;
            }
            else
            {
                this.controller.CheckForIdle = false;
                this.textBoxIdleAfterSeconds.Enabled = false;
                if (String.IsNullOrEmpty(this.textBoxIdleAfterSeconds.Text)) this.textBoxIdleAfterSeconds.Text = "0";
            }
        }

        private void textBoxIdleAfterSeconds_KeyPress(object sender, KeyPressEventArgs e)
        {
            // limit to only numbers and control (backspace, delete, copy/paste)
            // also limit the value to 4 digits
            bool isDigit = Char.IsDigit(e.KeyChar);
            bool isControl = Char.IsControl(e.KeyChar);
            if(isControl || (isDigit && this.textBoxIdleAfterSeconds.Text.Length < 4))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void textBoxIdleAfterSeconds_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxIdleAfterSeconds.Enabled)
            {
                if (!String.IsNullOrEmpty(this.textBoxIdleAfterSeconds.Text))
                {
                    int idleAfterSeconds = Convert.ToInt32(this.textBoxIdleAfterSeconds.Text);
                    this.controller.IdleAfterSeconds = idleAfterSeconds;
                }
            }
        }

        private void forwardListView_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripForwardDestinations.Hide();

            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = this.forwardListView.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag != null)
                {
                    ForwardDestination fc = item.Tag as ForwardDestination;
                    if (fc != null)
                    {
                        this.editToolStripMenuItem.Visible = !(fc is SubscribedForwardDestination);

                        this.contextMenuStripForwardDestinations.Tag = fc;
                        this.contextMenuStripForwardDestinations.Show(this.forwardListView, e.Location);
                    }
                }
            }
        }

        private void removeComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripForwardDestinations.Tag != null)
            {
                ForwardDestination fc = this.contextMenuStripForwardDestinations.Tag as ForwardDestination;
                if (fc != null)
                {
                    this.controller.RemoveForwardDestination(fc);
                }
            }
            this.contextMenuStripForwardDestinations.Tag = null;
            this.contextMenuStripForwardDestinations.Hide();
        }

        private void buttonSubscribe_Click(object sender, EventArgs e)
        {
            AddComputer f = new AddComputer(true);
            f.SetController(this.controller);
            f.ShowDialog(this);
        }

        private void buttonUnsubscribe_Click(object sender, EventArgs e)
        {
            if (this.subscribedListView.SelectedItems.Count == 1)
            {
                ListViewItem lvi = this.subscribedListView.SelectedItems[0];
                this.subscribedListView.Items.Remove(lvi);

                Subscription sub = (Subscription)lvi.Tag;
                this.controller.RemoveSubscription(sub);
            }
        }

        private void checkBoxEnableSubscriptions_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSubscriptions = this.checkBoxEnableSubscriptions.Checked;
            Properties.Settings.Default.Save();

            this.subscribedListView.AllDisabled = !Properties.Settings.Default.EnableSubscriptions;
            this.subscribedListView.Refresh();

            if (!this.checkBoxEnableSubscriptions.Checked)
                this.buttonUnsubscribe.Enabled = false;
            else
            {
                if (this.subscribedListView.SelectedIndices != null && this.subscribedListView.SelectedIndices.Count > 0) this.buttonUnsubscribe.Enabled = true;
            }

            if (this.initialized)
            {
                SubscriptionManager.Update(this.controller.Subscriptions, Properties.Settings.Default.EnableSubscriptions);
            }
        }

        private void subscribedListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.buttonUnsubscribe.Enabled = e.IsSelected;
        }

        private void forwardListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            this.buttonRemoveComputer.Enabled = e.IsSelected;
        }

        private void checkBoxAllowWebNotifications_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowWebNotifications = this.checkBoxAllowWebNotifications.Checked;
        }

        private void checkBoxAllowSubscriptions_CheckedChanged(object sender, EventArgs e)
        {
            this.controller.AllowSubscriptions = this.checkBoxAllowSubscriptions.Checked;
        }

        private void buttonSetAsDefault_Click(object sender, EventArgs e)
        {
            Display newDefault = (Display)this.listControlDisplays.SelectedItem;
            this.controller.DefaultDisplay = newDefault;
            this.listControlDisplays.Refresh();
        }

        private bool defaultDisplayComparer(object obj)
        {
            Display display = obj as Display;
            if (display != null)
            {
                if (display.ActualName == this.controller.DefaultDisplay.ActualName)
                    return true;
            }
            return false;
        }

        private void checkBoxMuteAllSounds_CheckedChanged(object sender, EventArgs e)
        {
            ApplicationMain.Program.Mute(this.checkBoxMuteAllSounds.Checked);
        }

        private void comboBoxPrefDuration_SelectionChangeCommitted(object sender, EventArgs e)
        {
            NotificationPreferences prefs = (NotificationPreferences)this.comboBoxPrefDuration.Tag;
            if (prefs != null)
            {
                PrefDuration prefDuration = (PrefDuration)this.comboBoxPrefDuration.SelectedItem;
                prefs.PrefDuration = prefDuration;
                this.controller.SaveApplicationPrefs();
            }
        }

        private void getDisplaysLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel ll = (LinkLabel)sender;
            string url = (string) ll.Tag;
            OpenLink(url);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripForwardDestinations.Tag != null)
            {
                ForwardDestination fc = this.contextMenuStripForwardDestinations.Tag as ForwardDestination;
                if (fc != null)
                {
                    AddComputer f = new AddComputer(fc);
                    f.SetController(this.controller);
                    f.ShowDialog(this); 
                }
            }
            this.contextMenuStripForwardDestinations.Tag = null;
            this.contextMenuStripForwardDestinations.Hide();
        }

        private void editSubscriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripSubscriptions.Tag != null)
            {
                Subscription sub = this.contextMenuStripSubscriptions.Tag as Subscription;
                if (sub != null)
                {
                    AddComputer f = new AddComputer(sub);
                    f.SetController(this.controller);
                    f.ShowDialog(this);
                }
            }
            this.contextMenuStripSubscriptions.Tag = null;
            this.contextMenuStripSubscriptions.Hide();
        }

        private void unsubscribeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.contextMenuStripSubscriptions.Tag != null)
            {
                Subscription sub = this.contextMenuStripSubscriptions.Tag as Subscription;
                if (sub != null)
                {
                    this.controller.RemoveSubscription(sub);
                }
            }
            this.contextMenuStripSubscriptions.Tag = null;
            this.contextMenuStripSubscriptions.Hide();
        }

        private void subscribedListView_MouseDown(object sender, MouseEventArgs e)
        {
            this.contextMenuStripSubscriptions.Hide();

            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = this.subscribedListView.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag != null)
                {
                    Subscription sub = item.Tag as Subscription;
                    if (sub != null)
                    {
                        this.contextMenuStripSubscriptions.Tag = sub;
                        this.contextMenuStripSubscriptions.Show(this.subscribedListView, e.Location);
                    }
                }
            }
        }

        private void contextMenuStripApplications_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            this.listControlApplications.Refresh();
        }

        private void historyFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            this.historyListView.Filter = this.historyFilterTextBox.Text;
        }

        void passwordManagerControl1_Updated(object sender, EventArgs e)
        {
            this.controller.SavePasswordPrefs();
        }

        private void pictureBoxMultipleMonitors_MouseDown(object sender, MouseEventArgs e)
        {
            // clean up any previous items in the list
            this.contextMenuStripMultipleMonitors.Hide();
            if (this.contextMenuStripMultipleMonitors.Items != null)
            {
                while (this.contextMenuStripMultipleMonitors.Items.Count > 0)
                {
                    ToolStripItem tsi = this.contextMenuStripMultipleMonitors.Items[0];
                    this.contextMenuStripMultipleMonitors.Items.RemoveAt(0);
                    tsi.Click -= monitorItem_Click;
                    tsi.Click -= identifyItem_Click;
                    tsi.Dispose();
                }
            }
            this.contextMenuStripMultipleMonitors.Items.Clear();

            // show the options
            if(e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
            {
                Display d = (Display)this.pictureBoxMultipleMonitors.Tag;
                string selectedDeviceName = this.controller.GetPreferredDeviceForDisplay(d);

                // add items
                bool anyChecked = false;
                Screen[] screens = Screen.AllScreens;
                for (int i = 0; i < screens.Length; i++)
                {
                    Screen screen = screens[i];
                    string deviceName = Growl.DisplayStyle.MultipleMonitorHelper.GetDeviceName(screen);
                    string displayName = (screen.Primary ? Properties.Resources.Displays_MultiMonitor_Primary : String.Format("{0} {1}", Properties.Resources.Displays_MultiMonitor_Monitor, i + 1));
                    string text = String.Format("{0} - {1}", i + 1, displayName);
                    ToolStripMenuItem item = new ToolStripMenuItem(text);
                    item.Tag = deviceName;
                    item.Click += new EventHandler(monitorItem_Click);
                    item.Checked = (deviceName == selectedDeviceName);
                    if (item.Checked) anyChecked = true;
                    this.contextMenuStripMultipleMonitors.Items.Add(item);
                }

                /*
                // TODO: FAKE: add one fake monitor for testing
                ToolStripMenuItem fake = new ToolStripMenuItem("9 - Fake Monitor");
                fake.Tag = "FAKE";
                fake.Click += new EventHandler(monitorItem_Click);
                fake.Checked = ("FAKE" == selectedDeviceName);
                if (fake.Checked) anyChecked = true;
                this.contextMenuStripMultipleMonitors.Items.Add(fake);
                 * */

                // if no explicit setting was found, default to the primary monitor
                if (!anyChecked) ((ToolStripMenuItem)this.contextMenuStripMultipleMonitors.Items[0]).Checked = true;

                ToolStripSeparator sep = new ToolStripSeparator();
                this.contextMenuStripMultipleMonitors.Items.Add(sep);
                ToolStripMenuItem identifyItem = new ToolStripMenuItem(Properties.Resources.Displays_MultiMonitor_Identify);
                identifyItem.Click += new EventHandler(identifyItem_Click);
                this.contextMenuStripMultipleMonitors.Items.Add(identifyItem);

                this.contextMenuStripMultipleMonitors.Show(this.pictureBoxMultipleMonitors, e.Location);
            }
        }

        void identifyItem_Click(object sender, EventArgs e)
        {
            Screen[] screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                Screen screen = screens[i];
                MonitorIdentifier tf2 = new MonitorIdentifier();
                tf2.Show(screen, i + 1);
            }
        }

        void monitorItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            string selectedDeviceName = null;
            foreach (ToolStripItem tsi in this.contextMenuStripMultipleMonitors.Items)
            {
                ToolStripMenuItem tsmi = tsi as ToolStripMenuItem;
                if(tsmi != null)
                {
                    bool selected = (item == tsmi);
                    if (selected) selectedDeviceName = tsmi.Tag.ToString();
                    tsmi.Checked = selected;
                }
            }

            // notify the display of the user's choice and save the setting
            Display d = (Display)this.pictureBoxMultipleMonitors.Tag;
            this.controller.SetPreferredDeviceForDisplay(d, selectedDeviceName);
        }
    }
}