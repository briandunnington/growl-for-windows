using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class NotifyIOSubscription : Subscription, IDisposable
    {
        private string outletUrl;

        [NonSerialized]
        NotificationType nt;

        [NonSerialized]
        private NotifyIOListener listener;

        [NonSerialized]
        private bool disposed;

        public NotifyIOSubscription(string name, bool enabled, string outletUrl)
            : base(name, enabled)
        {
            this.outletUrl = outletUrl;
            this.Platform = KnownDestinationPlatformType.NotifyIO;

            InitializeSender();
        }

        public override void OnDeserialization(object sender)
        {
            InitializeSender();
            base.OnDeserialization(sender);
        }

        private void InitializeSender()
        {
            this.nt = new NotificationType("notification", "Web Notification");
        }

        public override string Key
        {
            get
            {
                return this.OutletUrl;
            }
        }

        public string OutletUrl
        {
            get
            {
                return this.outletUrl;
            }
            set
            {
                this.outletUrl = value;
            }
        }

        public override string AddressDisplay
        {
            get
            {
                return this.OutletUrl;
            }
        }

        public override void Subscribe()
        {
            if (this.Enabled && !String.IsNullOrEmpty(this.OutletUrl))
            {
                Kill();

                ChangeStatus(false, "connecting....");

                this.listener = new NotifyIOListener(this.OutletUrl, null);
                this.listener.Connected += new EventHandler(listener_Connected);
                this.listener.Disconnected += new EventHandler(listener_Disconnected);
                this.listener.NotificationReceived += new EventHandler<NotifyIOListener.NotificationReceivedEventArgs>(listener_NotificationReceived);
                this.listener.Start();
            }
        }

        void listener_NotificationReceived(object sender, NotifyIOListener.NotificationReceivedEventArgs e)
        {
            // register first (TODO: maybe keep a list of already-registered sources?)
            Growl.Connector.Application app = new Growl.Connector.Application(e.Source);
            app.Icon = (String.IsNullOrEmpty(e.Icon) ? e.SourceIcon : e.Icon);
            Register(app, new NotificationType[] { this.nt });

            // wait just a bit to make sure the registration went through (we are on another thread here, so it is ok)
            System.Threading.Thread.Sleep(500);

            // send notification
            Notification n = new Notification(app.Name, this.nt.Name, "", Fixup(e.Title, "Web Notification"), Fixup(e.Text));
            n.Icon = e.Icon;
            n.Sticky = e.Sticky;
            CallbackContext c = null;
            if (!String.IsNullOrEmpty(e.Link))
            {
                c = new CallbackContext(e.Link);
            }
            Notify(n, c);
        }

        void listener_Disconnected(object sender, EventArgs e)
        {
            string additionalInfo = (this.Enabled ? "reconnecting...." : null);
            ChangeStatus(false, additionalInfo);
        }

        void listener_Connected(object sender, EventArgs e)
        {
            string additionalInfo = (this.Enabled ? String.Empty : null);
            ChangeStatus(true, additionalInfo);
        }

        public override void Kill()
        {
            if (this.listener != null)
            {
                this.listener.Connected -= new EventHandler(listener_Connected);
                this.listener.Disconnected -= new EventHandler(listener_Disconnected);
                this.listener.NotificationReceived -= new EventHandler<NotifyIOListener.NotificationReceivedEventArgs>(listener_NotificationReceived);
                this.listener.Stop();
                this.listener.Dispose();
                this.listener = null;

                string additionalInfo = (this.Enabled ? String.Empty : null);
                ChangeStatus(false, additionalInfo);
            }
        }

        private string Fixup(string input)
        {
            return Fixup(input, String.Empty);
        }

        private string Fixup(string input, string defaultValue)
        {
            if (String.IsNullOrEmpty(input))
                return defaultValue;

            string output = input.Replace("\r", "");
            return output;
        }

        public override DestinationBase Clone()
        {
            NotifyIOSubscription clone = new NotifyIOSubscription(this.Description, this.Enabled, this.OutletUrl);
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
