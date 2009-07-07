using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class BonjourListItem : ForwardListItem
    {
        private DetectedService ds;

        public BonjourListItem(DetectedService ds) : base(ds.Service.Name, ds.Platform.Icon)
        {
            this.ds = ds;
        }

        public DetectedService DetectedService
        {
            get
            {
                return this.ds;
            }
        }
    }
}
