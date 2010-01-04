using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class RssSubscription : Subscription
    {
        private string url;
        private string username;
        private string password;
        private int pollInterval = 300;

        public RssSubscription(string name, string url, string username, string password, int pollInterval, bool enabled)
            : base(name, enabled, true)
        {
            this.url = url;
            this.username = username;
            this.password = password;
            this.pollInterval = pollInterval;
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

        public int PollInterval
        {
            get
            {
                return this.pollInterval;
            }
            set
            {
                this.pollInterval = value;
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

        protected override void Subscribe()
        {
            base.Subscribe();
        }

        public override void Kill()
        {
            base.Kill();
        }

        public override string  AddressDisplay
        {
            get
            {
                return String.Format("{0} (Poll Interval:{1})", this.Description, this.PollInterval);
            }
        }

        public override DestinationBase Clone()
        {
            RssSubscription clone = new RssSubscription(this.Description, this.Url, this.Username, this.Password, this.PollInterval, this.Enabled);
            return clone;
        }
    }
}
