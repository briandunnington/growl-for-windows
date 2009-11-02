using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl
{
    [Serializable]
    public class Subscription : GNTPForwardDestination, IDeserializationCallback, IDisposable
    {
        public delegate void SubscriptionStatusChangedEventHandler(Subscription subscription);

        [field: NonSerialized]
        public event SubscriptionStatusChangedEventHandler StatusChanged;

        [NonSerialized]
        private bool available; // by not serializing this property, deserialized Subscriptions will default to being unavailable

        [NonSerialized]
        private Growl.Daemon.SubscriptionConnector sc;

        [NonSerialized]
        private string subscriberID;

        [NonSerialized]
        private bool allowed = true;

        [NonSerialized]
        private bool disposed;

        public Subscription(string name, bool enabled, string ipAddress, int port, string password, bool allowed)
            : base(name, enabled, ipAddress, port, password)
        {
            this.allowed = allowed;
            Subscribe();
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

        public override bool EnabledAndAvailable
        {
            get
            {
                return (base.EnabledAndAvailable && this.allowed);
            }
        }

        public string SubscriberID
        {
            get
            {
                return this.subscriberID;
            }
        }

        public string SubscriptionPassword
        {
            get
            {
                return this.Password + this.subscriberID;
            }
        }

        public bool Allowed
        {
            get
            {
                return this.allowed;
            }
            set
            {
                this.allowed = value;
                Update();
            }
        }

        public void Update()
        {
            if (this.Enabled)
            {
                this.AdditionalOfflineDisplayInfo = null;
                this.AdditionalOnlineDisplayInfo = null;
                Subscribe();
            }
            else
            {
                Kill();
            }
            this.OnStatusChanged();
        }

        private void Subscribe()
        {
            if (this.Allowed)
            {
                if (this.subscriberID == null) this.subscriberID = Utility.MachineID;
                this.AdditionalOfflineDisplayInfo = "connecting...";
                if (this.sc == null)
                {
                    Growl.Daemon.Subscriber subscriber = new Growl.Daemon.Subscriber(this.subscriberID, Environment.MachineName, Growl.Connector.GrowlConnector.TCP_PORT);
                    this.sc = new Growl.Daemon.SubscriptionConnector(subscriber, this.Password, this.IPAddress, this.Port);
                    this.sc.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
                    this.sc.OKResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_OKResponse);
                    this.sc.ErrorResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_ErrorResponse);
                }
                this.sc.Subscribe();
            }
        }

        internal void Kill()
        {
            if (this.sc != null)
            {
                this.sc.StopRenewing();
                this.sc = null;
            }
        }

        void sc_OKResponse(Growl.Daemon.SubscriptionResponse response)
        {
            if (this.Enabled)
            {
                this.AdditionalOfflineDisplayInfo = null;
                this.AdditionalOnlineDisplayInfo = String.Format("TTL: {0}", response.TTL);
            }
            this.Platform = ForwardDestinationPlatformType.FromString(response.PlatformName);
            this.available = true;
            OnStatusChanged();

            // if the subscription succeeds, the SubscriptionConnecter will take care of keeping the subscription alive
        }

        void sc_ErrorResponse(Growl.Daemon.SubscriptionResponse response)
        {
            if (this.Enabled)
            {
                this.AdditionalOfflineDisplayInfo = (response.ErrorCode == Growl.Connector.ErrorCode.NOT_AUTHORIZED ? "invalid password" : "server unavailable");
                this.AdditionalOnlineDisplayInfo = null;
            }
            this.available = false;
            OnStatusChanged();

            // if the subscription failed, the SubscriptionConnector will take care of trying to reestablish it
        }

        protected void OnStatusChanged()
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this);
            }
        }

        protected override void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            base.OnEnabledChanged(sender, eventArgs);
            if (this.Enabled)
            {
                this.AdditionalOfflineDisplayInfo = null;
                this.AdditionalOnlineDisplayInfo = null;
                Subscribe();
            }
            else
            {
                this.AdditionalOfflineDisplayInfo = "not enabled";
                this.AdditionalOnlineDisplayInfo = "not enabled";
                Kill();
            }
            this.OnStatusChanged();
        }

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            if(this.Enabled)
                Subscribe();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.sc != null) this.sc.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
