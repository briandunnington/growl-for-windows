using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides a textbox that can be highlighted to indicate user attention required.
    /// </summary>
    /// <remarks>
    /// Specifically, the highlight is used to indicate invalid values in a destination
    /// configuration UI. Implementors should use this class in place of standard textboxes
    /// to keep a consistent UI with GfW. The inputs should be highlighted when the values
    /// are invalid and require that the user fix the values before the form can be submitted.
    /// </remarks>
    public class HighlightTextBox : TextBox
    {
        bool highlighted;
        Color originalBackColor = Color.White;
        Color highlightColor = Color.FromArgb(254, 250, 184);

        /// <summary>
        /// Highlights the textbox
        /// </summary>
        public void Highlight()
        {
            if (!this.highlighted)
                this.originalBackColor = this.BackColor;
            this.BackColor = HighlightColor;
            this.highlighted = true;
        }

        /// <summary>
        /// Unhighlights the textbox
        /// </summary>
        public void Unhighlight()
        {
            this.BackColor = this.originalBackColor;
        }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public Color HighlightColor
        {
            get
            {
                return this.highlightColor;
            }
            set
            {
                this.highlightColor = value;
            }
        }
    }
}
