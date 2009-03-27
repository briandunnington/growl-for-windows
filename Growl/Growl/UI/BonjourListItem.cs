using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public delegate void BonjourListItemSelectedEventHandler(DetectedService ds);

    public class BonjourListItem
    {
        public event BonjourListItemSelectedEventHandler Selected;

        private DetectedService ds;
        private string text;
        private Image image;

        public BonjourListItem(DetectedService ds)
        {
            this.ds = ds;
            this.text = ds.Service.Name;
            this.image = ds.Platform.Icon;
        }

        public BonjourListItem(string text, Image image)
        {
            this.text = text;
            this.image = image;
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

        public void Select()
        {
            if (this.Selected != null)
            {
                this.Selected(this.ds);
            }
        }
    }
}
