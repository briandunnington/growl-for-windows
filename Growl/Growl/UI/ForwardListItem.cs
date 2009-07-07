using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl.UI
{
    public class ForwardListItem
    {
        public event EventHandler Selected;

        private string text;
        private Image image;
        private IForwardInputs inputs;

        public ForwardListItem(string text, Image image)
            : this(text, image, null)
        {
        }

        public ForwardListItem(string text, Image image, IForwardInputs inputs)
        {
            this.text = text;
            this.image = image;
            if (inputs != null)
                this.inputs = inputs;
            else
                this.inputs = new ForwardComputerInputs();
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

        public IForwardInputs Inputs
        {
            get
            {
                return this.inputs;
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
