using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.UI
{
    public partial class MissedNotificationsWindow : NotificationWindow
    {
        int borderWidth = 1;
        int radius = 16;
        Image normal = global::Growl.Properties.Resources.close_blue;
        Image hover = global::Growl.Properties.Resources.close_red;

        public MissedNotificationsWindow()
        {
            InitializeComponent();

            // localize text
            this.labelMessage.Text = Properties.Resources.Missed_Summary;

            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height - this.Height;
            this.DesktopLocation = new Point(x, y);

            this.historyListView1.GroupBy = HistoryGroupItemsBy.Application;

            this.pictureBox1.Image = global::Growl.Properties.Resources.growl;
            this.pictureBoxClose.Image = normal;

            Region r = Growl.DisplayStyle.Utility.CreateRoundedRegion(0, 0, this.Width, this.Height, radius, radius);
            this.Region = r;
        }

        public override void SetNotification(Notification n)
        {
            base.SetNotification(n);
        }

        public List<PastNotification> MissedNotifications
        {
            get
            {
                return this.historyListView1.PastNotifications;
            }
            set
            {
                this.historyListView1.PastNotifications = value;
                this.historyListView1.Draw();
                this.labelMessage.Text = String.Format("You missed {0} messages while you were away.", value.Count);
            }
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Brush borderBrush = Brushes.Black;
            e.Graphics.FillRegion(borderBrush, this.Region);

            Region gradientRegion = Growl.DisplayStyle.Utility.CreateRoundedRegion(borderWidth, borderWidth, this.Width - (1 * borderWidth), this.Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth);
            using (gradientRegion)
            {
                RectangleF rect = gradientRegion.GetBounds(e.Graphics);
                LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Gainsboro, Color.White, LinearGradientMode.Vertical);
                using (brush)
                {
                    float f1 = ((float)this.historyListView1.Top) / ((float)this.Height);
                    float f2 = ((float)this.historyListView1.Bottom) / ((float)this.Height);
                    Blend blend = new Blend();
                    blend.Factors = new float[] { 0.0F, 1.0F, 1.0F, 0.0F };
                    blend.Positions = new float[] { 0.0F, f1, f2, 1.0F };
                    brush.Blend = blend;
                    e.Graphics.FillRegion(brush, gradientRegion);
                }
            }
        }

        private void pictureBoxClose_MouseEnter(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = hover;
        }

        private void pictureBoxClose_MouseLeave(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = normal;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.normal != null)
                {
                    this.normal.Dispose();
                    this.normal = null;
                }

                if (this.hover != null)
                {
                    this.hover.Dispose();
                    this.hover = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}