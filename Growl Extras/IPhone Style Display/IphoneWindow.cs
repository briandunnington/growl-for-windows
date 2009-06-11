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


namespace IphoneStyle
{
    public partial class IphoneWindow : NotificationWindow
    {
        Timer fadeOutTimer;
        int opacity = 216;
        Bitmap bitmap;
        IphoneDisplay.Location location = IphoneDisplay.Location.TopRight;
        private int leftXLocation = 0;
        private int rightXLocation = 0;
        private int topYLocation = 0;
        private int bottomYLocation = 0;

        public IphoneWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(IphoneWindow_Load);
            this.AfterLoad += new EventHandler(IphoneWindow_AfterLoad);
            this.AutoClosing += new FormClosingEventHandler(IphoneWindow_AutoClosing);

            HookUpClickEvents(this);

            AutoClose(4000);
        }

        void IphoneWindow_Load(object sender, EventArgs e)
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);
        }

        void IphoneWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        public IphoneDisplay.Location DisplayLocation
        {
            get
            {
                return this.location;
            }
        }

        public void SetDisplayLocation(IphoneDisplay.Location location)
        {
            this.location = location;
        }

        public void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.AutoClose(n.Duration * 1000);

            this.pictureBox1.Image = n.Image;

            this.titleLabel.Text = n.Title;
            this.textLabel.Text = n.Description;
            this.Sticky = n.Sticky;

            Bitmap template = global::IphoneStyle.Properties.Resources.normal;
            switch (n.Priority)
            {
                case 2:
                    template = global::IphoneStyle.Properties.Resources.emergency;
                    break;
                case 1:
                    template = global::IphoneStyle.Properties.Resources.high;
                    break;
                case -1:
                    template = global::IphoneStyle.Properties.Resources.moderate;
                    break;
                case -2:
                    template = global::IphoneStyle.Properties.Resources.verylow;
                    break;
            }
            this.bitmap = SizeImage(template, this.Width, this.Height);
        }

        private void DoBeforeShow()
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height;
            this.leftXLocation = 0;
            this.rightXLocation = x;
            this.topYLocation = 0;
            this.bottomYLocation = y;
            this.DesktopLocation = new Point(x, y);

            switch (location)
            {
                case IphoneDisplay.Location.TopLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.topYLocation);
                    break;
                case IphoneDisplay.Location.BottomLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.bottomYLocation - this.Height);
                    break;
                case IphoneDisplay.Location.BottomRight:
                    this.DesktopLocation = new Point(this.rightXLocation, this.bottomYLocation - this.Height);
                    break;
                default: // TopRight
                    this.DesktopLocation = new Point(this.rightXLocation, this.topYLocation);
                    break;
            }
        }

        private System.Drawing.Bitmap SizeImage(System.Drawing.Bitmap originalImage, int newWidth, int newHeight)
        {
            /*
            int minHeight = 91;
            int topHeight = 38;
            int bottomHeight = 27;
             * */

            int minHeight = 90;
            int topHeight = 40;
            int bottomHeight = 20;

            if (newHeight < minHeight) newHeight = minHeight;

            Size originalTopSize = new Size(originalImage.Width, topHeight);
            Size newTopSize = new Size(newWidth, topHeight);
            Size originalBottomSize = new Size(originalImage.Width, bottomHeight);
            Size newBottomSize = new Size(newWidth, bottomHeight);
            int originalBottomY = originalImage.Height - bottomHeight;
            int newBottomY = newHeight - bottomHeight;
            Point originalBottomLocation = new Point(0, originalBottomY);
            Point newBottomLocation = new Point(0, newBottomY);
            Point originalMiddleLocation = new Point(0, topHeight);
            Point newMiddleLocation = new Point(0, topHeight);
            Size originalMiddleSize = new Size(originalImage.Width, originalImage.Height - originalTopSize.Height - originalBottomSize.Height);
            Size newMiddleSize = new Size(newWidth, newHeight - newTopSize.Height - newBottomSize.Height);

            System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(newWidth, newHeight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);
            using (g)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // draw top
                g.DrawImage(
                    originalImage,
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, newTopSize),
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, originalTopSize),
                    System.Drawing.GraphicsUnit.Pixel);

                // draw middle
                g.DrawImage(
                    originalImage,
                    new System.Drawing.Rectangle(newMiddleLocation, newMiddleSize),
                    new System.Drawing.Rectangle(originalMiddleLocation, originalMiddleSize),
                    System.Drawing.GraphicsUnit.Pixel);

                // draw bottom
                g.DrawImage(
                    originalImage,
                    new System.Drawing.Rectangle(newBottomLocation, newBottomSize),
                    new System.Drawing.Rectangle(originalBottomLocation, originalBottomSize),
                    System.Drawing.GraphicsUnit.Pixel);
            }
            return bmpResized;
        }

        protected override void OnShown(EventArgs e)
        {
            Utility.UpdateLayeredWindow(this.bitmap, this, this.Left, this.Top, (byte)opacity);
            base.OnShown(e);
        }

        void IphoneWindow_AutoClosing(object sender, FormClosingEventArgs e)
        {
            this.fadeOutTimer = new Timer();
            this.fadeOutTimer.Interval = 50;
            this.fadeOutTimer.Tick += new EventHandler(fadeOutTimer_Tick);
            this.fadeOutTimer.Start();
            e.Cancel = true;    // IMPORTANT!
        }

        void fadeOutTimer_Tick(object sender, EventArgs e)
        {
            this.opacity -= 10;
            if (this.opacity <= 0)
            {
                this.fadeOutTimer.Stop();
                this.Close();
            }
            else if(this.Visible)
            {
                Utility.UpdateLayeredWindow(this.bitmap, this, this.Left, this.Top, (byte)opacity);
            }
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

        private void textLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.titleLabel.Top += args.HeightChange;
                titleLabel_LabelHeightChanged(args);
            }
        }

        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Height += args.HeightChange;
            }
        }
    }
}