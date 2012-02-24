using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace Growl.Subscribers.FolderWatch
{
    public partial class FolderWatchSettings : DestinationSettingsPanel
    {
        public FolderWatchSettings()
        {
            InitializeComponent();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.textBoxPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        public override void Initialize(bool isSubscription, DestinationListItem fdli, DestinationBase db)
        {
            FolderWatchSubscription sub = db as FolderWatchSubscription;
            if (sub != null)
            {
                this.textBoxPath.Text = sub.Path;
                this.checkBoxSubdirectories.Checked = sub.IncludeSubfolders;
            }
        }

        public override DestinationBase Create()
        {
            FolderWatchSubscription sub = new FolderWatchSubscription(true);
            sub.Path = this.textBoxPath.Text;
            sub.IncludeSubfolders = this.checkBoxSubdirectories.Checked;
            return sub;
        }

        public override void Update(DestinationBase db)
        {
            FolderWatchSubscription sub = db as FolderWatchSubscription;
            if (sub != null)
            {
                sub.Path = this.textBoxPath.Text;
                sub.IncludeSubfolders = this.checkBoxSubdirectories.Checked;
                sub.Subscribe();
            }
        }

        private void textBoxPath_TextChanged(object sender, EventArgs e)
        {
            OnValidChanged(System.IO.Directory.Exists(this.textBoxPath.Text));
        }
    }
}
