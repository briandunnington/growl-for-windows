using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class HighlightTextBox : TextBox
    {
        Color originalBackColor = Color.Empty;
        Color highlightColor = Color.Red;

        public void Highlight()
        {
            this.BackColor = HighlightColor;
        }

        public void Unhighlight()
        {
            if (this.BackColor != this.originalBackColor)
                this.BackColor = this.originalBackColor;
        }

        public Color HighlightColor
        {
            get
            {
                return this.highlightColor;
            }
            set
            {
                this.originalBackColor = this.BackColor;
                this.highlightColor = value;
            }
        }
    }
}
