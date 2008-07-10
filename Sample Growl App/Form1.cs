using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Vortex.Growl.Framework;

namespace Sample_Growl_App
{
    public partial class Form1 : Form
    {
        NotificationType serverCrashed = new NotificationType("Server Crashed", true);
        NotificationType siteRestarted = new NotificationType("Site Restarted", true);

        public Form1()
        {
            InitializeComponent();
        }

        private void sendNotificationButton_Click(object sender, EventArgs e)
        {
            Growl growl = new Growl("Test Application");
            growl.Notify(serverCrashed, "Your server has crashed", this.notificationTextBox.Text, Priority.Moderate, false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NotificationType[] notificationTypes = new NotificationType[] { serverCrashed, siteRestarted };

            Growl growl = new Growl("Test Application");
            growl.Register(ref notificationTypes);
        }
    }
}