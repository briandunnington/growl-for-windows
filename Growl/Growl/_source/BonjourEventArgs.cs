using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class BonjourEventArgs : EventArgs
    {
        DestinationPlatformType platform = KnownDestinationPlatformType.Other;

        public BonjourEventArgs(DestinationPlatformType platform)
        {
            this.platform = platform;
        }

        public DestinationPlatformType Platform
        {
            get
            {
                return this.platform;
            }
        }
    }
}
