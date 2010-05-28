using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class DisplaySetting
    {
        string deviceName;

        public string DeviceName
        {
            get
            {
                return this.deviceName;
            }
            set
            {
                this.deviceName = value;
            }
        }
    }
}
