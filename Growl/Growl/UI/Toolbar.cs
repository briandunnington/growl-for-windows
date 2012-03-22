using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class Toolbar : ToolStrip
    {
        float scale;

        public Toolbar()
        {
            this.Renderer = new ToolbarRenderer();
        }
    }
}
