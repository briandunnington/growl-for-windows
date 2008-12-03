using System;
using System.Collections.Generic;
using System.Text;
using Growl.Framework;

namespace Growl.AppBridge
{
    public class AppBridge
    {
        protected const string REGISTERED_APPLICATIONS_SETTINGS_FILENAME = "applications.settings";
        protected const string FORWARD_COMPUTERS_SETTINGS_FILENAME = "forward.settings";

        private const string REGISTRATION_LOG_FORMAT = "{0}\tREGISTRATION\t{1}\t{2}";
        private const string NOTIFICATION_LOG_FORMAT = "{0}\tNOTIFICATION\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}";

        private FileLogger logger;
        private LocalReceiver localReceiver;
        private NetworkReceiver networkReciever;
        private WebReceiver webReceiver;
        protected Dictionary<string, RegisteredApplication> applications;
        protected Dictionary<string, ForwardComputer> forwards;
        protected Display defaultDisplay = Display.Default;
        protected bool listenForNetworkNotifications = false;
        protected bool allowRemoteRegistration = false;
        protected string networkPassword = null;
        protected bool listenForWebNotifications = false;
        protected bool autoEnableWebNotifications = false;
        protected bool doForwarding = false;
        private SettingSaver raSettingSaver = new SettingSaver(REGISTERED_APPLICATIONS_SETTINGS_FILENAME);
        private SettingSaver fcSettingSaver = new SettingSaver(FORWARD_COMPUTERS_SETTINGS_FILENAME);

        public delegate void RegistrationHandler(ReceivedRegistration rr);
        public delegate void NotificationHandler(ReceivedNotification rn);
        public event RegistrationHandler RegistrationReceived;
        public event NotificationHandler NotificationReceived;

        internal AppBridge()
        {
            // this class is like a singleton.
            // instances should only be created by BridgeFactory

            // load display styles first
            DisplayStyleManager.Load();

            // load up the application settings
            this.applications = (Dictionary<string, RegisteredApplication>)raSettingSaver.Load();
            if (this.applications == null) this.applications = new Dictionary<string, RegisteredApplication>();
            this.forwards = (Dictionary<string, ForwardComputer>)fcSettingSaver.Load();
            if (this.forwards == null) this.forwards = new Dictionary<string, ForwardComputer>();

            // set up the logger
            this.logger = new FileLogger();
        }

        public Dictionary<string, RegisteredApplication> RegisteredApplications
        {
            get
            {
                return this.applications;
            }
        }
        public Dictionary<string, ForwardComputer> ForwardComputers
        {
            get
            {
                return this.forwards;
            }
        }

        public bool ListenForNetworkNotifications
        {
            get
            {
                return this.listenForNetworkNotifications;
            }
            set
            {
                this.listenForNetworkNotifications = value;
                if (value)
                    StartListeningForNetworkNotifications();
                else
                    StopListeningForNetworkNotifications();
            }
        }

        public bool AllowRemoteRegistration
        {
            get
            {
                return this.allowRemoteRegistration;
            }
            set
            {
                this.allowRemoteRegistration = value;
            }
        }

        public void SetNetworkPassword(string password)
        {
            this.networkPassword = password;
            if (this.networkReciever != null) this.networkReciever.SetPassword(password);
        }

        public bool ListenForWebNotifications
        {
            get
            {
                return this.listenForWebNotifications;
            }
            set
            {
                this.listenForWebNotifications = value;
                if (value)
                    StartListeningForWebNotifications();
                else
                    StopListeningForWebNotifications();
            }
        }

        public bool AutoEnableWebNotifications
        {
            get
            {
                return this.autoEnableWebNotifications;
            }
            set
            {
                this.autoEnableWebNotifications = value;
            }
        }

        public Display DefaultDisplay
        {
            get
            {
                return this.defaultDisplay;
            }
            set
            {
                this.defaultDisplay = value;
            }
        }

        public bool DoForwarding
        {
            get
            {
                return this.doForwarding;
            }
            set
            {
                this.doForwarding = value;
            }
        }

