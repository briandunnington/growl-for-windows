using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Smokestack
{
    public partial class SmokestackWindow : NotificationWindow
    {
        Color color1 = Color.FromArgb(20, 20, 20);
        Color color2 = Color.Black;
        Color borderColor = Color.Gainsboro;

        private int leftXLocation = 0;
        private int rightXLocation = 0;
        private int topYLocation = 0;
        private int bottomYLocation = 0;
        private Image image;
        private SmokestackDisplay.Location location = SmokestackDisplay.Location.TopRight;

        Brush borderBrush;
        Brush arrowBrush;
        Point[] points;

        int width = 0;
        int height = 0;
        int borderTopOffset = 0;
        int arrowOffset = 24;
        int arrowSize = 14;
        int radius = 18;
        int borderWidth = 1;
        int arrowLeft = 0;
        int arrowTop = 0;
        int arrowXOffset = 0;
        int arrowYOffset = 0;
        int imageSize = 48;
        int imagePadding = 12;


        public SmokestackWindow()
        {
            InitializeComponent();

            this.Animator = new FadeAnimator(this, 250, 250, 1.0);

            HookUpClickEvents(this);

            this.AfterLoad += new EventHandler(SmokestackWindow_AfterLoad);

            // set size
            this.Width = 250;
            this.Height = this.Height + arrowSize;

            // set initial opacity
            //this.Opacity = 0.98;

            // border brush
            this.borderBrush = new SolidBrush(this.borderColor);

            int duration = 5000;
            string d = System.Configuration.ConfigurationManager.AppSettings["Duration"];
            if (!String.IsNullOrEmpty(d))
            {
                int.TryParse(d, out duration);
            }
            this.AutoClose(duration);

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        void SmokestackWindow_AfterLoad(object sender, EventArgs e)
        {
            DoBeforeShow();
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if(n.Duration > 0) this.AutoClose(n.Duration * 1000);

            //this.image = n.GetImage();
            this.image = n.Image;

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;
        }

        public SmokestackDisplay.Location DisplayLocation
        {
            get
            {
                return this.location;
            }
        }

        public void SetDisplayLocation(SmokestackDisplay.Location location)
        {
            this.location = location;
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

            this.width = this.Width;
            this.height = this.Height - arrowSize;

            switch (location)
            {
                case SmokestackDisplay.Location.TopLeft :
                    borderTopOffset = arrowSize;

                    this.DesktopLocation = new Point(this.leftXLocation, this.topYLocation);
                    arrowBrush = new SolidBrush(color1);
                    arrowLeft = arrowOffset;
                    arrowTop = 0;
                    arrowXOffset = 0;
                    arrowYOffset = 1;
                    Point tl1 = new Point(arrowLeft, arrowTop + arrowSize);
                    Point tl2 = new Point(arrowLeft + arrowSize, arrowTop);
                    Point tl3 = new Point(arrowLeft + arrowSize + arrowSize, arrowTop + arrowSize);
                    points = new Point[] { tl1, tl2, tl3, tl1 };
                    break;
                case SmokestackDisplay.Location.BottomLeft:
                    this.DesktopLocation = new Point(this.leftXLocation, this.bottomYLocation - this.Height);
                    arrowBrush = new SolidBrush(color2);
                    arrowLeft = arrowOffset;
                    arrowTop = height - 1;
                    arrowXOffset = 0;
                    arrowYOffset = -1;
                    Point bl1 = new Point(arrowLeft, arrowTop);
                    Point bl2 = new Point(arrowLeft + arrowSize, arrowTop + arrowSize);
                    Point bl3 = new Point(arrowLeft + arrowSize + arrowSize, arrowTop);
                    points = new Point[] { bl1, bl2, bl3, bl1 };
                    break;
                case SmokestackDisplay.Location.BottomRight:
                    this.DesktopLocation = new Point(this.rightXLocation, this.bottomYLocation - this.Height);
                    arrowBrush = new SolidBrush(color2);
                    arrowLeft = width - arrowOffset - arrowSize - arrowSize;
                    arrowTop = height - 1;
                    arrowXOffset = 0;
                    arrowYOffset = -1;
                    Point br1 = new Point(arrowLeft, arrowTop);
                    Point br2 = new Point(arrowLeft + arrowSize, arrowTop + arrowSize);
                    Point br3 = new Point(arrowLeft + arrowSize + arrowSize, arrowTop);
                    points = new Point[] { br1, br2, br3, br1 };
                    break;
                default : // TopRight
                    borderTopOffset = arrowSize;

                    this.DesktopLocation = new Point(this.rightXLocation, this.topYLocation);
                    arrowBrush = new SolidBrush(color1);
                    arrowLeft = width - arrowOffset - arrowSize - arrowSize;
                    arrowTop = 0;
                    arrowXOffset = 0;
                    arrowYOffset = 1;
                    Point tp1 = new Point(arrowLeft, arrowTop + arrowSize);
                    Point tp2 = new Point(arrowLeft + arrowSize, arrowTop);
                    Point tp3 = new Point(arrowLeft + arrowSize + arrowSize, arrowTop + arrowSize);
                    points = new Point[] { tp1, tp2, tp3, tp1 };
                    break;
            }

            this.panel1.Top = borderTopOffset;

            Region r = Growl.DisplayStyle.Utility.CreateRoundedRegion(0, borderTopOffset, width, height + borderTopOffset, radius, radius);
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(points);
            r.Union(gp);
            this.Region = r;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRegion(borderBrush, this.Region);
            g.FillPolygon(borderBrush, points);

            Region gradientRegion = Growl.DisplayStyle.Utility.CreateRoundedRegion(borderWidth, borderTopOffset + borderWidth, width - (1 * borderWidth), height + borderTopOffset - (1 * borderWidth), radius - borderWidth, radius - borderWidth);
            using (gradientRegion)
            {
                RectangleF rect = gradientRegion.GetBounds(e.Graphics);
                LinearGradientBrush gradientBrush = new LinearGradientBrush(rect, this.color1, this.color2, LinearGradientMode.Vertical);
                using (gradientBrush)
                {
                    Blend blend = new Blend();
                    blend.Factors = new float[] { 0.0F, 0.1F, 1.0F };
                    blend.Positions = new float[] { 0.0F, 0.3F, 1.0F };
                    gradientBrush.Blend = blend;
                    g.FillRegion(gradientBrush, gradientRegion);
                }
            }

            Point pp1 = new Point(points[0].X + arrowXOffset, points[0].Y + arrowYOffset);
            Point pp2 = new Point(points[1].X + arrowXOffset, points[1].Y + arrowYOffset);
            Point pp3 = new Point(points[2].X + arrowXOffset, points[2].Y + arrowYOffset);
            Point[] points2 = new Point[] { pp1, pp2, pp3, pp1 };
            g.FillPolygon(arrowBrush, points2);

            if (this.image != null)
                g.DrawImage(this.image, width - imageSize - imagePadding, imagePadding + borderTopOffset, imageSize, imageSize);
        }

        private void applicationNameLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
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
                this.descriptionLabel.Top += args.HeightChange;
                descriptionLabel_LabelHeightChanged(args);
            }
        }

        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Height += args.HeightChange;
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
                if (this.image != null) this.image.Dispose();
                if (this.arrowBrush != null) this.arrowBrush.Dispose();
                if (this.borderBrush != null) this.borderBrush.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}