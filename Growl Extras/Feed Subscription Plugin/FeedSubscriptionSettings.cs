using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    public partial class FeedSubscriptionSettings : DestinationSettingsPanel
    {
        private bool doValidation;

        public FeedSubscriptionSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the configuration UI when a subscription is being added or edited.
        /// </summary>
        /// <param name="isSubscription">will always be <c>true</c> for <see cref="Subscription"/>s</param>
        /// <param name="dli">The <see cref="DestinationListItem"/> that the user selected</param>
        /// <param name="db">The <see cref="DestinationBase"/> of the item if it is being edited;<c>null</c> otherwise</param>
        /// <remarks>
        /// When an instance is being edited (<paramref name="dli"/> != null), make sure to repopulate any
        /// inputs with the current values.
        /// 
        /// By default, the 'Save' button is disabled and you must call <see cref="DestinationSettingsPanel.OnValidChanged"/>
        /// in order to enable it when appropriate.
        /// </remarks>
        public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase db)
        {
            this.doValidation = true;

            // set text box values
            this.textBoxUrl.Text = String.Empty;
            this.textBoxUrl.Enabled = true;
            this.textBoxName.Text = String.Empty;
            this.textBoxName.Enabled = true;

            this.comboBoxPoll.DataSource = PollInterval.GetList();
            this.comboBoxPoll.DisplayMember = "Display";
            this.comboBoxPoll.ValueMember = "Value";

            this.textBoxUsername.Text = String.Empty;
            this.textBoxUsername.Enabled = true;
            this.textBoxPassword.Text = String.Empty;
            this.textBoxPassword.Enabled = true;

            FeedSubscription fs = db as FeedSubscription;
            if (fs != null)
            {
                this.textBoxUrl.Text = fs.FeedUrl;
                this.textBoxName.Text = fs.Description;
                this.comboBoxPoll.SelectedValue = fs.PollInterval;
                this.textBoxUsername.Text = fs.Username;
                this.textBoxPassword.Text = fs.Password;
            }
            ValidateInputs();

            this.textBoxName.Focus();
        }

        /// <summary>
        /// Creates a new instance of the subscriber.
        /// </summary>
        /// <returns>New <see cref="FeedSubscription"/></returns>
        /// <remarks>
        /// This is called when the user is adding a new subscription and clicks the 'Save' button.
        /// </remarks>
        public override DestinationBase Create()
        {
            FeedSubscription fs = new FeedSubscription(this.textBoxName.Text, this.textBoxUrl.Text, Convert.ToInt32(this.comboBoxPoll.SelectedValue), this.textBoxUsername.Text, this.textBoxPassword.Text, true);
            fs.Subscribe();
            return fs;
        }

        /// <summary>
        /// Updates the specified subscription instance.
        /// </summary>
        /// <param name="db">The <see cref="FeedSubscription"/> to update</param>
        /// <remarks>
        /// This is called when a user is editing an existing subscription and clicks the 'Save' button.
        /// </remarks>
        public override void Update(DestinationBase db)
        {
            FeedSubscription fs = db as FeedSubscription;
            if (fs != null)
            {
                fs.UpdateConfiguration(this.textBoxName.Text, this.textBoxUrl.Text, Convert.ToInt32(this.comboBoxPoll.SelectedValue), this.textBoxUsername.Text, this.textBoxPassword.Text);
            }
        }

        private void ValidateInputs()
        {
            bool valid = true;
            if (this.doValidation)
            {
                if (String.IsNullOrEmpty(this.textBoxUrl.Text))
                {
                    this.textBoxUrl.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxUrl.Unhighlight();
                }

                if (String.IsNullOrEmpty(this.textBoxName.Text))
                {
                    this.textBoxName.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxName.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }

        private class PollInterval
        {
            private static List<PollInterval> list;

            public static List<PollInterval> GetList()
            {
                if (list == null)
                {
                    list = new List<PollInterval>();
                    list.Add(new PollInterval(60, "1 minute"));
                    list.Add(new PollInterval(120, "2 minutes"));
                    list.Add(new PollInterval(300, "5 minutes"));
                    list.Add(new PollInterval(600, "10 minutes"));
                    list.Add(new PollInterval(900, "15 minutes"));
                    list.Add(new PollInterval(1800, "30 minutes"));
                    list.Add(new PollInterval(3600, "60 minutes"));
                }
                return list;
            }

            private int val;
            private string display;

            private PollInterval(int val, string display)
            {
                this.val = val;
                this.display = display;
            }

            public int Value
            {
                get
                {
                    return this.val;
                }
            }

            public string Display
            {
                get
                {
                    return this.display;
                }
            }
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxUrl_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }
    }
}
