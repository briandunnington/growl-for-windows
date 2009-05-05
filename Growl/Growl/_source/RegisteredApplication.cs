using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl
{
    [Serializable]
    public class RegisteredApplication : IRegisteredObject
    {
        private static System.Drawing.Image DefaultIcon = global::Growl.Properties.Resources.generic_application;

        private string name;
        private Dictionary<string, RegisteredNotification> notifications;
        private ApplicationPreferences preferences;
        private System.Drawing.Image icon;

        Dictionary<string, string> customTextAttributes = new Dictionary<string, string>();
        Dictionary<string, Resource> customBinaryAttributes = new Dictionary<string, Resource>();

        [NonSerialized]
        private bool linked;

        public RegisteredApplication(string name, Dictionary<string, RegisteredNotification> notifications, Dictionary<string, string> customTextAttributes, Dictionary<string, Resource> customBinaryAttributes)
            : this(name, notifications, customTextAttributes, customBinaryAttributes, ApplicationPreferences.Default)
        {
        }

        internal RegisteredApplication(string name, Dictionary<string, RegisteredNotification> notifications, Dictionary<string, string> customTextAttributes, Dictionary<string, Resource> customBinaryAttributes, ApplicationPreferences preferences)
        {
            this.name = name;
            this.notifications = notifications;
            this.preferences = preferences;
            this.customTextAttributes = customTextAttributes;
            this.customBinaryAttributes = customBinaryAttributes;
        }

        private void LinkNotifications()
        {
            if (!linked)
            {
                if (this.notifications != null)
                {
                    foreach (RegisteredNotification rn in this.notifications.Values)
                    {
                        rn.RegisteredApplication = this;
                    }
                }
                this.linked = true;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public System.Drawing.Image Icon
        {
            get
            {
                if (this.icon == null)
                    return DefaultIcon;
                else
                    return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        public ApplicationPreferences Preferences
        {
            get
            {
                if (this.preferences == null) this.preferences = ApplicationPreferences.Default;
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
                LinkNotifications();
                return this.notifications;
            }
            set
            {
                this.notifications = value;
                this.linked = false;
                LinkNotifications();
            }
        }

        public virtual bool Enabled
        {
            get
            {
                return this.preferences.PrefEnabled.Enabled;
            }
        }

        public virtual Display Display
        {
            get
            {
                if (this.preferences.PrefDisplay == null)
                    return Display.Default;
                else
                    return this.preferences.PrefDisplay;
            }
        }

        public virtual int Duration
        {
            get
            {
                if (this.preferences.PrefDuration.IsDefault)
                    return PrefDuration.Default.Duration;
                else
                    return this.preferences.PrefDuration.Duration;
            }
        }

        public virtual Growl.Connector.Priority Priority(Growl.Connector.Priority requestedPriority)
        {
            if (this.preferences.PrefPriority.IsDefault)
                return requestedPriority;
            else
                return this.preferences.PrefPriority.Priority.Value;
        }

        public virtual bool ShouldStayOnScreen(bool stayWhenIdle, bool isUserIdle, bool requested)
        {
            if (this.preferences.PrefSticky.IsDefault)
            {
                if (stayWhenIdle && isUserIdle)
                    return true;
                else
                    return requested;
            }
            else
            {
                return this.preferences.PrefSticky.ShouldStayOnScreen(stayWhenIdle, isUserIdle, requested).Value;
            }
        }

        public virtual bool ShouldForward(bool forwardingEnabled, out List<string> limitToTheseComputers)
        {
            limitToTheseComputers = null;
            if (this.preferences.PrefForward.IsDefault)
                return forwardingEnabled;
            else
            {
                if (this.preferences.PrefForward.IsCustom) limitToTheseComputers = this.preferences.PrefForwardCustomList;
                return this.preferences.PrefForward.Forward.Value;
            }
        }

        public virtual bool ShouldPlaySound(PrefSound defaultSound, out string soundFile)
        {
            soundFile = null;
            if (this.preferences.PrefSound.IsDefault)
            {
                if (defaultSound != null)
                {
                    if (defaultSound.PlaySound.HasValue && defaultSound.PlaySound.Value)
                    {
                        soundFile = defaultSound.SoundFile;
                        return true;
                    }
                }
            }
            else
            {
                if (this.preferences.PrefSound.PlaySound.HasValue && this.preferences.PrefSound.PlaySound.Value)
                {
                    soundFile = this.preferences.PrefSound.SoundFile;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a collection of custom text attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom text attribute is equivalent to a custom "X-" header
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, string> CustomTextAttributes
        {
            get
            {
                return this.customTextAttributes;
            }
        }

        /// <summary>
        /// Gets a collection of custom binary attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom binary attribute is equivalent to a custom "X-" header with a 
        /// "x-growl-resource://" value, as well as the necessary resource headers
        /// (Identifier, Length, and binary bytes)
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, Resource> CustomBinaryAttributes
        {
            get
            {
                return this.customBinaryAttributes;
            }
        }
    }
}
