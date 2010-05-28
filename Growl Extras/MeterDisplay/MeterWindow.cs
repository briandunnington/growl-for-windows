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

namespace Meter
{
    public partial class MeterWindow : NotificationWindow
    {
        Timer fadeOutTimer;
        int opacity = 216;
        int duration = 4000;
        bool replaced = false;
        Bitmap bitmap;
        MeterDisplay.Location location = MeterDisplay.Location.TopRight;
        private int leftXLocation = 0;
        private int rightXLocation = 0;
        private int topYLocation = 0;
        private int bottomYLocation = 0;

        public MeterWindow()
        {
            InitializeComponent();

            this.AfterLoad += new EventHandler(MeterWindow_AfterLoad);
            this.AutoClosing += new FormClosingEventHandler(MeterWindow_AutoClosing);

            HookUpClickEvents(this);

            SetAutoCloseInterval(duration);
            PauseWhenMouseOver = true;
        }

        void MeterWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        public MeterDisplay.Location DisplayLocation
        {
            get
            {
                return this.location;
            }
        }

        public void SetDisplayLocation(MeterDisplay.Location location)
        {
            this.location = location;
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.duration = n.Duration * 1000;
            this.SetAutoCloseInterval(duration);

            this.pictureBox1.Image = n.Image;

            bool ok = false;
            double val = 0;
            // check the special X-header first to see if it exists.
            // if not, try using the Description header
            if (n.CustomTextAttributes != null && n.CustomTextAttributes.ContainsKey("Progress-Value"))
                ok = double.TryParse(n.CustomTextAttributes["Progress-Value"], out val);
            if (!ok)
                ok = double.TryParse(n.Description, out val);

            if (ok)
            {
                val = Math.Min(100, val);
                val = Math.Max(0, val);
                double b2 = ((val - 0) / (100 - 0)) * (13 - 0) + 0;
                int b = Convert.ToInt32(b2);

                switch (b)
                {
                    case 0:
                        this.pictureBoxBars.Image = Properties.Resources._0;
                        break;
                    case 1:
                        this.pictureBoxBars.Image = Properties.Resources._1;
                        break;
                    case 2:
                        this.pictureBoxBars.Image = Properties.Resources._2;
                        break;
                    case 3:
                        this.pictureBoxBars.Image = Properties.Resources._3;
                        break;
                    case 4:
                        this.pictureBoxBars.Image = Properties.Resources._4;
                        break;
                    case 5:
                        this.pictureBoxBars.Image = Properties.Resources._5;
                        break;
                    case 6:
                        this.pictureBoxBars.Image = Properties.Resources._6;
                        break;
                    case 7:
                        this.pictureBoxBars.Image = Properties.Resources._7;
                        break;
                    case 8:
                        this.pictureBoxBars.Image = Properties.Resources._8;
                        break;
                    case 9:
                        this.pictureBoxBars.Image = Properties.Resources._9;
                        break;
                    case 10:
                        this.pictureBoxBars.Image = Properties.Resources._10;
                        break;
                    case 11:
                        this.pictureBoxBars.Image = Properties.Resources._11;
                        break;
                    case 12:
                        this.pictureBoxBars.Image = Properties.Resources._12;
                        break;
                    case 13:
                        this.pictureBoxBars.Image = Properties.Resources._13;
                        break;
                    default:
                        this.pictureBoxBars.Image = Properties.Resources._0;
                        break;
                }
                this.pictureBoxBars.Visible = true;
                this.labelAltText.Visible = false;
                this.labelAltText.Text = "";
            }
            else
            {
                this.pictureBoxBars.Visible = false;
                this.labelAltText.Visible = true;
                this.labelAltText.Text = n.Description;
            }

            this.bitmap = SizeImage((Bitmap)this.BackgroundImage, this.Width, this.Height);
        }

        public void Replace(Notification n)
        {
            this.replaced = true;
            SetNotification(n);
            Show();
            OnShown(EventArgs.Empty);   // call this manually since it might not fire if the form is already visible
        }

        private void DoBeforeShow()
        {
            // multiple monitor support
            MultiMonitorVisualDisplay d = (MultiMonitorVisualDisplay)this.Tag;
            Screen screen = d.GetPreferredDisplay();

            // set initial location
            this.leftXLocation = screen.WorkingArea.Left;
            this.rightXLocation = screen.WorkingArea.Right - this.Width;
            this.topYLocation = screen.WorkingArea.Top;
            this.bottomYLocation = screen.WorkingArea.Bottom - this.Height;

            switch (location)
            {
                case MeterDisplay.Location.TopLeft:
                    this.Location = new Point(this.leftXLocation, this.topYLocation);
                    break;
                case MeterDisplay.Location.BottomLeft:
                    this.Location = new Point(this.leftXLocation, this.bottomYLocation);
                    break;
                case MeterDisplay.Location.BottomRight:
                    this.Location = new Point(this.rightXLocation, this.bottomYLocation);
                    break;
                default: // TopRight
                    this.Location = new Point(this.rightXLocation, this.topYLocation);
                    break;
            }
        }

        private System.Drawing.Bitmap SizeImage(System.Drawing.Bitmap originalImage, int newWidth, int newHeight)
        {
            int minHeight = 90;
            int topHeight = 8;
            int bottomHeight = 8;

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

        void MeterWindow_AutoClosing(object sender, FormClosingEventArgs e)
        {
            this.replaced = false;
            this.fadeOutTimer = new Timer();
            this.fadeOutTimer.Interval = 10;
            this.fadeOutTimer.Tick += new EventHandler(fadeOutTimer_Tick);
            this.fadeOutTimer.Start();
            e.Cancel = true;    // IMPORTANT!
        }

        void fadeOutTimer_Tick(object sender, EventArgs e)
        {
            if (this.replaced)
                this.fadeOutTimer.Stop();
            else
            {
                this.opacity -= 10;
                if (this.opacity <= 0)
                {
                    this.fadeOutTimer.Stop();
                    this.Close();
                }
                else if (this.Visible)
                {
                    Utility.UpdateLayeredWindow(this.bitmap, this, this.Left, this.Top, (byte)opacity);
                }
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
    }
}