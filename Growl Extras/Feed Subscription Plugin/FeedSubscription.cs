using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;
using Growl.Connector;
using Growl.Destinations;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    [Serializable]
    public class FeedSubscription : Subscription, IDisposable
    {
        private string feedUrl;
        private int pollInterval;
        private string username;
        private string password;
        private DateTimeOffset feedLastUpdated;

        [NonSerialized]
        bool disposed;
        [NonSerialized]
        private Growl.CoreLibrary.WebClientEx webclient;
        [NonSerialized]
        private System.Timers.Timer timer;
        [NonSerialized]
        Growl.Connector.NotificationType ntNewFeedItem;
        [NonSerialized]
        Growl.Connector.NotificationType ntFeedError;

        public FeedSubscription(string description, string feedUrl, int pollInterval, string username, string password, bool enabled)
            : base(description, enabled)
        {
            this.feedUrl = feedUrl;
            this.pollInterval = pollInterval;
            this.username = username;
            this.password = password;

            Initialize();
        }

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        /// <remarks>
        /// When GfW is closed, information about configured forward destinations and subscriptions is serialized to disk.
        /// When GfW is restarted, that information is deserialized to reconstruct the instances.
        /// Use this method to perform any additional initialization that is required after the object has
        /// been deserialized (such as setting up timers, calling webservices, re-creating non-serialized fields, etc).
        /// </remarks>
        public override void OnDeserialization(object sender)
        {
            Initialize();
            base.OnDeserialization(sender);
        }

        private void Initialize()
        {
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = false;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            this.webclient = new Growl.CoreLibrary.WebClientEx();
            this.webclient.Encoding = Encoding.UTF8;
            this.webclient.OpenReadCompleted += new OpenReadCompletedEventHandler(webclient_OpenReadCompleted);

            this.ntNewFeedItem = new NotificationType("New Feed Item", "New Feed Item");
            this.ntFeedError = new NotificationType("Feed Error", "Feed Error");
        }

        public string AppName
        {
            get
            {
                return String.Format("{0} Feed", this.Description);
            }
        }

        public string FeedUrl
        {
            get
            {
                return this.feedUrl;
            }
        }

        public Uri FeedUri
        {
            get
            {
                Uri uri = null;
                if (Uri.TryCreate(this.FeedUrl, UriKind.Absolute, out uri))
                    return uri;
                else
                    return null;
            }
        }

        public int PollInterval
        {
            get
            {
                return this.pollInterval;
            }
        }

        public string Username
        {
            get
            {
                return this.username;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
        }

        public void UpdateConfiguration(string name, string url, int pollInterval, string username, string password)
        {
            this.Description = name;
            this.feedUrl = url;
            this.pollInterval = pollInterval;
            this.username = username;
            this.password = password;

            this.Available = true;
            Subscribe();
        }

        /// <summary>
        /// Gets the text used to identify the address/location of this instance to the user.
        /// </summary>
        /// <value>string</value>
        /// <remarks>
        /// This is shown as the second line of the item in the list view in GfW.
        /// </remarks>
        /// <remarks>
        /// When implemented in a derived class, this should return the effective address of the 
        /// instance, such as a url, ip:port, network name, or other identifying location.
        /// </remarks>
        public override string AddressDisplay
        {
            get
            {
                return this.FeedUrl;
            }
        }

        /// <summary>
        /// Starts the subscription listening/polling for notifications.
        /// </summary>
        /// <remarks>
        /// GfW will call this whenever the subscription should be started (enabled via UI,
        /// enabled via 'Subscribe to other computers...' setting, etc). Be careful to 
        /// ensure that you dont create duplicate subscriptions behind the scenes if this
        /// method is called multiple times.
        /// </remarks>
        public override void Subscribe()
        {
            Kill();

            Growl.Connector.Application app = new Growl.Connector.Application(this.AppName);
            app.Icon = (Growl.CoreLibrary.Resource)FeedSubscriptionHandler.Icon;
            Register(app, new NotificationType[] { this.ntNewFeedItem, this.ntFeedError });

            this.timer.Interval = this.pollInterval * 1000;
            CheckForUpdates();
        }

        /// <summary>
        /// Stops the subscription from listening/polling for notifications.
        /// </summary>
        /// <remarks>
        /// GfW will call this whenver the subscription should be disabled (disabled via UI, etc).
        /// </remarks>
        public override void Kill()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
            }

            if (this.webclient != null && this.webclient.IsBusy)
            {
                this.webclient.CancelAsync();
            }
        }

        /// <summary>
        /// Gets the icon used to represent this type of host.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        /// <remarks>
        /// By default, this will return the icon of the <see cref="DestinationBase.Platform"/>, but
        /// can be overridden to provide a custom icon.
        /// </remarks>
        public override System.Drawing.Image GetIcon()
        {
            return FeedSubscriptionHandler.Icon;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            if (this.Enabled)
            {
                Uri uri = this.FeedUri;
                if (uri != null)
                {
                    if (!this.webclient.IsBusy)
                    {
                        this.webclient.Credentials = new NetworkCredential(this.Username, this.Password);
                        this.webclient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1)");
                        this.webclient.OpenReadAsync(uri);
                    }
                }
                else
                {
                    ChangeStatus(false, "Invalid feed url");
                    SendErrorNotification("Invalid feed url.");
                }
            }
        }

        void webclient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            // process the feed
            if (!e.Cancelled)
            {
                if (e.Error == null && e.Result != null)
                {
                    FeedInfo info = null;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ProhibitDtd = false;
                    using (XmlReader reader = XmlReader.Create(e.Result, settings))
                    {
                        GenericFeedParser parser = new GenericFeedParser();
                        info = parser.Parse(reader);
                    }

                    if (info != null)
                    {
                        info.Url = this.FeedUrl;
                        ProcessFeed(info);

                        ChangeStatus(true, String.Empty);
                    }
                    else
                    {
                        // the loader couldn't load the feed
                        ChangeStatus(false, "Could not parse feed");
                        SendErrorNotification("Could not parse feed.");
                    }
                }
                else
                {
                    if (e.Error != null)
                    {
                        // there was an error returned from the call
                        ChangeStatus(false, "Error retrieving feed");
                        SendErrorNotification("Error retrieving feed.");
                    }
                    else
                    {
                        // an empty stream. 
                        ChangeStatus(false, "Error retrieving feed");
                        SendErrorNotification("Error retrieving feed.");
                    }
                }
            }

            // restart the timer
            this.timer.Start();
        }

        private void ProcessFeed(FeedInfo feed)
        {
            DateTimeOffset mostRecentItem = this.feedLastUpdated;
            if (mostRecentItem == DateTimeOffset.MaxValue) mostRecentItem = DateTimeOffset.MinValue;

            Growl.CoreLibrary.DebugInfo.WriteLine(String.Format("Feed Retrieved: {0} - Last Most Recent Item: {1}", this.Description, mostRecentItem));

            foreach (FeedItem item in feed.Items)
            {
                DateTimeOffset itemDate = item.PubDate;

                if (itemDate > mostRecentItem)
                    mostRecentItem = itemDate;

                if (itemDate > this.feedLastUpdated)
                {
                    Growl.CoreLibrary.DebugInfo.WriteLine(String.Format("{0} - New Feed Item - Published at: {1} - (last update at: {2})", this.Description, itemDate, this.feedLastUpdated));

                    item.SourceFeed = feed;

                    Notification n = new Notification(this.AppName, this.ntNewFeedItem.Name, String.Empty, this.AppName, item.Title);
                    n.Icon = feed.Icon;

                    CallbackContext c = null;
                    if (!String.IsNullOrEmpty(item.Link))
                    {
                        c = new CallbackContext(item.Link);
                    }

                    // the NotificationQueue provides some simple throttling so that the screen is not flooded with new items
                    // (especially important at load time when there could be lots of new items)
                    NotificationQueue.Enqueue(n, c);
                }
            }

            this.feedLastUpdated = mostRecentItem;  // feed.LastUpdatedTime is not always set =(
        }

        private void SendErrorNotification(string text)
        {
            Notification n = new Notification(this.AppName, this.ntFeedError.Name, String.Empty, this.AppName, text);
            Notify(n);

            Growl.CoreLibrary.DebugInfo.WriteLine(text);
        }

        public override DestinationBase Clone()
        {
            FeedSubscription clone = new FeedSubscription(this.Description, this.FeedUrl, this.PollInterval, this.Username, this.Password, this.Enabled);
            return clone;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Kill();

                    if (this.timer != null)
                    {
                        this.timer.Elapsed -= new System.Timers.ElapsedEventHandler(timer_Elapsed);
                        this.timer.Dispose();
                        this.timer = null;
                    }

                    if (this.webclient != null)
                    {
                        this.webclient.OpenReadCompleted -= new OpenReadCompletedEventHandler(webclient_OpenReadCompleted);
                        this.webclient.Dispose();
                        this.webclient = null;
                    }
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
