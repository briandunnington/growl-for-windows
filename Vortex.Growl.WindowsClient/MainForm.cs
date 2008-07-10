using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Vortex.Growl.AppBridge;
using Vortex.Growl.Framework;

namespace Vortex.Growl.WindowsClient
{
    public partial class MainForm : Form
    {
        // local variables
        private const string APPLICATION_AUTORUN_KEY = "GrowlClient";
        private delegate void RegistrationDelegate(ReceivedRegistration rr);
        private delegate void NotificationDelegate(ReceivedNotification rn);

        private AppBridge.AppBridge appBridge;
        private bool isAutoStart = false;
        private bool isRunning = false;
        private Display growlDefaultDisplay;
        private string defaultDisplayName;
        private Dictionary<string, Display> availableDisplays;
        private FileSystemWatcher displayStyleWatcher;
        private ListViewItem.ListViewSubItem applicationListViewDisplayItem;
        private ListViewItem.ListViewSubItem applicationListViewClickItem;
        private ListViewItem.ListViewSubItem notificationListViewDisplayItem;
        private ListViewItem.ListViewSubItem notificationListViewPriorityItem;
        private ListViewItem.ListViewSubItem notificationListViewStickyItem;

        // constructor
        public MainForm()
        {
            InitializeComponent();
        }

        // private methods
        private void InitializePreferences()
        {
            // FORM
            this.Size = Properties.Settings.Default.FormSize;
            this.Location = Properties.Settings.Default.FormLocation;


            // GENERAL TAB
            // start (default to running when launched - handled later in Form_Load)
            // auto start
            this.isAutoStart = Properties.Settings.Default.AutoStart;
            this.autoStartCheckbox.Checked = this.isAutoStart;
            // default display style
            LoadDisplays();
            try
            {
                this.defaultDisplayName = Properties.Settings.Default.DefaultDisplay;
                this.growlDefaultDisplay = this.availableDisplays[this.defaultDisplayName];
            }
            catch
            {
                this.growlDefaultDisplay = Display.Default;
                this.defaultDisplayName = Display.Default.Name;
            }
            InitializeDisplayStyleWatcher();
            BindDisplayList(this.defaultDisplayStyleComboBox, true);
            BindDisplayList(this.applicationDisplayComboBox, false);
            BindDisplayList(this.notificationDisplayComboBox, false);
            // logging enabled & child options
            // status item :: TODO: not in initial release, disabled
            // idle :: TODO: not in initial release, disabled
            // current version from assembly info
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);
            this.currentVersionInfoLabel.Text = f.FileVersion;
            this.currentAssemblyVersionLabel.Text = String.Format("(build: {0})", a.GetName().Version.ToString());
            // auto check for updates :: TODO: not in initial release, disabled - investigate ClickOnce

            // APPLICATIONS TAB
            // application & notification list
            BindApplicationList();
            BindPriorityList(this.notificationPriorityComboBox);
            BindStickyList(this.notificationStickyComboBox);
            AutoSizeListView(this.applicationListView, this.applicationListView.Columns[1]);
            AutoSizeListView(this.notificationsListView, this.notificationsListView.Columns[1]);
            // select first app and show notification list
            if (this.applicationListView.Items.Count > 0) this.applicationListView.Items[0].Selected = true;

            // DISPLAY TAB
            foreach (Display display in this.availableDisplays.Values)
            {
                this.displayOptionsListBox.Items.Add(display);
            }
            this.displayOptionsContainerPanel.Visible = false;

            // NETWORK TAB
            this.allowRemoteRegistrationCheckBox.Enabled = false;
            this.serverPasswordLabel.Enabled = false;
            this.serverPasswordTextBox.Enabled = false;
            this.autoEnableWebNotificationsCheckBox.Enabled = false;
            this.forwardNotificationsCheckBox.Checked = false;
            this.forwardToComputersListView.Enabled = false;
            this.addForwardButton.Enabled = false;
            this.removeComputerButton.Enabled = false;
            if (Properties.Settings.Default.ListenForNetworkNotifications)
            {
                this.listenForNotificationsCheckBox.Checked = true;
                this.allowRemoteRegistrationCheckBox.Enabled = true;
                this.serverPasswordLabel.Enabled = true;
                this.serverPasswordTextBox.Enabled = true;
            }
            this.allowRemoteRegistrationCheckBox.Checked = Properties.Settings.Default.AllowRemoteRegistration;
            this.serverPasswordTextBox.Text = Properties.Settings.Default.NetworkPassword;
            if (Properties.Settings.Default.AllowForwarding)
            {
                this.forwardNotificationsCheckBox.Checked = true;
                this.forwardToComputersListView.Enabled = true;
                this.addForwardButton.Enabled = true;
            }
            if (Properties.Settings.Default.ListenForWebNotifications)
            {
                this.listenForWebNotificationsCheckBox.Checked = true;
                this.autoEnableWebNotificationsCheckBox.Enabled = true;
            }
            this.autoEnableWebNotificationsCheckBox.Checked = Properties.Settings.Default.AutoEnableWebNotifications;
            BindForwardComputerList();
            AutoSizeListView(this.forwardToComputersListView, this.forwardToComputersListView.Columns[1]);

