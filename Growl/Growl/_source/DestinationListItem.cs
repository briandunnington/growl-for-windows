using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl
{
    public class DestinationListItem
    {
        public event EventHandler Selected;

        private string text;
        private Image image;
        private IDestinationHandler idh;

        public DestinationListItem(string text, Image image, IDestinationHandler idh)
        {
            this.text = text;
            this.image = image;
            this.idh = idh;
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        public IDestinationHandler Handler
        {
            get
            {
                return this.idh;
            }
        }

        public void Select()
        {
            this.OnSelect();
        }

        protected virtual void OnSelect()
        {
            if (this.Selected != null)
            {
                this.Selected(this, EventArgs.Empty);
            }
        }
    }
}
