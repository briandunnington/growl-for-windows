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
        private bool isSubscription;
        private IForwardInputs inputs;

        public AddComputer()
        {
            InitializeComponent();

            // localize text
            this.buttonSave.Text = Properties.Resources.Button_Save;
            this.buttonClose.Text = Properties.Resources.Button_Close;
            this.Text = Properties.Resources.AddComputer_FormTitle_Forward;
        }

        public AddComputer(bool isSubscription)
            : this()
        {
            this.isSubscription = isSubscription;
        }

        private void AddComputer_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(240, 240, 240);

            if (this.controller != null)
            {
                Dictionary<string, DetectedService> availableServices = controller.DetectedServices;
                foreach (DetectedService ds in availableServices.Values)
                {
                    BonjourListItem bli = new BonjourListItem(ds);
                    bli.Selected += new EventHandler(bli_Selected);
                    this.bonjourListBox1.AddItem(bli);
                }
            }

            // add manual option
            ForwardListItem manual = new ForwardListItem(Properties.Resources.AddComputer_ManualAdd, ForwardComputerPlatformType.Other.Icon);
            manual.Selected += new EventHandler(manual_Selected);
            this.bonjourListBox1.AddItem(manual);

            // add iphone/Prowl option
            ProwlListItem prowl = new ProwlListItem();
            prowl.Selected += new EventHandler(prowl_Selected);
            this.bonjourListBox1.AddItem(prowl);

            if (this.isSubscription)
            {
                this.Text = Properties.Resources.AddComputer_FormTitle_Subscriptions;
                ForwardListItem subscriptionInputs = new ForwardListItem(null, null);
                ShowInputs(subscriptionInputs);
            }
        }

        private void ShowInputs(ForwardListItem fli)
        {
            this.inputs = fli.Inputs;
            fli.Inputs.ValidChanged += new ValidChangedEventHandler(Inputs_ValidChanged);
            fli.Inputs.Initialize(this.isSubscription, fli);
            UserControl c = fli.Inputs.GetControl();
            c.Visible = true;
            this.panelDetails.Controls.Add(c);

            this.panelBonjour.Visible = false;
            this.panelDetails.Visible = true;
            this.buttonSave.Visible = true;
        }

        void Inputs_ValidChanged(bool isValid)
        {
            this.buttonSave.Enabled = isValid;
        }

        void manual_Selected(object sender, EventArgs args)
        {
            ForwardListItem fli = (ForwardListItem)sender;
            ShowInputs(fli);
        }

        void prowl_Selected(object sender, EventArgs args)
        {
            ProwlListItem pli = (ProwlListItem)sender;
            ShowInputs(pli);
        }

        void bli_Selected(object sender, EventArgs args)
        {
            BonjourListItem bli = (BonjourListItem)sender;
            this.isBonjour = true;
            this.selectedService = bli.DetectedService;
            ShowInputs(bli);
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
            ForwardComputer fc = this.inputs.Save();

            if (this.isSubscription)
            {
                Subscription subscription = (Subscription)fc;
                this.controller.AddSubscription(subscription);
            }
            else
            {
                this.controller.AddForwardComputer(fc);
            }
            this.Close();
        }
    }
}