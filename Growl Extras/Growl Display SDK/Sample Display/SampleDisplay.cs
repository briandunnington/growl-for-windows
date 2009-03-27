using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace Sample_Display
{
    public class SampleDisplay : Display
    {
        internal const string SETTING_FILE_NAME = "FILENAME";

        public SampleDisplay()
        {
            // make sure we associate the proper SettingsPanel with our display
            this.SettingsPanel = new SampleSettingsPanel();

            // NOTE: if you do not set the .SettingsPanel property, a default settings panel
            // with no customization options will be used instead
        }

        /// <summary>
        /// This is the name that will appear in the Growl client to identify this display
        /// </summary>
        public override string Name
        {
            get { return "SDK Sample"; }
        }

        /// <summary>
        /// This description will appear in the Growl client to describe what this display does.
        /// It should be no longer than about 20 or 30 words.
        /// </summary>
        public override string Description
        {
            get { return "SDK Sample - Logs notification information to a text file"; }
        }

        public override string Author
        {
            get { return "Growl Display SDK"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Website
        {
            get { return "http://www.growlforwindows.com"; }
        }

        /// <summary>
        /// This is where we actually deal with the notification. The Growl client will call this method
        /// when a notification is received that is configured to use this display.
        /// </summary>
        protected override void HandleNotification(Notification notification, string displayName)
        {
            // read the filename from the settings
            string filename = (string)this.SettingsCollection[SETTING_FILE_NAME];

            if (!String.IsNullOrEmpty(filename))
            {
                // write the data
                System.IO.StreamWriter sw = null;
                if (!System.IO.File.Exists(filename))
                    sw = System.IO.File.CreateText(filename);
                else
                    sw = System.IO.File.AppendText(filename);
                sw.WriteLine("Application: {0}\r\nType:        {1}\r\nID:          {2}\r\nTitle:       {3}\r\nDescription: {4}\r\nPriority:    {5}\r\nSticky:      {6}\r\n----------------------------\r\n\r\n", notification.ApplicationName, notification.Name, notification.NotificationID, notification.Title, notification.Description, notification.Priority, notification.Sticky);
                sw.Close();
            }
        }

        public override void CloseAllOpenNotifications()
        {
            // since we dont have 'open' notifications, we dont have to do anything here
        }

        public override void CloseLastNotification()
        {
            // since we dont have 'open' notifications, we dont have to do anything here
        }

        // since our display can't be clicked, we dont need to do anything else with these events
        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClicked;
        public override event Growl.CoreLibrary.NotificationCallbackEventHandler NotificationClosed;
    }
}
