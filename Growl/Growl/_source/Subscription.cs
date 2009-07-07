using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl
{
    [Serializable]
    public class Subscription : GNTPForwardComputer, IDeserializationCallback, IDisposable
    {
        private const int RETRY_INTERVAL = 30;

        public delegate void SubscriptionStatusChangedEventHandler(Subscription subscription);

        [field: NonSerialized]
        public event SubscriptionStatusChangedEventHandler StatusChanged;

        [NonSerialized]
        private bool available; // by not serializing this property, deserialized Subscriptions will default to being unavailable

        [NonSerialized]
        private Growl.Daemon.SubscriptionConnector sc;

        [NonSerialized]
        private System.Timers.Timer timer;

        [NonSerialized]
        private string subscriberID;

        [NonSerialized]
        private bool allowed = true;

        [NonSerialized]
        private bool disposed;

        public Subscription(string name, bool enabled, string ipAddress, int port, string password)
            : base(name, enabled, ipAddress, port, password)
        {
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
                if (value && this.Enabled)
                {
                    this.AdditionalOfflineDisplayInfo = null;
                    this.AdditionalOnlineDisplayInfo = null;
                    Subscribe();
                }
                else
                {
                    EnsureTimer();
                    StopRetryTimer();
                }
                this.OnStatusChanged();
            }
        }

        private void Subscribe()
        {
            EnsureTimer();
            StopRetryTimer();

            if (this.subscriberID == null) this.subscriberID = Growl.Daemon.Subscriber.GenerateID();
            this.AdditionalOfflineDisplayInfo = "connecting...";
            if (this.sc != null) this.sc = null;
            Growl.Daemon.Subscriber subscriber = new Growl.Daemon.Subscriber(this.subscriberID, Environment.MachineName, Growl.Connector.GrowlConnector.TCP_PORT);
            this.sc = new Growl.Daemon.SubscriptionConnector(subscriber, this.Password, this.IPAddress, this.Port);
            this.sc.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
            this.sc.OKResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_OKResponse);
            this.sc.ErrorResponse += new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_ErrorResponse);
            this.sc.Subscribe();
        }

        internal void Kill()
        {
            EnsureTimer();
            StopRetryTimer();
            if (this.sc != null)
            {
                this.sc.StopRenewing();
                this.sc = null;
            }
        }

        void sc_OKResponse(Growl.Daemon.SubscriptionResponse response)
        {
            this.Platform = ForwardComputerPlatformType.FromString(response.PlatformName);
            this.AdditionalOfflineDisplayInfo = null;
            this.AdditionalOnlineDisplayInfo = String.Format("TTL: {0}", response.TTL);
            this.available = true;
            OnStatusChanged();

            // if the subscription succeeds, the SubscriptionConnecter will take care of keeping the subscription alive
        }

        void sc_ErrorResponse(Growl.Daemon.SubscriptionResponse response)
        {
            this.AdditionalOfflineDisplayInfo = (response.ErrorCode == Growl.Connector.ErrorCode.NOT_AUTHORIZED ? "invalid password" : "server unavailable");
            this.AdditionalOnlineDisplayInfo = null;
            this.available = false;
            OnStatusChanged();

            // if the subscription failed, try again periodically in case the server comes online
            StartRetryTimer();
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
                EnsureTimer();
                StopRetryTimer();
            }
            this.OnStatusChanged();
        }

        private void EnsureTimer()
        {
            if (this.timer == null)
            {
                this.timer = new System.Timers.Timer();
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
        }

        /// <summary>
        /// Starts the retry timer.
        /// </summary>
        private void StartRetryTimer()
        {
            this.timer.Interval = (RETRY_INTERVAL * 1000);
            this.timer.Start();
        }

        /// <summary>
        /// Stops the retry timer.
        /// </summary>
        private void StopRetryTimer()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// Fires when the renewal timer elapses. Renews the caller's subscription.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event args</param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Subscribe();
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
                    if (this.timer != null) this.timer.Dispose();
                    if (this.sc != null) this.sc.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
