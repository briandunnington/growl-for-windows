using System;
using System.Collections.Generic;
using System.Text;
using Vortex.Growl.Framework;

namespace Vortex.Growl.AppBridge
{
    public class ReceivedNotification : RegisteredNotification
    {
        protected NotificationType notificationType;
        protected string title;
        protected string description;
        protected Display display;
        protected Priority priority;
        protected bool sticky;

        public ReceivedNotification(NotificationPacket np, RegisteredNotification rn, RegisteredApplication ra, Display defaultDisplay) : base(rn.Name, rn.ApplicationName, rn.Preferences)
        {
            this.notificationType = np.NotificationType;
            this.title = np.Title;
            this.description = np.Description;
            this.display = DetermineDisplay(defaultDisplay, ra.Preferences.Display, rn.Preferences.Display);
            this.priority = DeterminePriority(np.Priority, rn.Preferences.Priority);
            this.sticky = DetermineStick(np.Sticky, rn.Preferences.Sticky);
        }

        public NotificationType NotificationType
        {
            get
            {
                return this.notificationType;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
        }

        public Display Display
        {
            get
            {
                return this.display;
            }
        }

        public Priority Priority
        {
            get
            {
                return this.priority;
            }
        }

        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
        }

        public void Show()
        {
            this.display.HandleNotification(this);
        }

        private Display DetermineDisplay(Display growlDefaultDisplay, Display appDisplay, Display notificationDisplay)
        {
            Display display = growlDefaultDisplay;
            if (appDisplay != null && appDisplay != Display.Default) display = appDisplay;
            if (notificationDisplay != null && notificationDisplay != Display.Default) display = notificationDisplay;
            return display;
        }

        private Priority DeterminePriority(Priority requestedPriority, Priority? prefPriority)
        {
            return (prefPriority == null ? requestedPriority : prefPriority.Value);
        }

        private bool DetermineStick(bool requestedSticky, bool? prefSticky)
        {
            return (prefSticky == null ? requestedSticky : prefSticky.Value);
        }

        public static implicit operator Vortex.Growl.DisplayStyle.Notification(ReceivedNotification rn)
        {
            Vortex.Growl.DisplayStyle.Notification n = new Vortex.Growl.DisplayStyle.Notification();
            n.ApplicationName = rn.ApplicationName;
            n.Description = rn.Description;
            n.Name = rn.Name;
            n.Priority = (int)rn.Priority;
            n.Sticky = rn.Sticky;
            n.Title = rn.Title;
            return n;
        }
    }
}
