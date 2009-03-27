using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class ApplicationPreferences : NotificationPreferences
    {
        protected ApplicationPreferences() : base()
        {
        }

        public new static ApplicationPreferences Default
        {
            get
            {
                return new ApplicationPreferences();
            }
        }
    }
}
