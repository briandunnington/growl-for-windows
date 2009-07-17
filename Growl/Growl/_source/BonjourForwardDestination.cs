using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using ZeroconfService;

namespace Growl
{
    [Serializable]
    public class BonjourForwardDestination : GNTPForwardDestination, ISerializable
    {
        bool available;
        bool resolved;
        string serviceName;

        public BonjourForwardDestination(string serviceName, bool enabled, string password) : base(serviceName, enabled, null, 0, password)
        {
            this.serviceName = serviceName;
            //Resolve();
        }

        private BonjourForwardDestination(string serviceName, ForwardDestinationPlatformType platform, bool enabled, string password) : this(serviceName, enabled, password)
        {
            this.Platform = platform;
        }

        public override string Key
        {
            get
            {
                return this.serviceName;
            }
        }

        public override bool Available
        {
            get
            {
                return this.available;
            }
            protected set
            {
                this.available = value;
            }
        }

        public bool Resolved
        {
            get
            {
                return this.resolved;
            }
        }

        public void Update(NetService service, GrowlBonjourEventArgs args)
        {
            if (service != null && service.Addresses != null && service.Addresses.Count > 0)
            {
                System.Net.IPEndPoint endpoint = (System.Net.IPEndPoint)service.Addresses[0];
                this.Description = service.Name;
                this.IPAddress = endpoint.Address.ToString();
                this.Port = endpoint.Port;
                this.Platform = args.Platform;
                this.Available = true;
                this.resolved = true;
            }
        }

        public void NotAvailable()
        {
            this.Available = false;
        }

        public override ForwardDestination Clone()
        {
            BonjourForwardDestination clone = new BonjourForwardDestination(this.Description, this.Platform, this.Enabled, this.Password);
            clone.IPAddress = this.IPAddress;
            clone.Port = this.Port;
            clone.Available = this.Available;
            return clone;
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(BonjourForwardDestinationSerializationHelper));
            info.AddValue("serviceName", this.serviceName, typeof(string));
            info.AddValue("platform", this.Platform, typeof(ForwardDestinationPlatformType));
            info.AddValue("enabled", this.Enabled, typeof(bool));
            info.AddValue("password", this.Password, typeof(string));
        }

        #endregion


        [Serializable]
        private class BonjourForwardDestinationSerializationHelper : IObjectReference
        {
            private string serviceName = null;
            private ForwardDestinationPlatformType platform = ForwardDestinationPlatformType.Other;
            private bool enabled = false;
            private string password = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                if (this.platform == null) this.platform = ForwardDestinationPlatformType.Other;
                return new BonjourForwardDestination(this.serviceName, this.platform, this.enabled, this.password);
            }

            #endregion
        }
    }
}
