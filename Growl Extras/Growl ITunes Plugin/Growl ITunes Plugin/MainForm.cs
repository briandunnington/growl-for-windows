using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.ITunesPlugin
{
    public partial class MainForm : ITunesPluginApp.ConfigurationForm
    {
        GrowlPlugin gp = null;
        Timer timer;

        internal MainForm(GrowlPlugin gp)
        {
            InitializeComponent();

            this.gp = gp;
            this.timer = new Timer();
            this.timer.Interval = 1 * 750;
            this.timer.Tick += new EventHandler(timer_Tick);
            this.Load += new EventHandler(MainForm_Load);

            InitializeForm();
        }

         private void InitializeForm()
        {
            this.pictureBox1.Image = Bitmap.FromFile(String.Format(@"{0}\icon.png", System.Windows.Forms.Application.StartupPath));
            this.textBoxPassword.Text = Properties.Settings.Default.GrowlPassword;

            this.radioButtonSendNotifications.Checked = Properties.Settings.Default.SendGNTPNotifications;
            this.radioButtonSendUDP.Checked = Properties.Settings.Default.SendUDPNotifications;
            this.radioButtonDontSend.Checked = Properties.Settings.Default.DisableNotifications;

            Register();
        }

        void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void radioButtonSendNotifications_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSettings(sender);
        }

        private void radioButtonSendUDP_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSettings(sender);
        }

        private void radioButtonDontSend_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSettings(sender);
        }

        private void UpdateSettings(object sender)
        {
            if (sender is RadioButton)
            {
                RadioButton rb = (RadioButton)sender;
                if (rb.Checked)
                {
                    Properties.Settings.Default.SendGNTPNotifications = this.radioButtonSendNotifications.Checked;
                    Properties.Settings.Default.SendUDPNotifications = this.radioButtonSendUDP.Checked;
                    Properties.Settings.Default.DisableNotifications = this.radioButtonDontSend.Checked;

                    Register();

                    Properties.Settings.Default.Save();
                }
            }
        }

        private void Register()
        {
            if (this.radioButtonSendNotifications.Checked) gp.RegisterGNTP();
            if (this.radioButtonSendUDP.Checked) gp.RegisterUDP();
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
            gp.SetPassword(password);

            Properties.Settings.Default.GrowlPassword = password;
            Properties.Settings.Default.Save();
        }
    }
}
