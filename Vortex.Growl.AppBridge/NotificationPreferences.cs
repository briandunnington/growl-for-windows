using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Vortex.Growl.Framework;

namespace Vortex.Growl.AppBridge
{
    [Serializable]
    public class NotificationPreferences : DisplayPreferences
    {
        protected bool enabled = true;
        protected Priority? priority;
        protected bool? sticky = null;

        public NotificationPreferences(bool enabled, Display display, Priority? priority, bool? sticky) : base(display)
        {
            this.enabled = enabled;
            this.priority = priority;
            this.sticky = sticky;
        }

        protected NotificationPreferences(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        public Priority? Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        public bool? Sticky
        {
            get
            {
                return this.sticky;
            }
            set
            {
                this.sticky = value;
            }
        }

        public static NotificationPreferences Default
        {
            get
            {
                return new NotificationPreferences(true, Display.Default, null, null);
            }
        }
    }
}
