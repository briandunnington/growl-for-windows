using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Framework;

namespace Sample_Net_Growl_App
{
    public partial class Form1 : Form
    {
        NotificationType serverCrashed = new NotificationType("Informational", false);
        NotificationType siteRestarted = new NotificationType("Warning", true);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NetGrowl growl = new NetGrowl("127.0.0.1", NetGrowl.DEFAULT_PORT, "PHP Notifier", this.passwordTextBox.Text);
            growl.Notify(siteRestarted, "Apache", "PHP Warning", Priority.VeryLow, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NotificationType[] notificationTypes = new NotificationType[] { serverCrashed, siteRestarted };

            NetGrowl growl = new NetGrowl("127.0.0.1", NetGrowl.DEFAULT_PORT, "PHP Notifier", this.passwordTextBox.Text);
            growl.Register(ref notificationTypes);
        }
    }
}