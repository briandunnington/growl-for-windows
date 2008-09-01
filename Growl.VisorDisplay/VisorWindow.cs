using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.VisorDisplay
{
    public partial class VisorWindow : Form
    {
        protected Timer displayTimer;
        protected Timer slideInTimer;
        protected Timer slideOutTimer;
        private bool sticky = false;

        public VisorWindow()
        {
            InitializeComponent();

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.slideInTimer = new Timer();
            this.slideInTimer.Tick += new EventHandler(slideInTimer_Tick);
            this.slideOutTimer = new Timer();
            this.slideOutTimer.Tick += new EventHandler(slideOutTimer_Tick);
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

        private void Reset()
        {
            // dont show in taskbar
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // set size
            Screen screen = Screen.FromControl(this);
            int screenX = screen.WorkingArea.Width;
            int screenY = screen.WorkingArea.Height;
            int w = screenX;
            int h = (int)Math.Round(0.1 * screenY);
            this.Width = w;
            this.Height = h;

            // set initial location
            this.Location = new Point(0, -h + 10);  // the extra 10 is so that at least part of the form is on the screen initially, otherwise the opacity goes all screwy

            // set initial opacity
            this.Opacity = 0.7;

            // setup child controls
            //this.BackColor = //get from settings;
            Color textColor = GetTextColor(this.BackColor);
            this.titleLabel.ForeColor = textColor;
            this.descriptionLabel.ForeColor = textColor;
            this.applicationNameLabel.ForeColor = textColor;
            this.applicationNameLabel.Width = this.Width;
        }

        void displayTimer_Tick(object sender, EventArgs e)
        {
            SlideOut();
        }

        void slideInTimer_Tick(object sender, EventArgs e)
        {
            bool result = MoveWindow(10, 0, true);
            if (result)
            {
                this.slideInTimer.Stop();
                // set the window to hide in a few seconds unless we are sticky
                if (!this.sticky)
                {
                    this.displayTimer.Interval = 2500;
                    this.displayTimer.Start();
                }
            }
        }

        void slideOutTimer_Tick(object sender, EventArgs e)
        {
            int y = -this.Height;
            bool result = MoveWindow(-10, y, false);
            if (result)
            {
                this.slideOutTimer.Stop();
                this.Close();
            }
        }

        public new void Show()
        {
            Reset();

            User32DLL.ShowWindow(this.Handle, User32DLL.SW_SHOWNOACTIVATE);
            User32DLL.SetWindowPos(this.Handle, User32DLL.HWND_TOPMOST, this.Location.X, this.Location.Y, this.Width, this.Height, User32DLL.SWP_NOACTIVATE);
            OnShown(EventArgs.Empty);
        }

        public void SetNotification(Notification n)
        {
            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description;
            this.sticky = n.Sticky;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SlideIn();
        }

        private void SlideIn()
        {
            // start sliding the window in
            this.slideInTimer.Interval = 100;
            this.slideInTimer.Start();
        }

        private void SlideOut()
        {
            // start sliding the window out
            this.slideInTimer.Stop();
            this.displayTimer.Stop();
            this.slideOutTimer.Interval = 100;
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
    }
}