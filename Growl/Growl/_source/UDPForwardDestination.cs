using System;
using System.Collections.Generic;
using System.Text;
using Growl.UDPLegacy;

namespace Growl
{
    [Serializable]
    public class UDPForwardDestination : ForwardDestination
    {
        private string ipAddress;
        private int port;
        private string password;

        public UDPForwardDestination(string description, bool enabled, string ipAddress, int port, string password)
            : base(description, enabled)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
        }

        public override bool Available
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

        public override string AddressDisplay
        {
            get
            {
                if (this.Available)
                    return String.Format("UDP {0}:{1} {2}", this.ipAddress, this.port, (this.AdditionalOnlineDisplayInfo != null ? String.Format("({0})", this.AdditionalOnlineDisplayInfo) : null));
                else
                    return String.Format("(offline) {0}", (this.AdditionalOfflineDisplayInfo != null ? String.Format("- {0}", this.AdditionalOfflineDisplayInfo) : null));
            }
        }

        public override ForwardDestination Clone()
        {
            UDPForwardDestination clone = new UDPForwardDestination(this.Description, this.Enabled, this.IPAddress, this.Port, this.Password);
            return clone;
        }

        internal override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, bool isIdle)
        {
            Growl.UDPLegacy.NotificationType[] types = new Growl.UDPLegacy.NotificationType[notificationTypes.Count];
            for (int i = 0; i < notificationTypes.Count; i++)
            {
                Growl.Connector.NotificationType notificationType = notificationTypes[i];
                Growl.UDPLegacy.NotificationType nt = new Growl.UDPLegacy.NotificationType(notificationType.Name, notificationType.Enabled);
                types[i] = nt;
            }

            Growl.UDPLegacy.MessageSender netgrowl = new Growl.UDPLegacy.MessageSender(this.IPAddress, this.Port, application.Name, this.Password);
            netgrowl.Register(ref types);
        }

        internal override void ForwardNotification(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, bool isIdle, Forwarder.ForwardedNotificationCallbackHandler callbackFunction)
        {
            Growl.UDPLegacy.NotificationType nt = new Growl.UDPLegacy.NotificationType(notification.Name, true);
            Growl.UDPLegacy.MessageSender netgrowl = new Growl.UDPLegacy.MessageSender(this.IPAddress, this.Port, notification.ApplicationName, this.Password);
            netgrowl.Notify(nt, notification.Title, notification.Text, notification.Priority, notification.Sticky);
        }
    }
}

