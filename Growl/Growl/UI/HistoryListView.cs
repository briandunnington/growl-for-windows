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
        private const int SCROLLBAR_WIDTH = 26;
        private const string DATETIME_COMPARISON_INDICATOR = "datetime";

        private string filter = null;
        private HistoryGroupItemsBy groupBy = HistoryGroupItemsBy.Date;
        private int numberOfDays;
        private List<PastNotification> pastNotifications;
        private ImageList imageList;
        private List<string> dateGroups;
        private bool useCustomToolTips;
        private ToolTip tooltip = new ToolTip();
        private DateTime currentEndOfToday;

        ContextMenuStrip contextMenu;
        private int currentWidth = 0;
        ListViewColumnSorter lvcs = new ListViewColumnSorter();
        ColumnHeader[] tileColumns;
        ColumnHeader[] detailColumns;

        public HistoryListView()
        {
            InitializeComponent();

            this.SuspendLayout();

            this.View = View.Tile;

            this.imageList = new ImageList();
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(48, 48);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;

            this.OwnerDraw = true;
            this.DoubleBuffered = true;

            this.MultiSelect = false;
            this.Scrollable = true;
            this.ShowItemToolTips = true;
            this.LabelWrap = false;
            this.LargeImageList = this.imageList;
            this.SmallImageList = this.imageList;
            this.UseCompatibleStateImageBehavior = false;

            this.numberOfDays = DEFAULT_NUMBER_OF_DAYS;

            this.Resize += new EventHandler(HistoryListView_Resize);
            this.DrawItem += new DrawListViewItemEventHandler(HistoryListView_DrawItem);
            this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(HistoryListView_DrawColumnHeader);
            this.ColumnClick += new ColumnClickEventHandler(HistoryListView_ColumnClick);

            this.ItemMouseHover += new ListViewItemMouseHoverEventHandler(HistoryListView_ItemMouseHover);
            this.MouseLeave += new EventHandler(HistoryListView_MouseLeave);

            // 
            // contextMenu
            // 
            ToolStripMenuItem tileToolStripMenuItem = new ToolStripMenuItem();
            tileToolStripMenuItem.Name = "tileToolStripMenuItem";
            tileToolStripMenuItem.AutoSize = true;
            tileToolStripMenuItem.Text = Properties.Resources.History_TileView;
            tileToolStripMenuItem.Click += new EventHandler(tileToolStripMenuItem_Click);

            ToolStripMenuItem detailsToolStripMenuItem = new ToolStripMenuItem();
            detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
            detailsToolStripMenuItem.AutoSize = true;
            detailsToolStripMenuItem.Text = Properties.Resources.History_DetailsView;
            detailsToolStripMenuItem.Click += new EventHandler(detailsToolStripMenuItem_Click);

            this.contextMenu = new ContextMenuStrip();
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tileToolStripMenuItem,
            detailsToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.AutoSize = true;

            this.ContextMenuStrip = this.contextMenu;

            UpdateEndOfToday();

            this.ResumeLayout();
        }

        void HistoryListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == this.lvcs.ColumnToSort)
            {
                // Reverse the current sort direction for this column.
                if (this.lvcs.Order == SortOrder.Ascending)
                    this.lvcs.Order = SortOrder.Descending;
                else
                    this.lvcs.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.lvcs.ColumnToSort = e.Column;
                this.lvcs.Order = SortOrder.Ascending;
            }

            string tag = this.Columns[e.Column].Tag as string;
            if (!String.IsNullOrEmpty(tag) && tag == DATETIME_COMPARISON_INDICATOR)
                this.lvcs.Type = ListViewColumnSorter.ComparisonType.Date;
            else
                this.lvcs.Type = ListViewColumnSorter.ComparisonType.String;


            // redraw to show the sorted items
            this.Draw();
        }

        void HistoryListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
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
                using (img)
                {
                    if (img != null)
                    {
                        int x = e.Bounds.Left;
                        int y = e.Bounds.Top;
                        e.Graphics.DrawImage(img, x, y);
                        newX = e.Bounds.Left + img.Width + this.Margin.Right;
                    }
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

        public string Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                this.filter = value;
                Draw();
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
            /*
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
             * */
            this.Draw();
        }

        public void Draw()
        {
            this.SuspendLayout();

            // apply any filtering
            List<PastNotification> filteredList = ApplyFilter();

            // clear everything
            this.Columns.Clear();
            this.Groups.Clear();
            this.Items.Clear();
            this.imageList.Images.Clear();

            // go through notifications, purge old ones and prep valid ones
            List<string> groupNames = new List<string>();
            List<PastNotification> validNotifications = new List<PastNotification>();
            List<PastNotification> invalidNotifications = new List<PastNotification>();
            if (filteredList != null)
            {
                foreach (PastNotification pn in filteredList)
                {
                    if (IsInDateRange(pn.Timestamp))
                    {
                        if (this.groupBy == HistoryGroupItemsBy.Application)
                        {
                            if (!groupNames.Contains(pn.Notification.ApplicationName))
                                groupNames.Add(pn.Notification.ApplicationName);
                        }

                        /*
                        if (pn.HasImage && !this.imageList.Images.ContainsKey(pn.ImageKey))
                        {
                            //System.Drawing.Image image = pn.Notification.GetImage();
                            System.Drawing.Image image = pn.Image;
                            if (image != null)
                                this.imageList.Images.Add(pn.ImageKey, image);
                        }
                         * */

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
                if (pn.Timestamp < cutoff)
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

            if (this.View != View.Details) this.View = View.Tile;

            // prepare view layouts
            if (this.View == View.Tile)
            {
                this.View = System.Windows.Forms.View.Tile;

                if (this.tileColumns == null)
                {
                    ColumnHeader titleHeader = new ColumnHeader();
                    ColumnHeader textHeader = new ColumnHeader();
                    ColumnHeader appNameHeader = new ColumnHeader();
                    this.tileColumns = new ColumnHeader[] { titleHeader, textHeader, appNameHeader };
                }

                this.Columns.AddRange(this.tileColumns);
                this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
                this.imageList.ImageSize = new System.Drawing.Size(48, 48);
                this.LargeImageList = this.imageList;
            }
            else if (this.View == View.Details)
            {
                this.View = System.Windows.Forms.View.Details;

                if (detailColumns == null)
                {
                    int w = 100;
                    int x = (this.Width - (w * 2) - SCROLLBAR_WIDTH) / 2;

                    ColumnHeader titleHeader = new ColumnHeader();
                    titleHeader.Name = "TITLE";
                    titleHeader.Text = Properties.Resources.History_Columns_Title;
                    titleHeader.Width = x;
                    ColumnHeader textHeader = new ColumnHeader();
                    textHeader.Name = "TEXT";
                    textHeader.Text = Properties.Resources.History_Columns_Text;
                    textHeader.Width = x;
                    ColumnHeader appNameHeader = new ColumnHeader();
                    appNameHeader.Name = "APPLICATION";
                    appNameHeader.Text = Properties.Resources.History_Columns_Application;
                    appNameHeader.Width = w;
                    ColumnHeader dateHeader = new ColumnHeader();
                    dateHeader.Name = "TIMESTAMP";
                    dateHeader.Text = Properties.Resources.History_Columns_Timestamp;
                    dateHeader.Width = w;
                    dateHeader.Tag = DATETIME_COMPARISON_INDICATOR;

                    this.detailColumns = new ColumnHeader[] { titleHeader, textHeader, appNameHeader, dateHeader };
                }

                this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
                this.Columns.AddRange(this.detailColumns);
                this.imageList.ImageSize = new System.Drawing.Size(16, 16);
                this.SmallImageList = this.imageList;
                this.SetSortIcon(this.lvcs.ColumnToSort, this.lvcs.Order);
            }

            // add items
            List<ListViewItem> lviList = new List<ListViewItem>();
            foreach (PastNotification pn in validNotifications)
            {
                lviList.Add(CreateItem(pn));
            }
            lviList.Sort(this.lvcs);
            this.Items.AddRange(lviList.ToArray());

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

        private List<PastNotification> ApplyFilter()
        {
            if (!String.IsNullOrEmpty(this.filter))
            {
                List<PastNotification> filteredList = new List<PastNotification>();
                foreach (PastNotification pn in this.pastNotifications)
                {
                    if (StringContains(pn.Notification.ApplicationName, this.filter)
                        || StringContains(pn.Notification.Title, this.filter)
                        || StringContains(pn.Notification.Description, this.filter))
                    {
                        filteredList.Add(pn);
                    }
                }
                return filteredList;
            }
            else
            {
                return this.pastNotifications;
            }
        }

        private static bool StringContains(string str1, string str2)
        {
            bool contains = false;
            if (!String.IsNullOrEmpty(str1))
            {
                int i = str1.IndexOf(str2, StringComparison.InvariantCultureIgnoreCase);
                if (i >= 0) contains = true;
            }
            return contains;
        }

        private ListViewItem CreateItem(PastNotification pn)
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
                if (image != null)
                    this.imageList.Images.Add(pn.ImageKey, image);
            }

            string title = Escape(pn.Notification.Title);
            string text = Escape(pn.Notification.Description);
            string appName = Escape(pn.Notification.ApplicationName);
            string tooltip = String.Format("{0}\r\n{1}\r\n{4}: {2}\r\n{5}: {3}", pn.Notification.Title, pn.Notification.Description, pn.Notification.ApplicationName, pn.Timestamp.ToString(), Properties.Resources.LiteralString_ReceivedFrom, Properties.Resources.LiteralString_ReceivedAt);

            string[] items = new string[] { title, text, appName, pn.Timestamp.ToString() };
            ListViewItem lvi = new ListViewItem(items, pn.ImageKey, this.Groups[groupName]);
            lvi.ToolTipText = tooltip;
            return lvi;
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

                // the Visual Studio form designer has a bug in it, so we have to check first in order to be able to set the name
                // using the culture (since the culture doesnt get set until runtime)
                string name = date.DayOfWeek.ToString();
                if (Properties.Resources.Culture != null)
                    name = Properties.Resources.Culture.DateTimeFormat.DayNames[(int)date.DayOfWeek];

                // special case
                if (i == 0) name = Properties.Resources.LiteralString_Today;
                else if (i == 1) name = Properties.Resources.LiteralString_Yesterday;

                this.dateGroups.Add(name);
            }
        }

        private void UpdateEndOfToday()
        {
            DateTime newEndOfToday = DateTime.Now.Date.AddDays(1);  // end of current 'Today' group
            if (newEndOfToday != this.currentEndOfToday)
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
        [return: MarshalAs(UnmanagedType.Bool)]
        static private extern bool ShowScrollBar(System.IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);

        private bool isResizing = false;

        void HistoryListView_Resize(object sender, EventArgs e)
        {
            if (!this.isResizing)
            {
                this.isResizing = true;
                try
                {

                    if (this.View == View.Tile)
                    {
                        int parentWidth = this.Size.Width - SCROLLBAR_WIDTH; // account for scrollbar
                        int columns = (parentWidth / 200);
                        if (columns == 0) columns = 1;
                        int width = (parentWidth / columns) - 12; // account for margin/padding;
                        //MessageBox.Show(String.Format("p: {0}; c: {1}; w: {2}; ots: {3}", parentWidth, columns, width, this.TileSize.Width));
                        int height = (this.TileSize.Height == 0 ? DEFAULT_TILE_HEIGHT : this.TileSize.Height);
                        this.TileSize = new System.Drawing.Size(width, height);
                        this.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        ShowScrollBar(this.Handle, 0, false);
                    }
                    else
                    {
                        if (this.currentWidth > 0)
                        {
                            int diff = this.Width - this.currentWidth;
                            int half = diff / 2;
                            this.Columns["TITLE"].Width += half;
                            this.Columns["TEXT"].Width += half;

                            int columnWidth = SCROLLBAR_WIDTH;
                            foreach (ColumnHeader ch in this.Columns)
                            {
                                Console.WriteLine("Column Width: " + ch.Width.ToString());
                                columnWidth += ch.Width;
                            }
                            Console.WriteLine("Column Total Width: " + columnWidth.ToString() + " - Total Width: " + this.Width.ToString());
                            if(columnWidth < this.Width)
                                ShowScrollBar(this.Handle, 0, false);
                        }
                    }
                }
                catch
                {
                }
            }
            this.currentWidth = this.Width;
            this.isResizing = false;
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
                Growl.DisplayStyle.Win32.SetWindowPos(handle, Growl.DisplayStyle.Win32.HWND_TOPMOST, 0, 0, 0, 0, Growl.DisplayStyle.Win32.SWP_NOACTIVATE | Growl.DisplayStyle.Win32.SWP_NOMOVE | Growl.DisplayStyle.Win32.SWP_NOSIZE);
            }
        }

        void HistoryListView_MouseLeave(object sender, EventArgs e)
        {
            if (this.UseCustomToolTips && this.tooltip != null)
            {
                System.Drawing.Point p = PointToClient(Cursor.Position);
                ListViewHitTestInfo info = this.HitTest(p);
                if (info.Item == null)
                    this.tooltip.Hide(this);
            }
        }

        void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.View != View.Details)
            {
                this.View = View.Details;
                Draw();
            }
        }

        void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.View != View.Tile)
            {
                this.View = View.Tile;
                Draw();
            }
        }

        // -- stuff for sort arrows
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct LVCOLUMN
        {
            public Int32 mask;
            public Int32 cx;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public Int32 cchTextMax;
            public Int32 fmt;
            public Int32 iSubItem;
            public Int32 iImage;
            public Int32 iOrder;
        }

        private const Int32 HDI_FORMAT = 0x4;
        private const Int32 HDF_SORTUP = 0x400;
        private const Int32 HDF_SORTDOWN = 0x200;
        private const Int32 LVM_GETHEADER = 0x101f;
        private const Int32 HDM_GETITEM = 0x120b;
        private const Int32 HDM_SETITEM = 0x120c;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessageLVCOLUMN(IntPtr hWnd, Int32 Msg, IntPtr wParam, ref LVCOLUMN lPLVCOLUMN);

        public void SetSortIcon(int ColumnIndex, System.Windows.Forms.SortOrder Order)
        {
            IntPtr ColumnHeader = SendMessage(this.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            for (int ColumnNumber = 0; ColumnNumber <= this.Columns.Count - 1; ColumnNumber++)
            {
                IntPtr ColumnPtr = new IntPtr(ColumnNumber);
                LVCOLUMN lvColumn = new LVCOLUMN();
                lvColumn.mask = HDI_FORMAT;
                SendMessageLVCOLUMN(ColumnHeader, HDM_GETITEM, ColumnPtr, ref lvColumn);

                if (!(Order == System.Windows.Forms.SortOrder.None) && ColumnNumber == ColumnIndex)
                {
                    switch (Order)
                    {
                        case System.Windows.Forms.SortOrder.Ascending:
                            lvColumn.fmt &= ~HDF_SORTDOWN;
                            lvColumn.fmt |= HDF_SORTUP;
                            break;
                        case System.Windows.Forms.SortOrder.Descending:
                            lvColumn.fmt &= ~HDF_SORTUP;
                            lvColumn.fmt |= HDF_SORTDOWN;
                            break;
                    }
                }
                else
                {
                    lvColumn.fmt &= ~HDF_SORTDOWN & ~HDF_SORTUP;
                }

                SendMessageLVCOLUMN(ColumnHeader, HDM_SETITEM, ColumnPtr, ref lvColumn);
            }
        }
    }

    public enum HistoryGroupItemsBy
    {
        Date = 1,
        Application = 2
    }
}
