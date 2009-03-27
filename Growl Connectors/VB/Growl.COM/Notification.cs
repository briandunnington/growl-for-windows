using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Notification
    {
        private Growl.Connector.Notification n;

        public Notification()
        {
            this.n = new Growl.Connector.Notification(null, null, null, null, null);
        }

        //this.n.Priority;

        public string ApplicationName
        {
            get
            {
                return this.n.ApplicationName;
            }
            set
            {
                this.n.ApplicationName = value;
            }
        }

        public string Type
        {
            get
            {
                return this.n.Name;
            }
            set
            {
                this.n.Name = value;
            }
        }

        public string ID
        {
            get
            {
                return this.n.ID;
            }
            set
            {
                this.n.ID = value;
            }
        }

        public string Title
        {
            get
            {
                return this.n.Title;
            }
            set
            {
                this.n.Title = value;
            }
        }

        public string Text
        {
            get
            {
                return this.n.Text;
            }
            set
            {
                this.n.Text = value;
            }
        }

        public string CoalescingID
        {
            get
            {
                return this.n.CoalescingID;
            }
            set
            {
                this.n.CoalescingID = value;
            }
        }

        public bool Sticky
        {
            get
            {
                return this.n.Sticky;
            }
            set
            {
                this.n.Sticky = value;
            }
        }

        public string IconPath
        {
            get
            {
                if (this.n.Icon.IsUrl)
                    return this.n.Icon.Url;
                else
                    return null;
            }
            set
            {
                this.n.Icon = value;
            }
        }

        public byte[] IconBytes
        {
            get
            {
                if (this.n.Icon.IsRawData)
                    return this.n.Icon.Data.Data;
                else
                    return null;
            }
            set
            {
                this.n.Icon = new Growl.CoreLibrary.BinaryData(value);
            }
        }


        internal Growl.Connector.Notification UnderlyingNotification
        {
            get
            {
                return this.n;
            }
        }
    }
}
