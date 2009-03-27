using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class NotificationType
    {
        Growl.Connector.NotificationType nt;

        public NotificationType()
        {
            this.nt = new Growl.Connector.NotificationType("General Notification", "General Notification");
        }

        public string Name
        {
            get
            {
                return this.nt.Name;
            }
            set
            {
                this.nt.Name = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.nt.DisplayName;
            }
            set
            {
                this.nt.DisplayName = value;
            }
        }

        public string IconPath
        {
            get
            {
                if (this.nt.Icon.IsUrl)
                    return this.nt.Icon.Url;
                else
                    return null;
            }
            set
            {
                this.nt.Icon = value;
            }
        }

        public byte[] IconBytes
        {
            get
            {
                if (this.nt.Icon.IsRawData)
                    return this.nt.Icon.Data.Data;
                else
                    return null;
            }
            set
            {
                this.nt.Icon = new Growl.CoreLibrary.BinaryData(value);
            }
        }

        public bool Enabled
        {
            get
            {
                return this.nt.Enabled;
            }
            set
            {
                this.nt.Enabled = value;
            }
        }

        internal Growl.Connector.NotificationType UnderlyingNotificationType
        {
            get
            {
                return this.nt;
            }
        }
    }
}
