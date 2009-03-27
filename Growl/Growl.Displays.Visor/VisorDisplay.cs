using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Visor
{
    public class VisorDisplay : VisualDisplay
    {
        public const string SETTING_BGCOLOR = "BackgroundColor";

        public static Color BG_COLOR = Color.FromArgb(69, 69, 69);

        private Queue<VisorWindow> queuedNotifications = new Queue<VisorWindow>();
        private bool isVisible;

        public VisorDisplay()
        {
            this.SettingsPanel = new VisorSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Visor";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notifications in a slide-down window across the top of the screen.";
            }
        }

        public override string Author
        {
            get
            {
                return "Growl for Windows";
            }
        }

        public override string Website
        {
            get
            {
                return "http://www.growlforwindows.com";
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

        public override void Load()
        {
            base.Load();
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            VisorWindow win = new VisorWindow();
            win.BackColor = GetBgColor();
            win.SetNotification(notification);

            win.FormClosed += new FormClosedEventHandler(win_FormClosed);

            this.queuedNotifications.Enqueue(win);
            WorkQueue();
        }

        private Color GetBgColor()
        {
            Color bgColor = Color.Black;
            if (this.SettingsCollection != null && this.SettingsCollection.ContainsKey(SETTING_BGCOLOR))
            {
                try
                {
                    object val = this.SettingsCollection[SETTING_BGCOLOR];
                    if (val is Color)
                    {
                        bgColor = (Color)val;
                    }
                }
                catch
                {
                }
            }
            return bgColor;
        }

        void WorkQueue()
        {
            if (!isVisible && this.queuedNotifications.Count > 0)
            {
                VisorWindow win = this.queuedNotifications.Dequeue();
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
