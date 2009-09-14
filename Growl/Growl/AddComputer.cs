using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.UI;

namespace Growl
{
    public partial class AddComputer : Form
    {
        private Controller controller;
        private bool isSubscription;
        private ForwardDestinationSettingsPanel settingsPanel;
        private ForwardDestination fdEdit;

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
            if (this.isSubscription)
            {
                IForwardDestinationHandler handler = ForwardDestinationManager.GetHandler(typeof(Subscription));
                ShowInputs(null, handler);
            }
        }

        public AddComputer(ForwardDestination fd)
            : this()
        {
            this.fdEdit = fd;
            if (fd is Subscription) this.isSubscription = true;
            IForwardDestinationHandler handler = ForwardDestinationManager.GetHandler(fd);
            ShowInputs(null, handler);
        }

        private void AddComputer_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(240, 240, 240);

            List<ForwardDestinationListItem> list = ForwardDestinationManager.GetListItems();
            foreach (ForwardDestinationListItem fdli in list)
            {
                fdli.Selected += new EventHandler(fdli_Selected);
                this.bonjourListBox1.AddItem(fdli);
            }

            if (this.isSubscription) this.Text = Properties.Resources.AddComputer_FormTitle_Subscriptions;
        }

        void fdli_Selected(object sender, EventArgs e)
        {
            ForwardDestinationListItem fdli = (ForwardDestinationListItem)sender;
            ShowInputs(fdli);
        }

        private void ShowInputs(ForwardDestinationListItem fdli)
        {
            ShowInputs(fdli, fdli.Handler);
        }

        private void ShowInputs(ForwardDestinationListItem fdli, IForwardDestinationHandler handler)
        {
            this.panelBonjour.Visible = false;
            this.panelDetails.Visible = true;

            ForwardDestinationSettingsPanel panel = handler.GetSettingsPanel(this.fdEdit);
            this.settingsPanel = panel;
            panel.ValidChanged += new ForwardDestinationSettingsPanel.ValidChangedEventHandler(panel_ValidChanged);
            this.panelDetails.Controls.Add(panel);
            panel.Visible = true;
            panel.Initialize(this.isSubscription, fdli, this.fdEdit);

            this.buttonSave.Visible = true;
        }

        void panel_ValidChanged(bool isValid)
        {
            this.buttonSave.Enabled = isValid;
        }

        internal void Initialize(Controller controller, ForwardDestination fdEdit, bool isSubscription)
        {
            this.controller = controller;
            this.fdEdit = fdEdit;
            this.isSubscription = isSubscription;
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
            if (this.fdEdit != null)
            {
                this.settingsPanel.Update(this.fdEdit);
            }
            else
            {
                ForwardDestination fd = this.settingsPanel.Create();
                if (this.isSubscription)
                {
                    Subscription subscription = (Subscription)fd;
                    this.controller.AddSubscription(subscription);
                }
                else
                {
                    this.controller.AddForwardDestination(fd);
                }
            }
            this.Close();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.settingsPanel != null)
                {
                    this.settingsPanel.Dispose();
                    this.settingsPanel = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}