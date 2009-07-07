/*
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ListControl : ListBox
    {
        private const int IMAGE_SIZE = 16;

        private Color foreColor = Color.Black;
        //private int previouslySelectedIndex = -1;

        public delegate bool IsDefaultComparerDelegate(object obj);

        private IsDefaultComparerDelegate isDefaultComparer;

        public ListControl()
            : base()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;

            this.DrawItem += new DrawItemEventHandler(ListControl_DrawItem);
            this.ForeColorChanged += new EventHandler(ListControl_ForeColorChanged);
        }

        void ListControl_ForeColorChanged(object sender, EventArgs e)
        {
            this.foreColor = this.ForeColor;
        }

        void ListControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (this.Items != null && this.Items.Count > 0)
            {
                if (e.Index != ListBox.NoMatches)
                {
                    object obj = this.Items[e.Index];

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

                    Console.WriteLine("drawitem - " + text);
                }
            }
        }

        public void AddItem(ListControlItem item)
        {
            this.Items.Add(item);
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
    }
}
*/