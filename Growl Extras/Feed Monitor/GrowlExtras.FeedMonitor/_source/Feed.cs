using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;

namespace GrowlExtras.FeedMonitor
{
    public class Feed : IDisposable
    {
        public event EventHandler<FeedRetrievedEventArgs> FeedRetrieved;
        public event EventHandler<FeedUpdatedEventArgs> FeedUpdated;
        public event EventHandler<FeedErrorEventArgs> FeedError;

        public const string LOADING = "Loading...";
        private const string _nullResultErrorMessage = "The returned stream was null.";
        private const string _parseErrorMessage = "Unable to parse feed.";

        private string name;
        private Uri url;
        private int pollInterval;
        private DateTime lastCheckForUpdates;
        private DateTimeOffset feedLastUpdated;
        private WebClient webclient;
        private System.Timers.Timer timer;

        private Feed() { }

        public static Feed Create(string url, int pollInterval)
        {
            return new Feed(url, pollInterval, DateTime.MaxValue);
        }

        private Feed(string url, int pollInterval, DateTime lastCheckForUpdates)
        {
            this.name = LOADING;
            this.url = new Uri(url);
            this.pollInterval = pollInterval;
            this.lastCheckForUpdates = lastCheckForUpdates;
            this.feedLastUpdated = DateTimeOffset.MaxValue;

            this.timer = new System.Timers.Timer();
            this.timer.Interval = this.pollInterval * 60 * 1000;
            this.timer.AutoReset = false;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            this.webclient = new WebClient();
            this.webclient.Encoding = Encoding.UTF8;
            this.webclient.OpenReadCompleted += new OpenReadCompletedEventHandler(webclient_OpenReadCompleted);

            //CheckForUpdates();
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Url
        {
            get
            {
                return this.url.ToString();
            }
        }

        public int PollInterval
        {
            get
            {
                return this.pollInterval;
            }
            set
            {
                this.pollInterval = value;
                this.timer.Interval = this.pollInterval * 60 * 1000;
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();
            this.CheckForUpdates();
        }

        public void CheckForUpdates()
        {
            if (!this.webclient.IsBusy)
            {
                this.webclient.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1)");
                this.webclient.OpenReadAsync(this.url);
                this.lastCheckForUpdates = DateTime.Now;
            }
        }

        void webclient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            WebClient wc = (WebClient)sender;

            // process the feed
            if (e.Error == null && e.Result != null)
            {
                /* THIS IS JUST FOR TESTING
                List<byte> chars = new List<byte>();
                System.IO.StreamReader r = new System.IO.StreamReader(e.Result);
                using (r)
                {
                    while (r.Peek() > 0)
                    {
                        chars.Add((byte) r.Read());
                    }
                }
                byte[] bytes = chars.ToArray();
                string s = System.Text.Encoding.UTF8.GetString(bytes);
                Console.WriteLine(s);
                 * */

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
                    info.Url = this.Url;
                    OnFeedRetrieved(info);
                }
                else
                {
                    // the loader couldn't load the feed
                    FeedErrorEventArgs args = new FeedErrorEventArgs(new FeedException(_parseErrorMessage));
                    OnFeedError(args);
                }
            }
            else
            {
                if (e.Error != null)
                {
                    // there was an error returned from the call
                    FeedErrorEventArgs args = new FeedErrorEventArgs(e.Error);
                    OnFeedError(args);
                }
                else
                {
                    // an empty stream. 
                    FeedErrorEventArgs args = new FeedErrorEventArgs(new FeedException(_nullResultErrorMessage));
                    OnFeedError(args);
                }
            }

            // restart the timer
            this.timer.Start();
        }

        /// <summary>
        /// Internal method used to raise the feed event
        /// </summary>
        protected void OnFeedRetrieved(FeedInfo feed)
        {
            this.name = feed.Title;
            DateTimeOffset mostRecentItem = this.feedLastUpdated;
            if (mostRecentItem == DateTimeOffset.MaxValue) mostRecentItem = DateTimeOffset.MinValue;

            System.Diagnostics.Debug.WriteLine(String.Format("Feed Retrieved: {0} - Most Recent Item: {1}", this.name, mostRecentItem));

            if (FeedRetrieved != null)
            {
                FeedRetrievedEventArgs e = new FeedRetrievedEventArgs(feed);
                FeedRetrieved(this, e);
            }

            if (FeedUpdated != null)
            {
                List<FeedItem> newitems = new List<FeedItem>();
                foreach (FeedItem item in feed.Items)
                {
                    DateTimeOffset itemDate = item.PubDate;
                    //if (itemDate == DateTimeOffset.MinValue) itemDate = item.PublishDate;

                    System.Diagnostics.Debug.WriteLine(String.Format("Item Published at: {0} - (last update at: {1})", itemDate, this.feedLastUpdated));

                    if (itemDate > mostRecentItem)
                        mostRecentItem = itemDate;

                    if (itemDate > this.feedLastUpdated)
                    {
                        newitems.Add(item);
                        item.SourceFeed = feed;
                    }
                }

                FeedUpdatedEventArgs args = new FeedUpdatedEventArgs(newitems);
                FeedUpdated(this, args);
            }
            this.feedLastUpdated = mostRecentItem;  // feed.LastUpdatedTime is not always set =(
        }

        /// <summary>
        /// Internal method used to raise the error event
        /// </summary>
        protected void OnFeedError(FeedErrorEventArgs args)
        {
            if (FeedError != null)
                FeedError(this, args);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                    this.timer.Dispose();

                if (this.webclient != null)
                    this.webclient.Dispose();
            }
        }

        #endregion
    }

    public class FeedException : Exception
    {
        public FeedException()
            : base("Exception retrieving or processing feed.")
        {
        }

        public FeedException(string message)
            : base("Exception retrieving or processing feed. " + message)
        {
        }

        public FeedException(string message, Exception innerException)
            : base("Exception retrieving or processing feed. " + message, innerException)
        {
        }

    }

    public class FeedUpdatedEventArgs : EventArgs
    {
        private List<FeedItem> newitems;

        public FeedUpdatedEventArgs(List<FeedItem> newitems)
        {
            this.newitems = newitems;
        }

        public List<FeedItem> NewItems
        {
            get { return this.newitems; }
        }

    }

    public class FeedRetrievedEventArgs : EventArgs
    {
        private FeedInfo feed;

        public FeedRetrievedEventArgs(FeedInfo feed)
        {
            this.feed = feed;
        }

        public FeedInfo Feed
        {
            get { return this.feed; }
        }

    }

    public class FeedErrorEventArgs : EventArgs
    {
        private Exception exception;

        public FeedErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception Error
        {
            get { return this.exception; }
        }

    }
}
