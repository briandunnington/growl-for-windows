using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Toast
{
    public class ToastDisplay : VisualDisplay
    {
        private Queue<ToastWindow> queuedNotifications = new Queue<ToastWindow>();
        private bool isVisible = false;

        //LayoutManager lm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 5, 5);

        public override string Name
        {
            get { return "Toast"; }
        }

        public override string Description
        {
            get { return "Pops up notifications like toast from a toaster."; }
        }

        public override string Author
        {
            get { return "Growl for Windows"; }
        }

        public override string Website
        {
            get { return "http://www.growlforwindows.com"; }
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

        protected override void HandleNotification(Notification notification, string displayName)
        {
            ToastWindow win = new ToastWindow();
            win.FormClosed += new FormClosedEventHandler(win_FormClosed);
            win.SetNotification(notification);

            this.queuedNotifications.Enqueue(win);
            WorkQueue();
        }

        void WorkQueue()
        {
            if (!isVisible && this.queuedNotifications.Count > 0)
            {
                ToastWindow win = this.queuedNotifications.Dequeue();
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

        /*
        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return this.lm;
        }
         * */
    }
}
