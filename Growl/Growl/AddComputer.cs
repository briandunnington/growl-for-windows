using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZeroconfService;
using Growl.UI;

namespace Growl
{
    public partial class AddComputer : Form
    {
        private Controller controller;
        private DetectedService selectedService;
        private bool isBonjour;
        private bool doValidation;
        private Color highlightColor = Color.FromArgb(254, 250, 184);
        private bool isSubscription;

        public AddComputer()
        {
            InitializeComponent();
        }

        public AddComputer(bool isSubscription)
            : this()
        {
            this.isSubscription = isSubscription;
        }

        private void AddComputer_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(240, 240, 240);

            this.textBoxDescription.HighlightColor = highlightColor;
            this.textBoxAddress.HighlightColor = highlightColor;
            this.textBoxPort.HighlightColor = highlightColor;
            this.textBoxPassword.HighlightColor = highlightColor;

            this.comboBoxFormat.Items.Add("GNTP");
            this.comboBoxFormat.Items.Add("UDP");

            if (this.controller != null)
            {
                Dictionary<string, DetectedService> availableServices = controller.DetectedServices;
                foreach (DetectedService ds in availableServices.Values)
                {
                    BonjourListItem bli = new BonjourListItem(ds);
                    bli.Selected += new BonjourListItemSelectedEventHandler(bli_Selected);
                    this.bonjourListBox1.AddItem(bli);
                }
            }
            BonjourListItem manual = new BonjourListItem("[Click here to manually add a computer\nthat is not in this list]", ForwardComputerPlatformType.Other.Icon);
            manual.Selected += new BonjourListItemSelectedEventHandler(manual_Selected);
            this.bonjourListBox1.AddItem(manual);

            if (this.isSubscription)
            {
                this.Text = "Subscribe to notifications";
                manual_Selected(null);
                labelFormat.Visible = false;
                comboBoxFormat.Visible = false;
            }
        }

        void manual_Selected(DetectedService ds)
        {
            // set text box values
            this.isBonjour = false;
            this.textBoxDescription.Text = String.Empty;
            this.textBoxDescription.Enabled = true;
            this.textBoxAddress.Text = String.Empty;
            this.textBoxAddress.Enabled = true;
            this.textBoxPort.Text = Growl.Connector.GrowlConnector.TCP_PORT.ToString();
            this.textBoxPort.Enabled = true;
            this.comboBoxFormat.SelectedIndex = 0;
            this.comboBoxFormat.Enabled = true;
            this.buttonSave.Enabled = false;
            this.buttonSave.Visible = true;
            this.doValidation = true;
            ValidateInputs();

            this.panelBonjour.Visible = false;
            this.panelDetails.Visible = true;
        }

        void bli_Selected(DetectedService ds)
        {
            // set text box values
            this.isBonjour = true;
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
            this.buttonSave.Enabled = true;
            this.buttonSave.Visible = true;
            this.doValidation = true;
            ValidateInputs();

            this.panelBonjour.Visible = false;
            this.panelDetails.Visible = true;
        }

        internal void SetController(Controller controller)
        {
            this.controller = controller;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.isSubscription)
            {
                Subscription subscription = new Subscription(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text);
                this.controller.AddSubscription(subscription);
            }
            else
            {
                ForwardComputer fc = null;
                if (this.isBonjour)
                {
                    BonjourForwardComputer bfc = new BonjourForwardComputer(textBoxDescription.Text, true, textBoxPassword.Text);
                    bfc.Update(selectedService.Service, new GrowlBonjourEventArgs(selectedService.Platform));
                    fc = bfc;
                }
                else
                {
                    bool useUDP = (comboBoxFormat.SelectedItem.ToString() == "UDP" ? true : false);
                    fc = new ForwardComputer(textBoxDescription.Text, true, textBoxAddress.Text, Convert.ToInt32(textBoxPort.Text), textBoxPassword.Text, useUDP);
                }
                this.controller.AddForwardComputer(fc);
            }
            this.Close();
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

        private void ValidateInputs()
        {
            if (this.doValidation)
            {
                bool valid = true;

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

                this.buttonSave.Enabled = valid;
            }
        }
    }
}