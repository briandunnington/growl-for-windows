using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowlExtras.FeedMonitor
{
    public class FeedInfo
    {
        string actualTitle;
        string customTitle;
        string url;
        List<FeedItem> items;

        public string Title
        {
            get
            {
                if (String.IsNullOrEmpty(this.customTitle))
                    return this.actualTitle;
                else
                    return this.customTitle;
            }
        }

        public string ActualTitle
        {
            get
            {
                return this.actualTitle;
            }
            set
            {
                this.actualTitle = value;
            }
        }

        public string CustomTitle
        {
            get
            {
                return this.customTitle;
            }
            set
            {
                this.customTitle = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        public List<FeedItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
            }
        }
    }
}
