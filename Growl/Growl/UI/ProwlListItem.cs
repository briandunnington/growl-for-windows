using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ProwlListItem : ForwardListItem
    {
        public ProwlListItem()
            : base(Properties.Resources.AddComputer_AddProwl, ForwardComputerPlatformType.IPhone.Icon, new ProwlForwardInputs())
        {
        }
    }
}