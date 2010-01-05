using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.UDPLegacy;

namespace Growl
{
    [Serializable]
    public abstract class DestinationBase
    {
        [field: NonSerialized]
        public event EventHandler EnabledChanged;

        private string key;
        private string description;
        private bool enabled = true;
        private ForwardDestinationPlatformType platform = ForwardDestinationPlatformType.Other; // someday, we should change this, but it is serialized so we dont want to do it now

        [NonSerialized]
        private string additionalOnlineDisplayInfo;
        [NonSerialized]
        private string additionalOfflineDisplayInfo;


        public DestinationBase(string description, bool enabled)
        {
            this.description = description;
            this.enabled = enabled;
        }

        public virtual string Key
        {
            get
            {
                if (String.IsNullOrEmpty(this.key)) this.key = System.Guid.NewGuid().ToString();
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        public virtual string Description
        {
            get
            {
                return this.description;
            }
            set
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

        public abstract bool Available { get; protected set;}

        public virtual bool EnabledAndAvailable
        {
            get
            {
                return (this.Enabled && this.Available);
            }
        }

        public ForwardDestinationPlatformType Platform
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

        public abstract string AddressDisplay { get;}


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

        public abstract DestinationBase Clone();

        protected virtual void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(sender, eventArgs);
            }
        }
    }
}

