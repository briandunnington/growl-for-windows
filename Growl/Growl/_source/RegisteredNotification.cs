using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl
{
    [Serializable]
    public class RegisteredNotification : IRegisteredObject
    {
        private string name;
        private NotificationPreferences preferences;
        private System.Drawing.Image icon;

        Dictionary<string, string> customTextAttributes = new Dictionary<string, string>();
        Dictionary<string, Resource> customBinaryAttributes = new Dictionary<string, Resource>();

        [NonSerialized]
        private RegisteredApplication ra;

        internal RegisteredNotification(string name, bool enabled, Dictionary<string, string> customTextAttributes, Dictionary<string, Resource> customBinaryAttributes)
            : this(name, enabled, customTextAttributes, customBinaryAttributes, NotificationPreferences.Default)
        {
        }

        public RegisteredNotification(string name, bool enabled, Dictionary<string, string> customTextAttributes, Dictionary<string, Resource> customBinaryAttributes, NotificationPreferences preferences)
        {
            this.name = name;
            this.preferences = preferences;
            this.preferences.PrefEnabled = enabled;
            this.customTextAttributes = customTextAttributes;
            this.customBinaryAttributes = customBinaryAttributes;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public RegisteredApplication RegisteredApplication
        {
            get
            {
                return this.ra;
            }
            set
            {
                this.ra = value;
            }
        }

        public string ApplicationName
        {
            get
            {
                return this.ra.Name;
            }
        }

        public System.Drawing.Image Icon
        {
            get
            {
                if (this.icon == null)
                    return this.ra.Icon;
                else
                    return this.icon;
            }
            set
            {
                this.icon = value;
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

        public virtual bool Enabled
        {
            get
            {
                if (this.ra.Enabled && this.preferences.PrefEnabled.Enabled)
                    return true;
                else
                    return false;
            }
        }

        public virtual Display Display
        {
            get
            {
                if (this.preferences.PrefDisplay.IsDefault)
                    return this.ra.Display;
                else
                    return this.preferences.PrefDisplay;
            }
        }

        public virtual int Duration
        {
            get
            {
                if (this.preferences.PrefDuration.IsDefault)
                    return this.ra.Duration;
                else
                    return this.preferences.PrefDuration.Duration;
            }
        }

        public virtual Growl.Connector.Priority Priority(Growl.Connector.Priority requestedPriority)
        {
            if (this.preferences.PrefPriority.IsDefault)
                return this.ra.Priority(requestedPriority);
            else
                return this.preferences.PrefPriority.Priority.Value;
        }

        public virtual bool ShouldStayOnScreen(bool stayWhenIdle, bool isUserIdle, bool requested)
        {
            if (this.preferences.PrefSticky.IsDefault)
                return this.ra.ShouldStayOnScreen(stayWhenIdle, isUserIdle, requested);
            else
            {
                return this.preferences.PrefSticky.ShouldStayOnScreen(stayWhenIdle, isUserIdle, requested).Value;
            }
        }

        public virtual bool ShouldForward(bool forwardingEnabled, out List<string> limitToTheseComputers)
        {
            limitToTheseComputers = null;
            if (this.preferences.PrefForward.IsDefault)
                return this.ra.ShouldForward(forwardingEnabled, out limitToTheseComputers);
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
                return this.ra.ShouldPlaySound(defaultSound, out soundFile);
            else
            {
                if (this.preferences.PrefSound.PlaySound.HasValue && this.preferences.PrefSound.PlaySound.Value)
                {
                    soundFile = this.preferences.PrefSound.SoundFile;
                    return true;
                }
                return false;
            }
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
