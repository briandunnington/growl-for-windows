using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class GrowlBonjourEventArgs : EventArgs
    {
        ForwardDestinationPlatformType platform = ForwardDestinationPlatformType.Other;

        public GrowlBonjourEventArgs(ForwardDestinationPlatformType platform)
        {
            this.platform = platform;
        }

        public ForwardDestinationPlatformType Platform
        {
            get
            {
                return this.platform;
            }
        }
    }
}
