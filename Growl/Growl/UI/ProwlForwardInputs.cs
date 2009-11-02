using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class ProwlForwardInputs : ForwardDestinationSettingsPanel
    {
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);

        public ProwlForwardInputs()
        {
            InitializeComponent();

            // localize text
            this.labelDescription.Text = Properties.Resources.AddProwl_NameLabel;
            this.labelAPIKey.Text = Properties.Resources.AddProwl_APIKeyLabel;
            this.labelMinimumPriority.Text = Properties.Resources.AddProwl_MinimumPriorityLabel;
            this.checkBoxOnlyWhenIdle.Text = Properties.Resources.AddProwl_OnlyWhenIdle;
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxAPIKey_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

         public override void Initialize(bool isSubscription, ForwardDestinationListItem fdli, ForwardDestination fd)
        {
            this.doValidation = true;

            this.textBoxDescription.HighlightColor = highlightColor;
            this.textBoxAPIKey.HighlightColor = highlightColor;

            PrefPriority[] priorityChoices = PrefPriority.GetList(false);
            this.comboBoxMinimumPriority.Items.Add(Properties.Resources.AddProwl_AnyPriority);
            for (int i = 0; i < priorityChoices.Length; i++)
            {
                this.comboBoxMinimumPriority.Items.Add(priorityChoices[i]);
            }
            this.comboBoxMinimumPriority.SelectedIndex = 0;

            // set text box values
            this.textBoxDescription.Text = String.Empty;
            this.textBoxDescription.Enabled = true;
            this.textBoxAPIKey.Text = String.Empty;
            this.textBoxAPIKey.Enabled = true;
            this.comboBoxMinimumPriority.SelectedIndex = 0;
            this.comboBoxMinimumPriority.Enabled = true;

            ProwlForwardDestination pfd = fd as ProwlForwardDestination;
            if (pfd != null)
            {
                this.textBoxDescription.Text = pfd.Description;
                this.textBoxAPIKey.Text = pfd.APIKey;
                if (pfd.MinimumPriority != null && pfd.MinimumPriority.HasValue)
                    this.comboBoxMinimumPriority.SelectedItem = PrefPriority.GetByValue(pfd.MinimumPriority.Value);
                this.checkBoxOnlyWhenIdle.Checked = pfd.OnlyWhenIdle;
            }

            ValidateInputs();

            this.textBoxDescription.Focus();
        }

        public override ForwardDestination Create()
        {
            Growl.Connector.Priority? priority = null;
            PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
            if (prefPriority != null) priority = prefPriority.Priority.Value;
            ProwlForwardDestination pfd = new ProwlForwardDestination(this.textBoxDescription.Text, true, this.textBoxAPIKey.Text, priority, this.checkBoxOnlyWhenIdle.Checked);
            SendConfirmation(pfd);
            return pfd;
        }

        public override void Update(ForwardDestination fd)
        {
            ProwlForwardDestination pfd = fd as ProwlForwardDestination;
            if (pfd != null)
            {
                pfd.Description = this.textBoxDescription.Text;
                pfd.APIKey = this.textBoxAPIKey.Text;
                pfd.OnlyWhenIdle = this.checkBoxOnlyWhenIdle.Checked;

                PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
                pfd.MinimumPriority = (prefPriority != null ? prefPriority.Priority : null);

                SendConfirmation(pfd);
            }
        }

        private void SendConfirmation(ProwlForwardDestination pfd)
        {
            // always use Emergency priority in case they have it configured to restrict by priority
            Growl.Connector.Notification notification = new Growl.Connector.Notification(Properties.Resources.SystemNotification_ApplicationName, Properties.Resources.ProwlConfirmation_Title, null, Properties.Resources.ProwlConfirmation_Title, Properties.Resources.ProwlConfirmation_Text, null, false, Growl.Connector.Priority.Emergency, null);
            // always use isIdle in case they have it configured to only send when idle
            pfd.ForwardNotification(notification, null, null, true, null);
        }

        private void ValidateInputs()
        {
            bool valid = true;
            if (this.doValidation)
            {
                if (String.IsNullOrEmpty(this.textBoxDescription.Text))
                {
                    this.textBoxDescription.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxDescription.Unhighlight();
                }

                if (String.IsNullOrEmpty(this.textBoxAPIKey.Text))
                {
                    this.textBoxAPIKey.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxAPIKey.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }
    }
}
