using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.UI;
using Growl.Destinations;

namespace Growl
{
    public partial class AddComputer : Form
    {
        private Controller controller;
        private bool isSubscription;
        private DestinationSettingsPanel settingsPanel;
        private DestinationBase dbEdit;

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

        public AddComputer(DestinationBase db)
            : this()
        {
            this.dbEdit = db;
            if (db is Subscription) this.isSubscription = true;

            IDestinationHandler handler = null;
            if (isSubscription)
            {
                handler = SubscriptionManager.GetHandler(db);
            }
            else
            {
                handler = ForwardDestinationManager.GetHandler(db);
            }
            ShowInputs(null, handler);
        }

        private void AddComputer_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(240, 240, 240);

            List<DestinationListItem> list = null;
            if (this.isSubscription)
            {
                list = SubscriptionManager.GetListItems();
                
                this.Text = Properties.Resources.AddComputer_FormTitle_Subscriptions;
            }
            else
            {
                list = ForwardDestinationManager.GetListItems();
            }

            foreach (DestinationListItem dli in list)
            {
                dli.Selected += new EventHandler(dli_Selected);
                this.bonjourListBox1.AddItem(dli);
            }
        }

        void dli_Selected(object sender, EventArgs e)
        {
            DestinationListItem dli = (DestinationListItem)sender;
            ShowInputs(dli);
        }

        private void ShowInputs(DestinationListItem dli)
        {
            ShowInputs(dli, dli.Handler);
        }

        private void ShowInputs(DestinationListItem dli, IDestinationHandler handler)
        {
            this.panelBonjour.Visible = false;
            this.panelDetails.Visible = true;

            DestinationSettingsPanel panel = handler.GetSettingsPanel(this.dbEdit);
            this.settingsPanel = panel;
            panel.ValidChanged += new DestinationSettingsPanel.ValidChangedEventHandler(panel_ValidChanged);
            this.panelDetails.Controls.Add(panel);
            panel.Visible = true;
            panel.Initialize(this.isSubscription, dli, this.dbEdit);

            this.buttonSave.Visible = true;
        }

        void panel_ValidChanged(bool isValid)
        {
            this.buttonSave.Enabled = isValid;
        }

        internal void Initialize(Controller controller, DestinationBase dbEdit, bool isSubscription)
        {
            this.controller = controller;
            this.dbEdit = dbEdit;
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
            if (this.dbEdit != null)
            {
                try
                {
                    this.settingsPanel.Update(this.dbEdit);
                }
                catch
                {
                    Utility.WriteDebugInfo(String.Format("EXCEPTION: '{0}' Update() failed", this.dbEdit.Description));
                }

                if (this.isSubscription)
                    this.controller.EditSubscription((Subscription)this.dbEdit);
                else
                    this.controller.EditForwardDestination((ForwardDestination)this.dbEdit);
            }
            else
            {
                DestinationBase db = null;
                try
                {
                    db = this.settingsPanel.Create();
                }
                catch
                {
                    Utility.WriteDebugInfo("EXCEPTION: Create() forward destination or subscription failed");
                }

                if (this.isSubscription)
                    this.controller.AddSubscription((Subscription)db);
                else
                    this.controller.AddForwardDestination((ForwardDestination)db);
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
                    this.settingsPanel.ValidChanged -= new DestinationSettingsPanel.ValidChangedEventHandler(panel_ValidChanged);
                }

                if (this.bonjourListBox1.Items != null)
                {
                    foreach (DestinationListItem dli in this.bonjourListBox1.Items)
                    {
                        dli.Selected -= new EventHandler(dli_Selected);
                    }
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