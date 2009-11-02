using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class TwitterForwardInputs : ForwardDestinationSettingsPanel
    {
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);

        public TwitterForwardInputs()
        {
            InitializeComponent();

            // localize text
            this.labelUsername.Text = Properties.Resources.AddTwitter_UsernameLabel;
            this.labelPassword.Text = Properties.Resources.AddTwitter_PasswordLabel;
            this.labelFormat.Text = Properties.Resources.AddTwitter_FormatLabel;
            this.labelMinimumPriority.Text = Properties.Resources.AddTwitter_MinimumPriorityLabel;
            this.checkBoxOnlyWhenIdle.Text = Properties.Resources.AddTwitter_OnlyWhenIdle;
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxFormat_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        public override void Initialize(bool isSubscription, ForwardDestinationListItem fdli, ForwardDestination fd)
        {
            this.doValidation = true;

            this.textBoxUsername.HighlightColor = highlightColor;
            this.textBoxPassword.HighlightColor = highlightColor;
            this.textBoxFormat.HighlightColor = highlightColor;

            PrefPriority[] priorityChoices = PrefPriority.GetList(false);
            this.comboBoxMinimumPriority.Items.Add(Properties.Resources.AddProwl_AnyPriority);
            for (int i = 0; i < priorityChoices.Length; i++)
            {
                this.comboBoxMinimumPriority.Items.Add(priorityChoices[i]);
            }
            this.comboBoxMinimumPriority.SelectedIndex = 0;

            // set text box values
            this.textBoxUsername.Text = String.Empty;
            this.textBoxUsername.Enabled = true;
            this.textBoxPassword.Text = String.Empty;
            this.textBoxPassword.Enabled = true;
            this.textBoxFormat.Text = TwitterForwardDestination.DefaultFormat;
            this.textBoxFormat.Enabled = true;
            this.comboBoxMinimumPriority.SelectedIndex = 0;
            this.comboBoxMinimumPriority.Enabled = true;

            TwitterForwardDestination tfd = fd as TwitterForwardDestination;
            if(tfd != null)
            {
                this.textBoxUsername.Text = tfd.Username;
                this.textBoxPassword.Text = tfd.Password;
                this.textBoxFormat.Text = tfd.Format;
                this.checkBoxOnlyWhenIdle.Checked = tfd.OnlyWhenIdle;
                if (tfd.MinimumPriority != null && tfd.MinimumPriority.HasValue)
                    this.comboBoxMinimumPriority.SelectedItem = PrefPriority.GetByValue(tfd.MinimumPriority.Value);
            }

            ValidateInputs();

            this.textBoxUsername.Focus();
        }

        public override ForwardDestination Create()
        {
            Growl.Connector.Priority? priority = null;
            PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
            if (prefPriority != null) priority = prefPriority.Priority.Value;
            TwitterForwardDestination tfd = new TwitterForwardDestination(String.Format("@{0}", this.textBoxUsername.Text), true, this.textBoxUsername.Text, this.textBoxPassword.Text, this.textBoxFormat.Text, priority, this.checkBoxOnlyWhenIdle.Checked);
            return tfd;
        }

        public override void Update(ForwardDestination fd)
        {
            TwitterForwardDestination tfd = fd as TwitterForwardDestination;
            if (tfd != null)
            {
                tfd.Username = this.textBoxUsername.Text;
                tfd.Password = this.textBoxPassword.Text;
                tfd.Format = this.textBoxFormat.Text;
                tfd.OnlyWhenIdle = this.checkBoxOnlyWhenIdle.Checked;
                PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
                tfd.MinimumPriority = (prefPriority != null ? prefPriority.Priority : null);
            }
        }

        private void ValidateInputs()
        {
            bool valid = true;
            if (this.doValidation)
            {
                if (String.IsNullOrEmpty(this.textBoxUsername.Text))
                {
                    this.textBoxUsername.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxUsername.Unhighlight();
                }

                if (String.IsNullOrEmpty(this.textBoxPassword.Text))
                {
                    this.textBoxPassword.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxPassword.Unhighlight();
                }

                if (String.IsNullOrEmpty(this.textBoxFormat.Text))
                {
                    this.textBoxFormat.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxFormat.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }
    }
}
