using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Mono.Zeroconf;
using Growl.Destinations;

namespace Growl
{
    internal class Controller : IDisposable
    {
        public delegate void FailedToStartEventHandler(object sender, PortConflictEventArgs args);
        public delegate void ApplicationRegisteredDelegate(RegisteredApplication ra);
        public delegate void NotificationReceivedDelegate(Growl.DisplayStyle.Notification n);
        public delegate void NotificationPastDelegate(PastNotification pn);
        public delegate void SubscriptionsUpdatedDelegate(bool countChanged);
        public delegate void BonjourServiceUpdateDelegate(BonjourForwardDestination bfc);

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event FailedToStartEventHandler FailedToStart;
        public event FailedToStartEventHandler FailedToStartUDPLegacy;
        public event ApplicationRegisteredDelegate ApplicationRegistered;
        public event NotificationReceivedDelegate NotificationReceived;
        public event NotificationPastDelegate NotificationPast;
        public event EventHandler ForwardDestinationsUpdated;
        public event SubscriptionsUpdatedDelegate SubscriptionsUpdated;
        public event BonjourServiceUpdateDelegate BonjourServiceUpdate;

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
        
        private Growl.Connector.PasswordManager passwordManager;
        private ActivityMonitor activityMonitor;
        private Bonjour bonjour;
        private Display lastUsedDisplay;
        private List<PastNotification> missedNotifications = new List<PastNotification>();
        private MissedNotificationsDisplay missedNotificationsDisplay;

        private bool isStarted;
        private bool isRunning;
        private bool paused;
        private bool idle;

        private PrefSound growlDefaultSound = PrefSound.None;

        private Display growlDefaultDisplay;
        private string defaultDisplayName;

        protected Dictionary<string, RegisteredApplication> applications;
        protected Dictionary<string, ForwardDestination> forwards;
        protected Dictionary<string, Subscription> subscriptions;

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
            this.appPath = appPath;
            this.synchronizingObject = synchronizingObject;

            this.historyFolder = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"History\");
            PastNotificationManager.HistoryFolder = this.historyFolder;

            string imageCacheFolder = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"ImageCache\");
            ImageCache.CacheFolder = imageCacheFolder;

            // handle any upgrade logic
            bool upgrade = (Growl.Properties.Settings.Default.SettingsVersion < 2);
            if (upgrade)
            {
                Utility.WriteDebugInfo("UPGRADE INITIATED");

                SettingSaver raSettingSaver = new SettingSaver(REGISTERED_APPLICATIONS_SETTINGS_FILENAME, LegacyDeserializers.LegacyDeserializationHelper.OldApplicationsHelper);
                raSettingSaver.Save(raSettingSaver.Load());

                SettingSaver fcSettingSaver = new SettingSaver(FORWARD_COMPUTERS_SETTINGS_FILENAME, LegacyDeserializers.LegacyDeserializationHelper.OldForwardDestinationHelper);
                fcSettingSaver.Save(fcSettingSaver.Load());

                SettingSaver sbSettingSaver = new SettingSaver(SUBSCRIPTIONS_SETTINGS_FILENAME, LegacyDeserializers.LegacyDeserializationHelper.OldSubscriptionHelper);
                sbSettingSaver.Save(sbSettingSaver.Load());

                PastNotificationManager.DeleteHistory();

                Growl.Properties.Settings.Default.SettingsVersion = 2;

                Utility.WriteDebugInfo(String.Format("UPGRADE COMPLETED - NOW AT VERSION: {0}", Growl.Properties.Settings.Default.SettingsVersion));
            }

            DisplayStyleManager.DisplayLoaded += new DisplayStyleManager.DisplayLoadedEventHandler(DisplayStyleManager_DisplayLoaded);

            LoadPasswords();
            LoadMiscPrefs();
            LoadDisplays();     // this could take a long time as well, but other things depend on it so we have to do it synchronously and just wait
            LoadApplications();
            LoadForwardedComputers();
            LoadSubscriptions();

            // this must come *after* LoadForwardedComputers has ran
            StartBonjour();

