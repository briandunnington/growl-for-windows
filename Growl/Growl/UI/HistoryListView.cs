using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class HistoryListView : ListView
    {
        public EventHandler RedrawStarted;
        public EventHandler RedrawFinished;

        private const int DEFAULT_NUMBER_OF_DAYS = 7;
        public const int MIN_NUMBER_OF_DAYS = 1;
        public const int MAX_NUMBER_OF_DAYS = 7;
        private const int DEFAULT_TILE_HEIGHT = 56;
        private const int SCROLLBAR_WIDTH = 26;
        private const string DATETIME_COMPARISON_INDICATOR = "datetime";

        private string filter;
        private HistoryGroupItemsBy groupBy = HistoryGroupItemsBy.Date;
        private int numberOfDays;
        private List<PastNotification> pastNotifications;
        private ListViewGroup[] dateGroups;
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

            this.OwnerDraw = true;
            this.DoubleBuffered = true;

            this.MultiSelect = false;
            this.Scrollable = true;
            this.ShowItemToolTips = true;
            this.LabelWrap = false;
            this.UseCompatibleStateImageBehavior = false;
            this.ListViewItemSorter = null;

            this.numberOfDays = DEFAULT_NUMBER_OF_DAYS;

            this.Resize += new EventHandler(HistoryListView_Resize);
            this.DrawItem += new DrawListViewItemEventHandler(HistoryListView_DrawItem);
            this.DrawSubItem += new DrawListViewSubItemEventHandler(HistoryListView_DrawSubItem);
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

        void HistoryListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // draw the background and focus rectangle for selected and non-selected states
            if ((e.ItemState & ListViewItemStates.Selected) != 0)
            {
                e.Graphics.FillRectangle(System.Drawing.Brushes.LightGray, e.Bounds);
                e.DrawFocusRectangle(e.Item.Bounds);
            }
            else
            {
                //e.DrawBackground();
                //e.DrawFocusRectangle(e.Item.Bounds);
            }

            // if this is the first column, we want to draw an icon as well
            int newX = e.Bounds.Left;
            if (e.ColumnIndex == 0)
            {
                // draw the icon
                newX = newX + 20;
                PastNotification pn = (PastNotification)e.Item.Tag;
                if (pn != null)
                {
                    System.Drawing.Image img = PastNotificationManager.GetImage(pn);
                    using (img)
                    {
                        if (img != null)
                        {
                            int x = e.Bounds.Left + 2;
                            int y = e.Bounds.Top;
                            e.Graphics.DrawImage(img, new System.Drawing.Rectangle(x, y, 16, 16));
                        }
                    }
                }
            }

            // draw text
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(newX, e.Bounds.Top, e.Bounds.Right - newX, e.Item.Font.Height);
            System.Drawing.StringFormat sf = new System.Drawing.StringFormat();
            sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
            sf.FormatFlags = System.Drawing.StringFormatFlags.NoClip;
            System.Drawing.SolidBrush foreBrush = new System.Drawing.SolidBrush(e.Item.ForeColor);
            using (foreBrush)
            {
                TextFormatFlags flags = TextFormatFlags.Default | TextFormatFlags.ExternalLeading | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.NoClipping | TextFormatFlags.EndEllipsis | TextFormatFlags.LeftAndRightPadding;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, rect, e.SubItem.ForeColor, System.Drawing.Color.Transparent, flags);
            }
        }

        void HistoryListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (this.View == View.Tile)
            {
                // draw the background and focus rectangle for selected and non-selected states
                if ((e.State & ListViewItemStates.Focused) == ListViewItemStates.Focused)
                {
                    e.Graphics.FillRectangle(System.Drawing.Brushes.LightGray, e.Bounds);
                    e.DrawFocusRectangle();
                 }
                else if(e.State > 0)
                {
                    e.DrawBackground();
                    e.DrawFocusRectangle();
                }

                // draw icon
                PastNotification pn = (PastNotification) e.Item.Tag;
                int newX = e.Bounds.Left;
                if (pn != null)
                {
                    System.Drawing.Image img = PastNotificationManager.GetImage(pn);
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
                // DO NOT call e.DrawDefault or the DrawSubItem event will not be fired
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
            this.pastNotifications.Add(pn);

            this.Draw();
        }

        public void Draw()
        {
            //--DateTime st = DateTime.Now;
            //--Console.WriteLine("HLV.Draw() Start: {0}", st.Ticks);

            if (this.RedrawStarted != null)
                this.RedrawStarted(this, EventArgs.Empty);

            this.BeginUpdate();

            // clear everything first
            this.Items.Clear();
            this.Groups.Clear();

            //--Console.WriteLine("HLV.Draw() 0.5: {0}", (DateTime.Now - st).TotalSeconds);

            if (this.pastNotifications != null)
            {
                // create groups
                List<string> groupNames = new List<string>();
                //--Console.WriteLine("HLV.Draw() 1.0: {0}", (DateTime.Now - st).TotalSeconds);
                if (this.groupBy == HistoryGroupItemsBy.Date)
                    this.Groups.AddRange(this.dateGroups);
                //--Console.WriteLine("HLV.Draw() 2.2: {0}", (DateTime.Now - st).TotalSeconds);

                // go through notifications, purge old ones and prep valid ones
                bool applyFilter = !String.IsNullOrEmpty(this.filter);
                List<ListViewItem> lviList = new List<ListViewItem>();
                foreach (PastNotification pn in this.pastNotifications)
                {
                    // filter by keyword
                    if (applyFilter)
                    {
                        string val = String.Format("{0}|{1}|{2}|{3}", pn.Notification.ApplicationName, pn.Notification.Title, pn.Notification.Description, pn.Notification.OriginMachineName);
                        if (!StringContains(val, this.filter)) continue;
                    }

                    // filter by date range
                    if (!IsInDateRange(pn.Timestamp)) continue;

                    // anything else?

                    // if we made it here, the notification is good to show.
                    // handle application groups (MUST be done before caling CreateItem)
                    if (this.groupBy == HistoryGroupItemsBy.Application)
                    {
                        if (!groupNames.Contains(pn.Notification.ApplicationName))
                        {
                            groupNames.Add(pn.Notification.ApplicationName);
                            this.Groups.Add(pn.Notification.ApplicationName, pn.Notification.ApplicationName);
                        }
                    }

                    lviList.Add(CreateItem(pn));
                }
                //--Console.WriteLine("HLV.Draw() 1: {0}", (DateTime.Now - st).TotalSeconds);

                // prepare view layouts
                if (this.View != View.Details) this.View = View.Tile;
                //--Console.WriteLine("HLV.Draw() 1.1: {0}", (DateTime.Now - st).TotalSeconds);
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

                    this.Columns.Clear();
                    this.Columns.AddRange(this.tileColumns);
                    this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
                }
                else if (this.View == View.Details)
                {
                    this.View = System.Windows.Forms.View.Details;

                    if (detailColumns == null)
                    {
                        int w = 80;
                        int y = 60;
                        int x = (this.Width - (w * 2) - y - SCROLLBAR_WIDTH) / 2;
                        //int x = (this.Width - (w * 3) - SCROLLBAR_WIDTH);

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
                        ColumnHeader originHeader = new ColumnHeader();
                        originHeader.Name = "ORIGIN";
                        originHeader.Text = Properties.Resources.History_Columns_Origin;
                        originHeader.Width = y;

                        this.detailColumns = new ColumnHeader[] { titleHeader, textHeader, appNameHeader, dateHeader, originHeader };
                    }

                    this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
                    this.Columns.Clear();
                    this.Columns.AddRange(this.detailColumns);
                    this.SetSortIcon(this.lvcs.ColumnToSort, this.lvcs.Order);
                }

                //--Console.WriteLine("HLV.Draw() 2: {0}", (DateTime.Now - st).TotalSeconds);

                // add items
                lviList.Sort(this.lvcs);
                //--Console.WriteLine("HLV.Draw() 3: {0}", (DateTime.Now - st).TotalSeconds);
                this.Items.AddRange(lviList.ToArray());
                //--Console.WriteLine("HLV.Draw() 4: {0}", (DateTime.Now - st).TotalSeconds);

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
            }

            this.EndUpdate();

            if (this.RedrawFinished != null)
                this.RedrawFinished(this, EventArgs.Empty);


            //--DateTime et = DateTime.Now;
            //--Console.WriteLine("HLV.Draw() End: {0}", et.Ticks);
            //--Console.WriteLine("HLV.Draw() Elapsed: {0}", (et - st).TotalSeconds);
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
            ListViewGroup group = null;
            if (this.groupBy == HistoryGroupItemsBy.Date)
            {
                group = GetDateGroup(pn.Timestamp);
            }
            else
            {
                group = this.Groups[pn.Notification.ApplicationName];
            }

            string title = Escape(pn.Notification.Title);
            string text = Escape(pn.Notification.Description);
            string appName = Escape(pn.Notification.ApplicationName);
            string origin = (!String.IsNullOrEmpty(pn.Notification.OriginMachineName) ? pn.Notification.OriginMachineName : "Local Machine");
            string tooltipAppNameappName = Escape(String.Format("{0}{1}", pn.Notification.ApplicationName, (!String.IsNullOrEmpty(pn.Notification.OriginMachineName) ? String.Format("[{0}]", pn.Notification.OriginMachineName) : "")));
            string tooltip = String.Format("{0}\r\n{1}\r\n{4}: {2}\r\n{5}: {3}", pn.Notification.Title, pn.Notification.Description, tooltipAppNameappName, pn.Timestamp.ToString(), Properties.Resources.LiteralString_ReceivedFrom, Properties.Resources.LiteralString_ReceivedAt);

            string[] items = new string[] { title, text, appName, pn.Timestamp.ToString(), origin };
            ListViewItem lvi = new ListViewItem(items, group);
            lvi.ToolTipText = tooltip;
            lvi.Tag = pn;
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

        private ListViewGroup GetDateGroup(DateTime timestamp)
        {
            UpdateEndOfToday();

            TimeSpan ts = this.currentEndOfToday - timestamp;
            int days = ts.Days;
            if (days >= 0 && days < this.dateGroups.Length)
                return this.dateGroups[days];
            else
                return null;
        }

        private void GenerateDateGroups()
        {
            this.dateGroups = new ListViewGroup[this.NumberOfDays];

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

                this.dateGroups[i] = new ListViewGroup(name, name);
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
            if (!this.isResizing && this.Visible)
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
                        if (this.currentWidth > 0 && this.Columns.Count > 0)
                        {
                            int diff = this.Width - this.currentWidth;
                            int half = diff / 2;
                            this.Columns["TITLE"].Width += half;
                            this.Columns["TEXT"].Width += half;

                            int columnWidth = SCROLLBAR_WIDTH;
                            foreach (ColumnHeader ch in this.Columns)
                            {
                                columnWidth += ch.Width;
                            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.tooltip != null)
                {
                    this.tooltip.RemoveAll();
                    this.tooltip.Dispose();
                    this.tooltip = null;
                }

                if (this.contextMenu != null)
                {
                    this.contextMenu.Dispose();
                    this.contextMenu = null;
                }
            }

            base.Dispose(disposing);
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
