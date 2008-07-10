using System;
using System.Collections.Generic;
using System.Text;

namespace Vortex.Growl.AppBridge
{
    [Serializable]
    public class RegisteredApplication
    {
        protected string name;
        protected Dictionary<string, RegisteredNotification> notifications;
        protected ApplicationPreferences preferences;

        public RegisteredApplication(string name, Dictionary<string, RegisteredNotification> notifications, ApplicationPreferences preferences)
        {
            this.name = name;
            this.notifications = notifications;
            this.preferences = preferences;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public ApplicationPreferences Preferences
        {
            get
            {
                return this.preferences;
            }
            set
            {
                this.preferences = value;
            }
        }

        public Dictionary<string, RegisteredNotification> Notifications
        {
            get
            {
                return this.notifications;
            }
            set
            {
                this.notifications = value;
            }
        }
    }
}
