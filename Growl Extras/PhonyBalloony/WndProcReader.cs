using System;
using System.Windows.Forms;

namespace GrowlExtras.PhonyBalloony
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
            this.CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            // let the delegate process the message
            if (this.del != null) this.del(ref m);

            // if the delegate didnt set the result, let the message pass through to the base method.
            // otherwise, we assume the delegate set the result to an error value and return that instead.
            if(m.Result == IntPtr.Zero)
                base.WndProc(ref m);
        }
    }
}
