using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Growl
{
    [Serializable]
    public class ProwlForwardDestination : ForwardDestination
    {
        private const string URL = "https://prowl.weks.net/publicapi/add";

        private string apiKey;
        private Growl.Connector.Priority? minimumPriority = null;
        private bool onlyWhenIdle;

        public ProwlForwardDestination(string name, bool enabled, string apiKey, Growl.Connector.Priority? minimumPriority, bool onlyWhenIdle)
            : base(name, enabled)
        {
            this.apiKey = apiKey;
            this.minimumPriority = minimumPriority;
            this.onlyWhenIdle = onlyWhenIdle;
            this.Platform = ForwardDestinationPlatformType.IPhone;
        }

        public string APIKey
        {
            get
            {
                return this.apiKey;
            }
            set
            {
                this.apiKey = value;
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
                return String.Format("({1}/{2}) - {0}", this.APIKey, priorityDisplay, idleDisplay);
            }
        }

        public override ForwardDestination Clone()
        {
            ProwlForwardDestination clone = new ProwlForwardDestination(this.Description, this.Enabled, this.APIKey, this.MinimumPriority, this.OnlyWhenIdle);
            return clone;
        }

        internal override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS (since we have no way of filtering out already-registered apps at this point)
            //Send(application.Name, Properties.Resources.SystemNotification_AppRegistered_Title, String.Format(Properties.Resources.SystemNotification_AppRegistered_Text, application.Name));

            requestInfo.SaveHandlingInfo("Forwarding to Prowl cancelled - Application Registrations are not forwarded.");
        }

        internal override void ForwardNotification(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, bool isIdle, Forwarder.ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            // if a minimum priority is set, check that
            if (this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Prowl ({0}) cancelled - Notification priority must be at least '{1}' (was actually '{2}').", this.Description, this.MinimumPriority.Value.ToString(), notification.Priority.ToString()));
                send = false;
            }

            // if only sending when idle, check that
            if (send && this.OnlyWhenIdle && !isIdle)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Prowl ({0}) cancelled - Currently only configured to forward when idle", this.Description));
                send = false;
            }

            if (send)
            {
                string text = notification.Text;

                /* NOT YET
                // this appends the url from a url callback to the Prowl message (if specified)
                // this allows the user to click the url right from their Prowl app
                if (callbackInfo != null && callbackInfo.Context != null)
                {
                    string url = callbackInfo.Context.CallbackUrl;
                    if (!String.IsNullOrEmpty(url))
                    {
                        text += String.Format(" - {0}", url);
                    }
                }
                 * */

                Send(notification.ApplicationName, notification.Title, text, notification.Priority);
            }
        }

        private void Send(string applicationName, string title, string text, Growl.Connector.Priority priority)
        {
            // data
            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data.Add("apikey", this.APIKey);
            //data.Add("providerkey", "");
            data.Add("priority", ((int)priority).ToString());
            data.Add("application", applicationName);
            data.Add("event", title);
            data.Add("description", text);

            // send async (using threads instead of async WebClient methods since they seem to have a bug with KeepAlives in infrequent cases)
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SendAsync), data);
        }

        private void SendAsync(object state)
        {
            try
            {
                // data
                System.Collections.Specialized.NameValueCollection data = (System.Collections.Specialized.NameValueCollection)state;

                // prepare the WebClient
                Uri uri = new Uri(URL);
                WebClientEx wc = new WebClientEx();
                using (wc)
                {
                    wc.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "Growl for Windows/2.0");
                    wc.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");




                    // do it
                    try
                    {
                        byte[] bytes = wc.UploadValues(uri, "POST", data);
                        string response = System.Text.Encoding.ASCII.GetString(bytes);
                        Utility.WriteDebugInfo(String.Format("Prowl forwarding response: {0}", response));
                    }
                    catch(Exception ex)
                    {
                        Utility.WriteDebugInfo(String.Format("Prowl forwarding failed: {0}", ex.Message));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
