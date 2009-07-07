using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class ProwlForwardComputer : ForwardComputer
    {
        //private const string URL = "http://prowl.weks.net/api/add_notification.php";
        //private const string URL_FORMAT = "http://prowl.weks.net/api/add_notification.php?application={0}&event={1}&description={2}";
        private const string URL = "https://prowl.weks.net/api/add_notification.php";
        private const string URL_FORMAT = "https://prowl.weks.net/api/add_notification.php?application={0}&event={1}&description={2}";

        private string username;
        private string password;
        private Growl.Connector.Priority minimumPriority = Growl.Connector.Priority.VeryLow;

        /* The Prowl servers seem to be using an SSL certificate that Windows doesnt like. Maybe the cert requires an update i dont have,
         * or maybe it requires IE8 or something, but other people may run into the same issue. As such, i am overriding the default
         * behavior of rejecting the cert and instead manually allowing it. This is not the best solution, but it is the best we have for now.
         * */
        static ProwlForwardComputer()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CustomCertificateValidation);
        }

        private static bool CustomCertificateValidation(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }


        public ProwlForwardComputer(string name, bool enabled, string username, string password, Growl.Connector.Priority minimumPriority)
            : base(name, enabled)
        {
            this.username = username;
            this.password = password;
            this.minimumPriority = minimumPriority;
            this.Platform = ForwardComputerPlatformType.IPhone;
        }

        public string Username
        {
            get
            {
                return this.username;
            }
            protected set
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
            protected set
            {
                this.password = value;
            }
        }

        public Growl.Connector.Priority MinimumPriority
        {
            get
            {
                return this.minimumPriority;
            }
            protected set
            {
                this.minimumPriority = value;
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
                return String.Format("Username: {0}", this.Username);
            }
        }

        public override ForwardComputer Clone()
        {
            ProwlForwardComputer clone = new ProwlForwardComputer(this.Description, this.Enabled, this.Username, this.Password, this.MinimumPriority);
            return clone;
        }

        internal override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo)
        {
            // IGNORE REGISTRATION NOTIFICATIONS (since we have no way of filtering out already-registered apps at this point)
            //Send(application.Name, Properties.Resources.SystemNotification_AppRegistered_Title, String.Format(Properties.Resources.SystemNotification_AppRegistered_Text, application.Name));
        }

        internal override void ForwardNotification(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, Forwarder.ForwardedNotificationCallbackHandler callbackFunction)
        {
            if(notification.Priority >= this.minimumPriority)
                Send(notification.ApplicationName, notification.Title, notification.Text);
        }

        private void Send(string applicationName, string title, string text)
        {
            try
            {
                string url = String.Format(URL_FORMAT, System.Web.HttpUtility.UrlEncode(applicationName), System.Web.HttpUtility.UrlEncode(title), System.Web.HttpUtility.UrlEncode(text));

                string credentials = String.Format("{0}:{1}", this.Username, this.Password);
                string encodedCredentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                string authorizationHeaderValue = String.Format("Basic {0}", encodedCredentials);

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                //request.UserAgent = "ProwlScript/1.0";
                request.UserAgent = "Growl for Windows/2.0";
                request.Headers.Add("Authorization", authorizationHeaderValue);

                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
