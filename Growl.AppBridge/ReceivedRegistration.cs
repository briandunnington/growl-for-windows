using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AppBridge
{
    public class ReceivedRegistration : RegisteredApplication
    {
        protected bool initialRegistration = false;

        public ReceivedRegistration(RegisteredApplication ra) : this(ra, false)
        {
        }

        public ReceivedRegistration(RegisteredApplication ra, bool initialRegistration) : base(ra.Name, ra.Notifications, ra.Preferences)
        {
            this.initialRegistration = initialRegistration;
        }

        public bool InitialRegistration
        {
            get
            {
                return this.initialRegistration;
            }
        }

        public void Show()
        {
            Display.Default.HandleNotification(this);
        }

        public static implicit operator Growl.DisplayStyle.Notification(ReceivedRegistration rr)
        {
            Growl.DisplayStyle.Notification n = new Growl.DisplayStyle.Notification();
            n.ApplicationName = rr.Name;
            n.Description = String.Format("The application '{0}' has registered to send notifications.", rr.Name);
            n.Name = rr.Name;
            n.Priority = (int)Growl.Framework.Priority.Normal;
            n.Sticky = false;
            n.Title = "Application Registration";
            return n;
        }
    }
}
