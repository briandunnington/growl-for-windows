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
        private Dictionary<string, ForwardComputer> computers;

        public ForwardListView()
        {
            InitializeComponent();

            this.imageList = new ImageList();
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(48, 48);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // add default platform type icons
            this.imageList.Images.Add(ForwardComputerPlatformType.Windows.Name, ForwardComputerPlatformType.Windows.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.Mac.Name, ForwardComputerPlatformType.Mac.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.Linux.Name, ForwardComputerPlatformType.Linux.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.Internet.Name, ForwardComputerPlatformType.Internet.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.IPhone.Name, ForwardComputerPlatformType.IPhone.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.Mobile.Name, ForwardComputerPlatformType.Mobile.Icon);
            this.imageList.Images.Add(ForwardComputerPlatformType.Other.Name, ForwardComputerPlatformType.Other.Icon);

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
            ListViewHitTestInfo info = this.HitTest(e.Location);
            if (info.Item != null)
            {
                int y = info.Item.Bounds.Y + CHECKBOX_PADDING;
                if (e.X > CHECKBOX_PADDING && e.X < (CHECKBOX_PADDING + CHECKBOX_SIZE) && e.Y > y && e.Y < (y + CHECKBOX_SIZE))
                {
                    info.Item.Checked = !info.Item.Checked;
                    ForwardComputer fc = (ForwardComputer)info.Item.Tag;
                    fc.Enabled = info.Item.Checked;
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
                ForwardComputer fc = (ForwardComputer)e.Item.Tag;
                string display = Escape(fc.Display);
                string address = Escape(fc.AddressDisplay);
                string tooltip = String.Format("{0}\r\n{1}\r\n{2} {3}", fc.Display, fc.AddressDisplay, (fc.UseUDP ? Properties.Resources.Protocol_Type_UDP : Properties.Resources.Protocol_Type_GNTP), Properties.Resources.LiteralString_Format);
                e.Item.ToolTipText = tooltip;
                // NOTE: dont set the .Text or .SubItem properties here - it causes an erratic exception

                // draw the background and focus rectangle for selected and non-selected states
                if (this.Enabled)
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
                    if (e.Item.Selected)
                        ControlPaint.DrawFocusRectangle(e.Graphics, bounds);
                }
                else
                {

                }

                // draw icon
                int newX = bounds.X;
                //System.Drawing.Image img = this.imageList.Images[e.Item.ImageKey];
                System.Drawing.Image img = this.imageList.Images[fc.Platform.Name];
                if (img != null)
                {
                    int x = bounds.X;
                    int y = bounds.Top;
                    if (this.Enabled && e.Item.Checked)
                        e.Graphics.DrawImage(img, x, y);
                    else
                        ControlPaint.DrawImageDisabled(e.Graphics, img, x, y, System.Drawing.Color.Transparent);
                    newX += img.Width + this.Margin.Right;
                    img.Dispose();
                }

                // draw main text
                System.Drawing.Color textColor = (this.Enabled && e.Item.Checked ? e.Item.ForeColor : System.Drawing.Color.FromArgb(System.Drawing.SystemColors.GrayText.ToArgb()));
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
                if (this.Enabled)
                {
                    if (e.Item.Checked)
                        state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal;
                    else
                        state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                }
                else
                {
                    if (e.Item.Checked)
                        state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled;
                    else
                        state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled;
                }
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new System.Drawing.Point(e.Bounds.Left + CHECKBOX_PADDING, e.Bounds.Top + CHECKBOX_PADDING), state);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        public Dictionary<string, ForwardComputer> Computers
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
            foreach (ForwardComputer fc in this.computers.Values)
            {
                AddItem(fc);
            }

            this.ResumeLayout();
        }

        private void AddItem(ForwardComputer fc)
        {
            string display = Escape(fc.Display);
            string address = Escape(fc.AddressDisplay);
            string tooltip = String.Format("{0}\r\n{1}\r\n{2} {3}", fc.Display, fc.AddressDisplay, (fc.UseUDP ? Properties.Resources.Protocol_Type_UDP : Properties.Resources.Protocol_Type_GNTP), Properties.Resources.LiteralString_Format);

            string[] items = new string[] { display, address };
            ListViewItem lvi = new ListViewItem(items, fc.Platform.Name);
            lvi.ToolTipText = tooltip;
            lvi.Checked = fc.Enabled;
            lvi.Tag = fc;
            this.Items.Add(lvi);
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
