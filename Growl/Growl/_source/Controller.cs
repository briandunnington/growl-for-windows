using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Mono.Zeroconf;

namespace Growl
{
    internal class Controller : IDisposable
    {
        public delegate void ApplicationRegisteredDelegate(RegisteredApplication ra);
        public delegate void NotificationReceivedDelegate(Growl.DisplayStyle.Notification n);
        public delegate void NotificationPastDelegate(PastNotification pn);
        public delegate void BonjourServiceUpdateDelegate(BonjourForwardDestination bfc);
        public delegate void SubscriptionsUpdatedDelegate(bool countChanged);
        public delegate void FailedToStartEventHandler(object sender, PortConflictEventArgs args);

        private delegate void ShowNotificationDelegate(Growl.DisplayStyle.Notification n, Display display, Growl.Daemon.CallbackInfo cbInfo, bool recordInMissedNotifications, Growl.Daemon.RequestInfo requestInfo);
        private delegate void ShowMissedNotificationsDelegate(List<PastNotification> missedNotifications);
        private delegate void RefreshActiveNotificationsDelegate();
        internal delegate void ItemLoadedEventHandler(string itemLoaded);
        private delegate void OnBonjourServiceUpdateDelegate(BonjourForwardDestination bfc);
        private delegate void OnForwardDestinationsUpdatedDelegate();
        private delegate void OnSubscriptionsUpdatedDelegate(bool countChanged);

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event FailedToStartEventHandler FailedToStart;
        public event FailedToStartEventHandler FailedToStartUDPLegacy;
        internal event ItemLoadedEventHandler ItemLoaded;
        public event ApplicationRegisteredDelegate ApplicationRegistered;
        public event NotificationReceivedDelegate NotificationReceived;
        public event NotificationPastDelegate NotificationPast;
        public event BonjourServiceUpdateDelegate BonjourServiceUpdate;
        public event EventHandler ForwardDestinationsUpdated;
        public event SubscriptionsUpdatedDelegate SubscriptionsUpdated;

        private static Controller singleton;
        private bool disposed;
        private Mutex mutex;

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

        private bool isStarted;
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
        protected Dictionary<string, ForwardDestination> forwards;
        protected Dictionary<string, ForwardDestination> subscriptions;
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

            this.historyFolder = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"History" + System.IO.Path.DirectorySeparatorChar);
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(this.historyFolder);

            LoadPasswords();
            LoadMiscPrefs();
            LoadDisplays();
            LoadApplications();
            LoadForwardedComputers();
            LoadSubscriptions();
            LoadPastNotifications();

            ForwardDestinationManager.Initialize();

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

            bool createdNew = true;
            this.mutex = new Mutex(true, String.Format(@"Global\{0}", Growl.CoreLibrary.Detector.MUTEX_NAME), out createdNew);

            StartActivityMonitor();

            OnStarted();

            // send a notification that growl is running
            SendSystemNotification(Properties.Resources.SystemNotification_Running_Title, Properties.Resources.SystemNotification_Running_Text);

            this.isStarted = started;

