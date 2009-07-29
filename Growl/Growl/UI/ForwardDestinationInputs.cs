using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public partial class ForwardDestinationInputs : ForwardDestinationSettingsPanel
    {
        private DetectedService selectedService;
        private bool isBonjour;
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);
        private bool isSubscription;
        private string g;

        public ForwardDestinationInputs()
        {
            InitializeComponent();

            // localize text
            this.labelFormat.Text = Properties.Resources.AddComputer_FormatLabel;
            this.labelPassword.Text = Properties.Resources.AddComputer_PasswordLabel;
            this.labelPort.Text = Properties.Resources.AddComputer_PortLabel;
            this.labelAddress.Text = Properties.Resources.AddComputer_AddressLabel;
            this.labelDescription.Text = Properties.Resources.AddComputer_NameLabel;

            this.g = System.Guid.NewGuid().ToString();
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

        public override void Initialize(bool isSubscription, ForwardDestinationListItem fdli, ForwardDestination fd)
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
            BonjourListItem bli = fdli as BonjourListItem;
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
                this.textBoxPassword.Focus();
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
                this.textBoxDescription.Focus();
            }

            // edits
            if (fd != null)
            {
                this.textBoxDescription.Text = fd.Description;
                this.comboBoxFormat.Enabled = false;
                Subscription subscription = fd as Subscription;
                if (subscription != null)
                {
                    this.textBoxAddress.Text = subscription.IPAddress;
                    this.textBoxPort.Text = subscription.Port.ToString();
                    this.textBoxPassword.Text = subscription.Password;
                    this.comboBoxFormat.SelectedItem = Properties.Resources.Protocol_Type_GNTP;
                    this.comboBoxFormat.Visible = false;
                }
                GNTPForwardDestination gntp = fd as GNTPForwardDestination;
                if (gntp != null)
                {
                    this.textBoxAddress.Text = gntp.IPAddress;
                    this.textBoxPort.Text = gntp.Port.ToString();
                    this.textBoxPassword.Text = gntp.Password;
                    this.comboBoxFormat.SelectedItem = Properties.Resources.Protocol_Type_GNTP;
                }
                BonjourForwardDestination bonjour = fd as BonjourForwardDestination;
                if (bonjour != null)
                {
                    this.textBoxAddress.Text = bonjour.IPAddress;
                    this.textBoxPort.Text = bonjour.Port.ToString();
                    this.textBoxPassword.Text = bonjour.Password;
                    this.comboBoxFormat.SelectedItem = Properties.Resources.Protocol_Type_GNTP;
                    this.textBoxDescription.Enabled = false;
                    this.textBoxAddress.Enabled = false;
                    this.textBoxPort.Enabled = false;
                    this.textBoxPassword.Focus();
                }
                UDPForwardDestination udp = fd as UDPForwardDestination;
                if (udp != null)
                {
                    this.textBoxAddress.Text = udp.IPAddress;
                    this.textBoxPort.Text = udp.Port.ToString();
                    this.textBoxPassword.Text = udp.Password;
                    this.comboBoxFormat.SelectedItem = Properties.Resources.Protocol_Type_UDP;
                }
            }

            if (isSubscription)
            {
                this.labelFormat.Visible = false;
                this.comboBoxFormat.Visible = false;
            }

            ValidateInputs();
        }

        public override ForwardDestination Create()
        {
            ForwardDestination fc = null;
            if (this.isSubscription)
            {
                Subscription subscription = new Subscription(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text, Properties.Settings.Default.EnableSubscriptions);
                fc = subscription;
            }
            else
            {
                if (this.isBonjour)
                {
                    BonjourForwardDestination bfc = new BonjourForwardDestination(textBoxDescription.Text, true, textBoxPassword.Text);
                    bfc.Update(selectedService.Service, new GrowlBonjourEventArgs(selectedService.Platform));
                    fc = bfc;
                }
                else
                {
                    bool useUDP = (comboBoxFormat.SelectedItem.ToString() == Properties.Resources.Protocol_Type_UDP ? true : false);
                    if (useUDP)
                        fc = new UDPForwardDestination(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                    else
                        fc = new GNTPForwardDestination(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                }
            }
            return fc;
        }

        public override void Update(ForwardDestination fd)
        {
            if (this.isSubscription)
            {
                Subscription subscription = fd as Subscription;
                subscription.Description = this.textBoxDescription.Text;
                subscription.IPAddress = this.textBoxAddress.Text;
                subscription.Port = Convert.ToInt32(this.textBoxPort.Text);
                subscription.Password = this.textBoxPassword.Text;
                subscription.Update();
            }
            else
            {
                GNTPForwardDestination gntp = fd as GNTPForwardDestination;
                if (gntp != null)
                {
                    gntp.Description = this.textBoxDescription.Text;
                    gntp.IPAddress = this.textBoxAddress.Text;
                    gntp.Port = Convert.ToInt32(this.textBoxPort.Text);
                    gntp.Password = this.textBoxPassword.Text;
                }
                BonjourForwardDestination bonjour = fd as BonjourForwardDestination;
                if (bonjour != null)
                {
                    bonjour.Description = this.textBoxDescription.Text;
                    bonjour.Password = this.textBoxPassword.Text;
                }
                UDPForwardDestination udp = fd as UDPForwardDestination;
                if (udp != null)
                {
                    udp.Description = this.textBoxDescription.Text;
                    udp.IPAddress = this.textBoxAddress.Text;
                    udp.Port = Convert.ToInt32(this.textBoxPort.Text);
                    udp.Password = this.textBoxPassword.Text;
                }
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
    }
}
