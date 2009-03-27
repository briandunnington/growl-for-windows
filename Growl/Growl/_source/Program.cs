using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Growl.AutoUpdate;

namespace Growl
{
    public class Program : ApplicationContext, System.ComponentModel.ISynchronizeInvoke
    {
        //public event EventHandler ProgramInitialized;

        private SplashScreen splash;
        private Controller controller;
        private MainForm mainForm;
        private bool initialized;
        Timer initializationTimer;
        private Keys closeLastKeyCombo;
        private Keys closeAllKeyCombo;
        private bool isResponsingToKeyIntercept;
        private Updater updater;
        private Timer autoUpdateTimer = new Timer();
        private UpdateForm updateForm;

        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseGrowlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unpauseGrowlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

        public Program() : base()
        {
            Console.WriteLine(Utility.UserSettingFolder);   //NOTE: this is here as a bit of hack to ensure that the Utility static constructor runs first

            this.splash = new SplashScreen();
            this.splash.Shown += new EventHandler(splash_Shown);
            base.MainForm = this.splash;
        }

        void splash_Shown(object sender, EventArgs e)
        {
            InitializeComponent();

            BeginInitializeApplication();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseGrowlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unpauseGrowlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.contextMenu.SuspendLayout();

            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = global::Growl.Properties.Resources.growl_stopped;
            this.notifyIcon.Text = "Growl (not running)";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.pauseGrowlToolStripMenuItem,
            this.unpauseGrowlToolStripMenuItem,
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
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // pauseGrowlToolStripMenuItem
            // 
            this.pauseGrowlToolStripMenuItem.Name = "pauseGrowlToolStripMenuItem";
            this.pauseGrowlToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.pauseGrowlToolStripMenuItem.Text = "Pause Growl";
            this.pauseGrowlToolStripMenuItem.Visible = false;
            this.pauseGrowlToolStripMenuItem.Click += new System.EventHandler(this.pauseGrowlToolStripMenuItem_Click);
            // 
            // unpauseGrowlToolStripMenuItem
            // 
            this.unpauseGrowlToolStripMenuItem.Name = "unpauseGrowlToolStripMenuItem";
            this.unpauseGrowlToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.unpauseGrowlToolStripMenuItem.Text = "Unpause Growl";
            this.unpauseGrowlToolStripMenuItem.Visible = false;
            this.unpauseGrowlToolStripMenuItem.Click += new System.EventHandler(this.unpauseGrowlToolStripMenuItem_Click);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates";
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
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

            this.contextMenu.ResumeLayout();
        }

        private void BeginInitializeApplication()
        {
            // main form
            this.mainForm = new MainForm();
            IntPtr handle = this.mainForm.Handle;   // this forces the creation of the handle even when the form is not shown
            //this.mainForm.ShowForm();

            // initialization timer (we do the bulk of the initialization in the timer.Tick so that 1) the splash screen can draw completely without being held up, and 2) it frees up the UI thread)
            this.initializationTimer = new Timer();
            this.initializationTimer.Interval = 100;
            this.initializationTimer.Tick += new EventHandler(initializationTimer_Tick);
            this.initializationTimer.Start();

            // auto updater
            this.updater = new Updater(Application.StartupPath);
            this.updater.CheckForUpdateComplete += new CheckForUpdateCompleteEventHandler(updater_CheckForUpdateComplete);
            this.autoUpdateTimer.Interval = 20 * 1000;
            this.autoUpdateTimer.Tick += new EventHandler(autoUpdateTimer_Tick);
            this.autoUpdateTimer.Start();
        }