            return true;
        }

        public void Stop()
        {
            this.isStarted = false;
            this.isRunning = false;

            StopActivityMonitor();

            if (this.mutex != null)
            {
                this.mutex.ReleaseMutex();
                this.mutex.Close();
            }

            if (this.gntpListener != null) this.gntpListener.Stop();
            if (this.udpListener != null) this.udpListener.Stop();
            if (this.udpListenerLocal != null) this.udpListenerLocal.Stop();
            this.gntpListener = null;
            this.udpListener = null;
            this.udpListenerLocal = null;

            this.missedNotifications.Clear();

            SaveAppState();

            OnStopped();
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
            Properties.Settings.Default.Save();
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
            Properties.Settings.Default.Save();
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

            // remove any temporary password that may have gotten serialized
            List<string> listToRemove = new List<string>();
            foreach (Growl.Connector.Password p in this.passwordManager.Passwords.Values)
            {
                if (!p.Permanent) listToRemove.Add(p.ActualPassword);
            }
            foreach (string password in listToRemove)
            {
                this.passwordManager.Remove(password);
            }
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
                    string url = cbInfo.Context.CallbackUrl;
                    if (!String.IsNullOrEmpty(url))
                    {
                        // this will only fire on CLICK since that is the more expected behavior
                        // NOTE: there is probably a huge security risk by doing this (for now, I am relying on UriBuilder to protect us from from other types of commands)
                        if (result == Growl.CoreLibrary.CallbackResult.CLICK)
                        {
                            try
                            {
                                System.UriBuilder ub = new UriBuilder(url);

                                // do this in another thread so the Process.Start doesnt block
                                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(ub.Uri.AbsoluteUri);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(OpenUrl), ub.Uri.AbsoluteUri);
                            }
                            catch
                            {
                                // TODO: this is temporary (and thus not localized either) // LOCALIZE:
                                SendSystemNotification("Callback failure", String.Format("An application requested a callback via url, but the url was invalid. The url was: {0}", url));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens a url in the default browser.
        /// </summary>
        /// <param name="state">The url to open</param>
        private void OpenUrl(object state)
        {
            string url = (string)state;
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                // TODO: this is temporary (and thus not localized either) // LOCALIZE:
                SendSystemNotification("Callback failure", string.Format("Could not start or open '{0}':\r\n{1}", url, ex.Message));
            }
        }

        private void LoadForwardedComputers()
        {
            this.forwards = (Dictionary<string, ForwardDestination>)fcSettingSaver.Load();
            if (this.forwards == null) this.forwards = new Dictionary<string, ForwardDestination>();

            // remove any subscribed computers that may have gotten serialized
            Dictionary<string, ForwardDestination> listToRemove = new Dictionary<string, ForwardDestination>();
            foreach (KeyValuePair<string, ForwardDestination> item in this.forwards)
            {
                if (item.Value is SubscribedForwardDestination) listToRemove.Add(item.Key, item.Value);
            }
            foreach (KeyValuePair<string, ForwardDestination> item in listToRemove)
            {
                this.forwards.Remove(item.Key);
            }

            // handle old serialized values that were stored by description instead of key
            Dictionary<string, ForwardDestination> listToUpdate = new Dictionary<string,ForwardDestination>();
            foreach (KeyValuePair<string, ForwardDestination> item in this.forwards)
            {
                if (item.Key != item.Value.Key) listToUpdate.Add(item.Key, item.Value);
            }
            foreach (KeyValuePair<string, ForwardDestination> item in listToUpdate)
            {
                this.forwards.Remove(item.Key);
                this.forwards.Add(item.Value.Key, item.Value);
            }
        }

        private void LoadSubscriptions()
        {
            this.subscriptions = (Dictionary<string, ForwardDestination>)sbSettingSaver.Load();
            if (this.subscriptions == null) this.subscriptions = new Dictionary<string, ForwardDestination>();

            // handle old serialized values that were stored by description instead of key
            Dictionary<string, ForwardDestination> listToUpdate = new Dictionary<string, ForwardDestination>();
            foreach (KeyValuePair<string, ForwardDestination> item in this.subscriptions)
            {
                if (item.Key != item.Value.Key) listToUpdate.Add(item.Key, item.Value);
            }
            foreach (KeyValuePair<string, ForwardDestination> item in listToUpdate)
            {
                this.subscriptions.Remove(item.Key);
                this.subscriptions.Add(item.Value.Key, item.Value);
            }

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

            DateTime cutoffTime = DateTime.Now.Date.AddDays(-8); // this needs to change at some point to it is not hard-coded in case the HistoryListView control changes
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(this.historyFolder);
            System.IO.FileInfo[] files = d.GetFiles("*.notification", System.IO.SearchOption.AllDirectories);

            foreach (System.IO.FileInfo file in files)
            {
                if (file.CreationTime < cutoffTime)
                {
                    file.Delete();
                }
                else
                {
                    string data = System.IO.File.ReadAllText(file.FullName);
                    try
                    {
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
                    catch (Exception ex)
                    {
                        Utility.WriteDebugInfo(String.Format("Deserialization of history item failed. {0} :: {1} :: {2}", ex.Message, ex.StackTrace, data));
                    }
                }
            }
        }

        private void SaveAppState()
        {
            SaveApplicationPrefs();
            SavePasswordPrefs();
            SaveForwardPrefs();
            SaveSubsriptionPrefs();
        }

        public void SaveApplicationPrefs()
        {
            if (this.raSettingSaver != null) this.raSettingSaver.Save(this.applications);
        }

        public void SaveForwardPrefs()
        {
            if (this.fcSettingSaver != null) this.fcSettingSaver.Save(this.forwards);
        }

        public void SaveSubsriptionPrefs()
        {
            if (this.sbSettingSaver != null) this.sbSettingSaver.Save(this.subscriptions);
        }

        public void SavePasswordPrefs()
        {
            if (this.pmSettingSaver != null) this.pmSettingSaver.Save(this.passwordManager);
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
                return DisplayStyleManager.GetAvailableDisplayStyles();
            }
        }

        public Dictionary<string, ForwardDestination> ForwardDestinations
        {
            get
            {
                return this.forwards;
            }
        }

        public Dictionary<string, ForwardDestination> Subscriptions
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
                    foreach (ForwardDestination fc in this.ForwardDestinations.Values)
                    {
                        if (fc is BonjourForwardDestination)
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
                this.activityMonitor.IdleAfterSeconds = value;
            }
        }

        protected void OnStarted()
        {
            if (this.Started != null)
            {
                this.Started(this, EventArgs.Empty);
            }
        }

        protected void OnStopped()
        {
            if (this.Stopped != null)
            {
                this.Stopped(this, EventArgs.Empty);
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
                this.synchronizingObject.Invoke(snd, new object[] { notification, display, cbInfo, recordInMissedNotifications, requestInfo });
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
                this.synchronizingObject.Invoke(smnd, new object[] { missedNotifications });
            }
            else
            {
                ShowMissedNotifications(missedNotifications);
            }
        }

        private void ShowMissedNotifications(List<PastNotification> missedNotifications)
        {
            if(!Properties.Settings.Default.DisableMissedNotifications)
                this.missedNotificationsDisplay.HandleNotification(missedNotifications);
        }

        private void InvokeRefreshActiveNotifications()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                RefreshActiveNotificationsDelegate rand = new RefreshActiveNotificationsDelegate(RefreshActiveNotifications);
                this.synchronizingObject.Invoke(rand, null);
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
            if(this.CheckForIdle || this.idle || this.paused)
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

                    bool sticky = rn.ShouldStayOnScreen(false, this.activityMonitor.IsIdle, notification.Sticky);
                    Growl.Connector.Priority priority = rn.Priority(notification.Priority);

                    if (ra.Enabled && rn.Enabled)
                    {
                        DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
                        n.UUID = requestInfo.RequestID;
                        n.NotificationID = requestInfo.RequestID;
                        n.CoalescingID = notification.CoalescingID;
                        n.ApplicationName = notification.ApplicationName;
                        n.Description = notification.Text;
                        n.Name = notification.Name;
                        n.Priority = (int)priority;
                        n.Sticky = sticky;
                        n.Title = notification.Title;
                        n.Duration = rn.Duration;
                        if (requestInfo.WasForwarded()) n.OriginMachineName = notification.MachineName;

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
                        notification.Priority = priority;

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
                n.Image = ra.Icon;
                if (requestInfo.WasForwarded()) n.OriginMachineName = application.MachineName;

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
            SubscribedForwardDestination subscribedComputer = new SubscribedForwardDestination(subscriber, ttl);
            subscribedComputer.Unsubscribed += new SubscribedForwardDestination.SubscribingComputerUnscubscribedEventHandler(sfc_Unsubscribed);
            if (this.forwards.ContainsKey(subscribedComputer.Key))
            {
                ForwardDestination fc = this.forwards[subscribedComputer.Key];
                SubscribedForwardDestination sfc = fc as SubscribedForwardDestination;
                if (sfc != null)
                {
                    subscribedComputer.Enabled = sfc.Enabled;
                    alertUser = false;
                }
            }
            AddForwardDestination(subscribedComputer);

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

        void sfc_Unsubscribed(SubscribedForwardDestination sfc)
        {
            InvokeOnForwardDestinationsUpdated();
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
                    lock (pn.Image)
                    {
                        string data = Serialization.SerializeObject(pn);
                        w.Write(data);
                    }
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
            else if (args.Reason == ActivityMonitor.ActivityMonitorEventReason.UserInactivity)
                this.idle = true;
            else if (args.Reason == ActivityMonitor.ActivityMonitorEventReason.DesktopLocked)
                this.idle = true;
        }

        void activityMonitor_StillActive(object sender, EventArgs e)
        {
            if(!paused && !idle)    // really, this should never fire while paused or idle, but just in case
                this.missedNotifications.Clear();
        }

        void bonjour_ServiceFound(Bonjour sender, IResolvableService service, GrowlBonjourEventArgs args)
        {
            BonjourForwardDestination bfc = MatchBonjourServiceToForwardDestination(service);
            if (bfc != null)
            {
                bfc.Update(service, args);
                InvokeOnBonjourServiceUpdate(bfc);
            }
        }

        void bonjour_ServiceRemoved(Bonjour sender, IResolvableService service)
        {
            BonjourForwardDestination bfc = MatchBonjourServiceToForwardDestination(service);
            if (bfc != null)
            {
                bfc.NotAvailable();
                InvokeOnBonjourServiceUpdate(bfc);
            }
        }

        private BonjourForwardDestination MatchBonjourServiceToForwardDestination(IResolvableService service)
        {
            if(this.forwards.ContainsKey(service.Name))
            {
                ForwardDestination fc = this.forwards[service.Name];
                BonjourForwardDestination bfc = fc as BonjourForwardDestination;
                if (bfc != null)
                    return bfc;
            }
            return null;
        }

        protected void HandleForwarding(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardDestination fc in this.forwards.Values)
            {
                if((!limit || limitToTheseComputers.Contains(fc.Key)) && (fc.EnabledAndAvailable))
                {
                    try
                    {
                        requestInfo.SaveHandlingInfo(String.Format("Forwarding to {0} ({1})", fc.Description, fc.AddressDisplay));
                        fc.ForwardRegistration(application, notificationTypes, requestInfo, this.activityMonitor.IsIdle);
                    }
                    catch
                    {
                        // swallow any exceptions and move on to the next forwarded computer in the list
                        // this way, if one computer configuration is bad (invalid port or whatever), the rest are not affected
                    }
                }
                else
                {
                    requestInfo.SaveHandlingInfo(String.Format("Not forwarded to {0} ({1}) - ({2})", fc.Description, fc.AddressDisplay, (limit ? "disallowed by preferences" : (fc.Enabled ? "offline" : "disabled"))));
                }
            }
        }

        protected void HandleForwarding(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardDestination fc in this.forwards.Values)
            {
                if ((!limit || limitToTheseComputers.Contains(fc.Key)) && (fc.EnabledAndAvailable))
                {
                    try
                    {
                        requestInfo.SaveHandlingInfo(String.Format("Forwarding to {0} ({1})", fc.Description, fc.AddressDisplay));
                        fc.ForwardNotification(notification, callbackInfo, requestInfo, this.activityMonitor.IsIdle, new Forwarder.ForwardedNotificationCallbackHandler(growl_ForwardedNotificationCallback));
                    }
                    catch
                    {
                        // swallow any exceptions and move on to the next forwarded computer in the list
                        // this way, if one computer configuration is bad (invalid port or whatever), the rest are not affected
                    }
                }
                else
                {
                    requestInfo.SaveHandlingInfo(String.Format("Not forwarded to {0} ({1}) - ({2})", fc.Description, fc.AddressDisplay, (limit ? "disallowed by preferences" : (fc.Enabled ? "offline" : "disabled"))));
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

        private void InvokeOnBonjourServiceUpdate(BonjourForwardDestination bfc)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnBonjourServiceUpdateDelegate obsud = new OnBonjourServiceUpdateDelegate(OnBonjourServiceUpdate);
                this.synchronizingObject.Invoke(obsud, new object[] { bfc });
            }
            else
            {
                OnBonjourServiceUpdate(bfc);
            }
        }

        protected void OnBonjourServiceUpdate(BonjourForwardDestination bfc)
        {
            if (this.BonjourServiceUpdate != null)
            {
                this.BonjourServiceUpdate(bfc);
            }
        }

        public void AddForwardDestination(ForwardDestination fc)
        {
            if (this.forwards.ContainsKey(fc.Key))
                this.forwards.Remove(fc.Key);
            this.forwards.Add(fc.Key, fc);
            InvokeOnForwardDestinationsUpdated();
        }

        public void RemoveForwardDestination(ForwardDestination fc)
        {
            if (this.forwards.ContainsKey(fc.Key))
                this.forwards.Remove(fc.Key);
            InvokeOnForwardDestinationsUpdated();
        }

        private void InvokeOnForwardDestinationsUpdated()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnForwardDestinationsUpdatedDelegate ofcud = new OnForwardDestinationsUpdatedDelegate(OnForwardDestinationsUpdated);
                this.synchronizingObject.Invoke(ofcud, null);
            }
            else
            {
                OnForwardDestinationsUpdated();
            }
        }

        protected void OnForwardDestinationsUpdated()
        {
            if (this.ForwardDestinationsUpdated != null)
            {
                this.ForwardDestinationsUpdated(this, EventArgs.Empty);
            }
        }

        public void AddSubscription(Subscription subscription)
        {
            if (this.subscriptions.ContainsKey(subscription.Key))
                this.subscriptions.Remove(subscription.Key);

            subscription.StatusChanged += new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
            this.subscriptions.Add(subscription.Key, subscription);
            InvokeOnSubscriptionsUpdated(true);
        }

        public void RemoveSubscription(Subscription subscription)
        {
            if (this.subscriptions.ContainsKey(subscription.Key))
                this.subscriptions.Remove(subscription.Key);
            InvokeOnSubscriptionsUpdated(true);
        }

        private void InvokeOnSubscriptionsUpdated(bool countChanged)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                OnSubscriptionsUpdatedDelegate osud = new OnSubscriptionsUpdatedDelegate(OnSubscriptionsUpdated);
                this.synchronizingObject.Invoke(osud, new object[] {countChanged});
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
                this.passwordManager.Add(subscription.SubscriptionPassword, false);
            }
            else
            {
                this.passwordManager.Remove(subscription.SubscriptionPassword);
            }

            InvokeOnSubscriptionsUpdated(false);
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public bool IsOn
        {
            get
            {
                return this.isStarted;
            }
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.Save();
                if (this.gntpListener != null) this.gntpListener.AllowSubscriptions = value;
            }
        }

        private void PlaySound(string soundFile)
        {
            if (!Properties.Settings.Default.MuteAllSounds)
            {
                try
                {
                    System.Media.SoundPlayer sp = new System.Media.SoundPlayer(soundFile);
                    using (sp)
                    {
                        sp.Play();
                    }
                }
                catch
                {
                    // suppress - dont fail just because the sound could not play
                }
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
                    if (this.bonjour != null)
                        this.bonjour.Dispose();

                    if (this.activityMonitor != null)
                        this.activityMonitor.Dispose();

                    if (this.mutex != null)
                        this.mutex.Close();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
