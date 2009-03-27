using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl
{
    public class DisplayNotificationCallbackDelegate : Growl.CoreLibrary.NotificationCallbackDelegate
    {
        public event NotificationCallbackEventHandler NotificationCallback;

        protected override void InternalOnNotificationCallback(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (this.NotificationCallback != null)
            {
                this.NotificationCallback(args);
            }
        }
    }
}
