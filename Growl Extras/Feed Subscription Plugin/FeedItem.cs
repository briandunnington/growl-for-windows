using System;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    public class FeedItem
    {
        string title;
        string link;
        string description;
        DateTime pubDate;
        FeedInfo sourceFeed;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string Link
        {
            get
            {
                return this.link;
            }
            set
            {
                this.link = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public DateTime PubDate
        {
            get
            {
                return this.pubDate;
            }
            set
            {
                this.pubDate = value;
            }
        }

        public FeedInfo SourceFeed
        {
            get
            {
                return this.sourceFeed;
            }
            set
            {
                this.sourceFeed = value;
            }
        }
    }
}