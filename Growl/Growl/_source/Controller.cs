using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Win32;

namespace Growl
{
    internal class Controller : IDisposable
    {
        public delegate void ApplicationRegisteredDelegate(RegisteredApplication ra);
        public delegate void NotificationReceivedDelegate(Growl.DisplayStyle.Notification n);
        public delegate void NotificationPastDelegate(PastNotification pn);
        public delegate void BonjourServiceUpdateDelegate(BonjourForwardComputer bfc);
        public delegate void SubscriptionsUpdatedDelegate(bool countChanged);
        public delegate void FailedToStartEventHandler(object sender, PortConflictEventArgs args);

        private delegate void ShowNotificationDelegate(Growl.DisplayStyle.Notification n, Display display, Growl.Daemon.CallbackInfo cbInfo, bool recordInMissedNotifications, Growl.Daemon.RequestInfo requestInfo);
        private delegate void ShowMissedNotificationsDelegate(List<PastNotification> missedNotifications);
        private delegate void RefreshActiveNotificationsDelegate();
        internal delegate void ItemLoadedEventHandler(string itemLoaded);
        private delegate void OnBonjourServiceUpdateDelegate(BonjourForwardComputer bfc);
        private delegate void OnForwardComputersUpdatedDelegate();
        private delegate void OnSubscriptionsUpdatedDelegate(bool countChanged);

        public event FailedToStartEventHandler FailedToStart;
        public event FailedToStartEventHandler FailedToStartUDPLegacy;
        internal event ItemLoadedEventHandler ItemLoaded;
        public event ApplicationRegisteredDelegate ApplicationRegistered;
        public event NotificationReceivedDelegate NotificationReceived;
        public event NotificationPastDelegate NotificationPast;
        public event BonjourServiceUpdateDelegate BonjourServiceUpdate;
        public event EventHandler ForwardComputersUpdated;
        public event SubscriptionsUpdatedDelegate SubscriptionsUpdated;

        private static Controller singleton;
        private bool disposed;

        private const string APPLICATION_AUTORUN_KEY = "Growl";
        private const string REGISTERED_APPLICATIONS_SETTINGS_FILENAME = "applications.settings";
        private const string FORWARD_COMPUTERS_SETTINGS_FILENAME = "forward.settings";
        private const string PASSWORD_SETTINGS_FILENAME = "passwords.settings";
        private const string SUBSCRIPTIONS_SETTINGS_FILENAME = "subscriptions.settings";

        private string appPath;
        private System.ComponentModel.ISynchronizeInvoke synchronizingObject;
        private string historyFolder;

        private Growl.Daemon.GrowlServer gntpListener;
        private Growl.UDPLegacy.MessageReceiver udpListener;
        private Growl.UDPLegacy.MessageReceiver udpListenerLocal;
        private Bonjour bonjour;
        private Growl.Connector.PasswordManager passwordManager;

        private bool isRunning;
        private bool paused;
        private bool idle;
        private Display growlDefaultDisplay;
        private string defaultDisplayName;
        private Dictionary<string, Display> availableDisplays;
        private List<PastNotification> pastNotifications = new List<PastNotification>();
        private Display lastUsedDisplay;
        private ActivityMonitor activityMonitor;
        private List<PastNotification> missedNotifications = new List<PastNotification>();
        private MissedNotificationsDisplay missedNotificationsDisplay;
        private PrefSound growlDefaultSound = PrefSound.None;

        protected Dictionary<string, RegisteredApplication> applications;
        protected Dictionary<string, ForwardComputer> forwards;
        protected Dictionary<string, ForwardComputer> subscriptions;
        private SettingSaver raSettingSaver = new SettingSaver(REGISTERED_APPLICATIONS_SETTINGS_FILENAME);
        private SettingSaver fcSettingSaver = new SettingSaver(FORWARD_COMPUTERS_SETTINGS_FILENAME);
        private SettingSaver pmSettingSaver = new SettingSaver(PASSWORD_SETTINGS_FILENAME);
        private SettingSaver sbSettingSaver = new SettingSaver(SUBSCRIPTIONS_SETTINGS_FILENAME);

        public static Controller GetController()
        {
            if (singleton == null)
                singleton = new Controller();
            return singleton;
        }

        private Controller()
        {
        }

