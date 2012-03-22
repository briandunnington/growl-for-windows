using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Growl.AutoUpdate;

namespace Growl
{
    public class Program : ApplicationContext, System.ComponentModel.ISynchronizeInvoke, IDisposable
    {
        //public static System.Threading.ManualResetEvent ProgramLoadedResetEvent = new System.Threading.ManualResetEvent(false);
        public static System.Threading.AutoResetEvent ProgramLoadedResetEvent = new System.Threading.AutoResetEvent(false);

        public event EventHandler ProgramRunning;

        private bool running;
        private Controller controller;
        private WndProcReader wpr;

        private MainForm mainForm;
        private Updater updater;
        private Timer autoUpdateTimer = new Timer();
        private UpdateForm updateForm;

        private Keys closeLastKeyCombo;
        private Keys closeAllKeyCombo;
        private HotKeyManager closeLastHotKey;
        private HotKeyManager closeAllHotKey;

        private Icon iconRunning = Growl.Properties.Resources.growl_on;
        private Icon iconPaused = Growl.Properties.Resources.growl_dim;
        private Icon iconStopped = Growl.Properties.Resources.growl_stopped;

        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseGrowlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unpauseGrowlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem muteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unmuteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

        public Program() : base()
        {
            Utility.WriteDebugInfo(String.Format("PROGRAM FOLDER: {0}", Application.StartupPath));
            Utility.WriteDebugInfo(String.Format("USER FOLDER: {0}", Utility.UserSettingFolder));   //NOTE: this is here as a bit of hack to ensure that the Utility static constructor runs first
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseGrowlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unpauseGrowlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.muteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unmuteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.contextMenu.SuspendLayout();

            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = iconStopped;
            this.notifyIcon.Text = Properties.Resources.NotifyIcon_NotRunning;
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.pauseGrowlToolStripMenuItem,
            this.unpauseGrowlToolStripMenuItem,
            this.muteToolStripMenuItem,
            this.unmuteToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Size = new System.Drawing.Size(149, 142);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.settingsToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Settings;
            this.settingsToolStripMenuItem.Font = new System.Drawing.Font(this.settingsToolStripMenuItem.Font, FontStyle.Bold);
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // pauseGrowlToolStripMenuItem
            // 
            this.pauseGrowlToolStripMenuItem.Name = "pauseGrowlToolStripMenuItem";
            this.pauseGrowlToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.pauseGrowlToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Pause;
            this.pauseGrowlToolStripMenuItem.Visible = false;
            this.pauseGrowlToolStripMenuItem.Click += new System.EventHandler(this.pauseGrowlToolStripMenuItem_Click);
            // 
            // unpauseGrowlToolStripMenuItem
            // 
            this.unpauseGrowlToolStripMenuItem.Name = "unpauseGrowlToolStripMenuItem";
            this.unpauseGrowlToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.unpauseGrowlToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Unpause;
            this.unpauseGrowlToolStripMenuItem.Visible = false;
            this.unpauseGrowlToolStripMenuItem.Click += new System.EventHandler(this.unpauseGrowlToolStripMenuItem_Click);
            // 
            // muteToolStripMenuItem
            // 
            this.muteToolStripMenuItem.Name = "muteToolStripMenuItem";
            this.muteToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.muteToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Mute;
            this.muteToolStripMenuItem.Visible = false;
            this.muteToolStripMenuItem.Click += new System.EventHandler(this.muteToolStripMenuItem_Click);
            // 
            // unmuteToolStripMenuItem
            // 
            this.unmuteToolStripMenuItem.Name = "unmuteToolStripMenuItem";
            this.unmuteToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.unmuteToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Unmute;
            this.unmuteToolStripMenuItem.Visible = false;
            this.unmuteToolStripMenuItem.Click += new System.EventHandler(this.unmuteToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.checkForUpdatesToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_CheckForUpdates;
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(129, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.exitToolStripMenuItem.Text = Properties.Resources.NotifyIcon_ContextMenu_Exit;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

            this.contextMenu.ResumeLayout();
        }

        public void Run()
        {
            if (!this.running)
            {
                this.running = true;

                InitializeComponent();

                // SUPER IMPORTANT - since we are using the contextMenu as the synchronizing object, its .Handle must be created first
                // this will force creation of the handle without showing the menu
                Utility.WriteDebugInfo("ContextMenu Handle: {0}", this.contextMenu.Handle);

                // scaling factor
                float currentDPI = 0;
                using (Graphics g = this.contextMenu.CreateGraphics())
                {
                    currentDPI = g.DpiX;
                }
                ApplicationMain.ScalingFactor = currentDPI / 96;

                // configure the controller
                this.controller = Controller.GetController();
                this.controller.FailedToStart += new EventHandler<PortConflictEventArgs>(controller_FailedToStart);
                this.controller.FailedToStartUDPLegacy += new EventHandler<PortConflictEventArgs>(controller_FailedToStartUDPLegacy);
                this.controller.Initialize(Application.ExecutablePath, this.contextMenu);

                this.wpr = new WndProcReader(WndProc);

                StartGrowl();

                OnProgramRunning(this, EventArgs.Empty);

                HandleSystemNotifications();

                // signal the ProgramLoadedResetEvent so anybody that was waiting on it can start their work
                ProgramLoadedResetEvent.Set();

                // Normally we shouldnt ever explicitly call GC.Collect(), but since many of the prefs
                // are read from files on disk, they usually end up on the Large Object Heap. LOH objects
                // only get collected in Gen2 collections, so we want to do this here before the app
                // really gets going to clean up our initialization process.
                Utility.WriteDebugInfo("Program loaded. Force GC to clean up LOH");
                ApplicationMain.ForceGC();

                // auto updater
                this.updater = new Updater(Application.StartupPath);
                this.updater.CheckForUpdateComplete += new CheckForUpdateCompleteEventHandler(updater_CheckForUpdateComplete);
                this.autoUpdateTimer.Interval = 20 * 1000;
                this.autoUpdateTimer.Tick += new EventHandler(autoUpdateTimer_Tick);
                this.autoUpdateTimer.Start();

                // while it may seem strange to call the Detector to see if we are installed, it is the best way
                // to ensure that plugins/displays see the same values that we do
                Growl.CoreLibrary.Detector detector = new Growl.CoreLibrary.Detector();
                if (!detector.IsInstalled)
                {
                    // something is not right with the registry setting, so lets fix it now
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY, true);
                    if (key == null)
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY);
                    using (key)
                    {
                        key.SetValue(null, Application.ExecutablePath);
                    }
                }
            }
        }

