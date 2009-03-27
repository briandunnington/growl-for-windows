using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.VisualStudioAddIn
{
    public partial class ConfigForm : Form
    {
        public event EventHandler PasswordChanged;

        Timer timer;

        public ConfigForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.pictureBox1.Image = Properties.Resources.growl.ToBitmap();
            this.textBoxPassword.Text = Properties.Settings.Default.Password;
            this.checkBoxEnabled.Checked = Properties.Settings.Default.EnableNotifications;

            this.timer = new Timer();
            this.timer.Interval = 1 * 750;
            this.timer.Tick += new EventHandler(timer_Tick);

            // place these here (as opposed to in the .Designer) so that they dont fire when we initialize the form
            this.checkBoxEnabled.CheckedChanged += new System.EventHandler(this.checkBoxEnabled_CheckedChanged);
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
        }

        private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableNotifications = this.checkBoxEnabled.Checked;
            Properties.Settings.Default.Save();
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();

            string password = this.textBoxPassword.Text;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.Save();

            this.OnPasswordChanged(this, EventArgs.Empty);
        }

        protected void OnPasswordChanged(object sender, EventArgs args)
        {
            if(this.PasswordChanged != null)
            {
                this.PasswordChanged(sender, args);
            }
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}