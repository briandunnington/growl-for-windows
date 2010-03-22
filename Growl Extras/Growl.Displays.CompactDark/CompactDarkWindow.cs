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


namespace Growl.Displays.CompactDark
{
    public partial class CompactDarkWindow : Growl.DisplayStyle.NotificationWindow
    {
        int radius = 16;
        int borderWidth = 1;

        public CompactDarkWindow()
        {
            InitializeComponent();

            this.Load += new EventHandler(CompactDarkWindow_Load);
            this.FormClosed += new FormClosedEventHandler(CompactDarkWindow_FormClosed);

            this.Animator = new FadeAnimator(this);

            HookUpClickEvents(this);
            SetAutoCloseInterval(4000);
        }

        void CompactDarkWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(this.pictureBoxApp.Image != null)
            {
                this.pictureBoxApp.Image.Dispose();
            }
        }

        void CompactDarkWindow_Load(object sender, EventArgs e)
        {
            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Right - this.Size.Width;
            int y = screen.WorkingArea.Bottom - this.Size.Height;
            this.Location = new Point(x, y);
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);

            if (n.Duration > 0) SetAutoCloseInterval(n.Duration * 1000);

            this.applicationNameLabel.Text = n.ApplicationName;
            this.titleLabel.Text = n.Title;
            this.descriptionLabel.Text = n.Description.Replace("\n", "\r\n");
            this.Sticky = n.Sticky;

            Image image = n.Image;
            if (image != null)
            {
                this.pictureBoxApp.Image = image;
                this.pictureBoxApp.Visible = true;
            }
            else
            {
                // Move our application label over to account for no image (only half-way for aesthetic reasons).
                this.applicationNameLabel.Left -= this.pictureBoxApp.Width / 2;
                this.pictureBoxApp.Visible = false;
            }
            Region borderRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
            this.Region = borderRegion;
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Brush borderBrush = Brushes.White;
            Brush backgroundBrush = Brushes.Black;

            g.FillRegion(borderBrush, this.Region);
            Region backgroundRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(borderWidth, borderWidth, this.Width - (1 * borderWidth), Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth));
            g.FillRegion(backgroundBrush, backgroundRegion);
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

        private void applicationNameLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if(args.HeightChange != 0)
            {
                this.titleLabel.Top += args.HeightChange;
                titleLabel_LabelHeightChanged(args);
            }
        }

        private void titleLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if(args.HeightChange != 0)
            {
                this.descriptionLabel.Top += args.HeightChange;
                descriptionLabel_LabelHeightChanged(args);
            }
        }

        private void descriptionLabel_LabelHeightChanged(ExpandingLabel.LabelHeightChangedEventArgs args)
        {
            if (args.HeightChange != 0)
            {
                this.Size = new Size(this.Size.Width, this.Size.Height + args.HeightChange);
                this.Location = new Point(this.Location.X, this.Location.Y - args.HeightChange);

                // Recalculate our border regions.
                Region borderRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
                this.Region = borderRegion;
            }
        }

    }
}

