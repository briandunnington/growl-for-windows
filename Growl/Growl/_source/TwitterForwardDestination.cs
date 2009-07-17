using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class TwitterForwardDestination : ForwardDestination
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
            this.Platform = ForwardDestinationPlatformType.Twitter;
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

        public override ForwardDestination Clone()
        {
            TwitterForwardDestination clone = new TwitterForwardDestination(this.Description, this.Enabled, this.Username, this.Password, this.Format, this.MinimumPriority, this.OnlyWhenIdle);
            return clone;
        }

        internal override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Daemon.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS
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
                string message = this.format;
                message = message.Replace(PLACEHOLDER_APPNAME, notification.ApplicationName);
                message = message.Replace(PLACEHOLDER_TITLE, notification.Title);
                message = message.Replace(PLACEHOLDER_TEXT, notification.Text);
                message = message.Replace(PLACEHOLDER_PRIORITY, PrefPriority.GetFriendlyName(notification.Priority));
                message = message.Replace(PLACEHOLDER_SENDER, notification.MachineName);
                //Console.WriteLine(message);
                Send(message);
            }
        }

        private void Send(string message)
        {
             try
            {
                // trim
                if (message.Length > 140) message = message.Substring(0, 140);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes("status=" + message);

                string url = "http://twitter.com/statuses/update.xml";

                string credentials = String.Format("{0}:{1}", this.Username, this.Password);
                string encodedCredentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                string authorizationHeaderValue = String.Format("Basic {0}", encodedCredentials);

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Growl for Windows/2.0";
                request.Headers.Add("Authorization", authorizationHeaderValue);
                request.ContentLength = bytes.Length;

                AsyncCallback requestCallback = new AsyncCallback(EndGetRequestStream);
                State state = new State(request, bytes);
                request.BeginGetRequestStream(requestCallback, state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void EndGetRequestStream(IAsyncResult iar)
        {
            try
            {
                State state = (State)iar.AsyncState;
                System.Net.HttpWebRequest request = state.Request;
                byte[] bytes = state.Bytes;
                System.IO.Stream requestStream = request.EndGetRequestStream(iar);
                using (requestStream)
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                AsyncCallback callback = new AsyncCallback(EndGetResponse);
                request.BeginGetResponse(callback, request);
            }
            catch
            {
            }
        }

        private void EndGetResponse(IAsyncResult iar)
        {
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)iar.AsyncState;
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.EndGetResponse(iar);
                response.Close();
            }
            catch
            {
            }
        }

        private class State
        {
            public State(System.Net.HttpWebRequest request, byte[] bytes)
            {
                this.Request = request;
                this.Bytes = bytes;
            }

            public System.Net.HttpWebRequest Request;
            public byte[] Bytes;
        }
    }
}
