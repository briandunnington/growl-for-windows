using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Growl.DisplayStyle;

namespace Growl.Displays.Standard
{
    public partial class StandardWindow : NotificationWindow
    {
        private Color color1 = StandardDisplay.COLOR1;
        private Color color2 = StandardDisplay.COLOR2;
        private Color textColor1 = Color.White;
        private Color textColor2 = Color.Black;

        int radius = 12;
        int borderWidth = 1;

        public StandardWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(StandardWindow_Load);
            this.FormClosed += new FormClosedEventHandler(StandardWindow_FormClosed);

            this.Animator = new FadeAnimator(this, 300, 250, FadeAnimator.MAX_OPACITY);

            HookUpClickEvents(this);

            // set size
            this.Width = 250;
            this.Height = 75;

            int duration = 4000;
            string d = System.Configuration.ConfigurationManager.AppSettings["Duration"];
            if (!String.IsNullOrEmpty(d))
            {
                int.TryParse(d, out duration);
            }
            this.SetAutoCloseInterval(duration);
        }

        void StandardWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(this.pictureBox1.Image != null)
                this.pictureBox1.Image.Dispose();
        }

        void StandardWindow_Load(object sender, EventArgs e)
        {
            // set initial location
            int screenIndex = 0;
            string s = System.Configuration.ConfigurationManager.AppSettings["ScreenIndex"];
            if (!String.IsNullOrEmpty(s))
            {
                int.TryParse(s, out screenIndex);
                if (Screen.AllScreens.Length <= screenIndex) screenIndex = 0;
            }

            Screen screen = Screen.AllScreens[screenIndex];
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);

            // set initial opacity
            //this.Opacity = 0.98;
        }

        public Color Color1
        {
            get
            {
                return this.color1;
            }
            set
            {
                this.color1 = value;
                HandleTextColor();
                this.Invalidate();
            }
        }

        public Color Color2
        {
            get
            {
                return this.color2;
            }
            set
            {
                this.color2 = value;
                HandleTextColor();
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Brush borderBrush = Brushes.Black;
            g.FillRegion(borderBrush, this.Region);

            Region gradientRegion = Growl.DisplayStyle.Utility.CreateRoundedRegion(borderWidth, borderWidth, this.Width - (1 * borderWidth), Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth);
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

            int glassHeight = 22;
            Region glassRegion = Growl.DisplayStyle.Utility.CreateRoundedRegion(borderWidth, borderWidth, this.Width - (1 * borderWidth), glassHeight, radius - borderWidth, radius - borderWidth);
            using (glassRegion)
            {
                RectangleF glassRect = glassRegion.GetBounds(e.Graphics);
                Brush glassBrush = new LinearGradientBrush(glassRect, Color.FromArgb(140, Color.White), Color.FromArgb(20, Color.White), LinearGradientMode.Vertical);
                using (glassBrush)
                {
                    g.FillRegion(glassBrush, glassRegion);
                }
            }
        }

        private void HandleTextColor()
        {
            this.textColor1 = GetTextColor(this.color1);
            this.textColor2 = GetTextColor(this.color2);

            this.applicationNameLabel.ForeColor = this.textColor2;
            this.titleLabel.ForeColor = this.textColor1;
            this.descriptionLabel.ForeColor = this.textColor1;
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) this.SetAutoCloseInterval(n.Duration * 1000);

            //Image image = n.GetImage();
            Image image = n.Image;
            if (image != null)
            {
                this.pictureBox1.Image = image;
                this.pictureBox1.Visible = true;
                int offset = this.pictureBox1.Width + 6;
                this.descriptionLabel.Left = this.descriptionLabel.Left + offset;
                this.descriptionLabel.Width = this.descriptionLabel.Width - offset;
            }

            string applicationNameFormat = (!String.IsNullOrEmpty(n.OriginMachineName) ? "{0} on {1}" : "{0}");
            this.applicationNameLabel.Text = String.Format(applicationNameFormat, n.ApplicationName, n.OriginMachineName);
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;

            Region borderRegion = Growl.DisplayStyle.Utility.CreateRoundedRegion(0, 0, this.Width, this.Height, radius, radius);
            this.Region = borderRegion;
        }

        public Color GetTextColor(Color bg)
        {
            int nThreshold = 105;
            int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + (bg.B * 0.114));
            Color foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
            return foreColor;
        }

        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            this.pictureBox1.Top += args.HeightChange;
            this.descriptionLabel.Top += args.HeightChange;
            descriptionLabel_LabelHeightChanged(args);
        }

        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Size = new Size(this.Size.Width, this.Size.Height + args.HeightChange);
                this.Location = new Point(this.Location.X, this.Location.Y - args.HeightChange);
            }
        }
    }
}