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

            if (this.controller != null && this.prefs != null)
            {
                if (this.controller.ForwardComputers != null && this.controller.ForwardComputers.Count > 0)
                {
                    Dictionary<string, ForwardComputer> computers = new Dictionary<string,ForwardComputer>();
                    foreach (ForwardComputer fc in this.controller.ForwardComputers.Values)
                    {
                        bool enabled = this.prefs.PrefForwardCustomList.Contains(fc.Description);
                        //ForwardComputer clone = new ForwardComputer(fc.Description, enabled, fc.IPAddress, fc.Port, fc.Password, fc.UseUDP);
                        ForwardComputer clone = fc.Clone();
                        fc.Enabled = enabled;
                        computers.Add(clone.Description, clone);
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
            foreach (ForwardComputer fc in this.forwardListView1.Computers.Values)
            {
                if (fc.Enabled) computers.Add(fc.Description);
            }
            this.prefs.PrefForwardCustomList = computers;

            this.Close();
        }
    }
}