using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class EmailForwardDestination : Growl.Destinations.ForwardDestination
    {
        private string to;
        private SMTPConfiguration smtpConfig;
        private Growl.Connector.Priority? minimumPriority = null;
        private bool onlyWhenIdle;

        public EmailForwardDestination(string name, bool enabled, string to, SMTPConfiguration smtpConfig, Growl.Connector.Priority? minimumPriority, bool onlyWhenIdle)
            : base(name, enabled)
        {
            this.to = to;
            this.smtpConfig = (smtpConfig != null ? smtpConfig : SMTPConfiguration.Local);
            this.minimumPriority = minimumPriority;
            this.onlyWhenIdle = onlyWhenIdle;
            this.Platform = KnownDestinationPlatformType.Email;
        }

        public string To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public SMTPConfiguration SMTPConfiguration
        {
            get
            {
                return this.smtpConfig;
            }
            set
            {
                this.smtpConfig = value;
            }
        }

        public Growl.Connector.Priority? MinimumPriority
        {
            get
            {
                return this.minimumPriority;
            }
            set
            {
                this.minimumPriority = value;
            }
        }

        public bool OnlyWhenIdle
        {
            get
            {
                return this.onlyWhenIdle;
            }
            set
            {
                this.onlyWhenIdle = value;
            }
        }

        public override bool Available
        {
            get
            {
                return true;
            }
            protected set
            {
                throw new NotSupportedException("The .Available property is read-only.");
            }
        }

        public override string AddressDisplay
        {
            get
            {
                string priorityText = PrefPriority.GetByValue(this.MinimumPriority).Name;
                string priorityDisplay = (this.MinimumPriority != null && this.MinimumPriority.HasValue ? priorityText : Properties.Resources.AddProwl_AnyPriority);
                string idleDisplay = (this.OnlyWhenIdle ? Properties.Resources.LiteralString_IdleOnly : Properties.Resources.LiteralString_Always);
                return String.Format("{0} - ({1}/{2})", this.To, priorityDisplay, idleDisplay);
            }
        }

        private string FromAddress
        {
            get
            {
                try
                {
                    string from = Properties.Settings.Default.EmailForwardFromAddress;
                    System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(from);    // this ensures that the address is valid (properly formatted)
                    return from;
                }
                catch
                {
                    Utility.WriteDebugInfo(String.Format("Invalid email address configured as 'From' address: '{0}' - using default address instead.", Properties.Settings.Default.EmailForwardFromAddress));
                    return "growl@growlforwindows.com";
                }
            }
        }

        public override DestinationBase Clone()
        {
            EmailForwardDestination clone = new EmailForwardDestination(this.Description, this.Enabled, this.To, this.SMTPConfiguration, this.MinimumPriority, this.OnlyWhenIdle);
            return clone;
        }

        public override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS
            requestInfo.SaveHandlingInfo("Forwarding to Email cancelled - Application Registrations are not forwarded.");
        }

        public override void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            // if a minimum priority is set, check that
            if (this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Email ({0}) cancelled - Notification priority must be at least '{1}' (was actually '{2}').", this.Description, this.MinimumPriority.Value.ToString(), notification.Priority.ToString()));
                send = false;
            }

            // if only sending when idle, check that
            if (this.OnlyWhenIdle && !isIdle)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Email ({0}) cancelled - Currently only configured to forward when idle", this.Description));
                send = false;
            }

            if (send)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarded to Email '{0}' - Minimum Priority:'{1}', Actual Priority:'{2}'", this.Description, (this.MinimumPriority != null && this.MinimumPriority.HasValue ? this.MinimumPriority.Value.ToString() : "<any>"), notification.Priority.ToString()));

                string format = "Application: {0}\r\n\r\n{1}\r\n\r\n{2}\r\n\r\nSent From: {3} - {4}";
                string message = String.Format(format, notification.ApplicationName, notification.Title, notification.Text, notification.MachineName, DateTime.Now.ToString());
                Send(notification.ApplicationName, notification.Title, notification.Priority, message);
            }
        }

        private void Send(string appName, string subject, Growl.Connector.Priority priority, string message)
        {
            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage();
            m.To.Add(this.to);
            m.From = new System.Net.Mail.MailAddress(this.FromAddress, appName);
            m.Subject = subject;
            m.Body = message;
            m.Priority = GetMessagePriority(priority);
            m.Sender = new System.Net.Mail.MailAddress("growl@growlforwindows.com", "Growl for Windows");
            m.Headers.Add("X-Growl-Origin-Application", appName);

            // send the email using another thread (since the Smtp.SendAsync seems to be flakey)
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SendAsync), m);
        }

        private void SendAsync(object state)
        {
            System.Net.Mail.MailMessage m = (System.Net.Mail.MailMessage)state;

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
            smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            smtp.Host = this.SMTPConfiguration.Host;
            smtp.Port = this.SMTPConfiguration.Port;
            smtp.EnableSsl = this.SMTPConfiguration.UseSSL;
            if (this.SMTPConfiguration.UseAuthentication)
            {
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(this.SMTPConfiguration.Username, this.SMTPConfiguration.Password);
                smtp.Credentials = credentials;
            }

            try
            {
                smtp.Send(m);
                Utility.WriteDebugInfo(String.Format("Email successfully sent to {0}", m.To.ToString()));
            }
            catch(Exception ex)
            {
                string reason = "";
                while (ex != null)
                {
                    reason += String.Format("{0} :: ", ex.Message);
                    ex = ex.InnerException;
                }
                Utility.WriteDebugInfo(String.Format("Email failed: {0}", reason));
            }
        }

        private System.Net.Mail.MailPriority GetMessagePriority(Growl.Connector.Priority priority)
        {
            System.Net.Mail.MailPriority messagePriority = System.Net.Mail.MailPriority.Normal;
            if (priority == Growl.Connector.Priority.Emergency) messagePriority = System.Net.Mail.MailPriority.High;
            else if (priority == Growl.Connector.Priority.VeryLow) messagePriority = System.Net.Mail.MailPriority.Low;
            return messagePriority;
        }
    }

    [Serializable]
    public class SMTPConfiguration
    {
        private string host = "127.0.0.1";
        private int port = 25;
        private bool useSSL;
        private bool useAuthentication;
        private string username;
        private string password;

        public static SMTPConfiguration Local = new SMTPConfiguration();

        private SMTPConfiguration()
        {
        }

        public string Host
        {
            get
            {
                return this.host;
            }
            set
            {
                this.host = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        public bool UseSSL
        {
            get
            {
                return this.useSSL;
            }
            set
            {
                this.useSSL = value;
            }
        }

        public bool UseAuthentication
        {
            get
            {
                return this.useAuthentication;
            }
            set
            {
                this.useAuthentication = value;
            }
        }

        public string Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }
    }
}
