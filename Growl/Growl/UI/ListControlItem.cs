using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.UI
{
    public class ListControlItem
    {
        private string text;
        private IRegisteredObject registeredObject;

        public ListControlItem(string text, IRegisteredObject registeredObject)
        {
            this.text = text;
            this.registeredObject = registeredObject;
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        public IRegisteredObject RegisteredObject
        {
            get
            {
                return this.registeredObject;
            }
            set
            {
                this.registeredObject = value;
            }
        }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
