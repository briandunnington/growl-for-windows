using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ForwardListView : ListView
    {
        private const int DEFAULT_TILE_HEIGHT = 56;
        private const int CHECKBOX_SIZE = 16;
        private const int CHECKBOX_PADDING = 10;

        private ImageList imageList;
        private Dictionary<string, ForwardDestination> computers;
        private bool allDisabled = true;

        public ForwardListView()
        {
            InitializeComponent();

            this.imageList = new ImageList();
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(48, 48);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // add default platform type icons
            this.imageList.Images.Add(ForwardDestinationPlatformType.Windows.Name, ForwardDestinationPlatformType.Windows.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Mac.Name, ForwardDestinationPlatformType.Mac.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Linux.Name, ForwardDestinationPlatformType.Linux.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Internet.Name, ForwardDestinationPlatformType.Internet.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.IPhone.Name, ForwardDestinationPlatformType.IPhone.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Mobile.Name, ForwardDestinationPlatformType.Mobile.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Email.Name, ForwardDestinationPlatformType.Email.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Twitter.Name, ForwardDestinationPlatformType.Twitter.Icon);
            this.imageList.Images.Add(ForwardDestinationPlatformType.Other.Name, ForwardDestinationPlatformType.Other.Icon);

            this.HoverSelection = false;

            this.OwnerDraw = true;
            this.DoubleBuffered = true;

            // columns
            ColumnHeader displayHeader = new ColumnHeader();
            ColumnHeader addressHeader = new ColumnHeader();
            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                displayHeader,
                addressHeader});

            this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.LargeImageList = this.imageList;
            this.MultiSelect = false;
            this.UseCompatibleStateImageBehavior = false;
            this.View = System.Windows.Forms.View.Tile;
            this.Scrollable = true;
            this.ShowItemToolTips = true;
            this.LabelWrap = false;

            this.Resize += new EventHandler(ForwardListView_Resize);
            this.DrawItem += new DrawListViewItemEventHandler(ForwardListView_DrawItem);
            this.MouseClick += new MouseEventHandler(ForwardListView_MouseClick);
        }

        void ForwardListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewHitTestInfo info = this.HitTest(e.Location);
                if (info.Item != null)
                {
                    int y = info.Item.Bounds.Y + CHECKBOX_PADDING;
                    if (e.X > CHECKBOX_PADDING && e.X < (CHECKBOX_PADDING + CHECKBOX_SIZE) && e.Y > y && e.Y < (y + CHECKBOX_SIZE))
                    {
                        ForwardDestination fc = (ForwardDestination)info.Item.Tag;
                        fc.Enabled = !fc.Enabled;
                        this.Refresh();
                    }
                }
            }
        }

        void ForwardListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (this.View == View.Tile)
            {
                int checkBoxAreaWidth = CHECKBOX_PADDING + CHECKBOX_SIZE + CHECKBOX_PADDING;
                System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(e.Bounds.X + checkBoxAreaWidth, e.Bounds.Top, e.Bounds.Width - checkBoxAreaWidth, e.Bounds.Height);

                // update information
                ForwardDestination fc = (ForwardDestination)e.Item.Tag;
                string display = Escape(fc.Display);
                string address = Escape(fc.AddressDisplay);
                string tooltip = String.Format("{0}\r\n{1}", fc.Display, fc.AddressDisplay);
                e.Item.ToolTipText = tooltip;
                // NOTE: dont set the .Text or .SubItem properties here - it causes an erratic exception

                bool drawEnabled = ShouldDrawEnabled(fc);

                // draw the background for selected states
                if(drawEnabled)
                {
                    e.DrawBackground();
                    if ((e.State & ListViewItemStates.Selected) != 0)
                    {
                        e.Graphics.FillRectangle(System.Drawing.Brushes.LightGray, bounds);
                    }
                    else
                    {
                        System.Drawing.SolidBrush backBrush = new System.Drawing.SolidBrush(this.BackColor);
                        using (backBrush)
                        {
                            e.Graphics.FillRectangle(backBrush, bounds);
                        }
                    }
                }

                // draw the focus rectangle
                if (e.Item.Selected)
                    ControlPaint.DrawFocusRectangle(e.Graphics, bounds);

                // draw icon
                int newX = bounds.X;
                //System.Drawing.Image img = this.imageList.Images[e.Item.ImageKey];
                System.Drawing.Image img = this.imageList.Images[fc.Platform.Name];
                if (img != null)
                {
                    int x = bounds.X;
                    int y = bounds.Top;
                    if(drawEnabled)
                        e.Graphics.DrawImage(img, x, y);
                    else
                        ControlPaint.DrawImageDisabled(e.Graphics, img, x, y, System.Drawing.Color.Transparent);
                    newX += img.Width + this.Margin.Right;
                    img.Dispose();
                }

                // draw main text
                System.Drawing.Color textColor = (drawEnabled ? e.Item.ForeColor : System.Drawing.Color.FromArgb(System.Drawing.SystemColors.GrayText.ToArgb()));
                System.Drawing.RectangleF rect = new System.Drawing.RectangleF(newX, bounds.Top, bounds.Right - newX, e.Item.Font.Height);
                System.Drawing.StringFormat sf = new System.Drawing.StringFormat();
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoClip;
                System.Drawing.SolidBrush textBrush = new System.Drawing.SolidBrush(textColor);
                using (textBrush)
                {
                    e.Graphics.DrawString(display,
                        e.Item.Font,
                        textBrush,
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
                            e.Graphics.DrawString(address,
                                e.Item.Font,
                                subBrush,
                                rect,
                                sf);
                        }
                    }
                }

                // draw checkbox
                System.Windows.Forms.VisualStyles.CheckBoxState state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                if (fc.Enabled)
                    state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal;
                else
                    state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new System.Drawing.Point(e.Bounds.Left + CHECKBOX_PADDING, e.Bounds.Top + CHECKBOX_PADDING), state);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        public bool AllDisabled
        {
            get
            {
                return this.allDisabled;
            }
            set
            {
                this.allDisabled = value;
            }
        }

        public Dictionary<string, ForwardDestination> Computers
        {
            get
            {
                return this.computers;
            }
            set
            {
                this.computers = value;
            }
        }

        internal ImageList ImageList
        {
            get
            {
                return this.imageList;
            }
            set
            {
                this.imageList = value;
            }
        }

        public void Draw()
        {
            this.SuspendLayout();

            UpdateTileSize();

            // clear everything
            this.Items.Clear();

            // add items
            lock (this.computers)   // this isnt ideal, but it prevents the edge case where a subscriber comes online while we are trying to Draw
            {
                foreach (ForwardDestination fc in this.computers.Values)
                {
                    AddItem(fc);
                }
            }

            this.ResumeLayout();
        }

        private void AddItem(ForwardDestination fc)
        {
            string display = Escape(fc.Display);
            string address = Escape(fc.AddressDisplay);
            string tooltip = String.Format("{0}\r\n{1}", fc.Display, fc.AddressDisplay);

            string[] items = new string[] { display, address };
            ListViewItem lvi = new ListViewItem(items, fc.Platform.Name);
            lvi.ToolTipText = tooltip;
            lvi.Tag = fc;
            this.Items.Add(lvi);
        }

        private bool ShouldDrawEnabled(ForwardDestination fd)
        {
            if (!this.allDisabled && fd.Enabled)
                return true;
            else
                return false;
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

        void ForwardListView_Resize(object sender, EventArgs e)
        {
            UpdateTileSize();
        }

        private void UpdateTileSize()
        {
            try
            {
                int width = this.Size.Width - 26; // account for scrollbar
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
    }
}
