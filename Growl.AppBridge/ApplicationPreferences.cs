using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl.AppBridge
{
    [Serializable]
    public class ApplicationPreferences : DisplayPreferences
    {
        protected bool enabled = true;
        protected bool click = true;

        public ApplicationPreferences(bool enabled, Display display, bool click) : base(display)
        {
            this.enabled = enabled;
            this.click = click;
        }

        protected ApplicationPreferences(SerializationInfo info, StreamingContext context) : base(info, context)
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

        public bool Click
        {
            get
            {
                return this.click;
            }
            set
            {
                this.click = value;
            }
        }

        public static ApplicationPreferences Default
        {
            get
            {
                return new ApplicationPreferences(true, Display.Default, true);
            }
        }
    }
}
