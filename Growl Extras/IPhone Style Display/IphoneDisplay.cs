using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace IphoneStyle
{
    public class IphoneDisplay : MultiMonitorVisualDisplay
    {
        public const string SETTING_DISPLAYLOCATION = "DisplayLocation";

        LayoutManager tllm = new LayoutManager(LayoutManager.AutoPositionDirection.DownRight, 0, 0);
        LayoutManager bllm = new LayoutManager(LayoutManager.AutoPositionDirection.UpRight, 0, 0);
        LayoutManager trlm = new LayoutManager(LayoutManager.AutoPositionDirection.DownLeft, 0, 0);
        LayoutManager brlm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 0, 0);

        public IphoneDisplay()
            : base()
        {
            this.SettingsPanel = new IphoneSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "iPhone Style";
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
                return "Displays notifications using an iPhone-like alert.";
            }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            IphoneWindow win = new IphoneWindow();
            win.Tag = this;
            win.SetNotification(notification);
            win.SetDisplayLocation(GetLocationFromSetting());
            this.Show(win);
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow nw)
        {
            IphoneWindow win = (IphoneWindow)nw;
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