            // ABOUT TAB
            // load 'About' text :: TODO: make sure file exists
            this.aboutTextBox.Text = System.IO.File.ReadAllText(Application.StartupPath + @"\about.txt");

            // general last minute things
            UpdateDisplayRelatedControls();
            UpdatePriorityRelatedControls();
            UpdateStickyRelatedControls();

            // TEMPORARY - DISABLE FUNCTIONS THAT ARE NOT YET IMPLEMENTED
            //this.defaultDisplayStyleComboBox.Enabled = false;
            this.enableLoggingCheckBox.Enabled = false;
            this.loggingOptionsPanel.Enabled = false;
            this.statusItemCheckBox.Enabled = false;
            this.idleCheckBox.Enabled = false;
            this.autoCheckUpdatedCheckBox.Enabled = false;
        }

        internal void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            this.Activate();
        }

        internal void ExitApp()
        {
            // make sure to save any settings from an open settings panel
            if (this.displaySettingsPanel.Tag != null)
            {
                Vortex.Growl.DisplayStyle.SettingsPanelBase sp = (Vortex.Growl.DisplayStyle.SettingsPanelBase)this.displaySettingsPanel.Tag;
                sp.DeselectPanel();
            }

            // remember some form information for next time
            Properties.Settings.Default.FormSize = this.Size;
            Properties.Settings.Default.FormLocation = this.Location;

            // stop growl (which in turns persists all application data)
            this.appBridge.Stop();

            // save all other user settings
            Properties.Settings.Default.Save();

            // get rid of system tray icon
            this.notifyIcon.Dispose();
        }

        private void StartGrowl()
        {
            this.appBridge.Start();
            this.startStopButton.Text = "Stop Growl";
            this.runningStatusLabel.Text = "Growl is running";
            this.runningStatusLabel.ForeColor = Color.Green;
            this.isRunning = true;
        }

        private void StopGrowl()
        {
            this.appBridge.Stop();
            this.startStopButton.Text = "Start Growl";
            this.runningStatusLabel.Text = "Growl is stopped";
            this.runningStatusLabel.ForeColor = Color.Red;
            this.isRunning = false;
        }

        private void EnableAutoStart()
        {
            // add the application to the Registry so it will automatically run on startup
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            bool alreadyThere = (registryKey.GetValue(APPLICATION_AUTORUN_KEY) != null);
            if (!alreadyThere)
            {
                registryKey.SetValue(APPLICATION_AUTORUN_KEY, Application.ExecutablePath);
            }
            Properties.Settings.Default.AutoStart = true;
        }

        private void DisableAutoStart()
        {
            // remove the application from the Registry so it will not automatically run on startup
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            bool alreadyThere = (registryKey.GetValue(APPLICATION_AUTORUN_KEY) != null);
            if (alreadyThere)
            {
                registryKey.DeleteValue(APPLICATION_AUTORUN_KEY);
            }
            Properties.Settings.Default.AutoStart = false;
        }

        private void BindApplicationList()
        {
            this.applicationListView.Items.Clear();
            foreach (RegisteredApplication app in this.appBridge.RegisteredApplications.Values)
            {
                string[] data = new string[] { app.Preferences.Enabled.ToString(), app.Name, app.Preferences.Display.ToString(), app.Preferences.Click.ToString() };
                ListViewItem item = new ListViewItem(data);
                item.Name = app.Name;
                item.Checked = app.Preferences.Enabled;

                // remember the column position for each subitem (useful later)
                for (int c = 0; c < item.SubItems.Count; c++)
                {
                    item.SubItems[c].Tag = c;
                }

                this.applicationListView.Items.Add(item);
            }
        }

        private void BindForwardComputerList()
        {
            this.forwardToComputersListView.Items.Clear();
            foreach (ForwardComputer fc in this.appBridge.ForwardComputers.Values)
            {
                string[] data = new string[] { null, fc.Display, fc.Password };
                ListViewItem item = new ListViewItem(data);
                item.Checked = fc.Enabled;
                item.Name = fc.Description;
                this.forwardToComputersListView.Items.Add(item);
            }
        }

        private void BindDisplayList(ComboBox list)
        {
            BindDisplayList(list, false);
        }

        private void BindDisplayList(ComboBox list, bool omitDefault)
        {
            list.Items.Clear();
            if (!omitDefault) list.Items.Add(this.appBridge.DefaultDisplay);
            foreach (Display display in this.availableDisplays.Values)
            {
                list.Items.Add(display);
            }
            list.SelectedItem = growlDefaultDisplay;
        }

        private void BindPriorityList(ComboBox list)
        {
            PriorityWrapper[] items = PriorityWrapper.GetList();
            list.Items.Clear();
            list.Items.Add(PriorityWrapper.Default);
            foreach (PriorityWrapper pw in items)
            {
                list.Items.Add(pw);
            }
        }

        private void BindStickyList(ComboBox list)
        {
            StickyWrapper[] items = StickyWrapper.GetList();
            list.Items.Clear();
            list.Items.Add(StickyWrapper.Default);
            foreach (StickyWrapper sw in items)
            {
                list.Items.Add(sw);
            }
        }

        private void UpdateRegistrationList(ReceivedRegistration rr)
        {
            BindApplicationList();

            // display a notification if this is a new application registering for the first time
            if (rr.InitialRegistration)
            {
                rr.Show();

                /* cast rr as DisplayStyle.Notification */
                /*

                string baseUrl = null;
                string html = rr.GetHtml(out baseUrl);
                NotificationWindow nw = new NotificationWindow();
                nw.SetHtml(html, baseUrl);
                nw.Width = 350;
                nw.Height = 150;
                Screen screen = Screen.FromControl(this);
                int x = screen.WorkingArea.Right - nw.Width;
                int y = screen.WorkingArea.Bottom - nw.Height;
                nw.DesktopLocation = new Point(x, y);
                nw.Show();
                 * */
            }
        }

        private void UpdateNotificationList(ReceivedNotification rn)
        {
            rn.Show();

            /*
            string baseUrl = null;
            string html = rn.GetHtml(out baseUrl);
            NotificationWindow nw = new NotificationWindow();
            nw.Sticky = rn.Sticky;
            nw.SetHtml(html, baseUrl);
            nw.Width = 350;
            nw.Height = 150;
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - nw.Width;
            int y = screen.WorkingArea.Bottom - nw.Height;
            nw.DesktopLocation = new Point(x, y);
            nw.Show();
             * */
        }

        private string HandlePreferenceDisplay(bool pref)
        {
            return pref.ToString();
        }

        private string HandlePreferenceDisplay(DefaultablePreference pref)
        {
            if (pref == null) return DefaultablePreference.DEFAULT_DISPLAY_LABEL;
            else return pref.Name;
        }

        public void AddForwardComputer(ForwardComputer fc)
        {
            this.appBridge.ForwardComputers.Add(fc.Description, fc);
            BindForwardComputerList();
        }

        private void InitializeDisplayStyleWatcher()
        {
            /*
            displayStyleWatcher = new FileSystemWatcher(this.displayStylePath);
            displayStyleWatcher.IncludeSubdirectories = false;
            displayStyleWatcher.Changed += new FileSystemEventHandler(displayStyleWatcher_Changed);
            displayStyleWatcher.EnableRaisingEvents = true;
             * */
        }

        private void UpdateDisplayRelatedControls()
        {
            // resize any dropdowns or listview columns
            FitComboBoxToContents(this.applicationDisplayComboBox);
            this.applicationListView.Columns[2].Width = this.applicationDisplayComboBox.Width;
            FitComboBoxToContents(this.notificationDisplayComboBox);
            this.notificationsListView.Columns[2].Width = this.notificationDisplayComboBox.Width;

            AutoSizeListView(this.applicationListView, this.applicationListView.Columns[1]);
            AutoSizeListView(this.notificationsListView, this.notificationsListView.Columns[1]);
        }

        private void UpdatePriorityRelatedControls()
        {
            FitComboBoxToContents(this.notificationPriorityComboBox);
            this.notificationsListView.Columns[3].Width = this.notificationPriorityComboBox.Width;

            AutoSizeListView(this.notificationsListView, this.notificationsListView.Columns[1]);
        }

        private void UpdateStickyRelatedControls()
        {
            FitComboBoxToContents(this.notificationStickyComboBox);
            this.notificationsListView.Columns[4].Width = this.notificationStickyComboBox.Width;

            AutoSizeListView(this.notificationsListView, this.notificationsListView.Columns[1]);
        }

        private void LoadDisplays()
        {
            this.availableDisplays = this.appBridge.GetAvailableDisplayStyles();
        }

        private void UpdateApplicationDisplayPreference()
        {
            if (this.applicationListViewDisplayItem != null && this.applicationDisplayComboBox.Tag != null)
            {
                // get the info we need
                RegisteredApplication ra = (RegisteredApplication)this.applicationDisplayComboBox.Tag;

                // Set text of ListView item to match the ComboBox.
                this.applicationListViewDisplayItem.Text = this.applicationDisplayComboBox.Text;

                // Hide the ComboBox.
                this.applicationDisplayComboBox.Visible = false;
                this.applicationDisplayComboBox.Tag = null;

                // update the appBridge
                string s = this.applicationDisplayComboBox.SelectedItem.ToString();
                Display display = null;
                if (s == Display.DEFAULT_DISPLAY_LABEL)
                    display = Display.Default;
                else
                    display = this.availableDisplays[s];
                ra.Preferences.Display = display;
            }
        }

        private void UpdateNotificationDisplayPreference()
        {
            if (this.notificationListViewDisplayItem != null && this.notificationDisplayComboBox.Tag != null)
            {
                // get the info we need
                RegisteredNotification rn = (RegisteredNotification)this.notificationDisplayComboBox.Tag;

                // Set text of ListView item to match the ComboBox.
                this.notificationListViewDisplayItem.Text = this.notificationDisplayComboBox.Text;

                // Hide the ComboBox.
                this.notificationDisplayComboBox.Visible = false;
                this.notificationDisplayComboBox.Tag = null;

                // update the appBridge
                string s = this.notificationDisplayComboBox.SelectedItem.ToString();
                Display display = null;
                if (s == Display.DEFAULT_DISPLAY_LABEL)
                    display = Display.Default;
                else
                    display = this.availableDisplays[s];
                rn.Preferences.Display = display;
            }
        }

        private void UpdateNotificationPriorityPreference()
        {
            if (this.notificationListViewPriorityItem != null && this.notificationPriorityComboBox.Tag != null)
            {
                // get the info we need
                RegisteredNotification rn = (RegisteredNotification)this.notificationPriorityComboBox.Tag;

                // Set text of ListView item to match the ComboBox.
                this.notificationListViewPriorityItem.Text = this.notificationPriorityComboBox.Text;

                // Hide the ComboBox.
                this.notificationPriorityComboBox.Visible = false;
                this.notificationPriorityComboBox.Tag = null;

                // update the appBridge
                PriorityWrapper pw = (PriorityWrapper) this.notificationPriorityComboBox.SelectedItem;
                rn.Preferences.Priority = pw.Priority;
            }
        }

        private void UpdateNotificationStickyPreference()
        {
            if (this.notificationListViewStickyItem != null && this.notificationStickyComboBox.Tag != null)
            {
                // get the info we need
                RegisteredNotification rn = (RegisteredNotification)this.notificationStickyComboBox.Tag;

                // Set text of ListView item to match the ComboBox.
                this.notificationListViewStickyItem.Text = this.notificationStickyComboBox.Text;

                // Hide the ComboBox.
                this.notificationStickyComboBox.Visible = false;
                this.notificationStickyComboBox.Tag = null;

                // update the appBridge
                StickyWrapper sw = (StickyWrapper)this.notificationStickyComboBox.SelectedItem;
                rn.Preferences.Sticky = sw.Sticky;
            }
        }

        private void FitComboBoxToContents(ComboBox comboBox)
        {
            System.Drawing.Graphics g = comboBox.CreateGraphics();
            float maxWidth = 0f;
            foreach (object o in comboBox.Items)
            {
                float w = g.MeasureString(o.ToString(), comboBox.Font).Width;
                if (w > maxWidth)
                    maxWidth = w;
            }
            g.Dispose();
            comboBox.Width = (int)maxWidth + 20; // 20 is to take care of button width 
        }

        private void AutoSizeListView(ListView listView, ColumnHeader dynamicColumn)
        {
            int w = listView.ClientSize.Width;
            foreach (ColumnHeader c in listView.Columns)
            {
                if (c != dynamicColumn)
                {
                    w -= c.Width;
                }
            }
            dynamicColumn.Width = w;
        }

        /*
         * ------------------------------------------------------------------------------------
         */

        // form event handlers
        private void MainForm_Load(object sender, EventArgs e)
        {
            // link to the application bridge first
            this.appBridge = BridgeFactory.GetAppBridge();

            // initialize preferences & UI
            InitializePreferences();

            // configure application bridge
            this.appBridge.RegistrationReceived += new AppBridge.AppBridge.RegistrationHandler(appBridge_RegistrationReceived);
            this.appBridge.NotificationReceived += new AppBridge.AppBridge.NotificationHandler(appBridge_NotificationReceived);
            this.appBridge.DefaultDisplay = this.growlDefaultDisplay;
            this.appBridge.SetNetworkPassword(this.serverPasswordTextBox.Text);

            // start Growl
            StartGrowl();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // remember some form information for next time
            Properties.Settings.Default.FormSize = this.Size;
            Properties.Settings.Default.FormLocation = this.Location;

            // instead of closing, just hide the form (minimized to system tray)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                ExitApp();
            }
        }

        private void displayStyleWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadDisplays();
            BindDisplayList(this.defaultDisplayStyleComboBox, true);
            BindDisplayList(this.applicationDisplayComboBox, false);
            BindDisplayList(this.notificationDisplayComboBox, false);
            // add more here
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void contextMenuItemSettings_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void contextMenuItemQuit_Click(object sender, EventArgs e)
        {
            // exit
            Application.Exit();
        }

        private void appBridge_RegistrationReceived(ReceivedRegistration rr)
        {
            if (InvokeRequired)
            {
                RegistrationDelegate del = new RegistrationDelegate(UpdateRegistrationList);
                this.BeginInvoke(del, rr);
            }
            else
            {
                UpdateRegistrationList(rr);
            }
        }

        private void appBridge_NotificationReceived(ReceivedNotification rn)
        {
            if (InvokeRequired)
            {
                NotificationDelegate del = new NotificationDelegate(UpdateNotificationList);
                this.BeginInvoke(del, rn);
            }
            else
            {
                UpdateNotificationList(rn);
            }
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (this.isRunning)
            {
                StopGrowl();
            }
            else
            {
                StartGrowl();
            }
        }

        private void autoStartCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.autoStartCheckbox.Checked)
            {
                DisableAutoStart();
            }
            else
            {
                EnableAutoStart();
            }
            this.isAutoStart = this.autoStartCheckbox.Checked;
        }

        private void listenForNotificationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.listenForNotificationsCheckBox.Checked)
            {
                this.allowRemoteRegistrationCheckBox.Enabled = true;
                this.serverPasswordTextBox.Enabled = true;
                this.serverPasswordLabel.Enabled = true;
                this.appBridge.ListenForNetworkNotifications = true;
            }
            else
            {
                this.allowRemoteRegistrationCheckBox.Enabled = false;
                this.serverPasswordTextBox.Enabled = false;
                this.serverPasswordLabel.Enabled = false;
                this.appBridge.ListenForNetworkNotifications = false;
            }
            Properties.Settings.Default.ListenForNetworkNotifications = this.listenForNotificationsCheckBox.Checked;
        }

        private void allowRemoteRegistrationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.allowRemoteRegistrationCheckBox.Checked)
            {
                this.appBridge.AllowRemoteRegistration = true;
            }
            else
            {
                this.appBridge.AllowRemoteRegistration = false;
            }
            Properties.Settings.Default.AllowRemoteRegistration = this.allowRemoteRegistrationCheckBox.Checked;
        }

        private void serverPasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.NetworkPassword = this.serverPasswordTextBox.Text;
            this.appBridge.SetNetworkPassword(this.serverPasswordTextBox.Text);
        }

        private void applicationListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                this.notificationsListView.Items.Clear();
                RegisteredApplication ra = this.appBridge.RegisteredApplications[e.Item.Name];
                foreach (RegisteredNotification rn in ra.Notifications.Values)
                {
                    string[] data = new string[] { HandlePreferenceDisplay(rn.Preferences.Enabled), rn.Name, HandlePreferenceDisplay(rn.Preferences.Display), HandlePreferenceDisplay(PriorityWrapper.GetByValue(rn.Preferences.Priority)), HandlePreferenceDisplay(StickyWrapper.GetByValue(rn.Preferences.Sticky)) };
                    ListViewItem item = new ListViewItem(data);
                    item.Name = rn.Name;
                    item.Checked = rn.Preferences.Enabled;
                    item.Tag = rn;

                    // remember the column position for each subitem (useful later)
                    for (int c = 0; c < item.SubItems.Count; c++)
                    {
                        item.SubItems[c].Tag = c;
                    }

                    this.notificationsListView.Items.Add(item);
                }

                e.Item.BackColor = Color.Gainsboro;
            }
            else
            {
                e.Item.BackColor = Color.Transparent;
                this.notificationsListView.Items.Clear();
            }
        }

        private void AddForwardButton_Click(object sender, EventArgs e)
        {
            AddForward form = new AddForward();
            form.Location = new Point(this.Location.X + 50, this.Location.Y + 150);
            form.Show(this);
        }

        private void forwardNotificationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.forwardNotificationsCheckBox.Checked)
            {
                this.forwardToComputersListView.Enabled = true;
                this.addForwardButton.Enabled = true;
                this.appBridge.DoForwarding = true;
            }
            else
            {
                this.forwardToComputersListView.Enabled = false;
                this.addForwardButton.Enabled = false;
                this.appBridge.DoForwarding = false;
            }
            Properties.Settings.Default.AllowForwarding = this.forwardNotificationsCheckBox.Checked;
        }

        private void forwardToComputersListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                this.removeComputerButton.Enabled = true;
            }
            else
            {
                this.removeComputerButton.Enabled = false;
            }
        }

        private void removeComputerButton_Click(object sender, EventArgs e)
        {
            this.appBridge.ForwardComputers.Remove(this.forwardToComputersListView.SelectedItems[0].Name);
            BindForwardComputerList();
        }

        private void forwardToComputersListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.appBridge.ForwardComputers[e.Item.Name].Enabled = e.Item.Checked;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            AutoSizeListView(this.applicationListView, this.applicationListView.Columns[1]);
            AutoSizeListView(this.forwardToComputersListView, this.forwardToComputersListView.Columns[1]);
            AutoSizeListView(this.notificationsListView, this.notificationsListView.Columns[1]);
        }

        private void applicationListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.appBridge.RegisteredApplications[e.Item.Name].Preferences.Enabled = e.Item.Checked;
            e.Item.SubItems[0].Text = e.Item.Checked.ToString();
        }

        private void notificationsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            RegisteredNotification rn = (RegisteredNotification) e.Item.Tag;
            rn.Preferences.Enabled = e.Item.Checked;
            e.Item.SubItems[0].Text = e.Item.Checked.ToString();
        }

        private void defaultDisplayStyleComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string s = this.defaultDisplayStyleComboBox.SelectedItem.ToString();
            this.growlDefaultDisplay = this.availableDisplays[s];
            this.appBridge.DefaultDisplay = this.growlDefaultDisplay;
            Properties.Settings.Default.DefaultDisplay = s;
        }

        private void applicationDisplayComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateApplicationDisplayPreference();
        }

        private void applicationDisplayComboBox_Leave(object sender, EventArgs e)
        {
            UpdateApplicationDisplayPreference();
        }

        private void applicationDisplayComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.applicationListViewDisplayItem != null)
            {
                // Verify that the user presses ESC.
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // Reset the original text value, and then hide the ComboBox.
                            this.applicationDisplayComboBox.Text = this.applicationListViewDisplayItem.Text;
                            this.applicationDisplayComboBox.Visible = false;
                            break;
                        }

                    case (char)(int)Keys.Enter:
                        {
                            UpdateApplicationDisplayPreference();
                            break;
                        }
                }
            }
        }

        private void applicationListView_MouseUp(object sender, MouseEventArgs e)
        {
            const int displayColumn = 2;
            const int clickColumn = 3;

            // Get the item on the row that is clicked.
            ListViewItem item = this.applicationListView.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                ListViewItem.ListViewSubItem subItem = item.GetSubItemAt(e.X, e.Y);

                if (subItem != null && subItem.Tag != null)
                {
                    int column = Convert.ToInt32(subItem.Tag);
                    switch (column)
                    {
                        case displayColumn :
                            this.applicationListViewDisplayItem = subItem;

                            // position the combobox (remember that the subItem is relative to the listview, so we need to add those together)
                            this.applicationDisplayComboBox.Bounds = this.applicationListViewDisplayItem.Bounds;
                            this.applicationDisplayComboBox.Top += this.applicationListView.Top;
                            this.applicationDisplayComboBox.Left += this.applicationListView.Left;

                            // Set default text for ComboBox to match the item that is clicked.
                            this.applicationDisplayComboBox.Text = this.applicationListViewDisplayItem.Text;

                            // Display the ComboBox, and make sure that it is on top with focus.
                            this.applicationDisplayComboBox.Visible = true;
                            this.applicationDisplayComboBox.BringToFront();
                            this.applicationDisplayComboBox.Focus();

                            // remember which item we are dealing with
                            this.applicationDisplayComboBox.Tag = this.appBridge.RegisteredApplications[item.Name];

                            break;
                        case clickColumn :
                            this.applicationListViewClickItem = subItem;
                            break;
                    }
                }
            }
        }

        private void notificationDisplayComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateNotificationDisplayPreference();
        }

        private void notificationDisplayComboBox_Leave(object sender, EventArgs e)
        {
            UpdateNotificationDisplayPreference();
        }

        private void notificationDisplayComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.notificationListViewDisplayItem != null)
            {
                // Verify that the user presses ESC.
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // Reset the original text value, and then hide the ComboBox.
                            this.notificationDisplayComboBox.Text = this.notificationListViewDisplayItem.Text;
                            this.notificationDisplayComboBox.Visible = false;
                            break;
                        }

                    case (char)(int)Keys.Enter:
                        {
                            UpdateNotificationDisplayPreference();
                            break;
                        }
                }
            }
        }

        private void notificationsListView_MouseUp(object sender, MouseEventArgs e)
        {
            const int displayColumn = 2;
            const int priorityColumn = 3;
            const int stickyColumn = 4;

            // Get the item on the row that is clicked.
            ListViewItem item = this.notificationsListView.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                ListViewItem.ListViewSubItem subItem = item.GetSubItemAt(e.X, e.Y);

                if (subItem != null && subItem.Tag != null)
                {
                    int column = Convert.ToInt32(subItem.Tag);
                    switch (column)
                    {
                        case displayColumn:
                            this.notificationListViewDisplayItem = subItem;

                            // position the combobox (remember that the subItem is relative to the listview, so we need to add those together)
                            this.notificationDisplayComboBox.Bounds = this.notificationListViewDisplayItem.Bounds;
                            this.notificationDisplayComboBox.Top += this.notificationsListView.Top;
                            this.notificationDisplayComboBox.Left += this.notificationsListView.Left;

                            // Set default text for ComboBox to match the item that is clicked.
                            this.notificationDisplayComboBox.Text = this.notificationListViewDisplayItem.Text;

                            // Display the ComboBox, and make sure that it is on top with focus.
                            this.notificationDisplayComboBox.Visible = true;
                            this.notificationDisplayComboBox.BringToFront();
                            this.notificationDisplayComboBox.Focus();

                            // remember which item we are dealing with
                            this.notificationDisplayComboBox.Tag = item.Tag;

                            break;
                        case priorityColumn:
                            this.notificationListViewPriorityItem = subItem;

                            // position the combobox (remember that the subItem is relative to the listview, so we need to add those together)
                            this.notificationPriorityComboBox.Bounds = this.notificationListViewPriorityItem.Bounds;
                            this.notificationPriorityComboBox.Top += this.notificationsListView.Top;
                            this.notificationPriorityComboBox.Left += this.notificationsListView.Left;

                            // Set default text for ComboBox to match the item that is clicked.
                            this.notificationPriorityComboBox.SelectedItem = PriorityWrapper.GetByName(this.notificationListViewPriorityItem.Text);

                            // Display the ComboBox, and make sure that it is on top with focus.
                            this.notificationPriorityComboBox.Visible = true;
                            this.notificationPriorityComboBox.BringToFront();
                            this.notificationPriorityComboBox.Focus();

                            // remember which item we are dealing with
                            this.notificationPriorityComboBox.Tag = item.Tag;

                            break;
                        case stickyColumn:
                            this.notificationListViewStickyItem = subItem;

                            // position the combobox (remember that the subItem is relative to the listview, so we need to add those together)
                            this.notificationStickyComboBox.Bounds = this.notificationListViewStickyItem.Bounds;
                            this.notificationStickyComboBox.Top += this.notificationsListView.Top;
                            this.notificationStickyComboBox.Left += this.notificationsListView.Left;

                            // Set default text for ComboBox to match the item that is clicked.
                            this.notificationStickyComboBox.SelectedItem = StickyWrapper.GetByName(this.notificationListViewStickyItem.Text);

                            // Display the ComboBox, and make sure that it is on top with focus.
                            this.notificationStickyComboBox.Visible = true;
                            this.notificationStickyComboBox.BringToFront();
                            this.notificationStickyComboBox.Focus();

                            // remember which item we are dealing with
                            this.notificationStickyComboBox.Tag = item.Tag;

                            break;
                    }
                }
            }
        }

        private void notificationPriorityComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateNotificationPriorityPreference();
        }

        private void notificationPriorityComboBox_Leave(object sender, EventArgs e)
        {
            UpdateNotificationPriorityPreference();
        }

        private void notificationPriorityComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.notificationListViewPriorityItem != null)
            {
                // Verify that the user presses ESC.
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // Reset the original text value, and then hide the ComboBox.
                            this.notificationPriorityComboBox.Text = this.notificationListViewPriorityItem.Text;
                            this.notificationPriorityComboBox.Visible = false;
                            break;
                        }

                    case (char)(int)Keys.Enter:
                        {
                            UpdateNotificationPriorityPreference();
                            break;
                        }
                }
            }
        }

        private void notificationStickyComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateNotificationStickyPreference();
        }

        private void notificationStickyComboBox_Leave(object sender, EventArgs e)
        {
            UpdateNotificationStickyPreference();
        }

        private void notificationStickyComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.notificationListViewStickyItem != null)
            {
                // Verify that the user presses ESC.
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // Reset the original text value, and then hide the ComboBox.
                            this.notificationStickyComboBox.Text = this.notificationListViewStickyItem.Text;
                            this.notificationStickyComboBox.Visible = false;
                            break;
                        }

                    case (char)(int)Keys.Enter:
                        {
                            UpdateNotificationStickyPreference();
                            break;
                        }
                }
            }
        }

        private void displayOptionsListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            // hide any current panel
            if (this.displayOptionsContainerPanel.Tag != null && this.displayOptionsContainerPanel.Tag is Vortex.Growl.DisplayStyle.SettingsPanelBase)
            {
                Vortex.Growl.DisplayStyle.SettingsPanelBase sp = (Vortex.Growl.DisplayStyle.SettingsPanelBase)this.displayOptionsContainerPanel.Tag;
                sp.DeselectPanel();
            }
            this.displayOptionsContainerPanel.Tag = null;
            this.displaySettingsPanel.Controls.Clear();
            this.displayStyleNameLabel.Text = "";
            this.displayStyleDescriptionLabel.Text = "";
            this.displayStyleAuthorLabel.Text = "";
            this.displayStyleVersionLabel.Text = "";

            // show the new panel
            Display display = (Display) this.displayOptionsListBox.SelectedItem;
            Vortex.Growl.DisplayStyle.SettingsPanelBase panel = display.SettingsPanel;
            if (panel != null)
            {
                panel.Dock = DockStyle.Fill;
                panel.Display = display;
                panel.SettingsChanged -= new EventHandler(panel_SettingsChanged);
                panel.SettingsChanged += new EventHandler(panel_SettingsChanged);
                panel.SelectPanel();
                this.displaySettingsPanel.Controls.Add(panel);
                this.displayOptionsContainerPanel.Tag = panel;

                this.displayStyleNameLabel.Text = display.Name;
                this.displayStyleDescriptionLabel.Text = display.Description;
                this.displayStyleAuthorLabel.Text = display.Author;
                this.displayStyleVersionLabel.Text = display.Version;

                this.displayOptionsContainerPanel.Visible = true;
            }
        }

        void panel_SettingsChanged(object sender, EventArgs e)
        {
            Vortex.Growl.DisplayStyle.SettingsPanelBase sp = (Vortex.Growl.DisplayStyle.SettingsPanelBase)sender;
            Display d = (Display)sp.Display;
            d.SettingsCollection = sp.GetSettings();
        }

        private void listenForWebNotificationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.appBridge.ListenForWebNotifications = this.listenForWebNotificationsCheckBox.Checked;
            this.autoEnableWebNotificationsCheckBox.Enabled = this.listenForWebNotificationsCheckBox.Checked;

            Properties.Settings.Default.ListenForWebNotifications = this.listenForWebNotificationsCheckBox.Checked;
        }

        private void autoEnableWebNotificationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.appBridge.AutoEnableWebNotifications = this.autoEnableWebNotificationsCheckBox.Checked;
            Properties.Settings.Default.AutoEnableWebNotifications = this.autoEnableWebNotificationsCheckBox.Checked;
        }

        private void previewDisplayButton_Click(object sender, EventArgs e)
        {
            panel_SettingsChanged(this.displayOptionsContainerPanel.Tag, EventArgs.Empty);
            Display display = (Display)this.displayOptionsListBox.SelectedItem;
            this.appBridge.PreviewDisplay(display);
        }
    }
}