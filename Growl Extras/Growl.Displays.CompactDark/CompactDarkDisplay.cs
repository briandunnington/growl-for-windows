using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.CompactDark
{
    public class CompactDarkDisplay : VisualDisplay
    {
        LayoutManager lm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 5, 5);

        public override string Name
        {
            get { return "CompactDark"; }
        }

        public override string Description
        {
            get { return "Displays notifications in a compact, dark-colored alert in the corner of the screen."; }
        }

        public override string Author
        {
            get { return "Growl for Windows"; }
        }

        public override string Website
        {
            get { return "http://www.growlforwindows.com/displays/compactdark"; }
        }

        public override string Version
        {
            get
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);
                return f.FileVersion;
            }
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return this.lm;
        }


        protected override void HandleNotification(Notification notification, string displayName)
        {
            CompactDarkWindow win = new CompactDarkWindow();
            win.SetNotification(notification);
            this.Show(win);
            //win.FormClosed += new FormClosedEventHandler(win_FormClosed);
/*
            ToastWindow win = new ToastWindow();
            win.FormClosed += new FormClosedEventHandler(win_FormClosed);
            win.SetNotification(notification);

            this.queuedNotifications.Enqueue(win);
            WorkQueue();
 */
        }

    }
}
