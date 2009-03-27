using System;
using System.Windows.Forms;

namespace ITunesPluginApp
{
    internal class InvisibleForm : Form
    {
        public InvisibleForm()
        {
            // default these so that the form is initially invisible until our ShowForm is called
            this.Opacity = 0;
            this.ShowInTaskbar = false;
        }
    }
}
