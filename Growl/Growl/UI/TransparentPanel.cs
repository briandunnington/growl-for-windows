using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Growl.UI
{
    public class TransparentPanel : System.Windows.Forms.Panel
    {
        public TransparentPanel()
        {
        }

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return createParams;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do not paint background.
        }
    }

}