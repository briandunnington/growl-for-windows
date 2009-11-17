using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace MiniVisor
{
    public partial class MiniVisorWindow : NotificationWindow
    {
        private Timer displayTimer;
        private Timer slideInTimer;
        private Timer slideOutTimer;
        private int startY = 0;
        private int endY = 0;
        private int duration = 2500;
        Bitmap bitmap;
        int x = 0;
        int y = 0;
        int moveBy = 1;
        bool closed = true;
        int originalHeight;

        public MiniVisorWindow()
        {
            InitializeComponent();

            this.originalHeight = this.Height;

            this.AfterLoad += new EventHandler(MiniVisorWindow_AfterLoad);
            this.Click +=new EventHandler(MiniVisorWindow_Click);
            this.FormClosed += new FormClosedEventHandler(MiniVisorWindow_FormClosed);

            HookUpClickEvents(this, true, true);

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.slideInTimer = new Timer();
            this.slideInTimer.Tick += new EventHandler(slideInTimer_Tick);
            this.slideOutTimer = new Timer();
            this.slideOutTimer.Tick += new EventHandler(slideOutTimer_Tick);
        }

        void MiniVisorWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.closed = true;
            this.displayTimer.Stop();
            this.slideInTimer.Stop();
            this.slideOutTimer.Stop();
        }

        void displayTimer_Tick(object sender, EventArgs e)
        {
            SlideOut();
        }

        void slideInTimer_Tick(object sender, EventArgs e)
        {
            if (!this.closed)
            {
                bool result = MoveWindow(this.moveBy, this.endY, true);
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
        }

        void slideOutTimer_Tick(object sender, EventArgs e)
        {
            if (!this.closed)
            {
                bool result = MoveWindow(-this.moveBy * 2, this.startY, false);
                if (result)
                {
                    this.slideOutTimer.Stop();
                    this.Close();
                }
            }
        }

        void MiniVisorWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.duration = n.Duration * 1000;

            this.pictureBox1.Image = n.Image;

            this.textLabel.Text = String.Format("{0}: {1} - {2}", n.ApplicationName, n.Title, n.Description);
            this.Sticky = n.Sticky;

            this.bitmap = global::MiniVisor.Properties.Resources.background;
        }

        private void DoBeforeShow()
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            this.x = (screen.WorkingArea.Width - this.bitmap.Width)/2;
            this.y = -(this.bitmap.Height);
            this.DesktopLocation = new Point(this.x, this.y);
        }

        protected override void OnShown(EventArgs e)
        {
            Utility.UpdateLayeredWindow(this.bitmap, this, this.x, this.y);
            base.OnShown(e);
            this.closed = false;
            SlideIn();
        }

        private void SlideIn()
        {
            this.startY = this.y;
            this.endY = 0;

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

        private void MiniVisorWindow_Click(object sender, EventArgs e)
        {
            SlideOut();
        }

        private bool MoveWindow(int moveY, int destY, bool isGoingDown)
        {
            int currentY = this.y;
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

            this.y = newY;
            Utility.UpdateLayeredWindow(this.bitmap, this, this.x, this.y);

            return result;
        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                return cp;
            }
        }

        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.textLabel.Top += args.HeightChange;
                textLabel_LabelHeightChanged(args);
            }
        }

        private void textLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Height += args.HeightChange;
            }
        }

        private void textLabel_LabelHeightChanged_1(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.Height = this.originalHeight;
        }
    }
}