using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class CustomPanel : Panel
    {
        public event PaintEventHandler PaintBackground;

        public CustomPanel()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }
    }
}
