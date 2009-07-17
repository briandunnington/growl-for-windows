using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ForwardListBox : ListBox
    {
        private const int IMAGE_SIZE = 48;

        private Color foreColor = Color.Black;
        private int previouslySelectedIndex = -1;

        public ForwardListBox()
            : base()
        {
            InitializeComponent();

            this.DrawMode = DrawMode.OwnerDrawFixed;

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
                ForwardDestinationListItem item = obj as ForwardDestinationListItem;
                if (item != null)
                {
                    if(item.Image != null)
                    {
                        e.Graphics.DrawImage(item.Image, e.Bounds.X + 1, e.Bounds.Y + 1, IMAGE_SIZE, IMAGE_SIZE);
                    }
                    newX = e.Bounds.Left + IMAGE_SIZE + this.Margin.Right;

                    text = item.Text;
                }

                // handle text
                Rectangle rect = new Rectangle(newX, e.Bounds.Y, e.Bounds.Right - newX, e.Bounds.Height);
                TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoClipping | TextFormatFlags.VerticalCenter;
                TextRenderer.DrawText(e.Graphics, text, e.Font, rect, this.foreColor, flags);
            }
        }

        public void AddItem(ForwardDestinationListItem item)
        {
            this.Items.Add(item);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            int currentlySelectedIndex = this.SelectedIndex;
            if (currentlySelectedIndex != previouslySelectedIndex)
            {
                previouslySelectedIndex = currentlySelectedIndex;
                base.OnSelectedIndexChanged(e);
            }
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
                    if (this.Items.Count > i)
                    {
                        this.SelectedIndex = i;
                    }
                    else
                    {
                        this.SelectedIndices.Clear();
                    }
                }
            }
        }

        private void BonjourListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.SelectedItem != null)
            {
                if (this.SelectedItem is ForwardDestinationListItem)
                {
                    ForwardDestinationListItem fli = (ForwardDestinationListItem)this.SelectedItem;
                    fli.Select();
                }
            }
        }
    }
}