        void initializationTimer_Tick(object sender, EventArgs e)
        {
            this.initializationTimer.Stop();
            if (!this.initialized)
            {
                this.initialized = true;
                InitializeApplication();

                // start ourself one more time for post-initialization work
                this.initializationTimer.Start();
            }
            else
            {
                // while it may seem strange to call the Detector to see if we are installed, it is the best way
                // to ensure that plugins/displays see the same values that we do
                Growl.CoreLibrary.Detector detector = new Growl.CoreLibrary.Detector();
                if (!detector.IsAvailable)
                {
                    // something is not right with the registry setting, so lets fix it now
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY, true);
                    if (key == null)
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY);
                    key.SetValue(null, Application.ExecutablePath);
                }
            }
        }

        private void InitializeApplication()
        {
            this.mainForm.UpdateInitializationProgress("Loading Displays...", 25);

            // configure the controller
            this.controller = Controller.GetController();
            this.controller.ItemLoaded += new Controller.ItemLoadedEventHandler(controller_ItemLoaded);
            this.controller.FailedToStart += new Controller.FailedToStartEventHandler(controller_FailedToStart);
            this.controller.FailedToStartUDPLegacy += new Controller.FailedToStartEventHandler(controller_FailedToStartUDPLegacy);
            this.controller.ApplicationRegistered += new Controller.ApplicationRegisteredDelegate(controller_ApplicationRegistered);
            this.controller.NotificationReceived += new Controller.NotificationReceivedDelegate(controller_NotificationReceived);
            this.controller.NotificationPast += new Controller.NotificationPastDelegate(controller_NotificationPast);
            this.controller.BonjourServiceUpdate += new Controller.BonjourServiceUpdateDelegate(controller_BonjourServiceUpdate);
            this.controller.ForwardComputersUpdated += new EventHandler(controller_ForwardComputersUpdated);
            this.controller.SubscriptionsUpdated += new Controller.SubscriptionsUpdatedDelegate(controller_SubscriptionsUpdated);
            this.controller.Initialize(Application.ExecutablePath, this);

            this.mainForm.Controller = this.controller;

            this.mainForm.UpdateInitializationProgress("Loading Preferences...", 75);

            try
            {
                KeysConverter kc = new KeysConverter();
                this.closeLastKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseLast);
                this.closeAllKeyCombo = (Keys)kc.ConvertFromString(Properties.Settings.Default.KeyboardShortcutCloseAll);
                Keyboard.KeyIntercepted += new Keyboard.KeyInterceptedEventHandler(Keyboard_KeyIntercepted);
            }
            catch
            {
            }

            // load all user preferences
            Microsoft.Win32.SystemEvents.TimeChanged += new EventHandler(SystemEvents_TimeChanged);
            this.mainForm.InitializePreferences();
            this.mainForm.UpdateInitializationProgress("Starting Growl...", 90);

            // start growl (and set button at the same time)
            this.mainForm.OnOffButton.Switched +=new Growl.UI.OnOffSwitchedEventHandler(OnOffButton_Switched);
            this.mainForm.OnOffButton.On = true;

            this.mainForm.Hide();

            this.mainForm.DoneInitializing();
            this.mainForm.UpdateInitializationProgress("Ready", 100);

            base.MainForm.Hide();
        }

        protected override void OnMainFormClosed(object sender, EventArgs e)
        {
            //base.OnMainFormClosed(sender, e);
        }

        void autoUpdateTimer_Tick(object sender, EventArgs e)
        {
            this.autoUpdateTimer.Stop();

            // check less frequently after initial startup
            this.autoUpdateTimer.Interval = 4 * 60 * 60 * 1000; // four hours
            this.autoUpdateTimer.Tick += new EventHandler(autoUpdateTimer_Tick);

            if (Properties.Settings.Default.AutomaticallyCheckForUpdates)
            {
                this.updater.CheckForUpdate(false);
                this.autoUpdateTimer.Start();
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

        internal void AlreadyRunning()
        {
            this.controller.SendSystemNotification("Growl", "Growl is running", null);
        }

        private bool StartGrowl()
        {
            bool started = this.controller.Start();
            if (started) Keyboard.SetHook();

            UpdateState(started, false);

            return started;
        }

        private void StopGrowl()
        {
            UpdateState(false, false);

            this.controller.Stop();
            Keyboard.Unhook();
        }

        private void UpdateState(bool isRunning, bool isPaused)
        {
            this.pauseGrowlToolStripMenuItem.Visible = (isRunning && !isPaused);
            this.unpauseGrowlToolStripMenuItem.Visible = (isRunning && isPaused);

            if (!isRunning)
            {
                this.mainForm.UpdateState("Growl is not running", Color.Red);
                this.notifyIcon.Text = "Growl (not running)";
                this.notifyIcon.Icon = global::Growl.Properties.Resources.growl_stopped;
            }
            else if (isPaused)
            {
                this.mainForm.UpdateState("Growl is paused", Color.FromArgb(SystemColors.GrayText.ToArgb()));  // avoid using SystemColors directly
                this.notifyIcon.Text = "Growl (paused)";
                this.notifyIcon.Icon = global::Growl.Properties.Resources.growl_dim;
                this.controller.Pause();
            }
            else
            {
                this.mainForm.UpdateState("Growl is running", Color.Green);
                this.notifyIcon.Text = "Growl";
                this.notifyIcon.Icon = global::Growl.Properties.Resources.growl_on;
                this.controller.Unpause();
            }
        }

        void controller_ItemLoaded(string itemLoaded)
        {
            int val = this.mainForm.ProgressBarInitializing.Value + 10;
            if (val > 75) val = this.mainForm.ProgressBarInitializing.Value;

            this.mainForm.UpdateInitializationProgress(itemLoaded, val);
        }

        void controller_ApplicationRegistered(RegisteredApplication ra)
        {
            if (this.mainForm.IsHandleCreated && this.mainForm.InvokeRequired)
            {
                Controller.ApplicationRegisteredDelegate del = new Controller.ApplicationRegisteredDelegate(this.mainForm.OnApplicationRegistered);
                this.mainForm.BeginInvoke(del, new object[] { ra });
            }
            else
            {
                this.mainForm.OnApplicationRegistered(ra);
            }
        }

        void controller_NotificationReceived(Growl.DisplayStyle.Notification n)
        {
            if (this.mainForm.IsHandleCreated && this.mainForm.InvokeRequired)
            {
                Controller.NotificationReceivedDelegate del = new Controller.NotificationReceivedDelegate(this.mainForm.OnNotificationReceived);
                this.mainForm.BeginInvoke(del, new object[] { n });
            }
            else
            {
                this.mainForm.OnNotificationReceived(n);
            }
        }

        void controller_NotificationPast(PastNotification pn)
        {
            if (this.mainForm.IsHandleCreated && this.mainForm.InvokeRequired)
            {
                Controller.NotificationPastDelegate del = new Controller.NotificationPastDelegate(this.mainForm.OnNotificationPast);
                this.mainForm.BeginInvoke(del, new object[] { pn });
            }
            else
            {
                this.mainForm.OnNotificationPast(pn);
            }
        }

        void controller_FailedToStartUDPLegacy(object sender, PortConflictEventArgs e)
        {
            string caption = "Growl was unable to start";
            string text = String.Format("There is already a listener enabled on Growl's legacy UDP port ({0}).\r\n\r\nIf another version of Growl is already running, please close that version and try again.", e.Port);
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0, false);
        }

        void controller_FailedToStart(object sender, PortConflictEventArgs e)
        {
            string caption = "Growl was unable to start";
            string text = String.Format("There is already a listener enabled on Growl's TCP port ({0}).\r\n\r\nIf another version of Growl is already running, please close that version and try again.", e.Port);
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0, false);
        }

        void controller_BonjourServiceUpdate(BonjourForwardComputer bfc)
        {
            this.mainForm.OnBonjourServiceUpdated(bfc);
        }

        void controller_ForwardComputersUpdated(object sender, EventArgs e)
        {
            this.mainForm.OnForwardComputersUpdated();
        }

        void controller_SubscriptionsUpdated(bool countChanged)
        {
            this.mainForm.OnSubscriptionsUpdated(countChanged);
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

        void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            this.controller.ReloadPastNotifications();
            this.mainForm.OnSystemTimeChanged();
        }

        void Keyboard_KeyIntercepted(Keyboard.KeyboardHookEventArgs args)
        {
            if (!this.isResponsingToKeyIntercept)
            {
                if (args.KeyData == this.closeLastKeyCombo)
                {
                    this.isResponsingToKeyIntercept = true;
                    this.controller.CloseLastNotification();
                    this.isResponsingToKeyIntercept = false;
                }
                else if (args.KeyData == this.closeAllKeyCombo)
                {
                    this.isResponsingToKeyIntercept = true;
                    this.controller.CloseAllOpenNotifications();
                    this.isResponsingToKeyIntercept = false;
                }
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.mainForm.ShowForm();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mainForm.ShowForm();
        }

        private void pauseGrowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateState(true, true);

            // controller.Pause();
        }

        private void unpauseGrowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateState(true, false);

            // controller.Unpause();

        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.updater.CheckForUpdate(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return this.splash.BeginInvoke(method, args);
        }

        public object EndInvoke(IAsyncResult result)
        {
            return this.splash.EndInvoke(result);
        }

        public object Invoke(Delegate method, object[] args)
        {
            //return method.DynamicInvoke(args);
            return this.splash.Invoke(method, args);
        }

        public bool InvokeRequired
        {
            get { return this.splash.InvokeRequired; }
        }

        #endregion
    }
}