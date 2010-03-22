using System;
using System.Windows.Forms;

namespace Growl
{
    class WndProcReader : NativeWindow
    {
        public delegate void WndProcDelegate(ref Message m);

        CreateParams cp = new CreateParams();
        WndProcDelegate del;

        public WndProcReader(WndProcDelegate del)
        {
            this.del = del;

            // Create the actual window
            base.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            if (this.del != null) this.del(ref m);
            base.WndProc(ref m);
        }
    }
}