        internal void Initialize(string appPath, System.ComponentModel.ISynchronizeInvoke synchronizingObject)
        {
            DisplayStyleManager.DisplayLoaded += new DisplayStyleManager.DisplayLoadedEventHandler(DisplayStyleManager_DisplayLoaded);

            this.appPath = appPath;
            this.synchronizingObject = synchronizingObject;

            this.historyFolder = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"History\");
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(this.historyFolder);

            LoadPasswords();
            LoadMiscPrefs();
            LoadDisplays();
            LoadApplications();
            LoadForwardedComputers();
            LoadSubscriptions();
            LoadPastNotifications();

            if (Bonjour.IsSupported)
            {
                this.bonjour = new Bonjour();
                this.bonjour.ServiceFound += new Bonjour.NetServiceFoundEventHandler(bonjour_ServiceFound);
                this.bonjour.ServiceRemoved += new Bonjour.NetServiceRemovedEventHandler(bonjour_ServiceRemoved);
                this.bonjour.Start();
            }

            this.activityMonitor = new ActivityMonitor();
            this.activityMonitor.WentIdle += new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_WentIdle);
            this.activityMonitor.ResumedActivity += new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_ResumedActivity);
            this.activityMonitor.StillActive += new EventHandler(activityMonitor_StillActive);
        }

        void DisplayStyleManager_DisplayLoaded(string displayName)
        {
            this.OnItemLoaded(String.Format(Properties.Resources.Loading_Display, displayName));
        }

        public bool Start()
        {
            bool started = false;
            //string keyHex = "8917EEF78F63044182DB218FDC9715C16EE45AE1179A17A4521B0AFF559272AC";

            this.gntpListener = new Growl.Daemon.GrowlServer(Growl.Connector.GrowlConnector.TCP_PORT, this.passwordManager, Utility.UserSettingFolder);
            this.gntpListener.RegisterReceived += new Growl.Daemon.GrowlServer.RegisterReceivedEventHandler(gntpListener_RegisterReceived);
            this.gntpListener.NotifyReceived += new Growl.Daemon.GrowlServer.NotifyReceivedEventHandler(gntpListener_NotifyReceived);
            this.gntpListener.SubscribeReceived += new Growl.Daemon.GrowlServer.SubscribeReceivedEventHandler(gntpListener_SubscribeReceived);
            this.gntpListener.LoggingEnabled = Properties.Settings.Default.EnableLogging;
            this.gntpListener.AllowFlash = this.AllowWebNotifications;
            this.gntpListener.AllowNetworkNotifications = this.AllowNetworkNotifications;
            this.gntpListener.AllowSubscriptions = this.AllowSubscriptions;
            this.gntpListener.RequireLocalPassword = this.RequireLocalPassword;
            started = this.gntpListener.Start();
            if (!started)
            {
                this.OnFailedToStart(this, new PortConflictEventArgs(Growl.Connector.GrowlConnector.TCP_PORT));
                return false;
            }

            // this starts the legacy UDP listeners. it can be disabled via user.config if necessary
            if (!Properties.Settings.Default.DisableLegacySupport)
            {
                string udpLogFolder = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"Log\");

                // this is for network UDP requests (old Growl network protocol)
                this.udpListener = new Growl.UDPLegacy.MessageReceiver(Growl.UDPLegacy.MessageReceiver.NETWORK_PORT, this.passwordManager, Properties.Settings.Default.EnableLogging, udpLogFolder);
                this.udpListener.RegistrationReceived += new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                this.udpListener.NotificationReceived += new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                this.udpListener.AllowNetworkNotifications = this.AllowNetworkNotifications;
                this.udpListener.RequireLocalPassword = this.RequireLocalPassword;
                started = this.udpListener.Start();
                if (!started)
                {
                    if (this.gntpListener != null) this.gntpListener.Stop();    // stop the GNTP listener if it was already started
                    this.OnFailedToStartUDPLegacy(this, new PortConflictEventArgs(Growl.UDPLegacy.MessageReceiver.NETWORK_PORT));
                    return false;
                }

                // this is for local UDP requests (old GFW local protocol)
                this.udpListenerLocal = new Growl.UDPLegacy.MessageReceiver(Growl.UDPLegacy.MessageReceiver.LOCAL_PORT, this.passwordManager, Properties.Settings.Default.EnableLogging, udpLogFolder);
                this.udpListenerLocal.RegistrationReceived += new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                this.udpListenerLocal.NotificationReceived += new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                this.udpListenerLocal.AllowNetworkNotifications = false;    // always false
                this.udpListenerLocal.RequireLocalPassword = this.RequireLocalPassword;
                started = this.udpListenerLocal.Start();
                if (!started)
                {
                    if (this.gntpListener != null) this.gntpListener.Stop();    // stop the GNTP listener if it was already started
                    if (this.udpListener != null) this.udpListener.Stop();      // stop the network UDP listener if it was already started
                    this.OnFailedToStartUDPLegacy(this, new PortConflictEventArgs(Growl.UDPLegacy.MessageReceiver.NETWORK_PORT));
                    return false;
                }
            }

            this.isRunning = true;
            StartActivityMonitor();

            // send a notification that growl is running
            SendSystemNotification(Properties.Resources.SystemNotification_Running_Title, Properties.Resources.SystemNotification_Running_Text);

            return true;
        }

        public void Stop()
        {
            this.isRunning = false;

            StopActivityMonitor();

            if (this.gntpListener != null) this.gntpListener.Stop();
            if (this.udpListener != null) this.udpListener.Stop();
            if (this.udpListenerLocal != null) this.udpListenerLocal.Stop();
            this.gntpListener = null;
            this.udpListener = null;
            this.udpListenerLocal = null;

            this.missedNotifications.Clear();

            SaveAppState();
        }

        public void Pause()
        {
            this.activityMonitor.PauseApplication();
            this.isRunning = false;
        }

        public void Unpause()
        {
            this.activityMonitor.UnpauseApplication();
            this.isRunning = true;
        }

        public void StartActivityMonitor()
        {
            this.activityMonitor.Start();
        }

        public void StopActivityMonitor()
        {
            if (this.activityMonitor != null) this.activityMonitor.Stop();
            this.missedNotifications.Clear();
        }

        protected void OnFailedToStart(object sender, PortConflictEventArgs args)
        {
            if (this.FailedToStart != null)
            {
                this.FailedToStart(sender, args);
            }
        }

        protected void OnFailedToStartUDPLegacy(object sender, PortConflictEventArgs args)
        {
            if (this.FailedToStartUDPLegacy != null)
            {
                this.FailedToStartUDPLegacy(sender, args);
            }
        }

        public void ProcessRegistration(Growl.UDPLegacy.RegistrationPacket rp, string receivedFrom)
        {
            /* THIS METHOD RESENDS THE UDP MESSAGE AS A LOCAL GNTP MESSAGE
            Growl.Connector.Application application = new Growl.Connector.Application(rp.ApplicationName);
            List<Growl.Connector.NotificationType> notificationTypes = new List<Growl.Connector.NotificationType>();
            foreach (Growl.UDPLegacy.NotificationType nt in rp.NotificationTypes)
            {
                Growl.Connector.NotificationType notificationType = new Growl.Connector.NotificationType(nt.Name, nt.Name, null, nt.Enabled);
                notificationTypes.Add(notificationType);
            }

            Growl.Connector.GrowlConnector udpToGNTPForwarder = new Growl.Connector.GrowlConnector();
            udpToGNTPForwarder.Password = rp.Password;
            udpToGNTPForwarder.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
            udpToGNTPForwarder.Register(application, notificationTypes.ToArray());
             * */


            /* THIS METHOD AVOIDS SENDING ANOTHER MESSAGE, BUT TRADES THE DETAILED GNTP LOGGING */
            Growl.Connector.Application application = new Growl.Connector.Application(rp.ApplicationName);
            List<Growl.Connector.NotificationType> notificationTypes = new List<Growl.Connector.NotificationType>();
            foreach (Growl.UDPLegacy.NotificationType nt in rp.NotificationTypes)
            {
                Growl.Connector.NotificationType notificationType = new Growl.Connector.NotificationType(nt.Name, nt.Name, null, nt.Enabled);
                notificationTypes.Add(notificationType);
            }
            Growl.Daemon.RequestInfo requestInfo = new Growl.Daemon.RequestInfo();
            gntpListener_RegisterReceived(application, notificationTypes, requestInfo);
        }

        public void ProcessNotification(Growl.UDPLegacy.NotificationPacket np, string receivedFrom)
        {
            /* THIS METHOD RESENDS THE UDP MESSAGE AS A LOCAL GNTP MESSAGE
            Growl.Connector.Application application = new Growl.Connector.Application(np.ApplicationName);
            Growl.Connector.Notification notification = new Growl.Connector.Notification(np.ApplicationName, np.NotificationType.Name, String.Empty, np.Title, np.Description, null, np.Sticky, np.Priority, null);

            Growl.Connector.GrowlConnector udpToGNTPForwarder = new Growl.Connector.GrowlConnector();
            udpToGNTPForwarder.Password = np.Password;
            udpToGNTPForwarder.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
            udpToGNTPForwarder.Notify(application, notification);
             * */


            /* THIS METHOD AVOIDS SENDING ANOTHER MESSAGE, BUT TRADES THE DETAILED GNTP LOGGING */
            Growl.Connector.Notification notification = new Growl.Connector.Notification(np.ApplicationName, np.NotificationType.Name, String.Empty, np.Title, np.Description, null, np.Sticky, np.Priority, null);
            Growl.Daemon.RequestInfo requestInfo = new Growl.Daemon.RequestInfo();
            gntpListener_NotifyReceived(notification, null, requestInfo);
        }

        public void EnableAutoStart()
        {
            // add the application to the Registry so it will automatically run on startup
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            bool alreadyThere = (registryKey.GetValue(APPLICATION_AUTORUN_KEY) != null);
            if (!alreadyThere)
            {
                registryKey.SetValue(APPLICATION_AUTORUN_KEY, this.appPath);
            }
            Properties.Settings.Default.AutoStart = true;
        }

        public void DisableAutoStart()
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

        private void SendSystemNotification(string title, string text)
        {
            SendSystemNotification(title, text, this.growlDefaultDisplay);
        }

        internal void SendSystemNotification(string title, string text, Display display)
        {
            //Growl.Daemon.RequestInfo requestInfo = new Growl.Daemon.RequestInfo();
            //Growl.Connector.Resource icon = new Growl.Connector.BinaryData(bytes);
            //Growl.Connector.Notification notification = new Growl.Connector.Notification("Growl", "Growl System Message", String.Empty, title, text, icon, false, Growl.Connector.Priority.Normal);
            //gntpListener_NotifyReceived(notification, null, requestInfo);

            Growl.Daemon.RequestInfo requestInfo = new Growl.Daemon.RequestInfo();  // this is not used, but needed as a placeholder

            DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
            n.UUID = requestInfo.RequestID;
            n.ApplicationName = Properties.Resources.SystemNotification_ApplicationName;
            n.Description = text;
            n.Name = "Growl System Message";
            n.Priority = (int)Growl.Connector.Priority.Normal;
            n.Sticky = false;   // system notifications are never sticky
            n.Title = title;
            n.Image = global::Growl.Properties.Resources.growl;

            InvokeShowNotification(n, display, null, false, requestInfo);
        }

        private void LoadMiscPrefs()
        {
            // default sound
            string defaultSoundPath = Properties.Settings.Default.DefaultSound;
            PrefSound ps = PrefSound.FromFilePath(defaultSoundPath);
            this.growlDefaultSound = ps;
        }

        private void LoadApplications()
        {
            this.OnItemLoaded(Properties.Resources.Loading_ApplicationList);

            this.applications = (Dictionary<string, RegisteredApplication>) raSettingSaver.Load();
            if (this.applications == null) this.applications = new Dictionary<string, RegisteredApplication>();
        }

        private void LoadPasswords()
        {
            this.passwordManager = (Growl.Connector.PasswordManager)pmSettingSaver.Load();
            if (this.passwordManager == null) this.passwordManager = new Growl.Connector.PasswordManager();
        }

        private void LoadDisplays()
        {
            this.OnItemLoaded(Properties.Resources.Loading_Displays);

            this.missedNotificationsDisplay = new MissedNotificationsDisplay();

            DisplayStyleManager.Load();
            this.availableDisplays = DisplayStyleManager.GetAvailableDisplayStyles();

            // hook up callback event handlers
            foreach (Display display in this.availableDisplays.Values)
            {
                display.NotificationCallback += new Display.NotificationCallbackEventHandler(display_NotificationCallback);
            }

            try
            {
                this.defaultDisplayName = Properties.Settings.Default.DefaultDisplay;
                this.growlDefaultDisplay = this.availableDisplays[this.defaultDisplayName];
                Display.Default.Update(this.growlDefaultDisplay);
            }
            catch
            {
                // the default display was not found. that is bad
                throw new GrowlException("The default display was not found. It is suggested that Growl be re-installed to restore any missing files.");
            }
        }

        void display_NotificationCallback(Growl.Daemon.CallbackInfo cbInfo, Growl.CoreLibrary.CallbackResult result)
        {
            if (this.gntpListener != null && cbInfo != null && cbInfo.Context != null)
            {
                cbInfo.RequestInfo.SaveHandlingInfo(String.Format("Was responded to on {0} - Action: {1}", Environment.MachineName, result));
                Growl.Connector.Response response = new Growl.Connector.Response();
                response.SetCallbackData(cbInfo.NotificationID, cbInfo.Context, result);

                if (cbInfo.ShouldKeepConnectionOpen())
                {
                    this.gntpListener.WriteResponse(cbInfo, response);
                }
                else
                {
                    string data = cbInfo.GetUrlCallbackData(result);
                    Growl.Connector.UrlCallbackTarget target = cbInfo.Context.GetUrlCallbackTarget();
                    if (!String.IsNullOrEmpty(target.Url))
                    {
                        /*
                        // if the method is APP, we want to use a custom application protocol handler,
                        // otherwise, it is a web-based callback and we handle it behind the scenes
                        if (target.Method == Growl.Connector.UrlCallbackTarget.APP)
                        {
                            System.UriBuilder ub = new UriBuilder(target.Url);
                            if (ub.Query != null && ub.Query.Length > 1)
                                ub.Query = ub.Query.Substring(1) + "&" + data;
                            else
                                ub.Query = data;

                            System.Diagnostics.Process.Start(ub.Uri.AbsoluteUri);
                        }
                        else
                        {
                            System.Net.WebClient webclient = new System.Net.WebClient();
                            using (webclient)
                            {
                                webclient.Encoding = Encoding.UTF8;
                                webclient.Headers.Add("user-agent", String.Format("{0} - Notification Callback", this.gntpListener.ServerName));
                                Uri uri = new Uri(target.Url);
                                webclient.UploadStringAsync(uri, target.Method, data);
                            }
                        }
                         * */

                        // TODO:
                        System.Net.WebClient webclient = new System.Net.WebClient();
                        using (webclient)
                        {
                            webclient.Encoding = Encoding.UTF8;
                            webclient.Headers.Add("user-agent", String.Format("{0} - Notification Callback", this.gntpListener.ServerName));
                            Uri uri = new Uri(target.Url);
                            webclient.UploadStringAsync(uri, target.Method, data);
                        }
                    }
                }
            }
        }

        private void LoadForwardedComputers()
        {
            this.forwards = (Dictionary<string, ForwardComputer>)fcSettingSaver.Load();
            if (this.forwards == null) this.forwards = new Dictionary<string, ForwardComputer>();
        }

        private void LoadSubscriptions()
        {
            this.subscriptions = (Dictionary<string, ForwardComputer>)sbSettingSaver.Load();
            if (this.subscriptions == null) this.subscriptions = new Dictionary<string, ForwardComputer>();

            foreach (Subscription subscription in this.subscriptions.Values)
            {
                subscription.StatusChanged +=new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
            }
        }

        private void LoadPastNotifications()
        {
            this.OnItemLoaded(Properties.Resources.Loading_History);
            ReloadPastNotifications();
        }

        internal void ReloadPastNotifications()
        {
            if (this.pastNotifications == null) this.pastNotifications = new List<PastNotification>();
            this.pastNotifications.Clear();

            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(this.historyFolder);
            System.IO.FileInfo[] files = d.GetFiles("*.notification", System.IO.SearchOption.AllDirectories);

            foreach (System.IO.FileInfo file in files)
            {
                string data = System.IO.File.ReadAllText(file.FullName);
                object obj = Serialization.DeserializeObject(data);
                if (obj != null)
                {
                    try
                    {
                        PastNotification pn = (PastNotification)obj;
                        pn.LinkImage();
                        this.pastNotifications.Add(pn);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void SaveAppState()
        {
            if (this.raSettingSaver != null) this.raSettingSaver.Save(this.applications);
            if (this.fcSettingSaver != null) this.fcSettingSaver.Save(this.forwards);
            if (this.pmSettingSaver != null) this.pmSettingSaver.Save(this.passwordManager);
            if (this.sbSettingSaver != null) this.sbSettingSaver.Save(this.subscriptions);
        }

        public Growl.Connector.PasswordManager PasswordManager
        {
            get
            {
                return this.passwordManager;
            }
        }

        public Dictionary<string, RegisteredApplication> RegisteredApplications
        {
            get
            {
                return this.applications;
            }
        }

        public Dictionary<string, Display> AvailableDisplays
        {
            get
            {
                return this.availableDisplays;
            }
        }

        public Dictionary<string, ForwardComputer> ForwardComputers
        {
            get
            {
                return this.forwards;
            }
        }

        public Dictionary<string, ForwardComputer> Subscriptions
        {
            get
            {
                return this.subscriptions;
            }
        }

        public List<PastNotification> PastNotifications
        {
            get
            {
                return this.pastNotifications;
            }
        }

        public Dictionary<string, DetectedService> DetectedServices
        {
            get
            {
                Dictionary<string, DetectedService> services = new Dictionary<string, DetectedService>();
                if (this.bonjour != null)
                {
                    List<string> filter = new List<string>();
                    foreach (ForwardComputer fc in this.ForwardComputers.Values)
                    {
                        if (fc is BonjourForwardComputer)
                        {
                            filter.Add(fc.Description);
                        }
                    }
                    services = this.bonjour.GetAvailableServices(filter);
                }
                return services;
            }
        }

        public Display DefaultDisplay
        {
            get
            {
                return this.growlDefaultDisplay;
            }
            internal set
            {
                this.growlDefaultDisplay = value;
                Display.Default.Update(value);
                Properties.Settings.Default.DefaultDisplay = Display.Default.ActualName;
            }
        }

        public PrefSound DefaultSound
        {
            get
            {
                return this.growlDefaultSound;
            }
            internal set
            {
                this.growlDefaultSound = value;
                Properties.Settings.Default.DefaultSound = this.growlDefaultSound.SoundFile;
            }
        }

        public bool CheckForIdle
        {
            get
            {
                return Properties.Settings.Default.CheckForIdle;
            }
            set
            {
                Properties.Settings.Default.CheckForIdle = value;
                this.activityMonitor.CheckForIdle = value;
            }
        }

        public int IdleAfterSeconds
        {
            get
            {
                return Properties.Settings.Default.IdleAfterSeconds;
            }
            set
            {
                Properties.Settings.Default.IdleAfterSeconds = value;
                this.activityMonitor.IdleAfterSeconds = value;
            }
        }

        protected void OnApplicationRegistered(RegisteredApplication ra)
        {
            if (this.ApplicationRegistered != null)
            {
                this.ApplicationRegistered(ra);
            }
        }

        private void InvokeShowNotification(Growl.DisplayStyle.Notification notification, Display display, Growl.Daemon.CallbackInfo cbInfo, bool recordInMissedNotifications, Growl.Daemon.RequestInfo requestInfo)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                ShowNotificationDelegate snd = new ShowNotificationDelegate(ShowNotification);
                this.synchronizingObject.BeginInvoke(snd, new object[] { notification, display, cbInfo, recordInMissedNotifications, requestInfo });
            }
            else
            {
                ShowNotification(notification, display, cbInfo, recordInMissedNotifications, requestInfo);
            }
        }

        private void ShowNotification(Growl.DisplayStyle.Notification notification, Display display, Growl.Daemon.CallbackInfo cbInfo, bool recordInMissedNotifications, Growl.Daemon.RequestInfo requestInfo)
        {
            if (display == null) display = this.growlDefaultDisplay;

            // if not a system notification and we are paused, dont show
            if (recordInMissedNotifications && paused)
            {
                display = Display.None;
                requestInfo.SaveHandlingInfo(String.Format("Not shown - {0}", (paused ? "Paused" : "Idle")));
            }
            else
            {
                requestInfo.SaveHandlingInfo(String.Format("Displayed using '{0}' {1}", display.Name, (display.IsDefault ? "(" + display.ActualName + ")" : "")));
            }

            display.ProcessNotification(notification, cbInfo, requestInfo);
            this.lastUsedDisplay = display;
        }

        public void InvokeShowMissedNotifications(List<PastNotification> missedNotifications)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                ShowMissedNotificationsDelegate smnd = new ShowMissedNotificationsDelegate(ShowMissedNotifications);
                this.synchronizingObject.BeginInvoke(smnd, new object[] { missedNotifications });
            }
            else
            {
                ShowMissedNotifications(missedNotifications);
            }
        }

        private void ShowMissedNotifications(List<PastNotification> missedNotifications)
        {
            this.missedNotificationsDisplay.HandleNotification(missedNotifications);
        }

        private void InvokeRefreshActiveNotifications()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                RefreshActiveNotificationsDelegate rand = new RefreshActiveNotificationsDelegate(RefreshActiveNotifications);
                this.synchronizingObject.BeginInvoke(rand, null);
            }
            else
            {
                RefreshActiveNotifications();
            }
        }

        private void RefreshActiveNotifications()
        {
            foreach (Display display in this.AvailableDisplays.Values)
            {
                try
                {
                    display.Refresh();
                }
                catch
                {
                    // suppress any exceptions and try refreshing the other displays
                }
            }
        }

        private void AddToMissedNotificationList(PastNotification pn)
        {
            this.missedNotifications.Add(pn);
        }

        void udpListener_NotificationReceived(Growl.UDPLegacy.NotificationPacket np, string receivedFrom)
        {
            ProcessNotification(np, receivedFrom);
        }

        void udpListener_RegistrationReceived(Growl.UDPLegacy.RegistrationPacket rp, string receivedFrom)
        {
            ProcessRegistration(rp, receivedFrom);
        }

        Growl.Connector.Response gntpListener_NotifyReceived(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo)
        {
            Growl.Connector.Response response = null;

            if (IsApplicationRegistered(notification.ApplicationName))
            {
                RegisteredApplication ra = GetRegisteredApplication(notification.ApplicationName);

                if (ra.Notifications.ContainsKey(notification.Name))
                {
                    RegisteredNotification rn = ra.Notifications[notification.Name];

                    if (ra.Enabled && rn.Enabled)
                    {
                        bool sticky = rn.ShouldStayOnScreen(false, this.activityMonitor.IsIdle, notification.Sticky);

                        DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
                        n.UUID = requestInfo.RequestID;
                        n.NotificationID = requestInfo.RequestID;
                        n.ApplicationName = notification.ApplicationName;
                        n.Description = notification.Text;
                        n.Name = notification.Name;
                        n.Priority = (int)rn.Priority(notification.Priority);
                        n.Sticky = sticky;
                        n.Title = notification.Title;

                        if (notification.Icon != null && notification.Icon.IsSet)
                        {
                            n.Image = notification.Icon;
                        }
                        else
                        {
                            n.Image = rn.Icon;
                        }

                        // handle custom attributes
                        n.AddCustomTextAttributes(ra.CustomTextAttributes);
                        n.AddCustomBinaryAttributes(ra.CustomBinaryAttributes);
                        n.AddCustomTextAttributes(rn.CustomTextAttributes);
                        n.AddCustomBinaryAttributes(rn.CustomBinaryAttributes);
                        n.AddCustomTextAttributes(notification.CustomTextAttributes);
                        n.AddCustomBinaryAttributes(notification.CustomBinaryAttributes);

                        // play sound
                        string soundFile;
                        bool shouldPlaySound = rn.ShouldPlaySound(this.growlDefaultSound, out soundFile);
                        if (shouldPlaySound) PlaySound(soundFile);

                        InvokeShowNotification(n, rn.Display, callbackInfo, true, requestInfo);
                        this.OnNotificationReceived(n);

                        PastNotification pn = SaveToHistory(n, requestInfo.RequestID);
                        AddToMissedNotificationList(pn);
                        this.OnNotificationPast(pn);
                    }
                    else
                    {
                        // application or notification type is not enabled - but that is ok
                    }

                    // handle any forwarding after we have already handled it locally
                    // (NOTE: we forward if the application & notification are already registered, even if 
                    // the either one is disabled - this is in case they are configured differently on the
                    // next receiving server)
                    List<string> limitToTheseComputers = null;
                    if (rn.ShouldForward(Properties.Settings.Default.AllowForwarding, out limitToTheseComputers))
                    {
                        // convert urls to binary data
                        if (notification.Icon != null && notification.Icon.IsSet && notification.Icon.IsUrl)
                        {
                            System.Drawing.Image icon = (Image)notification.Icon;
                            notification.Icon = icon;
                        }

                        HandleForwarding(notification, callbackInfo, requestInfo, limitToTheseComputers);
                    }

                    // return OK response
                    response = new Growl.Connector.Response();
                }
                else
                {
                    // notification type is not registered
                    response = new Growl.Connector.Response(Growl.Connector.ErrorCode.UNKNOWN_NOTIFICATION, Growl.Connector.ErrorDescription.NOTIFICATION_TYPE_NOT_REGISTERED);
                }
            }
            else
            {
                response = new Growl.Connector.Response(Growl.Connector.ErrorCode.UNKNOWN_APPLICATION, Growl.Connector.ErrorDescription.APPLICATION_NOT_REGISTERED);
            }

            return response;
        }

        Growl.Connector.Response gntpListener_RegisterReceived(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo)
        {
            Growl.Connector.Response response = null;

            // get the icon
            System.Drawing.Image applicationIcon = null;
            if (application.Icon != null && application.Icon.IsSet)
            {
                try
                {
                    applicationIcon = application.Icon;
                }
                catch
                {
                    applicationIcon = null;
                }
            }

            // deal with notification list
            Dictionary<string, RegisteredNotification> rns = new Dictionary<string, RegisteredNotification>(notificationTypes.Count);
            foreach (Growl.Connector.NotificationType nt in notificationTypes)
            {
                System.Drawing.Image icon = null;
                try
                {
                    if (nt.Icon != null && nt.Icon.IsSet)
                    {
                        icon = nt.Icon;
                    }
                }
                catch
                {
                    icon = null;
                }

                RegisteredNotification rn = new RegisteredNotification(nt.DisplayName, nt.Enabled, nt.CustomTextAttributes, nt.CustomBinaryAttributes);
                rn.Icon = icon;
                rns.Add(nt.Name, rn);
            }

            // update/create the RegisteredApplication
            bool exisiting = false;
            RegisteredApplication ra = null;
            if (IsApplicationRegistered(application.Name))
            {
                exisiting = true;
                ra = GetRegisteredApplication(application.Name);
                ra.Icon = applicationIcon;

                // the application is already registered, so we want to use the new notification list, but preserve any exisiting preferences
                foreach (RegisteredNotification raRn in ra.Notifications.Values)
                {
                    foreach (RegisteredNotification rn in rns.Values)
                    {
                        if (raRn.Name == rn.Name)
                        {
                            // use the exisiting preferences
                            rn.Preferences = raRn.Preferences;
                        }
                    }
                }
                ra.Notifications = rns;
            }
            else
            {
                ra = new RegisteredApplication(application.Name, rns, application.CustomTextAttributes, application.CustomBinaryAttributes);
                ra.Icon = applicationIcon;
                if (Properties.Settings.Default.ManuallyEnableNewApplications) ra.Preferences.PrefEnabled = false;
                this.applications.Add(ra.Name, ra);

                // fire ApplicationRegistered event
                this.OnApplicationRegistered(ra);
            }

            if (ra.Enabled && !exisiting)
            {
                DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
                n.UUID = requestInfo.RequestID;
                n.NotificationID = requestInfo.RequestID;
                n.ApplicationName = ra.Name;
                n.Description = String.Format(Properties.Resources.SystemNotification_AppRegistered_Text, ra.Name);
                n.Name = ra.Name;
                n.Priority = (int)Growl.Connector.Priority.Normal;
                n.Sticky = false;   // registration notifications are never sticky
                n.Title = Properties.Resources.SystemNotification_AppRegistered_Title;

                if (application.Icon != null && application.Icon.IsSet)
                {
                    n.Image = application.Icon;
                }

                // handle custom attributes
                n.AddCustomTextAttributes(application.CustomTextAttributes);
                n.AddCustomBinaryAttributes(application.CustomBinaryAttributes);

                InvokeShowNotification(n, this.growlDefaultDisplay, null, false, requestInfo);

                response = new Growl.Connector.Response();
            }
            else
            {
                // application is disabled or already registered
                response = new Growl.Connector.Response();
            }

            // handle any forwarding after we have handled it locally
            List<string> limitToTheseComputers = null;
            if (ra.ShouldForward(Properties.Settings.Default.AllowForwarding, out limitToTheseComputers))
            {
                // update icon urls to binary data for forwarding
                if (application.Icon != null && application.Icon.IsSet && application.Icon.IsUrl)
                {
                    System.Drawing.Image icon = (Image) application.Icon;
                    if(icon != null)
                        application.Icon = icon;
                }
                foreach (Growl.Connector.NotificationType nt in notificationTypes)
                {
                    if (nt.Icon != null && nt.Icon.IsSet && nt.Icon.IsUrl)
                    {
                        System.Drawing.Image icon = (Image)nt.Icon;
                        if(icon != null)
                            nt.Icon = icon;
                    }
                }

                HandleForwarding(application, notificationTypes, requestInfo, limitToTheseComputers);
            }

            return response;
        }

        Growl.Daemon.SubscriptionResponse gntpListener_SubscribeReceived(Growl.Daemon.Subscriber subscriber, Growl.Daemon.RequestInfo requestInfo)
        {
            bool alertUser = true;
            int ttl = Properties.Settings.Default.SubscriptionTTL;
            SubscribedForwardComputer subscribedComputer = new SubscribedForwardComputer(subscriber, ttl);
            subscribedComputer.Unsubscribed += new SubscribedForwardComputer.SubscribingComputerUnscubscribedEventHandler(sfc_Unsubscribed);
            if (this.forwards.ContainsKey(subscribedComputer.Description))
            {
                ForwardComputer fc = this.forwards[subscribedComputer.Description];
                SubscribedForwardComputer sfc = fc as SubscribedForwardComputer;
                if (sfc != null)
                {
                    subscribedComputer.Enabled = sfc.Enabled;
                    alertUser = false;
                }
            }
            AddForwardComputer(subscribedComputer);

            if (alertUser)
            {
                string title = Properties.Resources.SystemNotification_ClientSubscribed_Title;
                string text = String.Format(Properties.Resources.SystemNotification_ClientSubscribed_Text, subscriber.Name);
                SendSystemNotification(title, text);
            }

            Growl.Daemon.SubscriptionResponse response = new Growl.Daemon.SubscriptionResponse(ttl);

            // SUBSCRIBE requests are *not* forwarded (it could cause an endless loop, and it doesnt make sense anyway)

            return response;
        }

        void sfc_Unsubscribed(SubscribedForwardComputer sfc)
        {
            InvokeOnForwardComputersUpdated();
        }

        private void OnNotificationReceived(Growl.DisplayStyle.Notification n)
        {
            if (this.NotificationReceived != null)
            {
                this.NotificationReceived(n);
            }
        }

        private void OnNotificationPast(PastNotification pn)
        {
            if (this.NotificationPast != null)
            {
                this.NotificationPast(pn);
            }
        }

        private void OnItemLoaded(string itemLoaded)
        {
            if (this.ItemLoaded != null)
            {
                this.ItemLoaded(itemLoaded);
            }
        }

        private PastNotification SaveToHistory(Growl.DisplayStyle.Notification notification, string requestID)
        {
            PastNotification pn = new PastNotification(notification, DateTime.Now);
            if (!String.IsNullOrEmpty(this.historyFolder))
            {
                string path = Growl.CoreLibrary.PathUtility.Combine(this.historyFolder, Growl.CoreLibrary.PathUtility.GetSafeFolderName(notification.ApplicationName));
                Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(path);
                string filename = String.Format(@"{0}.notification", requestID);
                string filepath = Growl.CoreLibrary.PathUtility.Combine(path, filename);
                System.IO.StreamWriter w = System.IO.File.CreateText(filepath);
                using (w)
                {
                    string data = Serialization.SerializeObject(pn);
                    w.Write(data);
                }
            }
            return pn;
        }

        public void CloseAllOpenNotifications()
        {
            foreach (Display display in this.AvailableDisplays.Values)
            {
                display.CloseAllOpenNotifications();
            }

            this.missedNotificationsDisplay.CloseAllOpenNotifications();
        }

        internal void CloseLastNotification()
        {
            if (this.lastUsedDisplay != null)
            {
                this.lastUsedDisplay.CloseLastNotification();
            }
        }

        internal void ClearHistory()
        {
            string[] subfolders = System.IO.Directory.GetDirectories(this.historyFolder);
            foreach (string subfolder in subfolders)
            {
                System.IO.Directory.Delete(subfolder, true);
            }
            string[] files = System.IO.Directory.GetFiles(this.historyFolder);
            if (files != null)
            {
                foreach (string file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
            this.PastNotifications.Clear();
            PastNotification.ClearImageList();
        }

        private bool IsApplicationRegistered(string applicationName)
        {
            return this.applications.ContainsKey(applicationName);
        }

        private RegisteredApplication GetRegisteredApplication(string applicationName)
        {
            return this.applications[applicationName];
        }

        void activityMonitor_ResumedActivity(ActivityMonitor.ActivityMonitorEventArgs args)
        {
            // refresh all displays (this handles the case where sticky notifications were received while the 
            // screen was locked and thus drawn with just a black space)
            InvokeRefreshActiveNotifications();

            lock (this.missedNotifications)
            {
                if(this.missedNotifications.Count > 0)
                {
                    // show missed notification summary
                    List<PastNotification> mn = new List<PastNotification>(this.missedNotifications);
                    InvokeShowMissedNotifications(mn);

                    Console.WriteLine("RESUMED ACTIVITY");
                    Console.WriteLine("You missed {0} notifications while you were away", mn.Count);

                    this.missedNotifications.Clear();
                }
            }
            this.idle = false;
            this.paused = false;
        }

        void activityMonitor_WentIdle(ActivityMonitor.ActivityMonitorEventArgs args)
        {
            if (args.Reason == ActivityMonitor.ActivityMonitorEventReason.ApplicationPaused)
                this.paused = true;
            else if(args.Reason == ActivityMonitor.ActivityMonitorEventReason.UserInactivity)
                this.idle = true;
        }

        void activityMonitor_StillActive(object sender, EventArgs e)
        {
            if(!paused && !idle)    // really, this should never fire while paused or idle, but just in case
                this.missedNotifications.Clear();
        }

        void bonjour_ServiceFound(Bonjour sender, ZeroconfService.NetService service, GrowlBonjourEventArgs args)
        {
            BonjourForwardComputer bfc = MatchBonjourServiceToForwardComputer(service);
            if (bfc != null)
            {
                bfc.Update(service, args);
                InvokeOnBonjourServiceUpdate(bfc);
            }
        }

        void bonjour_ServiceRemoved(Bonjour sender, ZeroconfService.NetService service)
        {
            BonjourForwardComputer bfc = MatchBonjourServiceToForwardComputer(service);
            if (bfc != null)
            {
                bfc.NotAvailable();
                InvokeOnBonjourServiceUpdate(bfc);
            }
        }

        private BonjourForwardComputer MatchBonjourServiceToForwardComputer(ZeroconfService.NetService service)
        {
            if(this.forwards.ContainsKey(service.Name))
            {
                ForwardComputer fc = this.forwards[service.Name];
                BonjourForwardComputer bfc = fc as BonjourForwardComputer;
                if (bfc != null)
                    return bfc;
            }
            return null;
        }

        protected void HandleForwarding(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardComputer fc in this.forwards.Values)
            {
                if((!limit || limitToTheseComputers.Contains(fc.Description)) && (fc.EnabledAndAvailable))
                {
                    try
                    {
                        if (fc.UseUDP)
                        {
                            Growl.UDPLegacy.NotificationType[] types = new Growl.UDPLegacy.NotificationType[notificationTypes.Count];
                            for (int i = 0; i < notificationTypes.Count; i++)
                            {
                                Growl.Connector.NotificationType notificationType = notificationTypes[i];
                                Growl.UDPLegacy.NotificationType nt = new Growl.UDPLegacy.NotificationType(notificationType.Name, notificationType.Enabled);
                                types[i] = nt;
                            }

                            Growl.UDPLegacy.MessageSender netgrowl = new Growl.UDPLegacy.MessageSender(fc.IPAddress, fc.Port, application.Name, fc.Password);
                            netgrowl.Register(ref types);
                        }
                        else
                        {
                            Forwarder growl = new Forwarder(fc.Password, fc.IPAddress, fc.Port, requestInfo);
                            growl.KeyHashAlgorithm = fc.HashAlgorithm;
                            growl.EncryptionAlgorithm = fc.EncryptionAlgorithm;
                            growl.Register(application, notificationTypes.ToArray());
                        }

                        requestInfo.SaveHandlingInfo(String.Format("Forwarded to {0} ({1}:{2}) using {3}", fc.Description, fc.IPAddress, fc.Port, (fc.UseUDP ? "UDP" : "GNTP")));
                    }
                    catch
                    {
                        // swallow any exceptions and move on to the next forwarded computer in the list
                        // this way, if one computer configuration is bad (invalid port or whatever), the rest are not affected
                    }
                }
                else
                {
                    requestInfo.SaveHandlingInfo(String.Format("Not forwarded to {0} ({1}) - ({2})", fc.Description, (fc.Available ? String.Format("{0}:{1}", fc.IPAddress, fc.Port) : "address not resolved"), (limit ? "disallowed by preferences" : (fc.Enabled ? "offline" : "disabled"))));
                }
            }
        }

        protected void HandleForwarding(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardComputer fc in this.forwards.Values)
            {
                if ((!limit || limitToTheseComputers.Contains(fc.Description)) && (fc.EnabledAndAvailable))
                {
                    try
                    {
                        if (fc.UseUDP)
                        {
                            Growl.UDPLegacy.NotificationType nt = new Growl.UDPLegacy.NotificationType(notification.Name, true);
                            Growl.UDPLegacy.MessageSender netgrowl = new Growl.UDPLegacy.MessageSender(fc.IPAddress, fc.Port, notification.ApplicationName, fc.Password);
                            netgrowl.Notify(nt, notification.Title, notification.Text, notification.Priority, notification.Sticky);
                        }
                        else
                        {
                            Forwarder growl = new Forwarder(fc.Password, fc.IPAddress, fc.Port, requestInfo, callbackInfo);
                            growl.KeyHashAlgorithm = fc.HashAlgorithm;
                            growl.EncryptionAlgorithm = fc.EncryptionAlgorithm;
                            growl.ForwardedNotificationCallback +=new Forwarder.ForwardedNotificationCallbackHandler(growl_ForwardedNotificationCallback);
                            growl.Notify(notification, callbackInfo.Context);
                        }

                        requestInfo.SaveHandlingInfo(String.Format("Forwarded to {0} ({1}:{2}) using {3}", fc.Description, fc.IPAddress, fc.Port, (fc.UseUDP ? "UDP" : "GNTP"))); ;
                    }
                    catch
                    {
                        // swallow any exceptions and move on to the next forwarded computer in the list
                        // this way, if one computer configuration is bad (invalid port or whatever), the rest are not affected
                    }
                }
                else
                {
                    requestInfo.SaveHandlingInfo(String.Format("Not forwarded to {0} ({1}) - ({2})", fc.Description, (fc.Available ? String.Format("{0}:{1}", fc.IPAddress, fc.Port) : "address not resolved"), (limit ? "disallowed by preferences" : (fc.Enabled ? "offline" : "disabled"))));
                }
            }
        }

        void growl_ForwardedNotificationCallback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData, Growl.Daemon.CallbackInfo callbackInfo)
        {
            if (this.gntpListener != null)
            {
                callbackInfo.RequestInfo.SaveHandlingInfo(String.Format("Was responded to on {0} - Action: {1}", response.MachineName, callbackData.Result));
                this.gntpListener.WriteResponse(callbackInfo, response);
            }
        }

        private void InvokeOnBonjourServiceUpdate(BonjourForwardComputer bfc)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnBonjourServiceUpdateDelegate obsud = new OnBonjourServiceUpdateDelegate(OnBonjourServiceUpdate);
                this.synchronizingObject.BeginInvoke(obsud, new object[] { bfc });
            }
            else
            {
                OnBonjourServiceUpdate(bfc);
            }
        }

        protected void OnBonjourServiceUpdate(BonjourForwardComputer bfc)
        {
            if (this.BonjourServiceUpdate != null)
            {
                this.BonjourServiceUpdate(bfc);
            }
        }

        public void AddForwardComputer(ForwardComputer fc)
        {
            if (this.forwards.ContainsKey(fc.Description))
                this.forwards.Remove(fc.Description);
            this.forwards.Add(fc.Description, fc);
            InvokeOnForwardComputersUpdated();
        }

        public void RemoveForwardComputer(ForwardComputer fc)
        {
            if (this.forwards.ContainsKey(fc.Description))
                this.forwards.Remove(fc.Description);
            InvokeOnForwardComputersUpdated();
        }

        private void InvokeOnForwardComputersUpdated()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnForwardComputersUpdatedDelegate ofcud = new OnForwardComputersUpdatedDelegate(OnForwardComputersUpdated);
                this.synchronizingObject.BeginInvoke(ofcud, null);
            }
            else
            {
                OnForwardComputersUpdated();
            }
        }

        protected void OnForwardComputersUpdated()
        {
            if (this.ForwardComputersUpdated != null)
            {
                this.ForwardComputersUpdated(this, EventArgs.Empty);
            }
        }

        public void AddSubscription(Subscription subscription)
        {
            if (this.subscriptions.ContainsKey(subscription.Description))
                this.subscriptions.Remove(subscription.Description);

            subscription.StatusChanged += new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
            this.subscriptions.Add(subscription.Description, subscription);
            InvokeOnSubscriptionsUpdated(true);
        }

        public void RemoveSubscription(Subscription subscription)
        {
            if (this.subscriptions.ContainsKey(subscription.Description))
                this.subscriptions.Remove(subscription.Description);
            InvokeOnSubscriptionsUpdated(true);
        }

        private void InvokeOnSubscriptionsUpdated(bool countChanged)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnSubscriptionsUpdatedDelegate osud = new OnSubscriptionsUpdatedDelegate(OnSubscriptionsUpdated);
                this.synchronizingObject.BeginInvoke(osud, new object[] {countChanged});
            }
            else
            {
                OnSubscriptionsUpdated(countChanged);
            }
        }

        protected void OnSubscriptionsUpdated(bool countChanged)
        {
            if (this.SubscriptionsUpdated != null)
            {
                this.SubscriptionsUpdated(countChanged);
            }
        }

        void subscription_StatusChanged(Subscription subscription)
        {
            if (subscription.EnabledAndAvailable)
            {
                this.passwordManager.Add(subscription.SubscriptionPassword);
            }
            else
            {
                this.passwordManager.Remove(subscription.SubscriptionPassword);
            }

            InvokeOnSubscriptionsUpdated(false);
        }

        public bool RequireLocalPassword
        {
            get
            {
                return Properties.Settings.Default.RequireLocalPassword;
            }
            set
            {
                Properties.Settings.Default.RequireLocalPassword = value;
                if (this.gntpListener != null) this.gntpListener.RequireLocalPassword = value;
                if (this.udpListener != null) this.udpListener.RequireLocalPassword = value;
                if (this.udpListenerLocal != null) this.udpListenerLocal.RequireLocalPassword = value;
            }
        }

        public bool AllowNetworkNotifications
        {
            get
            {
                return Properties.Settings.Default.AllowNetworkNotifications;
            }
            set
            {
                Properties.Settings.Default.AllowNetworkNotifications = value;
                if (this.gntpListener != null) this.gntpListener.AllowNetworkNotifications = value;
                if (this.udpListener != null) this.udpListener.AllowNetworkNotifications = value;
                // udpListenerLocal never allows network notifications
            }
        }

        public bool AllowWebNotifications
        {
            get
            {
                return Properties.Settings.Default.AllowWebNotifications;
            }
            set
            {
                Properties.Settings.Default.AllowWebNotifications = value;
                if (this.gntpListener != null) this.gntpListener.AllowFlash = value;
            }
        }

        public bool AllowSubscriptions
        {
            get
            {
                return Properties.Settings.Default.AllowSubscriptions;
            }
            set
            {
                Properties.Settings.Default.AllowSubscriptions = value;
                if (this.gntpListener != null) this.gntpListener.AllowSubscriptions = value;
            }
        }

        private void PlaySound(string soundFile)
        {
            try
            {
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer(soundFile);
                sp.Play();
            }
            catch
            {
                // suppress - dont fail just because the sound could not play
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.activityMonitor != null)
                        this.activityMonitor.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
