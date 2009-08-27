using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.FeedMonitor
{
    public partial class MainForm : Form
    {
        int borderWidth = 1;
        int radius = 16;
        Image normal = global::GrowlExtras.FeedMonitor.Properties.Resources.close_blue;
        Image hover = global::GrowlExtras.FeedMonitor.Properties.Resources.close_red;
        MainComponent mainComponent;
        int bottomPadding = 60;

        public MainForm()
        {
            InitializeComponent();
            this.pictureBoxClose.Image = normal;

            EnsureRegion();

            this.listViewFeeds.TileSize = this.listViewFeeds.Size;
            this.bottomPadding = this.Height - this.listViewFeeds.Bottom;

            HideContextMenuImageMargin(this.contextMenu);
        }

        private void HideContextMenuImageMargin(ToolStripDropDownMenu target)
        {
            target.ShowImageMargin = false;
            foreach (ToolStripItem menuItem in target.Items)
            {
                ToolStripMenuItem item = menuItem as ToolStripMenuItem;

                if (item != null && item.HasDropDownItems)
                {
                    ((ToolStripDropDownMenu)item.DropDown).ShowCheckMargin = true;
                    HideContextMenuImageMargin((ToolStripDropDownMenu)item.DropDown);
                }
            }
        }

        public void SetComponent(MainComponent component)
        {
            this.mainComponent = component;
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void pictureBoxClose_MouseEnter(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = hover;
        }

        private void pictureBoxClose_MouseLeave(object sender, EventArgs e)
        {
            this.pictureBoxClose.Image = normal;
        }

        public new void Show()
        {
            this.listViewFeeds.Items.Clear();
            foreach (Feed feed in this.mainComponent.Feeds)
            {
                ListViewItem lvi = new ListViewItem(new string[] {feed.Name, feed.Url});
                lvi.ToolTipText = String.Format("Updates every {0} minutes", feed.PollInterval);
                lvi.Tag = feed;
                this.listViewFeeds.Items.Add(lvi);
            }
            this.listViewFeeds.Height = (this.listViewFeeds.Items.Count * this.listViewFeeds.TileSize.Height);
            
            ResizeForm();
            base.Show();
        }

        private void EnsureRegion()
        {
            Region r = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, radius, radius));
            this.Region = r;
        }

        private void ResizeForm()
        {
            this.Height = this.listViewFeeds.Top + this.listViewFeeds.Height + this.bottomPadding;

            Screen screen = Screen.PrimaryScreen;
            int x = screen.WorkingArea.Right - this.Width;
            int y = screen.WorkingArea.Bottom - this.Height;
            this.Location = new Point(x, y);

            EnsureRegion();
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
                    float f1 = ((float)this.listViewFeeds.Top) / ((float)this.Height);
                    float f2 = ((float)this.listViewFeeds.Bottom) / ((float)this.Height);
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

        private void labelAddFeed_Click(object sender, EventArgs e)
        {
            this.panel1.Show();
            this.textBoxFeedUrl.Focus();
        }

        private void listViewFeeds_MouseClick(object sender, MouseEventArgs e)
        {
            this.contextMenu.Hide();

            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = this.listViewFeeds.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag != null)
                {
                    Feed feed = item.Tag as Feed;
                    if (feed != null)
                    {
                        foreach (ToolStripMenuItem tsi in this.setIntervalToolStripMenuItem.DropDown.Items)
                        {
                            tsi.Checked = false;
                            int i = Convert.ToInt32(tsi.Tag);
                            if (i == feed.PollInterval) tsi.Checked = true;
                        }
                        this.contextMenu.Tag = feed;
                        this.contextMenu.Show(this.listViewFeeds, e.Location);
                    }
                }
            }
        }

        private void checkNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Feed feed = (Feed)this.contextMenu.Tag;
            feed.CheckForUpdates();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Feed feed = (Feed)this.contextMenu.Tag;
            this.mainComponent.RemoveFeed(feed);
            this.Show();
            this.Refresh();
        }

        private void labelAdd_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.textBoxFeedUrl.Text))
            {
                this.mainComponent.AddFeed(this.textBoxFeedUrl.Text, Properties.Settings.Default.DefaultInterval);
                this.panel1.Hide();
                this.Show();
                this.Refresh();
            }
            else
            {
                this.panel1.Hide();
            }
        }

        private void labelAdd_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void labelAdd_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void labelAddFeed_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void labelAddFeed_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void SetFeedInterval(object sender)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            item.Checked = true;
            int i = Convert.ToInt32(item.Tag);
            Feed feed = (Feed)this.contextMenu.Tag;
            feed.PollInterval = i;
        }

        private void oneMinuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetFeedInterval(sender);
        }

        private void twoMinutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetFeedInterval(sender);
        }

        private void fiveMinutesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetFeedInterval(sender);
        }

        private void tenMinutesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetFeedInterval(sender);
        }

        private void thirtyMinutesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SetFeedInterval(sender);
        }
    }
}
