using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    public class BonjourListItem : ForwardDestinationListItem
    {
        private DetectedService ds;

        public BonjourListItem(DetectedService ds, IForwardDestinationHandler ifdh) : base(ds.Service.Name, ds.Platform.Icon, ifdh)
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
