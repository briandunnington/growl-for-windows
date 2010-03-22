using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.Destinations;

namespace Growl.UI
{
    public class ForwardListBox : ListBox
    {
        private const int IMAGE_SIZE = 48;

        private Color foreColor = Color.Black;

        public ForwardListBox()
            : base()
        {
            InitializeComponent();

            this.DrawMode = DrawMode.OwnerDrawFixed;

            // we cant use SelectionMode.One because that forces an automatic 'scroll into view' behavior that we dont want
            this.SelectionMode = SelectionMode.MultiSimple;

            this.DrawItem += new DrawItemEventHandler(BonjourListBox_DrawItem);
            this.ForeColorChanged += new EventHandler(BonjourListBox_ForeColorChanged);
        }

        void BonjourListBox_ForeColorChanged(object sender, EventArgs e)
        {
            this.foreColor = this.ForeColor;
        }

        void BonjourListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (this.Items != null && this.Items.Count > 0)
            {
                object obj = this.Items[e.Index];

                // handle background
                if ((e.State & DrawItemState.Selected) != 0)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                    //ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
                }
                else
                {
                    e.DrawBackground();
                }

                string text = obj.ToString();
                int newX = e.Bounds.X;
                DestinationListItem item = obj as DestinationListItem;
                if (item != null)
                {
                    if(item.Image != null)
                    {
                        e.Graphics.DrawImage(item.Image, e.Bounds.X + 1, e.Bounds.Y + 1, IMAGE_SIZE, IMAGE_SIZE);
                    }
                    newX = e.Bounds.Left + IMAGE_SIZE + this.Margin.Right;

                    text = Utility.GetResourceString(item.Text);
                }

                // handle text
                Rectangle rect = new Rectangle(newX, e.Bounds.Y, e.Bounds.Right - newX, e.Bounds.Height);
                TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoClipping | TextFormatFlags.VerticalCenter;
                TextRenderer.DrawText(e.Graphics, text, e.Font, rect, this.foreColor, flags);
            }
        }

        public void AddItem(DestinationListItem item)
        {
            this.Items.Add(item);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BonjourListBox
            // 
            this.Size = new System.Drawing.Size(120, 95);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BonjourListBox_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BonjourListBox_MouseMove);
            this.ResumeLayout(false);

        }

        private void BonjourListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i = 0;
            if(e.Y > 0)
            {
                i = e.Y / this.ItemHeight;
                i += this.TopIndex;
                if(this.SelectedIndex != i)
                {
                    this.SelectedIndices.Clear();
                    if (this.Items.Count > i)
                    {
                        this.SelectedIndices.Add(i);
                    }
                }
            }
        }

        private void BonjourListBox_MouseClick(object sender, MouseEventArgs e)
        {
            /* we cant use any of the this.Selected* properites because we are using the MultiSimple selection mode
             * instead, we have to determine which item was clicked on directly
             * */

            int i = 0;
            if (e.Y > 0)
            {
                i = e.Y / this.ItemHeight;
                i += this.TopIndex;
                if (this.Items.Count > i)
                {
                    DestinationListItem fli = (DestinationListItem)this.Items[i];
                    fli.Select();
                }
            }
        }
    }
}
