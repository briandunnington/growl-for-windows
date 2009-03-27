using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Scripty
{
    internal class ReadOnlyTextBox : TextBox
    {
        Color backColor = Color.White;

        public ReadOnlyTextBox()
        {
            SetStyle(ControlStyles.Selectable, false);

            this.ReadOnly = true;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (this.backColor != this.BackColor)
            {
                this.backColor = this.BackColor;
                this.ReadOnly = this.ReadOnly;  // this looks funny, but forces the read-only code to fire
                base.OnBackColorChanged(e);
            }
        }

        public new bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }
            set 
            {
                if (this.backColor != this.BackColor) this.BackColor = this.backColor;
                base.ReadOnly = value;
            }
        }
    }
}
