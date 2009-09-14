using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ListControl : Panel
    {
        private const int IMAGE_SIZE = 16;

        public event EventHandler SelectedIndexChanged;

        private Color foreColor = Color.Black;
        //private int previouslySelectedIndex = -1;

        private string headerText = null;
        private Font headerFont = null;
        private int headerHeight = 0;

        private ListBox listbox;

        public delegate bool IsDefaultComparerDelegate(object obj);

        private IsDefaultComparerDelegate isDefaultComparer;

        public ListControl()
            : base()
        {
            this.BorderStyle = BorderStyle.Fixed3D;
            this.BackColor = Color.White;

            this.Resize += new EventHandler(ListControl2_Resize);

            this.listbox = new ListBox();
            this.Controls.Add(listbox);
            this.listbox.DrawMode = DrawMode.OwnerDrawFixed;
            this.listbox.FormattingEnabled = true;
            this.listbox.IntegralHeight = false;
            this.listbox.ItemHeight = 18;
            this.listbox.BorderStyle = BorderStyle.None;
            this.listbox.DrawItem += new DrawItemEventHandler(ListControl_DrawItem);
            this.listbox.ForeColorChanged += new EventHandler(ListControl_ForeColorChanged);
            this.listbox.SelectedIndexChanged += new EventHandler(listbox_SelectedIndexChanged);
            this.listbox.MouseDown += new MouseEventHandler(listbox_MouseDown);
        }

        void listbox_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnListBoxMouseDown(e);
        }

        void listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnSelectedIndexChanged(sender, e);
        }

        void ListControl2_Resize(object sender, EventArgs e)
        {
            AdjustListBox();
        }

        void ListControl_ForeColorChanged(object sender, EventArgs e)
        {
            this.foreColor = this.ForeColor;
        }

        void ListControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (this.listbox.Items != null && this.listbox.Items.Count > 0)
            {
                if (e.Index != ListBox.NoMatches)
                {
                    object obj = this.listbox.Items[e.Index];

                    // handle background
                    if ((e.State & DrawItemState.Selected) != 0)
                    {
                        e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                        ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
                    }
                    else
                    {
                        e.DrawBackground();
                    }

                    int newX = e.Bounds.X;
                    ListControlItem item = obj as ListControlItem;
                    if (item != null)
                    {
                        if (item.RegisteredObject.Icon != null)
                        {
                            e.Graphics.DrawImage(item.RegisteredObject.Icon, e.Bounds.X + 1, e.Bounds.Y + 1, IMAGE_SIZE, IMAGE_SIZE);
                            newX = e.Bounds.Left + IMAGE_SIZE + this.Margin.Right;
                        }
                    }

                    // check if this is the 'default' value of the list
                    bool isDefault = false;
                    if(this.isDefaultComparer != null)
                    {
                        isDefault = this.isDefaultComparer(obj);
                    }

                    // handle text
                    string text = obj.ToString();
                    Font font = e.Font;
                    if (isDefault)
                    {
                        text = text + " [default]";
                        font = new Font(e.Font, FontStyle.Bold);
                    }
                    Rectangle rect = new Rectangle(newX, e.Bounds.Y, e.Bounds.Right - newX, e.Bounds.Height);
                    TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoClipping;
                    TextRenderer.DrawText(e.Graphics, text, font, rect, this.foreColor, flags);

                    //Console.WriteLine("drawitem - " + text);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle headerRect = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, this.headerHeight);
            Brush b1 = new SolidBrush(Color.FromArgb(222, 222, 222));
            using (b1)
            {
                e.Graphics.FillRectangle(b1, headerRect);
            }

            headerRect.Height = headerRect.Height - 1;
            Brush b2 = new SolidBrush(Color.FromArgb(248, 248, 248));
            using (b2)
            {
                e.Graphics.FillRectangle(b2, headerRect);
            }

            TextRenderer.DrawText(e.Graphics, this.headerText, this.headerFont, headerRect, this.ForeColor, TextFormatFlags.Left);

            base.OnPaint(e);
        }

        protected void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, e);
            }
        }

        public void AddItem(ListControlItem item)
        {
            this.listbox.Items.Add(item);
        }

        public IsDefaultComparerDelegate IsDefaultComparer
        {
            get
            {
                return this.isDefaultComparer;
            }
            set
            {
                this.isDefaultComparer = value;
            }
        }

        public string HeaderText
        {
            get
            {
                return this.headerText;
            }
            set
            {
                this.headerText = value;
            }
        }

        public ListBox.ObjectCollection Items
        {
            get
            {
                return this.listbox.Items;
            }
        }

        public object SelectedItem
        {
            get
            {
                return this.listbox.SelectedItem;
            }
            set
            {
                this.listbox.SelectedItem = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this.listbox.SelectedIndex;
            }
            set
            {
                this.listbox.SelectedIndex = value;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // suppress
        }

        protected void OnListBoxMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        public int IndexFromPoint(Point point)
        {
            return this.listbox.IndexFromPoint(point);
        }

        private void AdjustListBox()
        {
            this.headerFont = new Font(this.Font.FontFamily, 9.0f, GraphicsUnit.Point);
            this.headerHeight = this.headerFont.Height + 3;

            this.listbox.Location = new Point(0, headerHeight);
            this.listbox.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - this.headerHeight);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.headerFont != null)
                {
                    this.headerFont.Dispose();
                    this.headerFont = null;
                }

                if (this.listbox != null)
                {
                    this.listbox.Dispose();
                    this.listbox = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
