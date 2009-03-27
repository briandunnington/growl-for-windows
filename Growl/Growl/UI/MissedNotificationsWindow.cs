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

            // set initial location
            Screen screen = Screen.FromControl(this);
            int x = screen.WorkingArea.Width - this.Width;
            int y = screen.WorkingArea.Height - this.Height;
            this.DesktopLocation = new Point(x, y);

            this.historyListView1.GroupBy = HistoryGroupItemsBy.Application;

            this.pictureBox1.Image = global::Growl.Properties.Resources.growl;
            this.pictureBoxClose.Image = normal;

            Region r = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
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

            Region gradientRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(borderWidth, borderWidth, this.Width - (1 * borderWidth), this.Height - (1 * borderWidth), radius - borderWidth, radius - borderWidth));
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


        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
            );

        private void pictureBoxClose_MouseEnter(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = hover;
        }

        private void pictureBoxClose_MouseLeave(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = normal;
        }
    }
}