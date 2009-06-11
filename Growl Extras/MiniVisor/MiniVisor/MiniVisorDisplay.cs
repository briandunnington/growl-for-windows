using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace MiniVisor
{
    public class MiniVisorDisplay : VisualDisplay
    {
        private Queue<MiniVisorWindow> queuedNotifications = new Queue<MiniVisorWindow>();
        private bool isVisible;

        public override string Name
        {
            get
            {
                return "MiniVisor";
            }
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

        public override string Website
        {
            get
            {
                return "http://www.growlforwindows.com";
            }
        }

        public override string Author
        {
            get
            {
                return "Rory O'Kelly";
            }
        }

        public override string Description
        {
            get
            {
                return "A smaller, more compact version of Visor";
            }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            MiniVisorWindow win = new MiniVisorWindow();
            win.SetNotification(notification);
            win.FormClosed += new FormClosedEventHandler(win_FormClosed);

            this.queuedNotifications.Enqueue(win);
            WorkQueue();
        }

        void WorkQueue()
        {
            if (!isVisible && this.queuedNotifications.Count > 0)
            {
                MiniVisorWindow win = this.queuedNotifications.Dequeue();
                this.Show(win);
                isVisible = true;
            }
        }

        void win_FormClosed(object sender, FormClosedEventArgs e)
        {
            isVisible = false;
            WorkQueue();
        }

        public override void CloseAllOpenNotifications()
        {
            lock (this.queuedNotifications)
            {
                this.queuedNotifications.Clear();
            }
            base.CloseAllOpenNotifications();
        }
    }
}