        public void Start()
        {
            // start local receiver
            StartListeningForLocalNotifications();

            // start network receiver
            if (this.ListenForNetworkNotifications)
                StartListeningForNetworkNotifications();

            // start web receiver
            if (this.ListenForWebNotifications)
                StartListeningForWebNotifications();
        }

        public void Stop()
        {
            try
            {
                // stop web receiver
                if(this.webReceiver != null)
                {
                    this.webReceiver.Stop();
                    this.webReceiver = null;
                }

                // stop network receiver
                if (this.networkReciever != null)
                {
                    this.networkReciever.Stop();
                    this.networkReciever = null;
                }

                // stop local receiver
                if (this.localReceiver != null)
                {
                    this.localReceiver.Stop();
                    this.localReceiver = null;
                }

                // save application information
                SaveAppState();

                DisplayStyleManager.Unload();
            }
            catch
            {
                // swallow any exceptions (this handles the case when Growl is stopped while still listening for network notifications)
            }
        }

        private void StartListeningForLocalNotifications()
        {
            if (this.localReceiver == null)
            {
                this.localReceiver = new LocalReceiver();
                this.localReceiver.RegistrationReceived += new MessageReceiver.RegistrationHandler(localReceiver_RegistrationReceived);
                this.localReceiver.NotificationReceived += new MessageReceiver.NotificationHandler(localReceiver_NotificationReceived);
            }

            if (!this.localReceiver.IsRunning) this.localReceiver.Start();
        }

        private void StopListeningForLocalNotifications()
        {
            if (this.localReceiver != null) this.localReceiver.Stop();
        }

        private void StartListeningForNetworkNotifications()
        {
            if (this.networkReciever == null)
            {
                this.networkReciever = new NetworkReceiver();
                this.networkReciever.RegistrationReceived += new MessageReceiver.RegistrationHandler(netAppBridge_RegistrationReceived);
                this.networkReciever.NotificationReceived += new MessageReceiver.NotificationHandler(netAppBridge_NotificationReceived);
            }

            this.networkReciever.SetPassword(this.networkPassword);

            if(!this.networkReciever.IsRunning) this.networkReciever.Start();
        }

        private void StopListeningForNetworkNotifications()
        {
            if (this.networkReciever != null) this.networkReciever.Stop();
        }

        private void StartListeningForWebNotifications()
        {
            if (this.webReceiver == null)
            {
                this.webReceiver = new WebReceiver();
                this.webReceiver.RegistrationReceived += new MessageReceiver.RegistrationHandler(webAppBridge_RegistrationReceived);
                this.webReceiver.NotificationReceived += new MessageReceiver.NotificationHandler(webAppBridge_NotificationReceived);
            }

            if (!this.webReceiver.IsRunning) this.webReceiver.Start();
        }

        private void StopListeningForWebNotifications()
        {
            if (this.webReceiver != null) this.webReceiver.Stop();
        }

        private void SaveAppState()
        {
            raSettingSaver.Save(this.applications);
            fcSettingSaver.Save(this.forwards);
        }

        public Dictionary<string, Display> GetAvailableDisplayStyles()
        {
            return DisplayStyleManager.GetAvailableDisplayStyles();
        }

        public void PreviewDisplay(Display display)
        {
            // set up the special preview notification
            ApplicationPreferences appPrefs = new ApplicationPreferences(true, display, false);
            NotificationPreferences notPrefs = new NotificationPreferences(true, display, Priority.Normal, false);
            RegisteredApplication ra = new RegisteredApplication("Growl", null, appPrefs);
            RegisteredNotification rn = new RegisteredNotification("Preview Display Style", "Growl", notPrefs);
            NotificationType nt = new NotificationType("Preview Display Style", true);
            NotificationPacket np = new NotificationPacket(0, "Growl", "", nt, "Preview Display Style", String.Format("This is a preview of the '{0}' display style.", display.Name), Priority.Normal, false);
            ReceivedNotification n = new ReceivedNotification(np, rn, ra, this.defaultDisplay);
            this.OnNotificationReceived(n);
        }

