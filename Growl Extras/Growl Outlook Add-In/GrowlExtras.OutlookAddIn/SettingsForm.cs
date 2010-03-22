using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.OutlookAddIn
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            this.checkBoxEnableNewMail.Checked = Properties.Settings.Default.EnableNewMailNotifications;
            this.checkBoxEnableReminders.Checked = Properties.Settings.Default.EnableReminderNotifications;
            this.textBoxPassword.Text = Properties.Settings.Default.Password;

            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.labelVersion.Text = "v" + fvi.FileVersion;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableNewMailNotifications = this.checkBoxEnableNewMail.Checked;
            Properties.Settings.Default.EnableReminderNotifications = this.checkBoxEnableReminders.Checked;
            Properties.Settings.Default.Password = this.textBoxPassword.Text;
            Properties.Settings.Default.Save();

            this.Close();
        }
    }
}