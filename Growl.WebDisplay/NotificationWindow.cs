using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.WebDisplay
{
    public partial class NotificationWindow : Form
    {
        protected Timer displayTimer;
        protected Timer fadeTimer;
        private bool fading = false;
        private bool sticky = false;

        public NotificationWindow()
        {
            InitializeComponent();

            // dont show in taskbar
            this.ShowInTaskbar = false;

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.fadeTimer = new Timer();
            this.fadeTimer.Tick += new EventHandler(fadeTimer_Tick);
        }

        public void SetUrl(string url)
        {
            this.webKitBrowser.Navigate(url);
        }

        public void SetHtml(string html, string baseUrl)
        {
            this.webKitBrowser.SetHtml(html, baseUrl);
        }

        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
            set
            {
                this.sticky = value;
            }
        }

        void displayTimer_Tick(object sender, EventArgs e)
        {
            FadeOut();
        }

        void fadeTimer_Tick(object sender, EventArgs e)
        {
            double opacity = this.Opacity;
            if (opacity > 0) opacity -= .05;

            if (opacity > 0)
            {
                this.Opacity = opacity;
            }
            else
            {
                this.Close();
                this.fadeTimer.Stop();
            }
        }

        public new void Show()
        {
            User32DLL.ShowWindow(this.Handle, User32DLL.SW_SHOWNOACTIVATE);
            //User32DLL.SetWindowPos(this.Handle, User32DLL.HWND_TOPMOST, this.Location.X, this.Location.Y, this.Width, this.Height, User32DLL.SWP_NOACTIVATE);
            OnShown(EventArgs.Empty);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // set the window to hide in 5 seconds unless we are sticky
            if (!this.sticky)
            {
                this.displayTimer.Interval = 5000;
                this.displayTimer.Start();
            }
        }

        private void FadeOut()
        {
            this.displayTimer.Stop();
            if (!this.fadeTimer.Enabled && !this.fading)
            {
                this.fading = true;
                this.fadeTimer.Interval = 100;
                this.fadeTimer.Start();
            }
        }

        private void transparentPanel1_Click(object sender, EventArgs e)
        {
            FadeOut();
        }
    }
}