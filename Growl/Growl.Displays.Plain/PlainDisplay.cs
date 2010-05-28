using System;
using System.Collections.Generic;
using System.Text;
using Growl.DisplayStyle;

namespace Growl.Displays.Plain
{
    public class PlainDisplay : MultiMonitorVisualDisplay
    {
        public const string SETTING_DISPLAYLOCATION = "DisplayLocation";

        LayoutManager tllm = new LayoutManager(LayoutManager.AutoPositionDirection.DownRight, 6, 6);
        LayoutManager bllm = new LayoutManager(LayoutManager.AutoPositionDirection.UpRight, 6, 6);
        LayoutManager trlm = new LayoutManager(LayoutManager.AutoPositionDirection.DownLeft, 6, 6);
        LayoutManager brlm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 6, 6);

        public PlainDisplay()
            : base()
        {
            this.SettingsPanel = new PlainSettingsPanel();
        }

        public override string Author
        {
            get
            {
                return "Growl for Windows";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notifications in a simple white box";
            }
        }

        public override string Name
        {
            get 
            {
                return "Plain";
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

        protected override void HandleNotification(Notification notification, string displayName)
        {
            PlainWindow win = new PlainWindow();
            win.Tag = this;
            win.SetNotification(notification);
            win.SetDisplayLocation(GetLocationFromSetting());
            this.Show(win);
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

        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            PlainWindow pw = (PlainWindow)win;
            switch (pw.DisplayLocation)
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

        public enum Location
        {
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 3,
            BottomRight = 4
        }
    }
}
