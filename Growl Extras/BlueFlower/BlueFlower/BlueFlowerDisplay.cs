using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace BlueFlower
{
    public class BlueFlowerDisplay : MultiMonitorVisualDisplay
    {
        public const string SETTING_DISPLAYLOCATION = "DisplayLocation";

        LayoutManager tllm = new LayoutManager(LayoutManager.AutoPositionDirection.DownRight, 0, 0);
        LayoutManager bllm = new LayoutManager(LayoutManager.AutoPositionDirection.UpRight, 0, 0);
        LayoutManager trlm = new LayoutManager(LayoutManager.AutoPositionDirection.DownLeft, 0, 0);
        LayoutManager brlm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 0, 0);

        public BlueFlowerDisplay()
            : base()
        {
            this.SettingsPanel = new BlueFlowerSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Blue Flower";
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
                return "Displays notifications with a soothing blue hue";
            }
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            BlueFlowerWindow win = new BlueFlowerWindow();
            win.Tag = this;
            win.SetNotification(notification);
            win.SetDisplayLocation(GetLocationFromSetting());
            this.Show(win);
        }

        protected override LayoutManager GetLayoutManager(NotificationWindow nw)
        {
            BlueFlowerWindow win = (BlueFlowerWindow)nw;
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
