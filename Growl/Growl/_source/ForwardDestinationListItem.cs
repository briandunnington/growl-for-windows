using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl
{
    public class ForwardDestinationListItem
    {
        public event EventHandler Selected;

        private string text;
        private Image image;
        private IForwardDestinationHandler ifdh;

        public ForwardDestinationListItem(string text, Image image, IForwardDestinationHandler ifdh)
        {
            this.text = text;
            this.image = image;
            this.ifdh = ifdh;
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

        public IForwardDestinationHandler Handler
        {
            get
            {
                return this.ifdh;
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
