using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class TwitterForwardDestination : Growl.Destinations.ForwardDestination
    {
        public static string DefaultFormat = String.Format("{0}: {1} - {2}", PLACEHOLDER_APPNAME, PLACEHOLDER_TITLE, PLACEHOLDER_TEXT);

        private const string PLACEHOLDER_APPNAME = "{APPNAME}";
        private const string PLACEHOLDER_TITLE = "{TITLE}";
        private const string PLACEHOLDER_TEXT = "{TEXT}";
        private const string PLACEHOLDER_PRIORITY = "{PRIORITY}";
        private const string PLACEHOLDER_SENDER = "{SENDER}";

        private string username;
        private string password;
        private string format;
        private Growl.Connector.Priority? minimumPriority = null;
        private bool onlyWhenIdle;

        public TwitterForwardDestination(string name, bool enabled, string username, string password, string format, Growl.Connector.Priority? minimumPriority, bool onlyWhenIdle)
            : base(name, enabled)
        {
            this.username = username;
            this.password = password;
            this.format = (String.IsNullOrEmpty(format) ? TwitterForwardDestination.DefaultFormat : format);
            this.minimumPriority = minimumPriority;
            this.onlyWhenIdle = onlyWhenIdle;
            this.Platform = KnownDestinationPlatformType.Twitter;
        }

        public override string Description
        {
            get
            {
                return String.Format("@{0}", this.Username);
            }
            set
            {
                throw new NotSupportedException();
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

        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
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
                return String.Format("({0}/{1})", priorityDisplay, idleDisplay);
            }
        }

        public override DestinationBase Clone()
        {
            TwitterForwardDestination clone = new TwitterForwardDestination(this.Description, this.Enabled, this.Username, this.Password, this.Format, this.MinimumPriority, this.OnlyWhenIdle);
            return clone;
        }

        public override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS
            requestInfo.SaveHandlingInfo("Forwarding to Twitter cancelled - Application Registrations are not forwarded.");
        }

        public override void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            // if a minimum priority is set, check that
            if (this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Twitter ({0}) cancelled - Notification priority must be at least '{1}' (was actually '{2}').", this.Username, this.MinimumPriority.Value.ToString(), notification.Priority.ToString()));
                send = false;
            }

            // if only sending when idle, check that
            if (this.OnlyWhenIdle && !isIdle)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Twitter ({0}) cancelled - Currently only configured to forward when idle", this.Username));
                send = false;
            }

            if (send)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarded to Twitter '{0}' - Minimum Priority:'{1}', Actual Priority:'{2}'", this.Description, (this.MinimumPriority != null && this.MinimumPriority.HasValue ? this.MinimumPriority.Value.ToString() : "<any>"), notification.Priority.ToString()));

                string message = this.format;
                message = message.Replace(PLACEHOLDER_APPNAME, notification.ApplicationName);
                message = message.Replace(PLACEHOLDER_TITLE, notification.Title);
                message = message.Replace(PLACEHOLDER_TEXT, notification.Text);
                message = message.Replace(PLACEHOLDER_PRIORITY, PrefPriority.GetFriendlyName(notification.Priority));
                message = message.Replace(PLACEHOLDER_SENDER, notification.MachineName);
                //Utility.WriteLine(message);

                // trim
                if (message.Length > 140) message = message.Substring(0, 140);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes("status=" + message);

                // send async (using threads instead of async WebClient/HttpWebRequest methods since they seem to have a bug with KeepAlives in infrequent cases)
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SendAsync), bytes);
            }
        }

        private void SendAsync(object state)
        {
            try
            {
                byte[] data = (byte[])state;

                string url = "http://twitter.com/statuses/update.xml";

                string credentials = String.Format("{0}:{1}", this.Username, this.Password);
                string encodedCredentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                string authorizationHeaderValue = String.Format("Basic {0}", encodedCredentials);

                Growl.CoreLibrary.WebClientEx wc = new Growl.CoreLibrary.WebClientEx();
                wc.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "Growl for Windows/2.0");
                wc.Headers.Add(System.Net.HttpRequestHeader.Authorization, authorizationHeaderValue);
                wc.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                using (wc)
                {
                    try
                    {
                        byte[] bytes = wc.UploadData(url, "POST", data);
                        string response = System.Text.Encoding.ASCII.GetString(bytes);
                        Utility.WriteDebugInfo(String.Format("Twitter forwarding response: {0}", response));
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteDebugInfo(String.Format("Twitter forwarding failed: {0}", ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(ex.ToString());
            }
        }
    }
}
