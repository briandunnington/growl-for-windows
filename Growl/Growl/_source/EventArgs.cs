using System;

namespace Growl
{
    public class ApplicationRegisteredEventArgs : EventArgs
    {
        RegisteredApplication app;
        bool existing;

        public ApplicationRegisteredEventArgs(RegisteredApplication app, bool existing)
        {
            this.app = app;
            this.existing = existing;
        }

        public RegisteredApplication Application
        {
            get
            {
                return this.app;
            }
        }

        public bool Existing
        {
            get
            {
                return this.existing;
            }
        }
    }

    public class NotificationReceivedEventArgs : EventArgs
    {
        Growl.DisplayStyle.Notification n;

        public NotificationReceivedEventArgs(Growl.DisplayStyle.Notification n)
        {
            this.n = n;
        }

        public Growl.DisplayStyle.Notification Notification
        {
            get
            {
                return this.n;
            }
        }
    }

    public class NotificationPastEventArgs : EventArgs
    {
        PastNotification pn;

        public NotificationPastEventArgs(PastNotification pn)
        {
            this.pn = pn;
        }

        public PastNotification PastNotification
        {
            get
            {
                return this.pn;
            }
        }
    }

    public class SubscriptionsUpdatedEventArgs : EventArgs
    {
        bool countChagned;

        public SubscriptionsUpdatedEventArgs(bool countChagned)
        {
            this.countChagned = countChagned;
        }

        public bool CountChanged
        {
            get
            {
                return this.countChagned;
            }
        }
    }

    public class BonjourServiceUpdatedEventArgs : EventArgs
    {
        BonjourForwardDestination bfd;

        public BonjourServiceUpdatedEventArgs(BonjourForwardDestination bfd)
        {
            this.bfd = bfd;
        }

        public BonjourForwardDestination BonjourForwardDestination
        {
            get
            {
                return this.bfd;
            }
        }
    }

    public class PortConflictEventArgs : EventArgs
    {
        private int port;

        public PortConflictEventArgs(int port)
        {
            this.port = port;
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }
    }
}
