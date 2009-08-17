using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Growl.Connector;

namespace Growl
{
    [Serializable]
    public class SubscribedForwardDestination : GNTPForwardDestination, IDisposable
    {
        public delegate void SubscribingComputerUnscubscribedEventHandler(SubscribedForwardDestination sfc);

        [field:NonSerialized]
        public event SubscribingComputerUnscubscribedEventHandler Unsubscribed;

        [NonSerialized]
        private bool available; // by not serializing this property, deserialized SFCs will default to being unavailable
        [NonSerialized]
        private bool disposed;

        private int ttl = 300;
        private Cryptography.HashAlgorithmType hashAlgorithm;
        private Cryptography.SymmetricAlgorithmType encryptionAlgorithm;

        [NonSerialized]
        private System.Timers.Timer timer;

        public SubscribedForwardDestination(Growl.Daemon.Subscriber subscriber, int ttl)
            : this(subscriber.Name, true, subscriber.IPAddress, subscriber.Port, subscriber.Key.Password, ForwardDestinationPlatformType.FromString(subscriber.PlatformName), ttl)
        {
            this.Key = subscriber.ID;
            this.hashAlgorithm = subscriber.Key.HashAlgorithm;
            this.encryptionAlgorithm = subscriber.Key.EncryptionAlgorithm;
        }

        private SubscribedForwardDestination(string name, bool enabled, string ipAddress, int port, string password, ForwardDestinationPlatformType platform, int ttl, bool available)
            : this(name, enabled, ipAddress, port, password, platform, ttl)
        {
            // this is only used by the .Clone method to create an instance with no timer or key information
        }

        private SubscribedForwardDestination(string name, bool enabled, string ipAddress, int port, string password, ForwardDestinationPlatformType platform, int ttl)
            : base(name, enabled, ipAddress, port, password)
        {
            this.Platform = platform;
            this.available = true;
            this.ttl = ttl;
            this.AdditionalOnlineDisplayInfo = "subscribed";

            this.Renew();
        }

        public int TTL
        {
            get
            {
                return this.ttl;
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

        public override Cryptography.HashAlgorithmType HashAlgorithm
        {
            get
            {
                return this.hashAlgorithm;
            }
        }

        public override Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return this.encryptionAlgorithm;
            }
        }

        public void Renew()
        {
            EnsureTimer();
            this.timer.Stop();
            this.timer.Interval = this.ttl * 1000;
            this.timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // if the timer ticks, then we did not renew in time
            this.available = false;
            this.IPAddress = null;  // this makes the computer unavailable

            if (this.Unsubscribed != null)
            {
                this.Unsubscribed(this);
            }
        }

        private void EnsureTimer()
        {
            if (this.timer == null)
            {
                this.timer = new System.Timers.Timer();
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
        }

        public override ForwardDestination Clone()
        {
            SubscribedForwardDestination clone = new SubscribedForwardDestination(this.Description, this.Enabled, this.IPAddress, this.Port, this.Password, this.Platform, this.TTL, this.Available);
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
                    if(this.timer != null)
                        this.timer.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
