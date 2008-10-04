using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Growl.DisplayStyle;

namespace Growl.SimpleDisplay
{
    public partial class SimpleWindow : Form
    {
        protected Timer displayTimer;
        protected Timer fadeTimer;
        private bool fading = false;
        private bool sticky = false;
        private Color color1 = SimpleDisplay.COLOR1;
        private Color color2 = SimpleDisplay.COLOR2;
        private Color textColor1 = Color.White;
        private Color textColor2 = Color.Black;
        private GDIDB.DBGraphics dbg;

        public SimpleWindow()
        {
            InitializeComponent();

            this.Click += new EventHandler(SimpleWindow_Click);
            HookUpClickEvents(this.Controls);

            dbg = new GDIDB.DBGraphics();

            // deal with fade out
            this.displayTimer = new Timer();
            this.displayTimer.Tick += new EventHandler(displayTimer_Tick);
            this.fadeTimer = new Timer();
            this.fadeTimer.Tick += new EventHandler(fadeTimer_Tick);


            // dont show in taskbar
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // set size
            this.Width = 250;
            this.Height = 75;

            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height - this.Height;
            this.Location = new Point(x, y);

            // set initial opacity
            this.Opacity = 0.99;
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

        /*
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            int bw = 1;

            Rectangle brect = new Rectangle(0, 0, this.Width, this.Height);
            Brush borderBrush = Brushes.Black;
            e.Graphics.FillRectangle(borderBrush, brect);

            Rectangle rect = new Rectangle(bw, bw, this.Width - bw, this.Height - bw);
            LinearGradientBrush gradientBrush = new LinearGradientBrush(rect, this.color1, this.color2, LinearGradientMode.Vertical);
            //Blend blend = new Blend();
            //blend.Factors = new float[] { 0.3F };
            //blend.Positions = new float[] { 0.2F };
            //gradientBrush.Blend = blend;
            e.Graphics.FillRectangle(gradientBrush, rect);

            Rectangle t = new Rectangle(bw, bw, this.Width - bw, 18);
            //Brush b = Brushes.White;
            Brush b = new LinearGradientBrush(t, Color.FromArgb(60, Color.White), Color.FromArgb(20, Color.White), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(b, t);

            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 12, 12)); // adjust these parameters to get the lookyou want.
        }
         * */

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            //this.Invalidate();
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
            c_Click(sender, e);
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

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
            );

        private void HookUpClickEvents(Control.ControlCollection controls)
        {
            if (controls != null)
            {
                foreach (Control c in controls)
                {
                    HookUpClickEvents(c.Controls);
                    c.Click += new EventHandler(c_Click);
                }
            }
        }

        void c_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void SimpleWindow_Load(object sender, EventArgs e)
        {
            InitDBG();
        }

        private void SimpleWindow_Resize(object sender, EventArgs e)
        {
            InitDBG();
            this.Invalidate();
        }

        private void InitDBG()
        {
            dbg.CreateDoubleBuffer(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            bool buffered = dbg.CanDoubleBuffer();

            if (buffered) g = dbg.g;

            int radius = 12;
            int borderWidth = 1;

            Region borderRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
            Brush borderBrush = Brushes.Black;
            g.FillRegion(borderBrush, borderRegion);

            Region gradientRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(borderWidth, borderWidth, this.Width - (1 * borderWidth), Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth));
            RectangleF rect = gradientRegion.GetBounds(e.Graphics);
            LinearGradientBrush gradientBrush = new LinearGradientBrush(rect, this.color1, this.color2, LinearGradientMode.Vertical);
            g.FillRegion(gradientBrush, gradientRegion);

            RectangleF glassRect = gradientRegion.GetBounds(e.Graphics);
            glassRect.Height = 18;
            Brush glassBrush = new LinearGradientBrush(glassRect, Color.FromArgb(60, Color.White), Color.FromArgb(20, Color.White), LinearGradientMode.Vertical);
            g.FillRectangle(glassBrush, glassRect);

            this.Region = borderRegion;

            if (buffered) dbg.Render(e.Graphics);
        }
    }
}