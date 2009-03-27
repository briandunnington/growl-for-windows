using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace Growl
{
    internal class NoneDisplay : Growl.DisplayStyle.Display
    {
        public override string Name
        {
            get
            {
                return "None";
            }
        }

        public override string Description
        {
            get
            {
                return "Doesn't show the notification on screen, but still records the notification in history and allows forwarding.";
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
            /*
            // should this fire this, or do nothing?
            if (this.NotificationClosed != null)
            {
                Growl.CoreLibrary.NotificationCallbackEventArgs args = new Growl.CoreLibrary.NotificationCallbackEventArgs(notification.UUID, Growl.CoreLibrary.CallbackResult.CLOSE);
                this.NotificationClosed(args);
            }
             * */
        }

        public override void CloseAllOpenNotifications()
        {
        }

        public override void CloseLastNotification()
        {
        }

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClicked;

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClosed;
    }
}
