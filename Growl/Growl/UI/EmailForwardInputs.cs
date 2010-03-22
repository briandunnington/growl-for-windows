using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace Growl.UI
{
    public partial class EmailForwardInputs : DestinationSettingsPanel
    {
        private bool doValidation;
        private SMTPConfiguration smtp = SMTPConfiguration.Local;

        public EmailForwardInputs()
        {
            InitializeComponent();

            // localize text
            this.labelDescription.Text = Properties.Resources.AddEmail_NameLabel;
            this.labelEmail.Text = Properties.Resources.AddEmail_EmailAddressLabel;
            this.labelSMTPSettings.Text = Properties.Resources.AddEmail_SMTPSettingsLabel;
            this.labelMinimumPriority.Text = Properties.Resources.AddEmail_MinimumPriorityLabel;
            this.checkBoxOnlyWhenIdle.Text = Properties.Resources.AddEmail_OnlyWhenIdle;
            this.linkLabelEditSMTPValues.Text = Properties.Resources.AddEmail_EditSMTP;
            this.labelSMTPServer.Text = Properties.Resources.AddEmail_SMTPServerLabel;
            this.labelSMTPPort.Text = Properties.Resources.AddEmail_SMTPPortLabel;
            this.checkBoxSMTPUseAuthentication.Text = Properties.Resources.AddEmail_SMTPUseAuthentication;
            this.checkBoxSMTPUseSSL.Text = Properties.Resources.AddEmail_SMTPUseSSL;
            this.labelSMTPUsername.Text = Properties.Resources.AddEmail_SMTPUsernameLabel;
            this.labelSMTPPassword.Text = Properties.Resources.AddEmail_SMTPPasswordLabel;
            this.linkLabelSMTPDone.Text = Properties.Resources.AddEmail_SMTPDone;
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase fd)
        {
            this.doValidation = true;

            this.panelSMTPSettings.Visible = false;

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
                this.textBoxUsername.Text = efd.To;
                if (efd.MinimumPriority != null && efd.MinimumPriority.HasValue)
                    this.comboBoxMinimumPriority.SelectedItem = PrefPriority.GetByValue(efd.MinimumPriority.Value);
                this.checkBoxOnlyWhenIdle.Checked = efd.OnlyWhenIdle;
                this.smtp = efd.SMTPConfiguration;
            }
            this.labelSMTPValues.Text = String.Format("{0}", this.smtp.Host);

            ValidateInputs();

            this.textBoxDescription.Focus();
        }

        public override DestinationBase Create()
        {
            Growl.Connector.Priority? priority = null;
            PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
            if (prefPriority != null) priority = prefPriority.Priority.Value;

            EmailForwardDestination efd = new EmailForwardDestination(this.textBoxDescription.Text, true, this.textBoxUsername.Text, this.smtp, priority, this.checkBoxOnlyWhenIdle.Checked);
            return efd;
        }

        public override void Update(DestinationBase fd)
        {
            EmailForwardDestination efd = fd as EmailForwardDestination;
            if (efd != null)
            {
                efd.Description = this.textBoxDescription.Text;
                efd.To = this.textBoxUsername.Text;
                efd.OnlyWhenIdle = this.checkBoxOnlyWhenIdle.Checked;
                PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
                efd.MinimumPriority = (prefPriority != null ? prefPriority.Priority : null);
                efd.SMTPConfiguration = this.smtp;
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

        private void linkLabelEditSMTPValues_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.textBoxSMTPServer.Text = this.smtp.Host;
            this.textBoxSMTPPort.Text = this.smtp.Port.ToString();
            this.checkBoxSMTPUseAuthentication.Checked = this.smtp.UseAuthentication;
            this.checkBoxSMTPUseSSL.Checked = this.smtp.UseSSL;
            this.textBoxSMTPUsername.Text = this.smtp.Username;
            this.textBoxSMTPPassword.Text = this.smtp.Password;
            this.textBoxSMTPUsername.Enabled = this.smtp.UseAuthentication;
            this.textBoxSMTPPassword.Enabled = this.smtp.UseAuthentication;

            OnValidChanged(false);
            this.panelSMTPSettings.Visible = true;
        }

        private void linkLabelSMTPDone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.smtp.Host = this.textBoxSMTPServer.Text;
            this.smtp.Port = Convert.ToInt32(this.textBoxSMTPPort.Text);
            this.smtp.UseAuthentication = this.checkBoxSMTPUseAuthentication.Checked;
            this.smtp.UseSSL = this.checkBoxSMTPUseSSL.Checked;
            this.smtp.Username = this.textBoxSMTPUsername.Text;
            this.smtp.Password = this.textBoxSMTPPassword.Text;

            this.labelSMTPValues.Text = String.Format("{0}", this.smtp.Host);
            this.panelSMTPSettings.Visible = false;
            ValidateInputs();
        }

        private void checkBoxSMTPUseAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            this.smtp.UseAuthentication = this.checkBoxSMTPUseAuthentication.Checked;
            this.textBoxSMTPUsername.Enabled = this.smtp.UseAuthentication;
            this.textBoxSMTPPassword.Enabled = this.smtp.UseAuthentication;
        }
    }
}
