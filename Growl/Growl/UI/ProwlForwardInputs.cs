using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class ProwlForwardInputs : UserControl, IForwardInputs
    {
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);

        public ProwlForwardInputs()
        {
            InitializeComponent();

            // localize text
            this.labelDescription.Text = Properties.Resources.AddProwl_NameLabel;
            this.labelUsername.Text = Properties.Resources.AddProwl_UsernameLabel;
            this.labelPassword.Text = Properties.Resources.AddProwl_PasswordLabel;
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

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        #region IForwardInputs Members

        public void Initialize(bool isSubscription, ForwardListItem fli)
        {
            this.doValidation = true;

            this.textBoxDescription.HighlightColor = highlightColor;
            this.textBoxUsername.HighlightColor = highlightColor;
            this.textBoxPassword.HighlightColor = highlightColor;

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
            this.textBoxPassword.Text = String.Empty;
            this.textBoxPassword.Enabled = true;
            this.comboBoxMinimumPriority.SelectedIndex = 0;
            this.comboBoxMinimumPriority.Enabled = true;

            ValidateInputs();
        }

        public UserControl GetControl()
        {
            return this;
        }

        public ForwardComputer Save()
        {
            Growl.Connector.Priority priority = Growl.Connector.Priority.VeryLow;
            PrefPriority prefPriority = this.comboBoxMinimumPriority.SelectedItem as PrefPriority;
            if (prefPriority != null) priority = prefPriority.Priority.Value;
            ProwlForwardComputer pfc = new ProwlForwardComputer(this.textBoxDescription.Text, true, this.textBoxUsername.Text, this.textBoxPassword.Text, priority);
            return pfc;
        }

        public event ValidChangedEventHandler ValidChanged;

        #endregion

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

                if (String.IsNullOrEmpty(this.textBoxPassword.Text))
                {
                    this.textBoxPassword.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxPassword.Unhighlight();
                }
            }
            OnValidChanged(valid);
        }

        protected void OnValidChanged(bool isValid)
        {
            if (ValidChanged != null)
            {
                ValidChanged(isValid);
            }
        }
    }
}
