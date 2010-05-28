using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Visor
{
    public partial class VisorWindow : NotificationWindow
    {
        private Timer displayTimer;
        private Timer slideInTimer;
        private Timer slideOutTimer;
        private int startY = 0;
        private int endY = 0;
        private int duration = 2500;

        Screen screen;

        public VisorWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(VisorWindow_Load);

            this.PauseWhenMouseOver = true;

            HookUpClickEvents(this, true, true);

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.slideInTimer = new Timer();
            this.slideInTimer.Tick += new EventHandler(slideInTimer_Tick);
            this.slideOutTimer = new Timer();
            this.slideOutTimer.Tick += new EventHandler(slideOutTimer_Tick);
        }

        void VisorWindow_Load(object sender, EventArgs e)
        {
            // multiple monitor support
            MultiMonitorVisualDisplay d = (MultiMonitorVisualDisplay)this.Tag;
            this.screen = d.GetPreferredDisplay();

            // set size & location
            this.Width = screen.Bounds.Width;
            this.Location = screen.Bounds.Location;

            // set initial opacity
            this.Opacity = 0.90;
        }

        void displayTimer_Tick(object sender, EventArgs e)
        {
            SlideOut();
        }

        void slideInTimer_Tick(object sender, EventArgs e)
        {
            bool result = MoveWindow(2, this.endY, true);
            if (result)
            {
                this.slideInTimer.Stop();
                // set the window to hide in a few seconds unless we are sticky
                if (!this.Sticky)
                {
                    this.displayTimer.Interval = this.duration;
                    this.displayTimer.Start();
                }
            }
        }

        void slideOutTimer_Tick(object sender, EventArgs e)
        {
            bool result = MoveWindow(-3, this.startY, false);
            if (result)
            {
                this.slideOutTimer.Stop();
                this.Close();
            }
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.duration = n.Duration * 1000;

            //Image image = n.GetImage();
            Image image = n.Image;
            if (image != null)
            {
                this.pictureBox1.Image = image;
                this.pictureBox1.Visible = true;
            }

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description;
            this.Sticky = n.Sticky;

            // setup child controls
            Color textColor = GetTextColor(this.BackColor);
            this.titleLabel.ForeColor = textColor;
            this.descriptionLabel.ForeColor = textColor;
            this.applicationNameLabel.ForeColor = textColor;
            this.applicationNameLabel.Width = this.Width;
        }

        protected override void OnShown(EventArgs e)
        {
            // set initial location
            this.Location = new Point(this.Location.X, this.Location.Y - this.Height + 10);    // the extra 10 is so that at least part of the form is on the screen initially, otherwise the opacity goes all screwy

            Win32.SetWindowPos(this.Handle, Win32.HWND_TOPMOST, this.Location.X, this.Location.Y, this.Width, this.Height, Win32.SWP_NOACTIVATE);
            base.OnShown(e);
            SlideIn();
        }

        private void SlideIn()
        {
            this.startY = this.Location.Y;
            this.endY = screen.Bounds.Top;

            // start sliding the window in
            this.slideInTimer.Interval = 10;
            this.slideInTimer.Start();
        }

        private void SlideOut()
        {
            // start sliding the window out
            this.slideInTimer.Stop();
            this.displayTimer.Stop();
            this.slideOutTimer.Interval = 10;
            this.slideOutTimer.Start();
        }

        private void VisorWindow_Click(object sender, EventArgs e)
        {
            SlideOut();
        }

        private bool MoveWindow(int moveY, int destY, bool isGoingDown)
        {
            int currentY = this.Location.Y;
            int newY = currentY += moveY;

            bool result = false;
            if (isGoingDown && newY >= destY)
            {
                result = true;
                newY = destY;
            }
            else if (!isGoingDown && newY <= destY)
            {
                result = true;
                newY = destY;
            }

            this.Location = new Point(this.Location.X, newY);

            return result;
        }

        public Color GetTextColor(Color bg)
        {
            int nThreshold = 105;
            int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + (bg.B * 0.114));
            Color foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
            return foreColor;
        }

        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Height = this.Height + args.HeightChange;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.displayTimer != null) this.displayTimer.Dispose();
                if (this.slideInTimer != null) this.slideInTimer.Dispose();
                if (this.slideOutTimer != null) this.slideOutTimer.Dispose();
                if (this.pictureBox1 != null) this.pictureBox1.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}