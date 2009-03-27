using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITunesPluginApp
{
    public partial class ConfigurationForm : Form
    {
        private bool shouldShow = false;

        public ConfigurationForm()
        {
            InitializeComponent();
        }

        private void ConfigurationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Activate();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (this.shouldShow || this.DesignMode)
            {
                base.SetVisibleCore(value);
            }
            else
            {
                this.shouldShow = true;                
            }
        }
    }
}
