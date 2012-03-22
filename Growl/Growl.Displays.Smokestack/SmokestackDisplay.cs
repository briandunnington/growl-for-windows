using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Smokestack
{
    public class SmokestackDisplay : MultiMonitorVisualDisplay
    {
        public const string SETTING_DISPLAYLOCATION = "DisplayLocation";

        LayoutManager tllm = new LayoutManager(LayoutManager.AutoPositionDirection.DownRight, 6, 6);
        LayoutManager bllm = new LayoutManager(LayoutManager.AutoPositionDirection.UpRight, 6, 6);
        LayoutManager trlm = new LayoutManager(LayoutManager.AutoPositionDirection.DownLeft, 6, 6);
        LayoutManager brlm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 6, 6);

        public SmokestackDisplay()
        {
            this.SettingsPanel = new SmokestackSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Smokestack";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notifications as a bubble in any corner of the screen.";
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
            if (!String.IsNullOrEmpty(notification.CoalescingGroup))
            {
                foreach (NotificationWindow nw in this.ActiveWindows)
                {
                    if (nw.CoalescingGroup == notification.CoalescingGroup)
                    {
                        ((SmokestackWindow)nw).Close(true);
                        break;
                    }
                }
            }

            SmokestackWindow win = new SmokestackWindow();
            win.Tag = this;
            win.SetNotification(notification);
            win.SetDisplayLocation(GetLocationFromSetting());
            this.Show(win);
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            SmokestackWindow sw = (SmokestackWindow)win;
            switch (sw.DisplayLocation)
            {
                case Location.TopLeft :
                    return tllm;
                case Location.BottomLeft :
                    return bllm;
                case Location.TopRight :
                    return trlm;
                default :
                    return brlm;
            }
        }

        private Location GetLocationFromSetting()
        {
            Location location = Location.TopRight;
            if (this.SettingsCollection != null && this.SettingsCollection.ContainsKey(SETTING_DISPLAYLOCATION))
            {
                try
                {
                    object val = this.SettingsCollection[SETTING_DISPLAYLOCATION];
                    if (val != null)
                    {
                        location = (Location)val;
                    }
                }
                catch
                {
                }
            }
            return location;
        }

        public enum Location
        {
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 3,
            BottomRight = 4
        }
    }
}
