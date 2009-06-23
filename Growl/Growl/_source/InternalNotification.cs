using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class InternalNotification
    {
        private const string FILENAME = "sysnot.list";

        string title;
        string text;
        string display;

        public InternalNotification(string title, string text, string display)
        {
            this.title = title;
            this.text = text;
            this.display = display;
        }

        public string Title
        {
            get
            {
                return this.title;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public string Display
        {
            get
            {
                return this.display;
            }
        }

        public static void SaveToDisk(ref List<InternalNotification> notifications)
        {
            try
            {
                SettingSaver ss = new SettingSaver(FILENAME);
                ss.Save(notifications);
                ss = null;
            }
            catch
            {
            }
        }

        public static void ReadFromDisk(ref List<InternalNotification> notifications)
        {
            try
            {
                SettingSaver ss = new SettingSaver(FILENAME);
                List<InternalNotification> notificationFromDisk = (List<InternalNotification>)ss.Load();
                if (notifications == null) notifications = new List<InternalNotification>();
                if (notificationFromDisk != null) notifications.AddRange(notificationFromDisk);
                System.IO.File.Delete(ss.Path);
                ss = null;
            }
            catch
            {
            }
        }
    }
}
