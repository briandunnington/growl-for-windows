using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Framework;

namespace Growl.GrowlProtocolHandler
{
    public partial class MainForm : Form
    {
        private const string PROTOCOL_PREFIX = "growl://";
        private object locker = new object();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        internal void HandleMessage(ReadOnlyCollection<string> message)
        {
            // FORMAT: type:json
            //   where type is a single number equal to a PacketType
            //   and json is the json-serialized data required by the packet type

            if (message != null && message.Count == 1)
            {
                // determine which type of message we are dealing with
                string url = message[0];
                string data = url.Substring(PROTOCOL_PREFIX.Length);
                PacketType type = (PacketType) Convert.ToInt32(data.Substring(0, 1));
                string json = data.Substring(2);

                if (type == PacketType.Registration)
                {
                    // parse remaining data
                    NotificationType[] notificationTypes = JsonConverter.ToNotificationTypeArray(json);
                    lock (locker)
                    {
                        //NetGrowl growl = new NetGrowl(System.Net.IPAddress.Loopback.ToString(), NetGrowl.DEFAULT_PORT, appName, password);
                        //growl.Register(ref notificationTypes);
                        //growl = null;
                    }
                }
                else if (type == PacketType.Notification)
                {
                    // parse remaining data
                    NotificationType notificationType = null;
                    string title = null;
                    string description = null;
                    Priority priority = Priority.Normal;
                    bool sticky = false;

                    lock (locker)
                    {
                        //NetGrowl growl = new NetGrowl(System.Net.IPAddress.Loopback.ToString(), NetGrowl.DEFAULT_PORT, appName, password);
                        //growl.Notify(notificationType, title, description, priority, sticky);
                        //growl = null;
                    }
                }
            }
        }
    }
}