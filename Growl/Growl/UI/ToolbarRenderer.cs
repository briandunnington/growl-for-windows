using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ToolbarRenderer : ToolStripProfessionalRenderer
    {
        int offset = 1;
        int radius = 12;
        int borderWidth = 1;

        public ToolbarRenderer()
            : base(new GrowlProfessionalColorTable())
        {
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBackground(e);
        }

        protected override void  OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
 	         //base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            /* SQUARE BORDER - doesnt work
            ToolStripButton tsb = (ToolStripButton)e.Item;
            if (tsb.Checked)
            {
                Brush b = new SolidBrush(Color.FromArgb(240, 240, 240));
                Rectangle r = new Rectangle(e.Item.Bounds.X, e.Item.Bounds.Y + 1, e.Item.Bounds.Width, e.Item.Bounds.Height - 1);
                e.Graphics.FillRectangle(Brushes.Black, r);
                r = new Rectangle(r.X + 1, r.Y + 1, r.Width - 2, r.Height - 1);
                e.Graphics.FillRectangle(b, r);

                //Pen pen = new Pen(Color.Black, 1);
                //Point[] points = new Point[4];
                //points[0] = new Point(e.Item.Bounds.Left, e.Item.Bounds.Bottom);
                //points[1] = new Point(e.Item.Bounds.Left, e.Item.Bounds.Top);
                //points[2] = new Point(e.Item.Bounds.Right - 1, e.Item.Bounds.Top);
                //points[3] = new Point(e.Item.Bounds.Right - 1, e.Item.Bounds.Bottom);
                //e.Graphics.DrawLines(pen, points);
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Item.Bounds);
            }
            */

            /* HIGHLIGHT GRADIENT
            ToolStripButton tsb = (ToolStripButton)e.Item;
            if (tsb.Selected)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(e.Item.ContentRectangle);
                PathGradientBrush pgb = new PathGradientBrush(gp);
                //pgb.CenterColor = Color.FromArgb(196,253,247);
                pgb.CenterColor = Color.White;
                pgb.SurroundColors = new Color[] { Color.Transparent };
                Blend blend = new Blend();
                blend.Positions = new float[] { 0.0F, 0.5F, 1.0F };
                blend.Factors = new float[] { 0.0F, 0.5F, 1.0F };
                pgb.Blend = blend;
                e.Graphics.FillRectangle(pgb, e.Item.ContentRectangle);
                pgb.Dispose();
            }
             * */


            /* ROUNDED BORDER
            ToolStripButton tsb = (ToolStripButton)e.Item;
            if (tsb.Selected || tsb.Checked)
            {
                Region borderRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(offset, offset, tsb.Width - (2 * offset), tsb.Height, radius, radius));
                //RectangleF borderRect = borderRegion.GetBounds(e.Graphics);
                Brush borderBrush = new SolidBrush(this.ColorTable.ButtonSelectedBorder);
                e.Graphics.FillRegion(borderBrush, borderRegion);

                Color bgColor = (tsb.Checked ? this.ColorTable.ButtonCheckedHighlight : this.ColorTable.ButtonSelectedHighlight);


                Region bgRegion = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(offset + borderWidth, offset + borderWidth, tsb.Width - (2 * offset) - borderWidth, tsb.Height - borderWidth, radius - borderWidth, radius - borderWidth));
                //RectangleF bgRect = bgRegion.GetBounds(e.Graphics);
                Brush bgBrush = new SolidBrush(bgColor);
                e.Graphics.FillRegion(bgBrush, bgRegion);
            }
            */

            /* SOLID BACKGROUND */
            ToolStripButton tsb = (ToolStripButton)e.Item;
            if (tsb.Checked)
            {
                Brush b = new SolidBrush(Color.FromArgb(194, 209, 238));
                using (b)
                {
                    // move over 2 pixels to workaround a bug that causes some white pixels in the lower left corner of the first toolbar item
                    Rectangle rect = new Rectangle(2, tsb.ContentRectangle.Y, tsb.ContentRectangle.Width, tsb.ContentRectangle.Height);
                    e.Graphics.FillRectangle(b, rect);
                }
            }
            

            //base.OnRenderButtonBackground(e);
        }

        /*
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                e = new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, e.Item.BackgroundImage, e.ImageRectangle);
            }
            base.OnRenderItemImage(e);
        }
         * */

        private class GrowlProfessionalColorTable : ProfessionalColorTable
        {
            //private Color checkedHighlight = Color.FromArgb(249, 248, 247);
            //private Color checkedHighlight = Color.FromArgb(220, 219, 218);

            /* light blue, no border */
            private Color checkedHighlight = Color.FromArgb(194, 209, 238);
            private Color pressedHighlight = Color.FromArgb(194, 209, 238);
            private Color border = Color.FromArgb(194, 209, 238);

            /* light grey box with border
            private Color checkedHighlight = Color.FromArgb(240, 240, 240);
            private Color pressedHighlight = Color.FromArgb(238, 237, 236);
            private Color border = Color.FromArgb(128, 128, 128);
             */

            public override Color ButtonCheckedHighlight
            {
                get
                {
                    return checkedHighlight;
                }
            }

            public override Color ButtonCheckedHighlightBorder
            {
                get
                {
                    return checkedHighlight;
                }
            }

            /*
            public override Color ButtonSelectedBorder
            {
                get
                {
                    return border;
                }
            }
             * */

            public override Color ButtonPressedHighlight
            {
                get
                {
                    return pressedHighlight;
                }
            }

            /*
            public override Color ButtonSelectedHighlight
            {
                get
                {
                    return pressedHighlight;
                }
            }
             * */
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
            );
    }
}
