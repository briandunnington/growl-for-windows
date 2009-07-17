using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.UDPLegacy;

namespace Growl
{
    [Serializable]
    public abstract class ForwardDestination
    {
        [field: NonSerialized]
        public event EventHandler EnabledChanged;

        private string key;
        private string description;
        private bool enabled = true;
        private ForwardDestinationPlatformType platform = ForwardDestinationPlatformType.Other;

        [NonSerialized]
        private string additionalOnlineDisplayInfo;
        [NonSerialized]
        private string additionalOfflineDisplayInfo;


        public ForwardDestination(string description, bool enabled)
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

        public abstract bool Available {get; protected set;}

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

        public abstract string AddressDisplay {get;}

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

        public abstract ForwardDestination Clone();

        protected virtual void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(sender, eventArgs);
            }
        }

        internal abstract void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, bool isIdle);

        internal abstract void ForwardNotification(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, bool isIdle, Forwarder.ForwardedNotificationCallbackHandler callbackFunction);
    }
}

