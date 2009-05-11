using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    public partial class SplashScreen : Form
    {
        bool appContextLoaded = false;
        bool showSplashScreen = false;
        int borderWidth = 1;
        int radius = 16;

        public event EventHandler ApplicationContextLoaded;

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        public SplashScreen(bool showSplashScreen) :
            this()
        {
            this.showSplashScreen = showSplashScreen;
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        public SplashScreen()
        {
            InitializeComponent();

            // localize text
            this.labelAppName.Text = Properties.Resources.SplashScreen_Title;
            this.labelVersion.Text = Properties.Resources.SplashScreen_Version;
            this.labelLoading.Text = Properties.Resources.SplashScreen_Loading;

            this.BackColor = Color.White;
            this.pictureBox1.Image = global::Growl.Properties.Resources.growl;
            this.labelVersion.Text = String.Format(this.labelVersion.Text, Utility.FileVersionInfo.ProductVersion);

            this.timer = new Timer();
            this.timer.Interval = 750;
            this.timer.Tick += new EventHandler(timer_Tick);

            this.Shown += new EventHandler(SplashScreen_Shown);
            this.VisibleChanged += new EventHandler(SplashScreen_VisibleChanged);

            Region r = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
            this.Region = r;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!this.appContextLoaded)
            {
                this.appContextLoaded = true;
                if (this.showSplashScreen) base.SetVisibleCore(value);
                if (this.ApplicationContextLoaded != null)
                {
                    this.ApplicationContextLoaded(this, EventArgs.Empty);
                }
            }
            else
            {
                base.SetVisibleCore(value);
            }
        }

        void SplashScreen_Shown(object sender, EventArgs e)
        {
            this.timer.Start();
        }

        void SplashScreen_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible && this.timer != null) this.timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.labelLoading.Text += ".";
            Application.DoEvents();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Brush borderBrush = Brushes.Black;
            e.Graphics.FillRegion(borderBrush, this.Region);

            Region gradientRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(borderWidth, borderWidth, this.Width - (1 * borderWidth), this.Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth));
            using (gradientRegion)
            {
                RectangleF rect = gradientRegion.GetBounds(e.Graphics);
                LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Gainsboro, Color.White, LinearGradientMode.Vertical);
                using (brush)
                {
                    float f1 = ((float) 25) / ((float)this.Height);
                    float f2 = ((float) 125) / ((float)this.Height);
                    Blend blend = new Blend();
                    blend.Factors = new float[] { 0.0F, 1.0F, 1.0F, 0.0F };
                    blend.Positions = new float[] { 0.0F, f1, f2, 1.0F };
                    brush.Blend = blend;
                    e.Graphics.FillRegion(brush, gradientRegion);
                }
            }
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
    }
}