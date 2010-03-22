using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class GNTPSubscription : Subscription, IDisposable
    {
        private string ipAddress;
        private int port;
        private string password;

        [NonSerialized]
        private Growl.Daemon.SubscriptionConnector sc;

        [NonSerialized]
        private string subscriberID;

        [NonSerialized]
        private bool disposed;

        public GNTPSubscription(string name, bool enabled, string ipAddress, int port, string password)
            : base(name, enabled)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            set
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
            set
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
            set
            {
                this.password = value;
            }
        }

        public Cryptography.HashAlgorithmType HashAlgorithm
        {
            get
            {
                try
                {
                    return Cryptography.GetKeyHashType(Properties.Settings.Default.GNTPForwardHashType);
                }
                catch
                {
                    return Cryptography.HashAlgorithmType.MD5;
                }
            }
        }

        public Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                try
                {
                    return Cryptography.GetEncryptionType(Properties.Settings.Default.GNTPForwardEncryptionAlgorithm);
                }
                catch
                {
                    return Cryptography.SymmetricAlgorithmType.PlainText;
                }
            }
        }

        public override string AddressDisplay
        {
            get
            {
                return String.Format("GNTP {0}:{1}", this.ipAddress, this.port);

                /*
                if (this.Available)
                    return String.Format("GNTP {0}:{1} {2}", this.ipAddress, this.port, (this.AdditionalOnlineDisplayInfo != null ? String.Format("({0})", this.AdditionalOnlineDisplayInfo) : null));
                else
                    return String.Format("(offline) {0}", (this.AdditionalOfflineDisplayInfo != null ? String.Format("- {0}", this.AdditionalOfflineDisplayInfo) : null));
                 * */
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

        public override void Subscribe()
        {
            if (this.Enabled)
            {
                if (this.subscriberID == null) this.subscriberID = Utility.MachineID;
                this.AdditionalDisplayInfo = "connecting...";
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

        public override void Kill()
        {
            if (this.sc != null)
            {
                this.sc.StopRenewing();
                this.sc.OKResponse -= new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_OKResponse);
                this.sc.ErrorResponse -= new Growl.Daemon.SubscriptionConnector.ResponseEventHandler(sc_ErrorResponse);
                this.sc.Dispose();
                this.sc = null;
            }
        }

        void sc_OKResponse(Growl.Daemon.SubscriptionResponse response)
        {
            string additionalInfo = (this.Enabled ? String.Format("TTL: {0}", response.TTL) : null);
            this.Platform = KnownDestinationPlatformType.FromString(response.PlatformName);
            ChangeStatus(true, additionalInfo);

            // if the subscription succeeds, the SubscriptionConnecter will take care of keeping the subscription alive
        }

        void sc_ErrorResponse(Growl.Daemon.SubscriptionResponse response)
        {
            string additionalInfo = (this.Enabled ? (response.ErrorCode == Growl.Connector.ErrorCode.NOT_AUTHORIZED ? "invalid password" : "server unavailable") : null);
            ChangeStatus(false, additionalInfo);

            // if the subscription failed, the SubscriptionConnector will take care of trying to reestablish it
        }

        public override DestinationBase Clone()
        {
            GNTPSubscription clone = new GNTPSubscription(this.Description, this.Enabled, this.IPAddress, this.Port, this.Password);
            return clone;
        }

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
                    Kill();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