            this.activityMonitor = new ActivityMonitor();
            this.activityMonitor.WentIdle += new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_WentIdle);
            this.activityMonitor.ResumedActivity += new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_ResumedActivity);
            this.activityMonitor.StillActive += new EventHandler(activityMonitor_StillActive);
        }

        public bool Start()
        {
            bool started = false;

            this.gntpListener = new Growl.Daemon.GrowlServer(Growl.Connector.GrowlConnector.TCP_PORT, this.passwordManager, Utility.UserSettingFolder);
            this.gntpListener.RegisterReceived += new Growl.Daemon.GrowlServer.RegisterReceivedEventHandler(gntpListener_RegisterReceived);
            this.gntpListener.NotifyReceived += new Growl.Daemon.GrowlServer.NotifyReceivedEventHandler(gntpListener_NotifyReceived);
            this.gntpListener.SubscribeReceived += new Growl.Daemon.GrowlServer.SubscribeReceivedEventHandler(gntpListener_SubscribeReceived);
            this.gntpListener.LoggingEnabled = ApplicationMain.LoggingEnabled;
            this.gntpListener.AllowFlash = true;
            this.gntpListener.AllowNetworkNotifications = true;
            this.gntpListener.AllowSubscriptions = true;
            this.gntpListener.RequireLocalPassword = false;
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
                this.udpListener = new Growl.UDPLegacy.MessageReceiver(Growl.UDPLegacy.MessageReceiver.NETWORK_PORT, this.passwordManager, ApplicationMain.LoggingEnabled, udpLogFolder);
                this.udpListener.RegistrationReceived += new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                this.udpListener.NotificationReceived += new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                this.udpListener.AllowNetworkNotifications = true;
                this.udpListener.RequireLocalPassword = false;
                started = this.udpListener.Start();
                if (!started)
                {
                    if (this.gntpListener != null) this.gntpListener.Stop();    // stop the GNTP listener if it was already started
                    this.OnFailedToStartUDPLegacy(this, new PortConflictEventArgs(Growl.UDPLegacy.MessageReceiver.NETWORK_PORT));
                    return false;
                }

                // this is for local UDP requests (old GFW local protocol)
                this.udpListenerLocal = new Growl.UDPLegacy.MessageReceiver(Growl.UDPLegacy.MessageReceiver.LOCAL_PORT, this.passwordManager, ApplicationMain.LoggingEnabled, udpLogFolder);
                this.udpListenerLocal.RegistrationReceived += new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                this.udpListenerLocal.NotificationReceived += new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                this.udpListenerLocal.AllowNetworkNotifications = false;    // always false
                this.udpListenerLocal.RequireLocalPassword = false;
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

            this.isStarted = started;
            OnStarted();

            TimeSpan ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("FINAL LOAD TIME: {0}", ts.TotalSeconds);

            // send a notification that growl is running
            if (!ApplicationMain.SilentMode)
                SendSystemNotification(Properties.Resources.SystemNotification_Running_Title, Properties.Resources.SystemNotification_Running_Text);

            StartActivityMonitor();

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

            if (this.gntpListener != null)
            {
                this.gntpListener.Stop();
                this.gntpListener.Dispose();
                this.gntpListener = null;
            }
            if (this.udpListener != null)
            {
                this.udpListener.Stop();
                this.udpListener.Dispose();
                this.udpListener = null;
            }
            if (this.udpListenerLocal != null)
            {
                this.udpListenerLocal.Stop();
                this.udpListenerLocal.Dispose();
                this.udpListenerLocal = null;
            }

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

        private void StartBonjour()
        {
            if (Bonjour.IsSupported)
            {
                this.bonjour = new Bonjour();
                this.bonjour.ServiceFound += new Bonjour.NetServiceFoundEventHandler(bonjour_ServiceFound);
                this.bonjour.ServiceRemoved += new Bonjour.NetServiceRemovedEventHandler(bonjour_ServiceRemoved);
                this.bonjour.Start();
            }
        }

        # region Load* Methods

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

        private void LoadMiscPrefs()
        {
            // default sound
            string defaultSoundPath = Properties.Settings.Default.DefaultSound;
            PrefSound ps = PrefSound.FromFilePath(defaultSoundPath);
            this.growlDefaultSound = ps;
        }

        private void LoadApplications()
        {
            this.applications = (Dictionary<string, RegisteredApplication>)raSettingSaver.Load();
            if (this.applications == null) this.applications = new Dictionary<string, RegisteredApplication>();

            foreach (RegisteredApplication ra in this.applications.Values)
            {
                ra.LinkNotifications();
            }
        }

        private void LoadDisplays()
        {
            this.missedNotificationsDisplay = new MissedNotificationsDisplay();

            TimeSpan ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - Before DisplayStyleManager.Load(): " + ts.TotalSeconds.ToString());

            DisplayStyleManager.Load();

            ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - After DisplayStyleManager.Load(): " + ts.TotalSeconds.ToString());

            try
            {
                this.defaultDisplayName = Properties.Settings.Default.DefaultDisplay;
                this.growlDefaultDisplay = DisplayStyleManager.AvailableDisplayStyles[this.defaultDisplayName];
            }
            catch
            {
                // the default display was not found. that is bad
                Utility.WriteDebugInfo("The default display '{0}' was not found.", this.defaultDisplayName);
                // fall back to Standard
                this.growlDefaultDisplay = DisplayStyleManager.AvailableDisplayStyles["Standard"];
            }
            Display.Default.Update(this.growlDefaultDisplay);
        }

        private void LoadForwardedComputers()
        {
            TimeSpan ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - Before ForwardDestinationManager.Load(): " + ts.TotalSeconds.ToString());

            ForwardDestinationManager.Load();

            ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - After ForwardDestinationManager.Load(): " + ts.TotalSeconds.ToString());

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
            Dictionary<string, ForwardDestination> listToUpdate = new Dictionary<string, ForwardDestination>();
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
            TimeSpan ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - Before SubscriptionManager.Load(): " + ts.TotalSeconds.ToString());

            SubscriptionManager.Load();

            ts = DateTime.Now - ApplicationMain.st;
            Utility.WriteDebugInfo("TIMESTAMP - After SubscriptionManager.Load(): " + ts.TotalSeconds.ToString());

            this.subscriptions = (Dictionary<string, Subscription>)sbSettingSaver.Load();
            if (this.subscriptions == null) this.subscriptions = new Dictionary<string, Subscription>();

            // handle old serialized values that were stored by description instead of key
            Dictionary<string, Subscription> listToUpdate = new Dictionary<string, Subscription>();
            foreach (KeyValuePair<string, Subscription> item in this.subscriptions)
            {
                if (item.Key != item.Value.Key) listToUpdate.Add(item.Key, item.Value);
            }
            foreach (KeyValuePair<string, Subscription> item in listToUpdate)
            {
                this.subscriptions.Remove(item.Key);
                this.subscriptions.Add(item.Value.Key, item.Value);
            }

            foreach (Subscription subscription in this.subscriptions.Values)
            {
                subscription.StatusChanged += new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
                try
                {
                    SubscriptionManager.Update(subscription, Properties.Settings.Default.EnableSubscriptions);
                }
                catch
                {
                    Utility.WriteDebugInfo(String.Format("EXCEPTION: '{0}' Update() failed", subscription.Description));
                }
            }
        }

        # endregion Load* Methods

        #region Save* Methods

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

        public void SavePasswordPrefs()
        {
            if (this.pmSettingSaver != null) this.pmSettingSaver.Save(this.passwordManager);
        }

        public void SaveForwardPrefs()
        {
            if (this.fcSettingSaver != null) this.fcSettingSaver.Save(this.forwards);
        }

        public void SaveSubsriptionPrefs()
        {
            if (this.sbSettingSaver != null) this.sbSettingSaver.Save(this.subscriptions);
        }

        #endregion Save* Methods

        # region Notification handling

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
            Growl.Connector.RequestInfo requestInfo = new Growl.Connector.RequestInfo();
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
            Growl.Connector.RequestInfo requestInfo = new Growl.Connector.RequestInfo();
            gntpListener_NotifyReceived(notification, null, requestInfo);
        }

        void udpListener_NotificationReceived(Growl.UDPLegacy.NotificationPacket np, string receivedFrom)
        {
            ProcessNotification(np, receivedFrom);
        }

        void udpListener_RegistrationReceived(Growl.UDPLegacy.RegistrationPacket rp, string receivedFrom)
        {
            ProcessRegistration(rp, receivedFrom);
        }

        Growl.Connector.Response gntpListener_NotifyReceived(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Connector.RequestInfo requestInfo)
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
                            n.Image = rn.GetIcon();
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

                        ShowNotification(n, rn.Display, callbackInfo, true, requestInfo);
                        this.OnNotificationReceived(n);

                        PastNotification pn = SaveToHistory(n, requestInfo.RequestID);
                        AddToMissedNotificationList(pn);
                        this.OnNotificationPast(pn);

                        // handle any forwarding after we have already handled it locally
                        // (NOTE: as of 03.09.2010 (v2.0.2), notifications are NOT forwarded if
                        // they are not enabled (unlike before).
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
                    }
                    else
                    {
                        // application or notification type is not enabled - but that is ok
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

        Growl.Connector.Response gntpListener_RegisterReceived(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo)
        {
            Growl.Connector.Response response = null;

            // get the icon
            Growl.CoreLibrary.Resource applicationIcon = null;
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
                Growl.CoreLibrary.Resource icon = null;
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
                rn.SetIcon(icon, application.Name);
                rns.Add(nt.Name, rn);
            }

            // update/create the RegisteredApplication
            bool exisiting = false;
            RegisteredApplication ra = null;
            if (IsApplicationRegistered(application.Name))
            {
                exisiting = true;
                ra = GetRegisteredApplication(application.Name);
                ra.SetIcon(applicationIcon);

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
                ra.SetIcon(applicationIcon);
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
                n.Image = ra.GetIcon();
                if (requestInfo.WasForwarded()) n.OriginMachineName = application.MachineName;

                // handle custom attributes
                n.AddCustomTextAttributes(application.CustomTextAttributes);
                n.AddCustomBinaryAttributes(application.CustomBinaryAttributes);

                ShowNotification(n, this.growlDefaultDisplay, null, false, requestInfo);

                response = new Growl.Connector.Response();
            }
            else
            {
                // application is disabled or already registered
                response = new Growl.Connector.Response();
            }

            // handle any forwarding after we have handled it locally
            // (NOTE: as of 03.09.2010 (v2.0.2), notifications are NOT forwarded if
            // they are not enabled (unlike before).
            if (ra.Enabled)
            {
                List<string> limitToTheseComputers = null;
                if (ra.ShouldForward(Properties.Settings.Default.AllowForwarding, out limitToTheseComputers))
                {
                    // update icon urls to binary data for forwarding
                    if (application.Icon != null && application.Icon.IsSet && application.Icon.IsUrl)
                    {
                        System.Drawing.Image icon = (Image)application.Icon;
                        if (icon != null)
                            application.Icon = icon;
                    }
                    foreach (Growl.Connector.NotificationType nt in notificationTypes)
                    {
                        if (nt.Icon != null && nt.Icon.IsSet && nt.Icon.IsUrl)
                        {
                            System.Drawing.Image icon = (Image)nt.Icon;
                            if (icon != null)
                                nt.Icon = icon;
                        }
                    }

                    HandleForwarding(application, notificationTypes, requestInfo, limitToTheseComputers);
                }
            }

            return response;
        }

        Growl.Daemon.SubscriptionResponse gntpListener_SubscribeReceived(Growl.Daemon.Subscriber subscriber, Growl.Connector.RequestInfo requestInfo)
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

        # endregion Notification handling

        private bool IsApplicationRegistered(string applicationName)
        {
            return this.applications.ContainsKey(applicationName);
        }

        private RegisteredApplication GetRegisteredApplication(string applicationName)
        {
            return this.applications[applicationName];
        }

        private void SendSystemNotification(string title, string text)
        {
            SendSystemNotification(title, text, this.growlDefaultDisplay);
        }

        internal void SendSystemNotification(string title, string text, Display display)
        {
            Growl.Connector.RequestInfo requestInfo = new Growl.Connector.RequestInfo();  // this is not used, but needed as a placeholder

            DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
            n.UUID = requestInfo.RequestID;
            n.ApplicationName = Properties.Resources.SystemNotification_ApplicationName;
            n.Description = text;
            n.Name = "Growl System Message";
            n.Priority = (int)Growl.Connector.Priority.Normal;
            n.Sticky = false;   // system notifications are never sticky
            n.Title = title;
            n.Image = Growl.FormResources.growl;

            ShowNotification(n, display, null, false, requestInfo);
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

        public void CloseAllOpenNotifications()
        {
            foreach (Display display in DisplayStyleManager.AvailableDisplayStyles.Values)
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

        protected void HandleForwarding(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardDestination fc in this.forwards.Values)
            {
                if ((!limit || limitToTheseComputers.Contains(fc.Key)) && (fc.EnabledAndAvailable))
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

        protected void HandleForwarding(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Connector.RequestInfo requestInfo, List<string> limitToTheseComputers)
        {
            bool limit = (limitToTheseComputers != null);
            foreach (ForwardDestination fc in this.forwards.Values)
            {
                if ((!limit || limitToTheseComputers.Contains(fc.Key)) && (fc.EnabledAndAvailable))
                {
                    try
                    {
                        Growl.Connector.CallbackContext context = null;
                        if (callbackInfo != null) context = callbackInfo.Context;
                        requestInfo.SaveHandlingInfo(String.Format("Forwarding to {0} ({1})", fc.Description, fc.AddressDisplay));

                        callbackInfo.ForwardedNotificationCallback += new Growl.Daemon.CallbackInfo.ForwardedNotificationCallbackHandler(growl_ForwardedNotificationCallback);
                        fc.ForwardNotification(notification, context, requestInfo, this.activityMonitor.IsIdle, new ForwardDestination.ForwardedNotificationCallbackHandler(callbackInfo.HandleCallbackFromForwarder));
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

        public void AddForwardDestination(ForwardDestination fc)
        {
            if (fc != null)
            {
                if (this.forwards.ContainsKey(fc.Key))
                    this.forwards.Remove(fc.Key);
                this.forwards.Add(fc.Key, fc);
                OnForwardDestinationsUpdated();
            }
        }

        public void RemoveForwardDestination(ForwardDestination fc)
        {
            if (this.forwards.ContainsKey(fc.Key))
                this.forwards.Remove(fc.Key);
            OnForwardDestinationsUpdated();
        }

        public void AddSubscription(Subscription subscription)
        {
            if (subscription != null)
            {
                if (this.subscriptions.ContainsKey(subscription.Key))
                {
                    Subscription s = this.subscriptions[subscription.Key];
                    this.subscriptions.Remove(subscription.Key);
                    s.Kill();
                    s.StatusChanged -= new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
                    s = null;
                }

                subscription.StatusChanged += new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
                this.subscriptions.Add(subscription.Key, subscription);
                SubscriptionManager.Update(subscription, Properties.Settings.Default.EnableSubscriptions);
                OnSubscriptionsUpdated(true);
            }
        }

        public void RemoveSubscription(Subscription subscription)
        {
            if (subscription != null)
            {
                if (this.subscriptions.ContainsKey(subscription.Key))
                {
                    Subscription s = this.subscriptions[subscription.Key];
                    this.subscriptions.Remove(subscription.Key);
                    s.Kill();
                    s.StatusChanged -= new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
                    s = null;
                }
                OnSubscriptionsUpdated(true);
            }
        }

        private void ShowNotification(Growl.DisplayStyle.Notification notification, Display display, Growl.Daemon.CallbackInfo cbInfo, bool recordInMissedNotifications, Growl.Connector.RequestInfo requestInfo)
        {
            // could be cross-thread, so check for invokerequired
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    ShowNotification(notification, display, cbInfo, recordInMissedNotifications, requestInfo);
                });

                this.synchronizingObject.Invoke(invoker, null);
            }
            else
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
        }

        private void ShowMissedNotifications(List<PastNotification> missedNotifications)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    ShowMissedNotifications(missedNotifications);
                });

                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (!Properties.Settings.Default.DisableMissedNotifications)
                    this.missedNotificationsDisplay.HandleNotification(missedNotifications);
            }
        }

        private void AddToMissedNotificationList(PastNotification pn)
        {
            if (this.CheckForIdle || this.idle || this.paused)
                this.missedNotifications.Add(pn);
        }

        private PastNotification SaveToHistory(Growl.DisplayStyle.Notification notification, string requestID)
        {
            PastNotification pn = PastNotificationManager.Save(notification, requestID, DateTime.Now);
            return pn;
        }

        private void RefreshActiveNotifications()
        {
            // could be cross-thread, so check for invokerequired
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    RefreshActiveNotifications();
                });

                this.synchronizingObject.Invoke(invoker, null);
            }
            else
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

        private BonjourForwardDestination MatchBonjourServiceToForwardDestination(IResolvableService service)
        {
            if (this.forwards.ContainsKey(service.Name))
            {
                ForwardDestination fc = this.forwards[service.Name];
                BonjourForwardDestination bfc = fc as BonjourForwardDestination;
                if (bfc != null)
                    return bfc;
            }
            return null;
        }

        # region Activity Monitor Event Handlers

        void activityMonitor_ResumedActivity(ActivityMonitor.ActivityMonitorEventArgs args)
        {
            // refresh all displays (this handles the case where sticky notifications were received while the 
            // screen was locked and thus drawn with just a black space)
            RefreshActiveNotifications();

            lock (this.missedNotifications)
            {
                if (this.missedNotifications.Count > 0)
                {
                    // show missed notification summary
                    List<PastNotification> mn = new List<PastNotification>(this.missedNotifications);
                    ShowMissedNotifications(mn);

                    Utility.WriteDebugInfo("RESUMED ACTIVITY - You missed {0} notifications while you were away", mn.Count);

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
            if (!paused && !idle)    // really, this should never fire while paused or idle, but just in case
                this.missedNotifications.Clear();
        }

        # endregion Activity Monitor Event Handlers

        # region Bonjour Event Handlers

        void bonjour_ServiceFound(Bonjour sender, IResolvableService service, BonjourEventArgs args)
        {
            BonjourForwardDestination bfc = MatchBonjourServiceToForwardDestination(service);
            if (bfc != null)
            {
                bfc.Update(service, args);
                OnBonjourServiceUpdate(bfc);
            }
        }

        void bonjour_ServiceRemoved(Bonjour sender, IResolvableService service)
        {
            BonjourForwardDestination bfc = MatchBonjourServiceToForwardDestination(service);
            if (bfc != null)
            {
                bfc.NotAvailable();
                OnBonjourServiceUpdate(bfc);
            }
        }

        # endregion Bonjour Event Handlers

        void DisplayStyleManager_DisplayLoaded(Display display)
        {
            display.NotificationCallback += new Display.NotificationCallbackEventHandler(display_NotificationCallback);
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

        void subscription_StatusChanged(Subscription subscription)
        {
            GNTPSubscription gs = subscription as GNTPSubscription;
            if (gs != null)
            {
                if (gs.EnabledAndAvailable)
                {
                    this.passwordManager.Add(gs.SubscriptionPassword, false);
                }
                else
                {
                    this.passwordManager.Remove(gs.SubscriptionPassword);
                }
            }

            OnSubscriptionsUpdated(false);
        }

        void sfc_Unsubscribed(SubscribedForwardDestination sfc)
        {
            OnForwardDestinationsUpdated();
        }

        void growl_ForwardedNotificationCallback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData, Growl.Daemon.CallbackInfo callbackInfo)
        {
            callbackInfo.ForwardedNotificationCallback -= new Growl.Daemon.CallbackInfo.ForwardedNotificationCallbackHandler(growl_ForwardedNotificationCallback);

            if (this.gntpListener != null)
            {
                this.gntpListener.WriteResponse(callbackInfo, response);
            }
        }

        # region On* Methods

        protected void OnFailedToStart(object sender, PortConflictEventArgs args)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnFailedToStart(sender, args);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.FailedToStart != null)
                {
                    this.FailedToStart(sender, args);
                }
            }
        }

        protected void OnFailedToStartUDPLegacy(object sender, PortConflictEventArgs args)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnFailedToStartUDPLegacy(sender, args);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.FailedToStartUDPLegacy != null)
                {
                    this.FailedToStartUDPLegacy(sender, args);
                }
            }
        }

        protected void OnStarted()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnStarted();
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.Started != null)
                {
                    this.Started(this, EventArgs.Empty);
                }
            }
        }

        protected void OnStopped()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnStopped();
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.Stopped != null)
                {
                    this.Stopped(this, EventArgs.Empty);
                }
            }
        }

        protected void OnApplicationRegistered(RegisteredApplication ra)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnApplicationRegistered(ra);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.ApplicationRegistered != null)
                {
                    this.ApplicationRegistered(ra);
                }
                SaveApplicationPrefs();
            }
        }

        private void OnNotificationReceived(Growl.DisplayStyle.Notification n)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnNotificationReceived(n);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.NotificationReceived != null)
                {
                    this.NotificationReceived(n);
                }
            }
        }

        private void OnNotificationPast(PastNotification pn)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnNotificationPast(pn);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.NotificationPast != null)
                {
                    this.NotificationPast(pn);
                }
            }
        }

        protected void OnForwardDestinationsUpdated()
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnForwardDestinationsUpdated();
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.ForwardDestinationsUpdated != null)
                {
                    this.ForwardDestinationsUpdated(this, EventArgs.Empty);
                }
                SaveForwardPrefs();
            }
        }

        protected void OnSubscriptionsUpdated(bool countChanged)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnSubscriptionsUpdated(countChanged);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.SubscriptionsUpdated != null)
                {
                    this.SubscriptionsUpdated(countChanged);
                }
                if (countChanged) SaveSubsriptionPrefs();
            }
        }

        protected void OnBonjourServiceUpdate(BonjourForwardDestination bfc)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    OnBonjourServiceUpdate(bfc);
                });
                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                if (this.BonjourServiceUpdate != null)
                {
                    this.BonjourServiceUpdate(bfc);
                }
            }
        }

        # endregion On* Methods

        # region Properties

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
                return DisplayStyleManager.AvailableDisplayStyles;
            }
        }

        public Dictionary<string, ForwardDestination> ForwardDestinations
        {
            get
            {
                return this.forwards;
            }
        }

        public Dictionary<string, Subscription> Subscriptions
        {
            get
            {
                return this.subscriptions;
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

        # endregion Properties

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
                    if (this.mutex != null)
                        this.mutex.Close();

                    DisplayStyleManager.DisplayLoaded -= new DisplayStyleManager.DisplayLoadedEventHandler(DisplayStyleManager_DisplayLoaded);

                    if (this.bonjour != null)
                    {
                        this.bonjour.ServiceFound -= new Bonjour.NetServiceFoundEventHandler(bonjour_ServiceFound);
                        this.bonjour.ServiceRemoved -= new Bonjour.NetServiceRemovedEventHandler(bonjour_ServiceRemoved);
                        this.bonjour.Dispose();
                        this.bonjour = null;
                    }

                    if (this.activityMonitor != null)
                    {
                        this.activityMonitor.WentIdle -= new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_WentIdle);
                        this.activityMonitor.ResumedActivity -= new ActivityMonitor.ActivityMonitorEventHandler(activityMonitor_ResumedActivity);
                        this.activityMonitor.StillActive -= new EventHandler(activityMonitor_StillActive);
                        this.activityMonitor.Dispose();
                        this.activityMonitor = null;
                    }

                    if (this.gntpListener != null)
                    {
                        this.gntpListener.RegisterReceived -= new Growl.Daemon.GrowlServer.RegisterReceivedEventHandler(gntpListener_RegisterReceived);
                        this.gntpListener.NotifyReceived -= new Growl.Daemon.GrowlServer.NotifyReceivedEventHandler(gntpListener_NotifyReceived);
                        this.gntpListener.SubscribeReceived -= new Growl.Daemon.GrowlServer.SubscribeReceivedEventHandler(gntpListener_SubscribeReceived);
                        this.gntpListener.Dispose();
                        this.gntpListener = null;
                    }

                    if (this.udpListener != null)
                    {
                        this.udpListener.RegistrationReceived -= new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                        this.udpListener.NotificationReceived -= new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                        this.udpListener.Dispose();
                        this.udpListener = null;
                    }

                    if (this.udpListenerLocal != null)
                    {
                        this.udpListenerLocal.RegistrationReceived -= new Growl.UDPLegacy.MessageReceiver.RegistrationHandler(udpListener_RegistrationReceived);
                        this.udpListenerLocal.NotificationReceived -= new Growl.UDPLegacy.MessageReceiver.NotificationHandler(udpListener_NotificationReceived);
                        this.udpListenerLocal.Dispose();
                        this.udpListenerLocal = null;
                    }

                    if (this.subscriptions != null)
                    {
                        foreach (Subscription subscription in this.subscriptions.Values)
                        {
                            subscription.StatusChanged -= new Subscription.SubscriptionStatusChangedEventHandler(subscription_StatusChanged);
                        }
                    }

                    if (this.forwards != null)
                    {
                        foreach (ForwardDestination fd in this.forwards.Values)
                        {
                            SubscribedForwardDestination sfd = fd as SubscribedForwardDestination;
                            if(sfd != null)
                                sfd.Unsubscribed -= new SubscribedForwardDestination.SubscribingComputerUnscubscribedEventHandler(sfc_Unsubscribed);
                        }
                    }

                    if (DisplayStyleManager.AvailableDisplayStyles != null)
                    {
                        foreach (Display display in DisplayStyleManager.AvailableDisplayStyles.Values)
                        {
                            display.NotificationCallback -= new Display.NotificationCallbackEventHandler(display_NotificationCallback);
                        }
                    }
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
