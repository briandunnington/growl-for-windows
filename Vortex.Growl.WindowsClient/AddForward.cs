using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Vortex.Growl.AppBridge;

namespace Vortex.Growl.WindowsClient
{
    public partial class AddForward : Form
    {
        public AddForward()
        {
            InitializeComponent();
            this.PortTextBox.Text = Vortex.Growl.Framework.NetGrowl.DEFAULT_PORT.ToString();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormAcceptButton_Click(object sender, EventArgs e)
        {
            // TODO: validation
            string description = this.DescriptionTextBox.Text;
            string ipAddress = this.IPAddressTextBox.Text;
            int port = Convert.ToInt32(this.PortTextBox.Text);
            string password = this.PasswordTextBox.Text;

            ForwardComputer fc = new ForwardComputer(description, true, ipAddress, port, password);
            ((MainForm) this.Owner).AddForwardComputer(fc);
            this.Close();
        }
    }
}