        private void OnProgramRunning(object sender, EventArgs e)
        {
            if (this.ProgramRunning != null)
            {
                this.ProgramRunning(sender, e);
            }
        }

        public void ShowForm()
        {
            if (this.mainForm != null && this.mainForm.Visible)
            {
                // do nothing for now
            }
            else
            {
                this.mainForm = new MainForm();
                this.mainForm.FormClosed += new FormClosedEventHandler(mainForm_FormClosed);
                this.mainForm.InitializePreferences();

                this.mainForm.OnOffButton.On = this.controller.IsOn;
                this.mainForm.OnOffButton.Switched += new Growl.UI.OnOffSwitchedEventHandler(OnOffButton_Switched);

                UpdateState(this.controller.IsOn, !this.controller.IsRunning);
                this.mainForm.DoneInitializing();

                this.mainForm.ShowForm();
            }

            Growl.DisplayStyle.Win32.SetForegroundWindow(this.mainForm.Handle);
        }

        private void KillForm(object obj)
        {
            if (mainForm != null)
            {
                this.mainForm.FormClosed -= new FormClosedEventHandler(mainForm_FormClosed);
                this.mainForm.OnOffButton.Switched -= new Growl.UI.OnOffSwitchedEventHandler(OnOffButton_Switched);

                this.mainForm.Dispose();
                this.mainForm = null;

                // Normally we shouldnt ever explicitly call GC.Collect(), but since the form can use a 
                // relatively large amount of memory for the UI components, we want to make sure that is
                // all freed when the form is closed.
                Utility.WriteDebugInfo("Form closed. Force GC to clean up LOH");
                ApplicationMain.ForceGC();
            }
        }