        void HandleRegistrationReceived(RegistrationPacket rp, string receivedFrom)
        {
            Dictionary<string, RegisteredNotification> notifications = new Dictionary<string, RegisteredNotification>();
            foreach (NotificationType nt in rp.NotificationTypes)
            {
                NotificationPreferences prefs = NotificationPreferences.Default;
                prefs.Enabled = nt.Enabled;
                RegisteredNotification rn = new RegisteredNotification(nt.Name, rp.ApplicationName, prefs);
                notifications.Add(rn.Name, rn);
            }
            RegisteredApplication ra = new RegisteredApplication(rp.ApplicationName, notifications, ApplicationPreferences.Default);

            bool existing = false;
            RegisteredApplication app = null;
            if (this.applications.ContainsKey(rp.ApplicationName))
            {
                app = this.applications[rp.ApplicationName];
                existing = true;
            }
            else
            {
                app = ra;
                this.applications.Add(app.Name, app);
            }
            if (app.Preferences.Enabled)
            {
                if (existing)
                {
                    // the application is already registered, so we want to use the new notification list, but preserve any exisiting preferences
                    foreach (RegisteredNotification n in app.Notifications.Values)
                    {
                        foreach (RegisteredNotification rn in ra.Notifications.Values)
                        {
                            if (n.Name == rn.Name)
                            {
                                // use the exisiting preferences
                                rn.Preferences = n.Preferences;
                            }
                        }
                    }
                    app.Notifications = ra.Notifications;
                }

                ReceivedRegistration rr = new ReceivedRegistration(app, !existing);
                this.OnRegistrationReceived(rr);
            }
            else
            {
                // application is disabled
            }
        }

        void HandleNotificationReceived(NotificationPacket np, string receivedFrom)
        {
            // if the app is registered, we have to see if this type of notification is registered
            RegisteredApplication ra = this.applications[np.ApplicationName];
            if (ra.Preferences.Enabled)
            {
                if (ra.Notifications.ContainsKey(np.NotificationType.Name))
                {
                    RegisteredNotification rn = ra.Notifications[np.NotificationType.Name];
                    if (rn.Preferences.Enabled)
                    {
                        ReceivedNotification n = new ReceivedNotification(np, rn, ra, this.defaultDisplay);
                        this.OnNotificationReceived(n);
                    }
                    else
                    {
                        // notification is disabled
                    }
                }
                else
                {
                    // notification type is not registered
                }
            }
            else
            {
                // application is disabled
            }
        }

        protected virtual void OnRegistrationReceived(ReceivedRegistration rr)
        {
            if (this.RegistrationReceived != null) this.RegistrationReceived(rr);
        }

        protected virtual void OnNotificationReceived(ReceivedNotification rn)
        {
            if (this.NotificationReceived != null) this.NotificationReceived(rn);
        }

        protected void HandleForwarding(BasePacket packet)
        {
            if (this.doForwarding)
            {
                foreach (ForwardComputer fc in this.forwards.Values)
                {
                    if (fc.Enabled)
                    {
                        try
                        {
                            NetGrowl netgrowl = new NetGrowl(fc.IPAddress, fc.Port, packet.ApplicationName, fc.Password);
                            if (packet is RegistrationPacket)
                            {
                                RegistrationPacket rp = (RegistrationPacket)packet;
                                NotificationType[] nTypes = rp.NotificationTypes;
                                netgrowl.Register(ref nTypes);
                            }
                            else if (packet is NotificationPacket)
                            {
                                NotificationPacket np = (NotificationPacket)packet;
                                netgrowl.Notify(np.NotificationType, np.Title, np.Description, np.Priority, np.Sticky);
                            }
                        }
                        catch
                        {
                            // swallow any exceptions and move on to the next forwarded computer in the list
                            // this way, if one computer configuration is bad (invalid port or whatever), the rest are not affected
                        }
                    }
                }
            }
        }

