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
    public partial class ChooseForwarding : Form
    {
        private Controller controller;
        private NotificationPreferences prefs;

        public ChooseForwarding()
        {
            InitializeComponent();

            // localize text
            this.buttonSave.Text = Properties.Resources.Button_Save;
            this.buttonClose.Text = Properties.Resources.Button_Cancel;
            this.Text = Properties.Resources.ChooseForwarding_FormTitle;
        }

        private void ChooseForwarding_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.BackColor = Color.FromArgb(240, 240, 240);

            this.forwardListView1.AllDisabled = false;
            if (this.controller != null && this.prefs != null)
            {
                if (this.controller.ForwardDestinations != null && this.controller.ForwardDestinations.Count > 0)
                {
                    /*
                    Dictionary<string, DestinationBase> computers = new Dictionary<string, DestinationBase>();
                    foreach (ForwardDestination fc in this.controller.ForwardDestinations.Values)
                    {
                        bool enabled = this.prefs.PrefForwardCustomList.Contains(fc.Key);
                        DestinationBase clone = fc.Clone();
                        clone.Key = fc.Key;
                        clone.Enabled = enabled;
                        computers.Add(clone.Description, clone);
                    }
                    this.forwardListView1.Computers = computers;
                    this.forwardListView1.Draw();
                     * */

                    DestinationBase[] computers = new DestinationBase[this.controller.ForwardDestinations.Count];
                    int i = 0;
                    foreach (ForwardDestination fc in this.controller.ForwardDestinations.Values)
                    {
                        bool enabled = this.prefs.PrefForwardCustomList.Contains(fc.Key);
                        DestinationBase clone = fc.Clone();
                        clone.Key = fc.Key;
                        clone.Enabled = enabled;
                        computers[i++] = clone;
                    }
                    this.forwardListView1.Computers = computers;
                    this.forwardListView1.Draw();
                }
                else
                {
                    // need to show some kind of 'no computers available message'
                }
            }
            else
            {
                this.Close();
            }

            this.ResumeLayout();
        }

        internal void SetController(Controller controller)
        {
            this.controller = controller;
        }

        internal void SetPrefs(NotificationPreferences prefs)
        {
            if (prefs.PrefForwardCustomList == null) prefs.PrefForwardCustomList = new List<string>();
            this.prefs = prefs;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            List<string> computers = new List<string>();
            foreach (ForwardDestination fc in this.forwardListView1.Computers)
            {
                if (fc.Enabled) computers.Add(fc.Key);
            }
            this.prefs.PrefForwardCustomList = computers;

            this.Close();
        }
    }
}