        void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(KillForm));
        }

        void OnOffButton_Switched(Growl.UI.OnOffSwitchedEventArgs args)
        {
            if (this.mainForm.OnOffButton.On)
            {
                StopGrowl();
            }
            else
            {
                bool started = StartGrowl();
                args.Cancel = !started;
            }
        }

        internal void ExitApp()
        {
            this.StopGrowl();

            // save all user settings
            Properties.Settings.Default.Save();

            // make sure notifyIcon is removed
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Visible = false;
                this.notifyIcon.Dispose();
            }

            // kill the app
            Application.Exit();
        }

        internal void AlreadyRunning(int signalFlag, int signalValue)
        {
            ApplicationMain.Signal signal = (ApplicationMain.Signal)signalFlag;
            bool silent = ((signal & ApplicationMain.Signal.Silent) == ApplicationMain.Signal.Silent);
            bool reloadDisplays = ((signal & ApplicationMain.Signal.ReloadDisplays) == ApplicationMain.Signal.ReloadDisplays);
            bool updateLanguage = ((signal & ApplicationMain.Signal.UpdateLanguage) == ApplicationMain.Signal.UpdateLanguage);
            bool handleListenUrl = ((signal & ApplicationMain.Signal.HandleListenUrl) == ApplicationMain.Signal.HandleListenUrl);
            bool reloadForwarders = ((signal & ApplicationMain.Signal.ReloadForwarders) == ApplicationMain.Signal.ReloadForwarders);
            bool reloadSubscribers = ((signal & ApplicationMain.Signal.ReloadSubscribers) == ApplicationMain.Signal.ReloadSubscribers);
            bool showSettings = ((signal & ApplicationMain.Signal.ShowSettings) == ApplicationMain.Signal.ShowSettings);

            if(!silent && this.controller != null) 
                this.controller.SendSystemNotification(Properties.Resources.SystemNotification_Running_Title, Properties.Resources.SystemNotification_Running_Text, null);

            if(reloadDisplays)
            {
                DisplayStyleManager.DiscoverNewDisplayPlugins();
                //if(this.mainForm != null) this.mainForm.BindDisplayList();
            }

            if(updateLanguage)
            {
                if (signalValue == 0)
                    Properties.Settings.Default.CultureCode = "";
                else
                {
                    // read each subfolder in the app folder and find the one with the matching hash
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.StartupPath);
                    foreach (System.IO.DirectoryInfo directory in di.GetDirectories())
                    {
                        if (directory.Name.GetHashCode() == signalValue)
                        {
                            Properties.Settings.Default.CultureCode = directory.Name;
                            break;
                        }
                    }
                }
            }

            if (handleListenUrl)
            {
                HandleListenUrls();
            }

            if (reloadForwarders)
            {
                ForwardDestinationManager.DiscoverNewPlugins();

                /* NOTE: the signalValue should be the hash of the plugin folder that was loaded,
                 * so we could loop through the folders looking for a matching has (similar to how
                 * the updateLanguage section works above), but the DiscoverNewPlugins already
                 * loops through the folders anyway, so we can just let it do its thing. */
            }

            if (reloadSubscribers)
            {
                SubscriptionManager.DiscoverNewPlugins();

                /* NOTE: the signalValue should be the hash of the plugin folder that was loaded,
                 * so we could loop through the folders looking for a matching has (similar to how
                 * the updateLanguage section works above), but the DiscoverNewPlugins already
                 * loops through the folders anyway, so we can just let it do its thing. */
            }

            if (showSettings)
            {
                this.ShowForm();
            }
        }

        private bool StartGrowl()
        {
            bool started = this.controller.Start();

            RegisterHotKeys();

            UpdateState(started, false);
            Mute(Properties.Settings.Default.MuteAllSounds);

            return started;
        }

        private void StopGrowl()
        {
            UpdateState(false, false);

            this.controller.Stop();

            UnregisterHotKeys();
        }

        private void UpdateState(bool isRunning, bool isPaused)
        {
            this.pauseGrowlToolStripMenuItem.Visible = (isRunning && !isPaused);
            this.unpauseGrowlToolStripMenuItem.Visible = (isRunning && isPaused);

            if (!isRunning)
            {
                if (this.mainForm != null) this.mainForm.UpdateState(Properties.Resources.General_ApplicationStopped, Color.Red);
                this.notifyIcon.Text = Properties.Resources.NotifyIcon_NotRunning;
                this.notifyIcon.Icon = iconStopped;
            }
            else if (isPaused)
            {
                if (this.mainForm != null) this.mainForm.UpdateState(Properties.Resources.General_ApplicationPaused, Color.FromArgb(SystemColors.GrayText.ToArgb()));  // avoid using SystemColors directly
                this.notifyIcon.Text = Properties.Resources.NotifyIcon_Paused;
                this.notifyIcon.Icon = iconPaused;
                if (this.controller != null) this.controller.Pause();
            }
            else
            {
                if (this.mainForm != null) this.mainForm.UpdateState(Properties.Resources.General_ApplicationRunning, Color.Green);
                this.notifyIcon.Text = Properties.Resources.NotifyIcon_Running;
                this.notifyIcon.Icon = iconRunning;
                if (this.controller != null) this.controller.Unpause();
            }
        }

        public void RegisterHotKeys()
        {
            try
            {
                KeysConverter kc = new KeysConverter();

                if (this.closeLastHotKey == null)
                {
                    this.closeLastKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseLast);
                    this.closeLastHotKey = new HotKeyManager(this.wpr.Handle, this.closeLastKeyCombo);
                    this.closeLastHotKey.Register();
                }

                if (this.closeAllHotKey == null)
                {
                    this.closeAllKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseAll);
                    this.closeAllHotKey = new HotKeyManager(this.wpr.Handle, this.closeAllKeyCombo);
                    this.closeAllHotKey.Register();
                }
            }
            catch
            {
            }
        }

        public void UnregisterHotKeys()
        {
            if (this.closeLastHotKey != null)
            {
                this.closeLastHotKey.Dispose();
                this.closeLastHotKey = null;
            }
            if (this.closeAllHotKey != null)
            {
                this.closeAllHotKey.Dispose();
                this.closeAllHotKey = null;
            }
        }

        void controller_FailedToStartUDPLegacy(object sender, PortConflictEventArgs e)
        {
            string caption = Properties.Resources.FailedToStart_Caption;
            string text = String.Format(Properties.Resources.FailedToStart_Message_UDP, e.Port);
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0, false);
        }

        void controller_FailedToStart(object sender, PortConflictEventArgs e)
        {
            string caption = Properties.Resources.FailedToStart_Caption;
            string text = String.Format(Properties.Resources.FailedToStart_Message_GNTP, e.Port);
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0, false);
        }

        void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void pauseGrowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pause(true);
        }

        private void unpauseGrowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pause(false);
        }

        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mute(true);
        }

        private void unmuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mute(false);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.updater.CheckForUpdate(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        void autoUpdateTimer_Tick(object sender, EventArgs e)
        {
            this.autoUpdateTimer.Stop();

            // check less frequently after initial startup
            this.autoUpdateTimer.Interval = 24 * 60 * 60 * 1000; // 24 hours

            if (Properties.Settings.Default.AutomaticallyCheckForUpdates)
            {
                this.updater.CheckForUpdate(false);
            }
        }

        void updater_CheckForUpdateComplete(Updater sender, CheckForUpdateCompleteEventArgs args)
        {
            // make sure any exisiting form is closed first (we dont want to re-use it directly because it might get
            // disposed out from under us).
            if (this.updateForm != null)
            {
                try
                {
                    this.updateForm.Close();
                    this.updateForm.Dispose();
                    this.updateForm = null;
                }
                finally
                {
                    // this is just in case the form was closed but not disposed of properly
                }
            }

            // if there is an update available or if the user asked specifically, show the update form
            if (args.UpdateAvailable || args.UserInitiated)
            {
                this.updateForm = new UpdateForm(this.updater);
                this.updateForm.LaunchUpdater(args.Manifest, args.UpdateAvailable, args.ErrorArgs);
            }
            this.autoUpdateTimer.Start();
        }

        internal void Pause(bool pause)
        {
            UpdateState(true, pause);
        }

        internal void Mute(bool mute)
        {
            this.muteToolStripMenuItem.Visible = !mute;
            this.unmuteToolStripMenuItem.Visible = mute;
            Properties.Settings.Default.MuteAllSounds = mute;
            Properties.Settings.Default.Save();

            if (this.mainForm != null)
                this.mainForm.Mute(mute);
        }

        public void HandleSystemNotifications()
        {
            List<InternalNotification> queuedNotifications = new List<InternalNotification>();
            InternalNotification.ReadFromDisk(ref queuedNotifications);
            HandleSystemNotifications(ref queuedNotifications);
        }

        public void HandleSystemNotifications(ref List<InternalNotification> queuedNotifications)
        {
            if (queuedNotifications != null)
            {
                foreach (InternalNotification n in queuedNotifications)
                {
                    Display display = (n.Display != null ? DisplayStyleManager.FindDisplayStyle(n.Display) : null);
                    SendSystemNotification(n.Title, n.Text, display);
                }

                queuedNotifications.Clear();
            }
            queuedNotifications = null;
        }

        internal void HandleListenUrls()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Utility.UserSettingFolder);
            System.IO.FileInfo[] files = di.GetFiles("*.ListenUrl");
            foreach (System.IO.FileInfo fi in files)
            {
                string outletUrl = System.IO.File.ReadAllText(fi.FullName).Trim();

                NotifyIOSubscription nios = new NotifyIOSubscription("Notify.io Notifications", true, outletUrl);
                if(Properties.Settings.Default.EnableSubscriptions) nios.Subscribe();
                this.controller.AddSubscription(nios);

                SendSystemNotification("Notify.io outlet installed", "Growl is now listening for notifications from notify.io", null);

                fi.IsReadOnly = false;
                System.IO.File.Delete(fi.FullName);

                Utility.WriteDebugInfo(String.Format("Loading notify.io ListenUrl from: {0}", fi.FullName));
                Utility.WriteDebugInfo(String.Format("Notify.io outlet url from ListenUrl: {0}", outletUrl));
            }
        }

        internal void SendSystemNotification(string title, string text, Display display)
        {
            if (this.controller != null)
            {
                this.controller.SendSystemNotification(title, text, display);
            }
        }

        private void WndProc(ref Message m)
        {
            // check if we got a hot key pressed.
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                // we could get the actual key pressed, but it is easier to just check the id
                int id = m.WParam.ToInt32();

                if (id == this.closeLastHotKey.ID)
                    this.controller.CloseLastNotification();
                else if (id == this.closeAllHotKey.ID)
                    this.controller.CloseAllOpenNotifications();
            }
        }

        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return this.contextMenu.BeginInvoke(method, args);
        }

        public object EndInvoke(IAsyncResult result)
        {
            return this.contextMenu.EndInvoke(result);
        }

        public object Invoke(Delegate method, object[] args)
        {
            return this.contextMenu.Invoke(method, args);
        }

        public bool InvokeRequired
        {
            get { return this.contextMenu.InvokeRequired; }
        }

        #endregion

        #region IDisposable Members

        protected override void  Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Microsoft.Win32.SystemEvents.TimeChanged -= new EventHandler(SystemEvents_TimeChanged);

                    if (this.components != null)
                    {
                        this.components.Dispose();
                    }

                    if (this.mainForm != null)
                    {
                        if (this.mainForm.OnOffButton != null)
                        {
                            this.mainForm.OnOffButton.Switched -= new Growl.UI.OnOffSwitchedEventHandler(OnOffButton_Switched);
                        }

                        this.mainForm.FormClosed -= new FormClosedEventHandler(mainForm_FormClosed);
                        this.mainForm.Dispose();
                        this.mainForm = null;
                    }

                    if (this.settingsToolStripMenuItem != null)
                    {
                        this.settingsToolStripMenuItem.Click -= new System.EventHandler(this.settingsToolStripMenuItem_Click);
                        this.settingsToolStripMenuItem.Dispose();
                        this.settingsToolStripMenuItem = null;
                    }

                    if (this.pauseGrowlToolStripMenuItem != null)
                    {
                        this.pauseGrowlToolStripMenuItem.Click -= new System.EventHandler(this.pauseGrowlToolStripMenuItem_Click);
                        this.pauseGrowlToolStripMenuItem.Dispose();
                        this.pauseGrowlToolStripMenuItem = null;
                    }

                    if (this.unpauseGrowlToolStripMenuItem != null)
                    {
                        this.unpauseGrowlToolStripMenuItem.Click -= new System.EventHandler(this.unpauseGrowlToolStripMenuItem_Click);
                        this.unpauseGrowlToolStripMenuItem.Dispose();
                        this.unpauseGrowlToolStripMenuItem = null;
                    }

                    if (this.muteToolStripMenuItem != null)
                    {
                        this.muteToolStripMenuItem.Click -= new System.EventHandler(this.muteToolStripMenuItem_Click);
                        this.muteToolStripMenuItem.Dispose();
                        this.muteToolStripMenuItem = null;
                    }

                    if (this.unmuteToolStripMenuItem != null)
                    {
                        this.unmuteToolStripMenuItem.Click -= new System.EventHandler(this.unmuteToolStripMenuItem_Click);
                        this.unmuteToolStripMenuItem.Dispose();
                        this.unmuteToolStripMenuItem = null;
                    }

                    if (this.checkForUpdatesToolStripMenuItem != null)
                    {
                        this.checkForUpdatesToolStripMenuItem.Click -= new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
                        this.checkForUpdatesToolStripMenuItem.Dispose();
                        this.checkForUpdatesToolStripMenuItem = null;
                    }

                    if (this.exitToolStripMenuItem != null)
                    {
                        this.exitToolStripMenuItem.Click -= new System.EventHandler(this.exitToolStripMenuItem_Click);
                        this.exitToolStripMenuItem.Dispose();
                        this.exitToolStripMenuItem = null;
                    }

                    if (this.toolStripSeparator1 != null)
                    {
                        this.toolStripSeparator1.Dispose();
                        this.toolStripSeparator1 = null;
                    }

                    if (this.contextMenu != null)
                    {
                        this.contextMenu.Close();
                        this.contextMenu.Dispose();
                        this.contextMenu = null;
                    }

                    if (this.notifyIcon != null)
                    {
                        this.notifyIcon.DoubleClick -= new System.EventHandler(this.notifyIcon_DoubleClick);
                        this.notifyIcon.Dispose();
                        this.notifyIcon = null;
                    }

                    if (this.updateForm != null)
                    {
                        this.updateForm.Close();
                        this.updateForm.Dispose();
                        this.updateForm = null;
                    }

                    if (this.updater != null)
                    {
                        this.updater.CheckForUpdateComplete -= new CheckForUpdateCompleteEventHandler(updater_CheckForUpdateComplete);
                        this.updater.Dispose();
                        this.updater = null;
                    }

                    if (this.controller != null)
                    {
                        this.controller.FailedToStart -= new EventHandler<PortConflictEventArgs>(controller_FailedToStart);
                        this.controller.FailedToStartUDPLegacy -= new EventHandler<PortConflictEventArgs>(controller_FailedToStartUDPLegacy);
                        this.controller.Dispose();
                        this.controller = null;
                    }

                    if (this.autoUpdateTimer != null)
                    {
                        this.autoUpdateTimer.Tick -= new EventHandler(autoUpdateTimer_Tick);
                        this.autoUpdateTimer.Dispose();
                        this.autoUpdateTimer = null;
                    }

                    UnregisterHotKeys();

                    if (this.wpr != null)
                    {
                        this.wpr.DestroyHandle();
                        this.wpr = null;
                    }
                }
                catch
                {
                    // suppress
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}