using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Growl.DisplayStyle;

namespace Growl.EmailDisplay
{
    public class EmailDisplay : Display
    {
        private Dictionary<int, string> prioritySettings = new Dictionary<int, string>();

        public EmailDisplay()
        {
            this.SettingsPanel = new EmailSettingsPanel();

            prioritySettings.Add(2, "PriorityEmergency");
            prioritySettings.Add(1, "PriorityHigh");
            prioritySettings.Add(0, "PriorityNormal");
            prioritySettings.Add(-1, "PriorityModerate");
            prioritySettings.Add(-2, "PriorityVeryLow");
        }

        public override string Name
        {
            get
            {
                return "Email Me";
            }
        }

        public override string Description
        {
            get
            {
                return "Forwards Growl notifications to an email address.";
            }
        }

        public override string Author
        {
            get
            {
                return "Vortex Software";
            }
        }

        public override void HandleNotification(Notification notification, string displayName)
        {
            bool send = SendBasedOnPriority(notification.Priority);
            if (send && this.SettingsCollection.ContainsKey("EmailAddress"))
            {
                string emailAddress = (string)this.SettingsCollection["EmailAddress"];
                string subject = String.Format("Growl Notification: {0}", notification.Title);
                string body = String.Format("From:  {0}\r\n\r\nTitle: {1}\r\n\r\n{2}\r\n\r\n\r\nSent on {3} at {4}", notification.ApplicationName, notification.Title, notification.Description, DateTime.Now.ToLongDateString(), DateTime.Now.ToShortTimeString());

                // send email
                MailMessage m = new MailMessage();
                m.To.Add(emailAddress);
                m.From = new MailAddress("growl@growl.growl", "Growl Notifier");
                m.Subject = subject;
                m.Body = body;

                SmtpClient smtp = new SmtpClient("localhost");
                smtp.SendAsync(m, null);
            }
        }

        private bool SendBasedOnPriority(int notificationPriority)
        {
            bool send = false;

            string settingName = null;
            if (this.prioritySettings.ContainsKey(notificationPriority))
                settingName = this.prioritySettings[notificationPriority];

            if (this.SettingsCollection.ContainsKey(settingName))
            {
                object setting = this.SettingsCollection[settingName];
                if (setting.ToString() == bool.TrueString)
                    send = true;
            }
            return send;
        }
    }
}
