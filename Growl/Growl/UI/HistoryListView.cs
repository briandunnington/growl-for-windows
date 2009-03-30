using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class HistoryListView : ListView
    {
        private const int DEFAULT_NUMBER_OF_DAYS = 7;
        public const int MIN_NUMBER_OF_DAYS = 1;
        public const int MAX_NUMBER_OF_DAYS = 7;
        private const int DEFAULT_TILE_HEIGHT = 56;

        private HistoryGroupItemsBy groupBy = HistoryGroupItemsBy.Date;
        private int numberOfDays;
        private List<PastNotification> pastNotifications;
        private ImageList imageList;
        private List<string> dateGroups;
        private bool useCustomToolTips;
        private ToolTip tooltip = new ToolTip();
        private DateTime currentEndOfToday;

        public HistoryListView()
        {
            InitializeComponent();

            this.SuspendLayout();

            this.imageList = new ImageList();
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(48, 48);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;

            this.OwnerDraw = true;
            this.DoubleBuffered = true;

            // columns
            ColumnHeader titleHeader = new ColumnHeader();
            ColumnHeader textHeader = new ColumnHeader();
            ColumnHeader appNameHeader = new ColumnHeader();
            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                titleHeader,
                textHeader,
                appNameHeader});

            this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.LargeImageList = this.imageList;
            this.MultiSelect = false;
            this.UseCompatibleStateImageBehavior = false;
            this.View = System.Windows.Forms.View.Tile;
            this.Scrollable = true;
            this.ShowItemToolTips = true;
            this.LabelWrap = false;
            //this.Sorting = SortOrder.None;
            //this.ListViewItemSorter = PastNotification;

            //this.TileSize = new System.Drawing.Size(this.TileSize.Width, DEFAULT_TILE_HEIGHT);

            this.NumberOfDays = DEFAULT_NUMBER_OF_DAYS;

            this.Resize += new EventHandler(HistoryListView_Resize);
            this.DrawItem += new DrawListViewItemEventHandler(HistoryListView_DrawItem);

            this.ItemMouseHover +=new ListViewItemMouseHoverEventHandler(HistoryListView_ItemMouseHover);
            this.MouseLeave += new EventHandler(HistoryListView_MouseLeave);

            UpdateEndOfToday();

            this.ResumeLayout();
        }

        void HistoryListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (this.View == View.Tile)
            {
                // draw the background and focus rectangle for selected and non-selected states
                if ((e.State & ListViewItemStates.Selected) != 0)
                {
                    e.Graphics.FillRectangle(System.Drawing.Brushes.LightGray, e.Bounds);
                    e.DrawFocusRectangle();
                }
                else
                {
                    e.DrawBackground();
                    e.DrawFocusRectangle();
                }

                // draw icon
                int newX = e.Bounds.Left;
                System.Drawing.Image img = this.imageList.Images[e.Item.ImageKey];
                if (img != null)
                {
                    int x = e.Bounds.Left;
                    int y = e.Bounds.Top;
                    e.Graphics.DrawImage(img, x, y);
                    newX = e.Bounds.Left + img.Width + this.Margin.Right;
                    img.Dispose();
                }

                // draw main text
                System.Drawing.RectangleF rect = new System.Drawing.RectangleF(newX, e.Bounds.Top, e.Bounds.Right - newX, e.Item.Font.Height);
                System.Drawing.StringFormat sf = new System.Drawing.StringFormat();
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoClip;
                System.Drawing.SolidBrush foreBrush = new System.Drawing.SolidBrush(e.Item.ForeColor);
                using (foreBrush)
                {
                    e.Graphics.DrawString(e.Item.Text,
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

        [System.ComponentModel.Description("Use custom drawn tooltips instead of the built-in tooltips")]
        public bool UseCustomToolTips
        {
            get
            {
                return this.useCustomToolTips;
            }
            set
            {
                if (value) this.ShowItemToolTips = false;
                this.useCustomToolTips = value;
            }
        }

        public HistoryGroupItemsBy GroupBy
        {
            get
            {
                return this.groupBy;
            }
            set
            {
                this.groupBy = value;
            }
        }

        public int NumberOfDays
        {
            get
            {
                return this.numberOfDays;
            }
            set
            {
                if (value < MIN_NUMBER_OF_DAYS) value = MIN_NUMBER_OF_DAYS;
                if (value > MAX_NUMBER_OF_DAYS) value = MAX_NUMBER_OF_DAYS;
                this.numberOfDays = value;
                GenerateDateGroups();
            }
        }

        public List<PastNotification> PastNotifications
        {
            get
            {
                return this.pastNotifications;
            }
            set
            {
                this.pastNotifications = value;
            }
        }

        public void AddNotification(PastNotification pn)
        {
            string groupName = pn.Notification.ApplicationName;
            if (this.groupBy == HistoryGroupItemsBy.Date)
            {
                groupName = GetDateGroup(pn.Timestamp);
            }

            this.pastNotifications.Add(pn);
            ListViewGroup group = this.Groups[groupName];
            if (group != null && group.Items.Count > 1)
            {
                // add item to group
                AddItem(pn);
            }
            else
            {
                // redraw the entire control
                this.Draw();
            }
        }

        public void Draw()
        {
            this.SuspendLayout();

            // clear everything
            this.Groups.Clear();
            this.Items.Clear();
            this.imageList.Images.Clear();

            // go through notifications, purge old ones and prep valid ones
            List<string> groupNames = new List<string>();
            List<PastNotification> validNotifications = new List<PastNotification>();
            List<PastNotification> invalidNotifications = new List<PastNotification>();
            if (this.pastNotifications != null)
            {
                foreach (PastNotification pn in this.pastNotifications)
                {
                    if (IsInDateRange(pn.Timestamp))
                    {
                        if (this.groupBy == HistoryGroupItemsBy.Application)
                        {
                            if (!groupNames.Contains(pn.Notification.ApplicationName))
                                groupNames.Add(pn.Notification.ApplicationName);
                        }

                        if (pn.HasImage && !this.imageList.Images.ContainsKey(pn.ImageKey))
                        {
                            //System.Drawing.Image image = pn.Notification.GetImage();
                            System.Drawing.Image image = pn.Image;
                            if(image != null)
                                this.imageList.Images.Add(pn.ImageKey, image);
                        }

                        validNotifications.Add(pn);
                    }
                    else
                    {
                        invalidNotifications.Add(pn);
                    }
                }
            }

            // remove invalid entries //TODO: maybe move this out into some kind of scheduled timer process
            DateTime cutoff = DateTime.Now.AddDays(-MAX_NUMBER_OF_DAYS).Date;
            foreach (PastNotification pn in invalidNotifications)
            {
                if(pn.Timestamp < cutoff)
                    this.pastNotifications.Remove(pn);
            }
            invalidNotifications.Clear();
            invalidNotifications = null;

            // handle date group names
            if (this.groupBy == HistoryGroupItemsBy.Date)
            {
                groupNames.AddRange(this.dateGroups);
            }

            // create groups
            foreach (string groupName in groupNames)
            {
                this.Groups.Add(groupName, groupName);
            }

            validNotifications.Sort();

            // add items
            foreach (PastNotification pn in validNotifications)
            {
                AddItem(pn);
            }

            // handle empty groups
            foreach (ListViewGroup group in this.Groups)
            {
                if (group.Items.Count == 0)
                {
                    string[] items = new string[this.Columns.Count];
                    items[0] = Properties.Resources.History_NoNotificationsForDate;
                    ListViewItem lvi = new ListViewItem(items, group);
                    this.Items.Add(lvi);
                }
            }

            this.ResumeLayout();
        }

        private void AddItem(PastNotification pn)
        {
            System.Diagnostics.Debug.Assert(!this.InvokeRequired, "InvokeRequired");

            string groupName = pn.Notification.ApplicationName;
            if (this.groupBy == HistoryGroupItemsBy.Date)
            {
                groupName = GetDateGroup(pn.Timestamp);
            }

            if (pn.HasImage && !this.imageList.Images.ContainsKey(pn.ImageKey))
            {
                //System.Drawing.Image image = pn.Notification.GetImage();
                System.Drawing.Image image = pn.Image;
                if(image != null)
                    this.imageList.Images.Add(pn.ImageKey, image);
            }

            string title = Escape(pn.Notification.Title);
            string text = Escape(pn.Notification.Description);
            string appName = Escape(pn.Notification.ApplicationName);
            string tooltip = String.Format("{0}\r\n{1}\r\n{4}: {2}\r\n{5}: {3}", pn.Notification.Title, pn.Notification.Description, pn.Notification.ApplicationName, pn.Timestamp.ToString(), Properties.Resources.LiteralString_ReceivedFrom, Properties.Resources.LiteralString_ReceivedAt);

            string[] items = new string[] { title, text, appName };
            ListViewItem lvi = new ListViewItem(items, pn.ImageKey, this.Groups[groupName]);
            lvi.ToolTipText = tooltip;
            this.Items.Add(lvi);
        }

        private bool IsInDateRange(DateTime timestamp)
        {
            DateTime endOfToday = DateTime.Now.Date.AddDays(1);  // end of day today
            DateTime cutoff = endOfToday.AddDays(-this.NumberOfDays);
            if (timestamp < endOfToday && timestamp > cutoff)
                return true;
            else
                return false;
        }

        private string GetDateGroup(DateTime timestamp)
        {
            UpdateEndOfToday();

            TimeSpan ts = this.currentEndOfToday - timestamp;
            int days = ts.Days;
            if (days >= 0 && days < this.dateGroups.Count)
                return this.dateGroups[days];
            else
                return null;
        }

        private void GenerateDateGroups()
        {
            this.dateGroups = new List<string>();

            DateTime today = DateTime.Now.Date;
            for (int i = 0; i < this.NumberOfDays; i++)
            {
                DateTime date = today.AddDays(-i);
                string name = date.DayOfWeek.ToString();

                // special case
                if (i == 0) name = Properties.Resources.LiteralString_Today;
                else if (i == 1) name = Properties.Resources.LiteralString_Yesterday;

                this.dateGroups.Add(name);
            }
        }

        private void UpdateEndOfToday()
        {
            DateTime newEndOfToday = DateTime.Now.Date.AddDays(1);  // end of current 'Today' group
            if(newEndOfToday != this.currentEndOfToday)
            {
                this.currentEndOfToday = newEndOfToday;
                GenerateDateGroups();
                Draw();
            }
        }

        private static string Escape(string input)
        {
            string output = input;
            output = output.Replace("...", "..");
            output = output.Replace("\n", " - ");
            return output;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [ return: MarshalAs(UnmanagedType.Bool)]
        static private extern bool ShowScrollBar(System.IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);

        void HistoryListView_Resize(object sender, EventArgs e)
        {
            try
            {
                int parentWidth = this.Size.Width - 26; // account for scrollbar
                int columns = (parentWidth / 200);
                if (columns == 0) columns = 1;
                int width = (parentWidth / columns) - 12; // account for margin/padding;
                //MessageBox.Show(String.Format("p: {0}; c: {1}; w: {2}; ots: {3}", parentWidth, columns, width, this.TileSize.Width));
                int height = (this.TileSize.Height == 0 ? DEFAULT_TILE_HEIGHT : this.TileSize.Height);
                this.TileSize = new System.Drawing.Size(width, height);
                this.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                ShowScrollBar(this.Handle, 0, false);
            }
            catch
            {

            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        private void HistoryListView_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (this.UseCustomToolTips)
            {
                //System.Drawing.Point position = Cursor.Position;
                System.Drawing.Point position = this.PointToClient(Cursor.Position);
                position.Offset(0, Cursor.Current.Size.Height - 10);
                IntPtr handle = (IntPtr)typeof(ToolTip).GetProperty("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this.tooltip, null);
                this.tooltip.Show(e.Item.ToolTipText, this, position, this.tooltip.AutoPopDelay);
                Growl.DisplayStyle.User32DLL.SetWindowPos(handle, Growl.DisplayStyle.User32DLL.HWND_TOPMOST, 0, 0, 0, 0, Growl.DisplayStyle.User32DLL.SWP_NOACTIVATE | Growl.DisplayStyle.User32DLL.SWP_NOMOVE | Growl.DisplayStyle.User32DLL.SWP_NOSIZE);
            }
        }

        void HistoryListView_MouseLeave(object sender, EventArgs e)
        {
            if (this.UseCustomToolTips && this.tooltip != null)
            {
                System.Drawing.Point p = PointToClient(Cursor.Position);
                ListViewHitTestInfo info = this.HitTest(p);
                if(info.Item == null)
                    this.tooltip.Hide(this);
            }
        }
    }

    public enum HistoryGroupItemsBy
    {
        Date = 1,
        Application = 2
    }
}
