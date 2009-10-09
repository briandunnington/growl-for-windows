using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GrowlExtras.FeedMonitor
{
    public partial class FeedListView : ListView
    {
        private ToolTip tooltip = new ToolTip();

        public FeedListView(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            this.SuspendLayout();

            this.DoubleBuffered = true;
            this.OwnerDraw = true;
            this.DrawItem += new DrawListViewItemEventHandler(FeedListView_DrawItem);

            // columns
            ColumnHeader nameHeader = new ColumnHeader();
            ColumnHeader urlHeader = new ColumnHeader();
            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                nameHeader,
                urlHeader});

            this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            //this.LargeImageList = this.imageList;
            this.MultiSelect = false;
            this.UseCompatibleStateImageBehavior = false;
            this.View = System.Windows.Forms.View.Tile;
            this.Scrollable = false;
            this.ShowItemToolTips = true;
            this.LabelWrap = false;

            // uncomment this to use custom tool tips
            //this.ShowItemToolTips = false;
            this.ItemMouseHover += new ListViewItemMouseHoverEventHandler(FeedListView_ItemMouseHover);
            //this.MouseLeave += new EventHandler(FeedListView_MouseLeave);

            this.ResumeLayout();
        }

        void FeedListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (this.View == View.Tile)
            {
                Feed feed = (Feed) e.Item.Tag;

                // draw the background and focus rectangle for selected and non-selected states
                e.DrawBackground();
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(System.Drawing.Brushes.LightGray, e.Bounds);
                    ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
                }

                // draw icon
                int newX = e.Bounds.Left;
                /*
                System.Drawing.Image img = this.imageList.Images[e.Item.ImageKey];
                if (img != null)
                {
                    int x = e.Bounds.Left;
                    int y = e.Bounds.Top;
                    e.Graphics.DrawImage(img, x, y);
                    newX = e.Bounds.Left + img.Width + this.Margin.Right;
                    img.Dispose();
                }
                 * */

                // draw main text
                System.Drawing.RectangleF rect = new System.Drawing.RectangleF(newX, e.Bounds.Top, e.Bounds.Right - newX, e.Item.Font.Height);
                System.Drawing.StringFormat sf = new System.Drawing.StringFormat();
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoClip;
                System.Drawing.SolidBrush foreBrush = new System.Drawing.SolidBrush(e.Item.ForeColor);
                using (foreBrush)
                {
                    e.Graphics.DrawString(feed.Name,
                        e.Item.Font,
                        foreBrush,
                        rect,
                        sf);
                }

                // draw subitems
                System.Drawing.Color subColor = System.Drawing.Color.FromArgb(System.Drawing.SystemColors.GrayText.ToArgb());
                System.Drawing.SolidBrush subBrush = new System.Drawing.SolidBrush(subColor);
                using (subBrush)
                {
                    for (int i = 1; i < this.Columns.Count; i++)
                    {
                        if (i < e.Item.SubItems.Count)
                        {
                            rect.Offset(0, e.Item.Font.Height);
                            e.Graphics.DrawString(e.Item.SubItems[i].Text,
                                e.Item.Font,
                                subBrush,
                                rect,
                                sf);
                        }
                    }
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        void FeedListView_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            // update tooltip text
            Feed feed = e.Item.Tag as Feed;
            if (feed != null)
            {
                e.Item.ToolTipText = String.Format("Updates every {0} minutes", feed.PollInterval);
            }

            /*
            Feed feed = e.Item.Tag as Feed;
            if (feed != null)
            {
                string text = String.Format("Updates every {0} minutes", feed.PollInterval);

                //System.Drawing.Point position = Cursor.Position;
                System.Drawing.Point position = this.PointToClient(Cursor.Position);
                position.Offset(0, Cursor.Current.Size.Height - 10);
                IntPtr handle = (IntPtr)typeof(ToolTip).GetProperty("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this.tooltip, null);
                this.tooltip.Show(text, this, position, this.tooltip.AutoPopDelay);
                User32DLL.SetWindowPos(handle, User32DLL.HWND_TOPMOST, 0, 0, 0, 0, User32DLL.SWP_NOACTIVATE | User32DLL.SWP_NOMOVE | User32DLL.SWP_NOSIZE);
            }
             * */
        }

        void FeedListView_MouseLeave(object sender, EventArgs e)
        {
            System.Drawing.Point p = PointToClient(Cursor.Position);
            ListViewHitTestInfo info = this.HitTest(p);
            if (info.Item == null)
                this.tooltip.Hide(this);
        }
    }
}
