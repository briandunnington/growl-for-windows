using System;
using System.Collections.Generic;
using System.Text;

namespace Vortex.Growl.AppBridge
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

        public static implicit operator Vortex.Growl.DisplayStyle.Notification(ReceivedRegistration rr)
        {
            Vortex.Growl.DisplayStyle.Notification n = new Vortex.Growl.DisplayStyle.Notification();
            n.ApplicationName = rr.Name;
            n.Description = String.Format("The application '{0}' has registered to send notifications.", rr.Name);
            n.Name = rr.Name;
            n.Priority = (int)Vortex.Growl.Framework.Priority.Normal;
            n.Sticky = false;
            n.Title = "Application Registration";
            return n;
        }
    }
}
