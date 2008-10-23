using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    public class NotificationWindow : Form
    {
        public NotificationWindow()
        {
            // dont show in taskbar
            this.ShowInTaskbar = false;

            // always show on top of other windows
            this.TopMost = true;
        }

        public new virtual void Show()
        {
            User32DLL.ShowWindow(this.Handle, User32DLL.SW_SHOWNOACTIVATE);
            OnShown(EventArgs.Empty);
        }
    }
}
