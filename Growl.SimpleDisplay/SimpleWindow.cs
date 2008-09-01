using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.SimpleDisplay
{
    public partial class SimpleWindow : Form
    {
        protected Timer displayTimer;
        protected Timer fadeTimer;
        private bool fading = false;
        private bool sticky = false;
        private Color color1 = Color.SkyBlue;
        private Color color2 = Color.White;
        private Color textColor1 = Color.White;
        private Color textColor2 = Color.Black;

        public SimpleWindow()
        {
            InitializeComponent();

            this.Click += new EventHandler(SimpleWindow_Click);

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.fadeTimer = new Timer();
            this.fadeTimer.Tick += new EventHandler(fadeTimer_Tick);
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
                this.Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            LinearGradientBrush gradientBrush = new LinearGradientBrush(rect, this.color1, this.color2, LinearGradientMode.ForwardDiagonal);
            Blend blend = new Blend();
            blend.Factors = new float[] { 0.3F };
            blend.Positions = new float[] { 0.2F };
            gradientBrush.Blend = blend;
            e.Graphics.FillRectangle(gradientBrush, rect);

            Rectangle t = new Rectangle(0, 0, this.Width, 30);
            Brush b = new LinearGradientBrush(t, this.color1, Color.DarkGray, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(b, t);
        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
            base.OnResize(e);
        }

        private void HandleTextColor()
        {
            this.textColor1 = GetTextColor(this.color1);
            this.textColor2 = GetTextColor(this.color2);

            this.applicationNameLabel.ForeColor = this.textColor1;
            this.titleLabel.ForeColor = this.textColor2;
            this.descriptionLabel.ForeColor = this.textColor2;
        }

        private void Reset()
        {
            // dont show in taskbar
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // set size
            this.Width = 250;
            this.Height = 100;

            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - this.Width;
            int y = screen.WorkingArea.Bottom - this.Height;
            this.DesktopLocation = new Point(x, y);

            // set initial opacity
            this.Opacity = 0.9;
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
                this.Hide();
                this.fadeTimer.Stop();
            }
        }

        public new void Show()
        {
            Reset();
            User32DLL.ShowWindow(this.Handle, User32DLL.SW_SHOWNOACTIVATE);
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

            // set the window to hide in 5 seconds unless we are sticky
            if (!this.sticky)
            {
                this.displayTimer.Interval = 5000;
                this.displayTimer.Start();
            }
        }

        void SimpleWindow_Click(object sender, EventArgs e)
        {
            FadeOut();
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

        public Color GetTextColor(Color bg)
        {
            int nThreshold = 105;
            int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + (bg.B * 0.114));
            Color foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
            return foreColor;
        }
    }
}