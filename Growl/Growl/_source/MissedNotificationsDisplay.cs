using System;
using System.Collections.Generic;
using System.Text;
using Growl.UI;
using Growl.DisplayStyle;

namespace Growl
{
    internal class MissedNotificationsDisplay : VisualDisplay
    {
        public override string Name
        {
            get 
            {
                return "Missed Notifications";
            }
        }

        public override string Description
        {
            get
            {
                return "Shows a list of notifications missed while away";
            }
        }

        public override string Author
        {
            get
            {
                return "Growl for Windows";
            }
        }

        public override string Website
        {
            get
            {
                return "http://www.growlforwindows.com";
            }
        }

        public override string Version
        {
            get
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);
                return f.FileVersion;
            }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            throw new NotImplementedException("MissedNotificationDisplay does not implement the HandleNotification method.");
        }

        public void HandleNotification(List<PastNotification> missedNotifications)
        {
            MissedNotificationsWindow win = new MissedNotificationsWindow();
            win.MissedNotifications = missedNotifications;

            // hide any exisiting missed notification windows just in case they are still open
            this.CloseAllOpenNotifications();

            this.Show(win);
        }
    }
}
