using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace Scripty
{
    public class ScriptyDisplay : Display
    {
        internal const string SETTING_FILE_NAME = "FILENAME";

        public ScriptyDisplay()
        {
            this.SettingsPanel = new ScriptySettingsPanel();
        }

        public override string Author
        {
            get { return "Growl"; }
        }

        public override string Description
        {
            get { return "Runs a user-defined script in response to a notification"; }
        }

        public override string Name
        {
            get { return "Scripty"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Website
        {
            get { return "http://www.growlforwindows.com/extras/displays/scripty"; }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            string filename = (string) this.SettingsCollection[SETTING_FILE_NAME];

            if (!String.IsNullOrEmpty(filename))
            {
                System.Text.StringBuilder sb = new StringBuilder();
                sb.AppendFormat("-app \"{0}\" -id \"{1}\" -type \"{2}\" -title \"{3}\" -desc \"{4}\" -priority \"{5}\" -sticky \"{6}\"", Escape(notification.ApplicationName), Escape(notification.NotificationID), Escape(notification.Name), Escape(notification.Title), Escape(notification.Description), notification.Priority, notification.Sticky);
                if (notification.CustomTextAttributes != null)
                {
                    foreach (KeyValuePair<string, string> item in notification.CustomTextAttributes)
                    {
                        sb.AppendFormat(" -{0} \"{1}\"", item.Key, item.Value);
                    }
                }

                string arguments = sb.ToString();
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(filename, arguments);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process.Start(psi);
            }
        }

        public override void CloseAllOpenNotifications()
        {
            // nothing
        }

        public override void CloseLastNotification()
        {
            // nothing
        }

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClicked;

        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClosed;

        private string Escape(string input)
        {
            string output = input;
            if (input != null)
            {
                output = output.Replace("\r\n", "\\r\\n");
                output = output.Replace("\r", "\\r");
                output = output.Replace("\n", "\\n");
            }
            return output;
        }
    }
}
