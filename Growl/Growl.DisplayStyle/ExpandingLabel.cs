using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    /// <summary> 
    /// A label that supports vertical auto resizing. 
    /// </summary> 
    public class ExpandingLabel : System.Windows.Forms.Label
    {
        private bool dontExpand = false;
        private System.Drawing.Text.TextRenderingHint textRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        private int minimumHeight = 1;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ExpandingLabel()
        {
            InitializeComponent();

            minimumHeight = this.Height;

            this.AutoSize = false;
            this.UseMnemonic = false;
        }

        /// <summary>
        /// Gets or sets the text rendering hint.
        /// </summary>
        /// <value><see cref="System.Drawing.Text.TextRenderingHint"/></value>
        public System.Drawing.Text.TextRenderingHint TextRenderingHint
        {
            get
            {
                return this.textRenderingHint;
            }
            set
            {
                this.textRenderingHint = value;
            }
        }
        
        /* dont include this until a later release
        /// <summary>
        /// Gets or sets a value that tells the control to not automatically expand
        /// </summary>
        /// <value><see cref="bool"/></value>
        public bool DontExpand
        {
            get
            {
                return this.dontExpand;
            }
            set
            {
                this.dontExpand = value;
            }
        }
         * */

        /// <summary>
        /// Fires the Paint event
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = this.textRenderingHint;
            base.OnPaint(e);
        }

        /// <summary>
        /// Handles the <c>LabelHeightChange</c> event
        /// </summary>
        /// <param name="args">The <see cref="LabelHeightChangedEventArgs"/> associated with the event</param>
        public delegate void LabelHeightChangedEventHandler(LabelHeightChangedEventArgs args);

        /// <summary>
        /// Fired when an expanding label's height is automatically changed (increased or decreased)
        /// to fit its contents
        /// </summary>
        public event LabelHeightChangedEventHandler LabelHeightChanged;

        /// <summary>
        /// Indicates if the label is in the process of changing its height.
        /// </summary>
        private bool bChanging;

        /// <summary>
        /// Returns the <see cref="Size"/> of a rectangle required to render the given text
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="font">The <see cref="Font"/> to use</param>
        /// <param name="desWidth">The desired with of the label</param>
        /// <param name="minHeight">The minimum height of the label (if the text does not expand beyond it)</param>
        /// <returns><see cref="Size"/> of the rectangle required to render the text</returns>
        private Size MeasureStringExtended(string text, Font font, int desWidth, int minHeight)
        {
            /* THIS ALMOST ALWAYS RESULTS IN A TOO-SMALL RECTANGLE
            Graphics g = this.CreateGraphics();
            g.TextRenderingHint = this.TextRenderingHint;
            StringFormat format = StringFormat.GenericTypographic;
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            format.FormatFlags |= StringFormatFlags.NoClip;
            format.FormatFlags |= StringFormatFlags.LineLimit;
            format.Trimming = StringTrimming.None;
            SizeF size = g.MeasureString(text, font, desWidth, format);

            int rem = 0;
            int lines = Math.DivRem(Convert.ToInt32(Math.Ceiling(size.Height)), font.Height, out rem);
            if (rem != 0) lines++;
            int adjustedHeight = lines * font.Height;
            size = new SizeF(size.Width, adjustedHeight);
            
            Size returnSize = size.ToSize();
            * */

            Size size = new Size(desWidth, Int32.MaxValue);
            TextFormatFlags flags = TextFormatFlags.Default | TextFormatFlags.ExternalLeading | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.NoClipping | TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.TextBoxControl;
            //Size textSize = TextRenderer.MeasureText(text, font, size, flags);
            //Size returnSize = new Size(textSize.Width, (int)(textSize.Height / scalingFactor));
            Size returnSize = TextRenderer.MeasureText(text, font, size, flags);

            // REMOVED 9.23.2011 - this part works, but we were always passing the current size (not minimum size), so it was never allowing the size to shrink once expanded
            if (returnSize.Height < minHeight)
                returnSize = new Size(returnSize.Width, minHeight);

            /*
            // HACK - actually, this probably is not needed anymore since the TextBoxControl flag should take care of things
            if (returnSize.Width > desWidth)
                returnSize = new Size(returnSize.Width, returnSize.Height + minHeight);
             * */

            return returnSize;
        }

        /// <summary>
        /// Performs an initialization of the control
        /// </summary>
        private void InitializeComponent()
        {
            bChanging = false;
        }

        /// <summary>
        /// Raises the <c>TextChanged</c> event
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data</param>
        protected override void OnTextChanged(EventArgs e)
        {
            // IMPORTANT: dont trigger this unless the event handler is hooked up or else the sizes will get out of sync.
            if(this.LabelHeightChanged != null && !dontExpand && !bChanging)
            {
                bChanging = true;
                int originalHeight = this.Height;

                int preferredWidth = this.ClientSize.Width;
                Size size = this.MeasureStringExtended(this.Text, this.Font, preferredWidth, minimumHeight);
                int newHeight = size.Height;
                this.Height = newHeight;

                LabelHeightChangedEventArgs args = new LabelHeightChangedEventArgs(originalHeight, newHeight);
                if (args.HeightChange != 0)
                {
                    this.LabelHeightChanged(args);
                }

                base.OnTextChanged(e);

                bChanging = false;
            }
            else
            {
                this.minimumHeight = this.Height;
                base.OnTextChanged(e);
            }
        }

        /// <summary>
        /// Provides data for the <see cref="LabelHeightChanged"/> event
        /// </summary>
        public class LabelHeightChangedEventArgs : System.EventArgs
        {
            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            /// <param name="originalHeight">The original height of the label</param>
            /// <param name="newHeight">The new height of the label</param>
            public LabelHeightChangedEventArgs(int originalHeight, int newHeight)
            {
                this.OriginalHeight = originalHeight;
                this.NewHeight = newHeight;
                this.HeightChange = newHeight - originalHeight;
            }

            /// <summary>
            /// The starting height of the label
            /// </summary>
            public int OriginalHeight;

            /// <summary>
            /// The new height of the label
            /// </summary>
            public int NewHeight;

            /// <summary>
            /// The height changed (positive or negative)
            /// </summary>
            public int HeightChange;
        }
    }
}
