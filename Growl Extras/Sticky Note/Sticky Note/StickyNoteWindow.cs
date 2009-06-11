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

namespace StickyNote
{
    public partial class StickyNoteWindow : NotificationWindow
    {
        Timer fadeOutTimer;
        int opacity = 255;
        Bitmap bitmap;
        StickyNoteDisplay.Location location = StickyNoteDisplay.Location.TopRight;
        private int leftXLocation = 0;
        private int rightXLocation = 0;
        private int topYLocation = 0;
        private int bottomYLocation = 0;

        public StickyNoteWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(StickyNoteWindow_Load);
            this.AfterLoad += new EventHandler(StickyNoteWindow_AfterLoad);
            this.AutoClosing += new FormClosingEventHandler(StickyNoteWindow_AutoClosing);

            HookUpClickEvents(this);

            AutoClose(4000);
        }

        void StickyNoteWindow_Load(object sender, EventArgs e)
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);
        }

        void StickyNoteWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        public StickyNoteDisplay.Location DisplayLocation
        {
            get
            {
                return this.location;
            }
        }

        public void SetDisplayLocation(StickyNoteDisplay.Location location)
        {
            this.location = location;
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.AutoClose(n.Duration * 1000);

            this.pictureBox1.Image = n.Image;

            this.titleLabel.Text = n.Title;
            this.textLabel.Text = n.Description;
            this.Sticky = n.Sticky;

            Bitmap template = global::StickyNote.Properties.Resources.sticky;
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
                case StickyNoteDisplay.Location.TopLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.topYLocation);
                    break;
                case StickyNoteDisplay.Location.BottomLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.bottomYLocation - this.Height);
                    break;
                case StickyNoteDisplay.Location.BottomRight:
                    this.DesktopLocation = new Point(this.rightXLocation, this.bottomYLocation - this.Height);
                    break;
                default: // TopRight
                    this.DesktopLocation = new Point(this.rightXLocation, this.topYLocation);
                    break;
            }
        }

        private System.Drawing.Bitmap SizeImage(System.Drawing.Bitmap originalImage, int newWidth, int newHeight)
        {
            int minHeight = 117;
            int topHeight = 65;
            int bottomHeight = 51;

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

        void StickyNoteWindow_AutoClosing(object sender, FormClosingEventArgs e)
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
    }
}