using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.UDPLegacy;

namespace Growl
{
    [Serializable]
    public class ForwardComputer
    {
        [field: NonSerialized]
        public event EventHandler EnabledChanged;

        private string description;
        private bool enabled = true;
        private string ipAddress;
        private int port;
        private string password;
        private bool useUDP = false;
        private ForwardComputerPlatformType platform = ForwardComputerPlatformType.Other;

        [NonSerialized]
        private string additionalOnlineDisplayInfo;
        [NonSerialized]
        private string additionalOfflineDisplayInfo;

        /*
        [NonSerialized]
        private Forwarder gntpForwarder;
        [NonSerialized]
        private MessageSender udpForwarder;
         * */

        public ForwardComputer(string description, bool enabled, string ipAddress, int port, string password, bool useUDP)
        {
            this.description = description;
            this.enabled = enabled;
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
            this.useUDP = useUDP;
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            protected set
            {
                this.description = value;
            }
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
                this.OnEnabledChanged(this, EventArgs.Empty);
            }
        }

        public virtual bool Available
        {
            get
            {
                return (this.IPAddress != null);
            }
            protected set
            {
                throw new NotSupportedException("The .Available property is read-only.");
            }
        }

        public virtual bool EnabledAndAvailable
        {
            get
            {
                return (this.Enabled && this.Available);
            }
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            protected set
            {
                this.ipAddress = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            protected set
            {
                this.port = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            protected set
            {
                this.password = value;
            }
        }

        public bool UseUDP
        {
            get
            {
                return this.useUDP;
            }
            protected set
            {
                this.useUDP = value;
            }
        }

        public ForwardComputerPlatformType Platform
        {
            get
            {
                return this.platform;
            }
            protected set
            {
                this.platform = value;
            }
        }

        public virtual string Display
        {
            get
            {
                return this.Description;
            }
        }

        public virtual string AddressDisplay
        {
            get
            {
                if (this.Available)
                    return String.Format("{0}:{1} {2}", this.ipAddress, this.port, (this.additionalOnlineDisplayInfo != null ? String.Format("({0})", this.additionalOnlineDisplayInfo) : null));
                else
                    return String.Format("(offline) {0}", (this.additionalOfflineDisplayInfo != null ? String.Format("- {0}", this.additionalOfflineDisplayInfo) : null));
            }
        }

        public virtual Cryptography.HashAlgorithmType HashAlgorithm
        {
            get
            {
                return Cryptography.HashAlgorithmType.MD5;
            }
        }

        public virtual Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return Cryptography.SymmetricAlgorithmType.PlainText;
            }
        }

        protected string AdditionalOnlineDisplayInfo
        {
            get
            {
                return this.additionalOnlineDisplayInfo;
            }
            set
            {
                this.additionalOnlineDisplayInfo = value;
            }
        }

        protected string AdditionalOfflineDisplayInfo
        {
            get
            {
                return this.additionalOfflineDisplayInfo;
            }
            set
            {
                this.additionalOfflineDisplayInfo = value;
            }
        }

        public virtual ForwardComputer Clone()
        {
            ForwardComputer clone = new ForwardComputer(this.Description, this.Enabled, this.IPAddress, this.Port, this.Password, this.UseUDP);
            return clone;
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(sender, eventArgs);
            }
        }

        /*
        internal void Forward()
        {
        }

        private void EnsureForwarders()
        {
            if(this.gntpForwarder == null) this.gntpForwarder = new Forwarder(fc.Password, fc.IPAddress, fc.Port, requestInfo)
        }
         * */
    }
}

