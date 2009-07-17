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

        [NonSerialized]
        private Dictionary<string, WebClient> busyWebClients;

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
        }

        internal override void ForwardNotification(Growl.Connector.Notification notification, Growl.Daemon.CallbackInfo callbackInfo, Growl.Daemon.RequestInfo requestInfo, bool isIdle, Forwarder.ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            // if a minimum priority is set, check that
            if (this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
                send = false;

            // if only sending when idle, check that
            if (this.OnlyWhenIdle && !isIdle)
                send = false;

            if (send)
            {
                string text = notification.Text;
                /* THIS IS NOT READY YET 
                 * this appends the url from a url callback to the Prowl message (if specified)
                 * this allows the user to click the url right from their Prowl app
                 *
                if (callbackInfo != null && callbackInfo.Context != null)
                {
                    Growl.Connector.UrlCallbackTarget target = callbackInfo.Context.GetUrlCallbackTarget();
                    if (!String.IsNullOrEmpty(target.Url))
                    {
                        string data = callbackInfo.GetUrlCallbackData(Growl.CoreLibrary.CallbackResult.CLICK);
                        System.UriBuilder ub = new UriBuilder(target.Url);
                        if (ub.Query != null && ub.Query.Length > 1)
                            ub.Query = ub.Query.Substring(1) + "&" + data;
                        else
                            ub.Query = data;
                        text += String.Format(" - {0}", ub.Uri.AbsoluteUri);
                    }
                }
                 * */
                Send(notification.ApplicationName, notification.Title, text, notification.Priority);
            }
        }

        private void Send(string applicationName, string title, string text, Growl.Connector.Priority priority)
        {
            try
            {
                // data
                System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
                data.Add("apikey", this.APIKey);
                //data.Add("providerkey", "");
                data.Add("priority", ((int) priority).ToString());
                data.Add("application", applicationName);
                data.Add("event", title);
                data.Add("description", text);

                // prepare the WebClient
                Uri uri = new Uri(URL);
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "Growl for Windows/2.0");
                wc.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                wc.UploadValuesCompleted += new UploadValuesCompletedEventHandler(wc_UploadValuesCompleted);

                /*
                // no proxy
                WebProxy proxy = null;

                // auto-detect (use IE settings) - this is the default, so really not needed
                WebProxy proxy = WebRequest.DefaultWebProxy;

                // custom proxy
                WebProxy proxy = new WebProxy();
                proxy.Address = "http://localhost:8080";
                proxy.BypassProxyOnLocal = true;
                proxy.Credentials = new NetworkCredential("username", "password");

                wc.Proxy = proxy;

                Console.WriteLine(wc.Proxy);
                 * */

                // save the WebClient so we can dispose of it when it completes
                if (this.busyWebClients == null) this.busyWebClients = new Dictionary<string, WebClient>();
                string key = System.Guid.NewGuid().ToString();
                this.busyWebClients.Add(key, wc);

                // do it
                wc.UploadValuesAsync(uri, "POST", data, key);

                // not sure why, but either this code is bad or the Prowl servers are flakey
                // but if this method exits immediately, then it will often result in a 'connection failed' exception
                System.Threading.Thread.Sleep(100);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void wc_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            string key = (string)e.UserState;
            if (!String.IsNullOrEmpty(key))
            {
                if (this.busyWebClients.ContainsKey(key))
                {
                    WebClient wc = this.busyWebClients[key];
                    wc.Dispose();
                    wc = null;
                    this.busyWebClients.Remove(key);
                }
            }
        }
    }
}
