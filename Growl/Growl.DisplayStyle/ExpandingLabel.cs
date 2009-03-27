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
        private static char[] BREAK_CHARS = new char[] { ' ', ';' };

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ExpandingLabel()
        {
            InitializeComponent();

            this.AutoSize = false;
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
            Graphics g = this.CreateGraphics();
            SizeF size = g.MeasureString(text, font, desWidth, StringFormat.GenericDefault);
            Size returnSize = size.ToSize();

            if (returnSize.Height < minHeight)
                returnSize = new Size(returnSize.Width, minHeight);

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
            if (!bChanging)
            {
                bChanging = true;
                int originalHeight = this.Height;

                int preferredWidth = this.ClientSize.Width;
                Size size = this.MeasureStringExtended(this.Text, this.Font, preferredWidth, this.Height);
                int newHeight = size.Height;
                this.Height = newHeight;

                LabelHeightChangedEventArgs args = new LabelHeightChangedEventArgs(originalHeight, newHeight);
                if (args.HeightChange != 0 && this.LabelHeightChanged != null)
                {
                    this.LabelHeightChanged(args);
                }

                base.OnTextChanged(e);

                bChanging = false;
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
