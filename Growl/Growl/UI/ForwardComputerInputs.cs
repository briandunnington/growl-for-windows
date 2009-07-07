using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class ForwardComputerInputs : UserControl, IForwardInputs
    {
        private DetectedService selectedService;
        private bool isBonjour;
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);
        private bool isSubscription;

        public ForwardComputerInputs()
        {
            InitializeComponent();

            // localize text
            this.labelFormat.Text = Properties.Resources.AddComputer_FormatLabel;
            this.labelPassword.Text = Properties.Resources.AddComputer_PasswordLabel;
            this.labelPort.Text = Properties.Resources.AddComputer_PortLabel;
            this.labelAddress.Text = Properties.Resources.AddComputer_AddressLabel;
            this.labelDescription.Text = Properties.Resources.AddComputer_NameLabel;
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        private void textBoxPort_TextChanged(object sender, EventArgs e)
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
            this.isSubscription = isSubscription;

            this.textBoxDescription.HighlightColor = highlightColor;
            this.textBoxAddress.HighlightColor = highlightColor;
            this.textBoxPort.HighlightColor = highlightColor;
            this.textBoxPassword.HighlightColor = highlightColor;

            this.comboBoxFormat.Items.Add(Properties.Resources.Protocol_Type_GNTP);
            this.comboBoxFormat.Items.Add(Properties.Resources.Protocol_Type_UDP);

            // set text box values
            BonjourListItem bli = fli as BonjourListItem;
            if (bli != null)
            {
                this.isBonjour = true;
                DetectedService ds = bli.DetectedService;
                this.selectedService = ds;
                System.Net.IPEndPoint endpoint = (System.Net.IPEndPoint)ds.Service.Addresses[0];
                this.textBoxDescription.Text = ds.Service.Name;
                this.textBoxDescription.Enabled = false;
                this.textBoxAddress.Text = endpoint.Address.ToString();
                this.textBoxAddress.Enabled = false;
                this.textBoxPort.Text = endpoint.Port.ToString();
                this.textBoxPort.Enabled = false;
                this.comboBoxFormat.SelectedIndex = 0;
                this.comboBoxFormat.Enabled = false;
            }
            else
            {
                this.textBoxDescription.Text = String.Empty;
                this.textBoxDescription.Enabled = true;
                this.textBoxAddress.Text = String.Empty;
                this.textBoxAddress.Enabled = true;
                this.textBoxPort.Text = Growl.Connector.GrowlConnector.TCP_PORT.ToString();
                this.textBoxPort.Enabled = true;
                this.comboBoxFormat.SelectedIndex = 0;
                this.comboBoxFormat.Enabled = true;
            }

            if (isSubscription)
            {
                this.labelFormat.Visible = false;
                this.comboBoxFormat.Visible = false;
            }

            ValidateInputs();
        }

        public UserControl GetControl()
        {
            return this;
        }

        public ForwardComputer Save()
        {
            ForwardComputer fc = null;
            if (this.isSubscription)
            {
                Subscription subscription = new Subscription(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                fc = subscription;
            }
            else
            {
                if (this.isBonjour)
                {
                    BonjourForwardComputer bfc = new BonjourForwardComputer(textBoxDescription.Text, true, textBoxPassword.Text);
                    bfc.Update(selectedService.Service, new GrowlBonjourEventArgs(selectedService.Platform));
                    fc = bfc;
                }
                else
                {
                    bool useUDP = (comboBoxFormat.SelectedItem.ToString() == Properties.Resources.Protocol_Type_UDP ? true : false);
                    if (useUDP)
                        fc = new UDPForwardComputer(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                    else
                        fc = new GNTPForwardComputer(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                }
            }
            return fc;
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

                if (String.IsNullOrEmpty(this.textBoxAddress.Text))
                {
                    this.textBoxAddress.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxAddress.Unhighlight();
                }

                int port;
                bool validPort = int.TryParse(this.textBoxPort.Text, out port);
                if (String.IsNullOrEmpty(this.textBoxPort.Text) || !validPort)
                {
                    this.textBoxPort.Highlight();
                    valid = false;
                }
                else
                {
                    this.textBoxPort.Unhighlight();
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
