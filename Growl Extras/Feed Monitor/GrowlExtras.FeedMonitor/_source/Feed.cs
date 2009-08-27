using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;

namespace GrowlExtras.FeedMonitor
{
    public class Feed
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
                this.webclient.OpenReadAsync(this.url);
                this.lastCheckForUpdates = DateTime.Now;
            }
        }

        void webclient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            // process the feed
            if (e.Error == null && e.Result != null)
            {
                using (XmlReader reader = XmlReader.Create(e.Result))
                {
                    try
                    {
                        SyndicationFeed feed = null;
                        try
                        {
                            feed = SyndicationFeed.Load(reader);
                        }
                        catch
                        {
                            // the loader couldn't load the feed, but it might be RSS 1.0, so try that
                            Rss10FeedFormatter formatter = new Rss10FeedFormatter();
                            if (formatter.CanRead(reader))
                            {
                                formatter.ReadFrom(reader);
                                feed = formatter.Feed;
                            }
                        }
                        OnFeedRetrieved(feed);
                    }
                    catch (Exception ex)
                    {
                        // the loader couldn't load the feed
                        FeedErrorEventArgs args = new FeedErrorEventArgs(new FeedException(_parseErrorMessage, ex));
                        OnFeedError(args);
                    }
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
        protected void OnFeedRetrieved(SyndicationFeed feed)
        {
            this.name = feed.Title.Text;
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
                List<SyndicationItem> newitems = new List<SyndicationItem>();
                foreach (SyndicationItem item in feed.Items)
                {
                    DateTimeOffset itemDate = item.LastUpdatedTime;
                    if (itemDate == DateTimeOffset.MinValue) itemDate = item.PublishDate;

                    System.Diagnostics.Debug.WriteLine(String.Format("Item Published at: {0} - (last update at: {1})", itemDate, this.feedLastUpdated));

                    if (itemDate > mostRecentItem)
                        mostRecentItem = itemDate;

                    if (itemDate > this.feedLastUpdated)
                    {
                        newitems.Add(item);
                        //item.SourceFeed.Title = feed.Title; // override this value since some feed items specify a different source (and we dont want that)
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
        private List<SyndicationItem> newitems;

        public FeedUpdatedEventArgs(List<SyndicationItem> newitems)
        {
            this.newitems = newitems;
        }

        public List<SyndicationItem> NewItems
        {
            get { return this.newitems; }
        }

    }

    public class FeedRetrievedEventArgs : EventArgs
    {
        private SyndicationFeed feed;

        public FeedRetrievedEventArgs(SyndicationFeed feed)
        {
            this.feed = feed;
        }

        public SyndicationFeed Feed
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
