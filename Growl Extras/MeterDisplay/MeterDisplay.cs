using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace Meter
{
    public class MeterDisplay : MultiMonitorVisualDisplay
    {
        public const string SETTING_DISPLAYLOCATION = "DisplayLocation";

        LayoutManager tllm = new LayoutManager(LayoutManager.AutoPositionDirection.DownRight, 10, 10);
        LayoutManager bllm = new LayoutManager(LayoutManager.AutoPositionDirection.UpRight, 10, 10);
        LayoutManager trlm = new LayoutManager(LayoutManager.AutoPositionDirection.DownLeft, 10, 10);
        LayoutManager brlm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 10, 10);

        public MeterDisplay()
            : base()
        {
            this.SettingsPanel = new MeterSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Meter";
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
                return "Brian Dunnington";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notification information in a progress-bar like form.";
            }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            bool replace = false;
            if (!String.IsNullOrEmpty(notification.CoalescingGroup))
            {
                foreach (NotificationWindow nw in this.ActiveWindows)
                {
                    if (nw.CoalescingGroup == notification.CoalescingGroup)
                    {
                        ((MeterWindow)nw).Replace(notification);
                        replace = true;
                        break;
                    }
                }
            }

            if (!replace)
            {
                MeterWindow win = new MeterWindow();
                win.Tag = this;
                win.SetNotification(notification);
                win.SetDisplayLocation(GetLocationFromSetting());
                this.Show(win);
            }
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow nw)
        {
            MeterWindow win = (MeterWindow)nw;
            switch (win.DisplayLocation)
            {
                case Location.TopLeft:
                    return tllm;
                case Location.BottomLeft:
                    return bllm;
                case Location.TopRight:
                    return trlm;
                default:
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
