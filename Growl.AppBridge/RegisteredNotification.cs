using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AppBridge
{
    [Serializable]
    public class RegisteredNotification
    {
        protected string name;
        protected string applicationName;
        protected NotificationPreferences preferences;

        public RegisteredNotification(string name, string applicationName, NotificationPreferences preferences)
        {
            this.name = name;
            this.applicationName = applicationName;
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

        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        public NotificationPreferences Preferences
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
    }
}
