using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class EmailForwardInputs : ForwardDestinationSettingsPanel
    {
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);

        public EmailForwardInputs()
        {
            InitializeComponent();

            // localize text
            this.labelDescription.Text = Properties.Resources.AddProwl_NameLabel;
            this.labelEmail.Text = Properties.Resources.AddProwl_APIKeyLabel;
            this.labelSMTPSettings.Text = Properties.Resources.AddProwl_APIKeyLabel;
            this.labelMinimumPriority.Text = Properties.Resources.AddProwl_MinimumPriorityLabel;
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        public override void Initialize(bool isSubscription, ForwardDestinationListItem fdli, ForwardDestination fd)
        {
            this.doValidation = true;

            this.textBoxDescription.HighlightColor = highlightColor;
            this.textBoxUsername.HighlightColor = highlightColor;

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
            this.textBoxUsername.Text = String.Empty;
            this.textBoxUsername.Enabled = true;
            this.comboBoxMinimumPriority.SelectedIndex = 0;
            this.comboBoxMinimumPriority.Enabled = true;

            EmailForwardDestination efd = fd as EmailForwardDestination;
            if (efd != null)
            {
                this.textBoxDescription.Text = efd.Description;
                this.textBoxUsername.Text = efd.To; // TODO:
                if (efd.MinimumPriority != null && efd.MinimumPriority.HasValue)
                    this.comboBoxMinimumPriority.SelectedItem = PrefPriority.GetByValue(efd.MinimumPriority.Value);
                this.checkBoxOnlyWhenIdle.Checked = efd.OnlyWhenIdle;
                // TODO: SMTP settings
            }

            ValidateInputs();
        }

        public override ForwardDestination Create()
        {
            Growl.Connector.Priority? priority = null;
            PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
            if (prefPriority != null) priority = prefPriority.Priority.Value;
            EmailForwardDestination efd = new EmailForwardDestination(this.textBoxDescription.Text, true, this.textBoxUsername.Text, null, priority, this.checkBoxOnlyWhenIdle.Checked);
            return efd;
        }

        public override void Update(ForwardDestination fd)
        {
            EmailForwardDestination efd = fd as EmailForwardDestination;
            if (efd != null)
            {
                efd.Description = this.textBoxDescription.Text;
                efd.To = this.textBoxUsername.Text; // TODO:
                efd.OnlyWhenIdle = this.checkBoxOnlyWhenIdle.Checked;
                PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
                efd.MinimumPriority = (prefPriority != null ? prefPriority.Priority : null);

                /* // TODO:
                SMTPConfiguration smtpConfig = new SMTPConfiguration();
                smtpConfig.Host;
                smtpConfig.Port;
                smtpConfig.Username;
                smtpConfig.Password;
                smtpConfig.UseAuthentication;
                smtpConfig.UseSSL;
                efd.SMTPConfiguration = smtpConfig;
                 * */
            }
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

                if (String.IsNullOrEmpty(this.textBoxUsername.Text))
                {
                    this.textBoxUsername.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxUsername.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }
    }
}
