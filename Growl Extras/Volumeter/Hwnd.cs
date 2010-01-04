using System;
using System.Windows.Forms;

namespace Volumeter
{
    class Hwnd : NativeWindow
    {
        public delegate void WndProcDelegate(ref Message m);

        CreateParams cp = new CreateParams();
        WndProcDelegate del;

        public Hwnd(WndProcDelegate del)
        {
            this.del = del;

            // Create the actual window
            this.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            if (this.del != null) this.del(ref m);
            base.WndProc(ref m);
        }
    }
}
