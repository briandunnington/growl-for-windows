using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.Displays.Smokestack
{
    public class PanelEx : Panel
    {
        public PanelEx()
        {
            /*
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.Paint += new PaintEventHandler(PanelEx_Paint);
             * */
        }

        /*
        void PanelEx_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(System.Drawing.Pens.Red, 0, 0, this.Width - 1, this.Height - 1);
            e.Graphics.DrawLine(System.Drawing.Pens.Red, 0, this.Height - 1, this.Width - 1, 0);
        }
         * */
    }
}
