using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    public class NotifyIOListener : IDisposable
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<NotificationReceivedEventArgs> NotificationReceived;

        private string outletUrl;
        private bool autoReset = true;
        private ISynchronizeInvoke synchronizingObject;
        CometClient comet;
        private bool isStopping;

        public NotifyIOListener(string outletUrl, ISynchronizeInvoke synchronizingObject)
        {
            this.outletUrl = outletUrl;
            this.synchronizingObject = synchronizingObject;
        }

        public string OutletUrl
        {
            get
            {
                return this.outletUrl;
            }
        }

        public bool AutoReset
        {
            get
            {
                return this.autoReset;
            }
            set
            {
                this.autoReset = value;
            }
        }

        public void Start()
        {
            if (!this.isStopping)
            {
                this.isStopping = false;

                //string url = String.Format("http://api.notify.io/v1/listen/{0}?api_key={1}", this.hash, this.apiKey);
                //string url = "http://api.notify.io/v1/listen/a56bd4435f989f8d7447f737e24991519ea94652";
                //http://www.notify.io/outlets/a56bd4435f989f8d7447f737e24991519ea94652.ListenURL

                string url = this.outletUrl;

                if (this.comet == null)
                {
                    this.comet = new CometClient(url);
                    comet.Connected += new EventHandler(comet_Connected);
                    comet.Disconnected += new EventHandler(comet_Disconnected);
                    comet.ResponseReceived += new CometClient.ResponseReceivedEventHandler(comet_ResponseReceived);
                }

                StartCometClient(comet, 0);
            }
        }

        public void Stop()
        {
            if (this.comet != null)
            {
                this.isStopping = true;
                this.comet.Stop();
                this.comet.Connected -= new EventHandler(comet_Connected);
                this.comet.Disconnected -= new EventHandler(comet_Disconnected);
                this.comet.ResponseReceived -= new CometClient.ResponseReceivedEventHandler(comet_ResponseReceived);
                this.comet.Dispose();
                this.comet = null;
            }
        }

        void comet_Connected(object sender, EventArgs e)
        {
            this.OnConnected();
        }

        void comet_Disconnected(object sender, EventArgs e)
        {
            this.OnDisconnected();

            if (this.AutoReset && !this.isStopping)
            {
                StartCometClient((CometClient)sender, 1);
            }
        }

        private void StartCometClient(CometClient client, int wait)
        {
            if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
            {
                MethodInvoker invoker = new MethodInvoker(delegate()
                {
                    StartCometClient(client, wait);
                });

                this.synchronizingObject.Invoke(invoker, null);
            }
            else
            {
                client.Stop();
                client.Start();
            }
        }

        void comet_ResponseReceived(string response)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(HandleNotification), response);
        }

        private void HandleNotification(object state)
        {
            try
            {
                string response = (string)state;
                Utility.WriteDebugInfo(response);

                if (String.IsNullOrEmpty(response)) return;  // empty response

                int index = response.IndexOf("\r\n");
                if (index < 1) return;                       // malformed response - missing \r\n

                string l = response.Substring(0, index);
                if (String.IsNullOrEmpty(l)) return;         // malformed response - no length specifier
                int length = 0;
                bool ok = int.TryParse(l, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out length);
                if (!ok) return;                            // malformed response - invalid length specifier

                int start = index + 1;  // index plus one to account for \n
                int total = start + length;
                if (start >= response.Length) return;        // missing notification data
                if (total > response.Length) return;         // truncated notification data

                // ok - if we are here, we are good to go
                string json = response.Substring(start, length);

                /* DONT USE THIS - we cant control the JsonSerializer so we cant set the MissingMemberHandling property
                object obj = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(json, typeof(NotificationReceivedEventArgs));
                NotificationReceivedEventArgs args = (NotificationReceivedEventArgs)obj;
                 * */

                System.IO.StringReader sr = new System.IO.StringReader(json);
                using (sr)
                {
                    Newtonsoft.Json.JsonSerializer js = new Newtonsoft.Json.JsonSerializer();
                    js.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                    js.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    NotificationReceivedEventArgs args = (NotificationReceivedEventArgs) js.Deserialize(sr, typeof(NotificationReceivedEventArgs));
                    this.OnNotificationReceived(args);
                }
            }
            catch
            {
                // dont fail if the notification could not be handled properly
            }
        }

        protected void OnConnected()
        {
            if (this.Connected != null)
            {
                if(this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
                {
                    this.synchronizingObject.Invoke(new MethodInvoker(delegate(){
                        OnConnected();
                    }), null);
                }
                else
                {
                    this.Connected(this, EventArgs.Empty);
                }
            }
        }

        protected void OnDisconnected()
        {
            if (this.Disconnected != null)
            {
                if(this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
                {
                    this.synchronizingObject.Invoke(new MethodInvoker(delegate(){
                        OnDisconnected();
                    }), null);
                }
                else
                {
                    this.Disconnected(this, EventArgs.Empty);
                }
            }
        }

        protected void OnNotificationReceived(NotificationReceivedEventArgs nrea)
        {
            if (this.NotificationReceived != null)
            {
                if (this.synchronizingObject != null && this.synchronizingObject.InvokeRequired)
                {
                    this.synchronizingObject.Invoke(new MethodInvoker(delegate()
                    {
                        OnNotificationReceived(nrea);
                    }), null);
                }
                else
                {
                    this.NotificationReceived(this, nrea);
                }
            }
        }

        public class NotificationReceivedEventArgs : EventArgs
        {
            [Newtonsoft.Json.JsonProperty("title")]
            public string Title;

            [Newtonsoft.Json.JsonProperty("text")]
            public string Text;

            [Newtonsoft.Json.JsonProperty("sticky")]
            public bool Sticky;

            [Newtonsoft.Json.JsonProperty("icon")]
            public string Icon;

            [Newtonsoft.Json.JsonProperty("link")]
            public string Link;

            [Newtonsoft.Json.JsonProperty("tags")]
            public string Tags;

            [Newtonsoft.Json.JsonProperty("source")]
            public string Source = "notify.io";

            [Newtonsoft.Json.JsonProperty("sourceIcon")]
            public string SourceIcon = "http://www.growlforwindows.com/gfw/images/plugins/notifyio_icon.png";

            [Newtonsoft.Json.JsonProperty("approval")]
            public bool Approval;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Stop();
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
