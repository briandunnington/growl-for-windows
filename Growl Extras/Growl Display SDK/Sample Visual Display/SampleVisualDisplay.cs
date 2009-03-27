using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using Growl.DisplayStyle;

namespace Sample_Visual_Display
{
    public class SampleVisualDisplay : VisualDisplay
    {
        /// <summary>
        /// By using the built-in LayoutManager, it will automatically handle repositioning
        /// any already-on-screen notifications when a new notification is received
        /// </summary>
        LayoutManager lm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 10, 10);

        public const string SETTING_BGCOLOR = "BgColor";
        public static Color BGCOLOR = Color.FromArgb(255, 228, 4);

        /// <summary>
        /// This is the name that will appear in the Growl client to identify this display
        /// </summary>
        public override string Name
        {
            get { return "SDK Visual Sample"; }
        }

        /// <summary>
        /// This description will appear in the Growl client to describe what this display does.
        /// It should be no longer than about 20 or 30 words.
        /// </summary>
        public override string Description
        {
            get { return "SDK Visual Sample - fades notifications in and out"; }
        }

        public override string Author
        {
            get { return "Growl Display SDK"; }
        }

        /// <summary>
        /// This demonstrates a more dynamic method of returning the version information.
        /// </summary>
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
            get { return "http://www.growlforwindows.com"; }
        }

        /// <summary>
        /// This is where we actually deal with the notification. The Growl client will call this method
        /// when a notification is received that is configured to use this display.
        /// </summary>
        protected override void HandleNotification(Notification notification, string displayName)
        {
            // 1. create your notification window
            // 2. call SetNotification to set the title, description, image, etc
            // 3. set any other custom settings
            // 4. call this.Show(win) - NOTE: you should always call the display's Show method
            //    rather than calling win.Show() directly. this ensures that all necessary events
            //    for click callbacks and layout management.
            SampleVisualWindow win = new SampleVisualWindow();
            win.SetNotification(notification);
            win.BackColor = GetColorFromSetting(SETTING_BGCOLOR, BGCOLOR);
            this.Show(win);
        }

        /// <summary>
        /// By using the built-in LayoutManager, it will automatically handle repositioning
        /// any already-on-screen notifications when a new notification is received
        /// 
        /// If you want to do your own layout management, simply don't override this method
        /// or set it to return null.
        /// </summary>
        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return this.lm;
        }

        private Color GetColorFromSetting(string settingName, Color defaultColor)
        {
            Color color = defaultColor;
            if (this.SettingsCollection != null && this.SettingsCollection.ContainsKey(settingName))
            {
                try
                {
                    object val = this.SettingsCollection[settingName];
                    if (val is Color)
                    {
                        color = (Color)val;
                    }
                }
                catch
                {
                }
            }
            return color;
        }
    }
}
