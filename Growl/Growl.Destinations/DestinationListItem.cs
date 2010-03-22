using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Represents an item in the list of choices displayed to the user when adding a new
    /// forwarder or subscription.
    /// </summary>
    public class DestinationListItem
    {
        /// <summary>
        /// Occurs when the item is selected.
        /// </summary>
        public event EventHandler Selected;

        /// <summary>
        /// The text of the item
        /// </summary>
        private string text;

        /// <summary>
        /// The icon of the item
        /// </summary>
        private Image image;

        /// <summary>
        /// The <see cref="IDestinationHandler"/> that manages the <see cref="DestinationBase"/>
        /// that this item represents.
        /// </summary>
        private IDestinationHandler idh;


        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationListItem"/> class.
        /// </summary>
        /// <param name="text">The text of the item.</param>
        /// <param name="image">The icon of the item.</param>
        /// <param name="idh">The <see cref="IDestinationHandler"/> that manages the <see cref="DestinationBase"/> hat this item represents.</param>
        public DestinationListItem(string text, Image image, IDestinationHandler idh)
        {
            this.text = text;
            this.image = image;
            this.idh = idh;
        }

        /// <summary>
        /// Gets the text of the item.
        /// </summary>
        /// <value>string</value>
        public string Text
        {
            get
            {
                return this.text;
            }
        }

        /// <summary>
        /// Gets the icon of the item.
        /// </summary>
        /// <value><see cref="System.Drawing.Image"/></value>
        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDestinationHandler"/> that manages the <see cref="DestinationBase"/>
        /// that this item represents.
        /// </summary>
        /// <value><see cref="IDestinationHandler"/></value>
        public IDestinationHandler Handler
        {
            get
            {
                return this.idh;
            }
        }

        /// <summary>
        /// Selects the item.
        /// </summary>
        public void Select()
        {
            this.OnSelect();
        }

        /// <summary>
        /// Called when the item is selected.
        /// </summary>
        protected virtual void OnSelect()
        {
            if (this.Selected != null)
            {
                this.Selected(this, EventArgs.Empty);
            }
        }
    }
}
