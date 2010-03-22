using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.AutoUpdate
{
    public partial class UpdateForm : Form
    {
        private Updater updater;

        public UpdateForm()
        {
            InitializeComponent();

            // localize text
            this.Text = Properties.Resources.Updater_FormTitle;
            this.InfoLabel.Text = Properties.Resources.Updater_GrowlIsUpToDate;
            this.YesButton.Text = Properties.Resources.Button_Yes;
            this.NoButton.Text = Properties.Resources.Button_Later;
            this.OKButton.Text = Properties.Resources.Button_OK;

            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        public UpdateForm(Updater updater)
            : this()
        {
            this.updater = updater;
            this.updater.DownloadProgressChanged += new ProgressChangedEventHandler(updater_DownloadProgressChanged);
            this.updater.DownloadComplete += new EventHandler(updater_DownloadComplete);
            this.updater.UpdateError += new UpdateErrorEventHandler(updater_UpdateError);
        }

        public void LaunchUpdater(Manifest manifest, bool updateAvailable, UpdateErrorEventArgs args)
        {
            if (this.updater != null)
            {
                if (args != null)
                {
                    this.updater_UpdateError(this.updater, args);
                }
                else if (updateAvailable)
                {
                    this.NoButton.Visible = true;
                    this.YesButton.Visible = true;
                    this.progressBar1.Visible = false;
                    this.OKButton.Visible = false;
                    this.InfoLabel.Text = String.Format(Properties.Resources.Updater_UpdateAvailable, manifest.Version, this.updater.CurrentVersion);
                }
                else
                {
                    this.NoButton.Visible = false;
                    this.YesButton.Visible = false;
                    this.progressBar1.Visible = false;
                    this.OKButton.Visible = true;
                    this.InfoLabel.Text = String.Format(Properties.Resources.Updater_GrowlIsUpToDate, this.updater.CurrentVersion, this.updater.CurrentVersion);
                }
                this.Show();
                this.Activate();
            }
        }

        void updater_UpdateError(Updater sender, UpdateErrorEventArgs args)
        {
            this.NoButton.Visible = false;
            this.YesButton.Visible = false;
            this.progressBar1.Visible = false;
            this.OKButton.Visible = true;

            this.InfoLabel.Text = args.UserMessage;
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            this.InfoLabel.Text = Properties.Resources.Updater_DownloadingUpdate;
            this.progressBar1.Value = 0;
            this.progressBar1.Visible = true;
            this.NoButton.Enabled = false;
            this.YesButton.Enabled = false;
            this.updater.Update();
        }

        void updater_DownloadComplete(object sender, EventArgs e)
        {
            this.InfoLabel.Text = Properties.Resources.Updater_DownloadComplete;
        }

        void updater_DownloadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.updater != null)
                {
                    this.updater.DownloadProgressChanged -= new ProgressChangedEventHandler(updater_DownloadProgressChanged);
                    this.updater.DownloadComplete -= new EventHandler(updater_DownloadComplete);
                    this.updater.UpdateError -= new UpdateErrorEventHandler(updater_UpdateError);
                }
            }
            base.Dispose(disposing);
        }
    }
}