        private bool IsRegistered(string application)
        {
            return this.applications.ContainsKey(application);
        }

        void localReceiver_RegistrationReceived(RegistrationPacket rp, string receivedFrom)
        {
            // always handle forwarding first
            HandleForwarding(rp);

            HandleRegistrationReceived(rp, receivedFrom);

            // log
            string action = "ALLOWED";
            LogRegistration(receivedFrom, action, rp.ApplicationName);
        }

        void localReceiver_NotificationReceived(NotificationPacket np, string receivedFrom)
        {
            // always handle forwarding first
            HandleForwarding(np);

            bool alreadyRegistered = IsRegistered(np.ApplicationName);

            if(alreadyRegistered)
                HandleNotificationReceived(np, receivedFrom);

            // log
            string action = (alreadyRegistered ? "ALLOWED" : "BLOCKED");
            LogNotification(receivedFrom, action, np.ApplicationName, np.NotificationType.Name, np.Title, np.Description, np.Priority, np.Sticky);
        }

        void netAppBridge_RegistrationReceived(RegistrationPacket rp, string receivedFrom)
        {
            // always handle forwarding first
            HandleForwarding(rp);

            string action = "BLOCKED";
            bool alreadyRegistered = IsRegistered(rp.ApplicationName);
            if (alreadyRegistered || this.AllowRemoteRegistration)
            {
                HandleRegistrationReceived(rp, receivedFrom);
                action = "ALLOWED";
            }

            // log
            LogRegistration(receivedFrom, action, rp.ApplicationName);
        }

        void netAppBridge_NotificationReceived(NotificationPacket np, string receivedFrom)
        {
            // always handle forwarding first
            HandleForwarding(np);

            bool alreadyRegistered = IsRegistered(np.ApplicationName);

            if (alreadyRegistered)
                HandleNotificationReceived(np, receivedFrom);

            // log
            string action = (alreadyRegistered ? "ALLOWED" : "BLOCKED");
            LogNotification(receivedFrom, action, np.ApplicationName, np.NotificationType.Name, np.Title, np.Description, np.Priority, np.Sticky);
        }

        void webAppBridge_RegistrationReceived(RegistrationPacket rp, string receivedFrom)
        {
            //TODO: if(this.ForwardWebNotifications)

            // always handle forwarding first
            HandleForwarding(rp);

            bool alreadyRegistered = IsRegistered(rp.ApplicationName);
            if (!alreadyRegistered && !this.AutoEnableWebNotifications)
            {
                foreach (NotificationType nt in rp.NotificationTypes)
                {
                    nt.Enabled = false;
                }
            }

            HandleRegistrationReceived(rp, receivedFrom);

            // log
            string action = "ALLOWED";
            LogRegistration(receivedFrom, action, rp.ApplicationName);
        }

        void webAppBridge_NotificationReceived(NotificationPacket np, string receivedFrom)
        {
            // always handle forwarding first
            HandleForwarding(np);

            bool alreadyRegistered = IsRegistered(np.ApplicationName);

            if (alreadyRegistered)
                HandleNotificationReceived(np, receivedFrom);

            // log
            string action = (alreadyRegistered ? "ALLOWED" : "BLOCKED");
            LogNotification(receivedFrom, action, np.ApplicationName, np.NotificationType.Name, np.Title, np.Description, np.Priority, np.Sticky);
        }

        private void LogRegistration(string receivedFrom, string action, string applicationName)
        {
            Log(String.Format(REGISTRATION_LOG_FORMAT, receivedFrom, action, applicationName));
        }

        private void LogNotification(string receivedFrom, string action, string applicationName, string notificationTypeName, string title, string description, Priority priority, bool sticky)
        {
            Log(String.Format(NOTIFICATION_LOG_FORMAT, receivedFrom, action, applicationName, notificationTypeName, title, description, priority, sticky));
        }

        private void Log(string message)
        {
            this.logger.Log(message);
        }
    }
}
