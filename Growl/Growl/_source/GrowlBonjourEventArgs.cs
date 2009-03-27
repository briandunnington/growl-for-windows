using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class GrowlBonjourEventArgs : EventArgs
    {
        ForwardComputerPlatformType platform = ForwardComputerPlatformType.Other;

        public GrowlBonjourEventArgs(ForwardComputerPlatformType platform)
        {
            this.platform = platform;
        }

        public ForwardComputerPlatformType Platform
        {
            get
            {
                return this.platform;
            }
        }
    }
}
