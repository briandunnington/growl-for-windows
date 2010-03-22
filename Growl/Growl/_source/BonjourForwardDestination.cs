using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Growl.Destinations;
using Mono.Zeroconf;

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
         }

        private BonjourForwardDestination(string serviceName, DestinationPlatformType platform, bool enabled, string password) : this(serviceName, enabled, password)
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

        public override string AddressDisplay
        {
            get
            {
                if (this.Available)
                    return base.AddressDisplay;
                else
                    return "(offline)";
            }
        }

        public bool Resolved
        {
            get
            {
                return this.resolved;
            }
        }

        public void Update(IResolvableService service, BonjourEventArgs args)
        {
            if (service != null && service.HostEntry != null)
            {

                string host = DetectedService.GetHostname(service);
                int port = (int) service.Port;

                this.Description = service.Name;
                this.IPAddress = host;
                this.Port = port;
                this.Platform = args.Platform;
                this.Available = true;
                this.resolved = true;
            }
        }

        public void NotAvailable()
        {
            this.Available = false;
        }

        public override DestinationBase Clone()
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
            info.AddValue("platform", this.Platform, typeof(DestinationPlatformType));
            info.AddValue("enabled", this.Enabled, typeof(bool));
            info.AddValue("password", this.Password, typeof(string));
        }

        #endregion


        [Serializable]
        private class BonjourForwardDestinationSerializationHelper : IObjectReference
        {
            private string serviceName = null;
            private DestinationPlatformType platform = KnownDestinationPlatformType.Other;
            private bool enabled = false;
            private string password = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                if (this.platform == null) this.platform = KnownDestinationPlatformType.Other;
                BonjourForwardDestination bfd = new BonjourForwardDestination(this.serviceName, this.platform, this.enabled, this.password);
                bfd.NotAvailable();
                return bfd;
            }

            #endregion
        }
    }
}
