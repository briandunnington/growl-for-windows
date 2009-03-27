using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Standard
{
    public class StandardDisplay : VisualDisplay
    {
        public const string DEFAULT_DISPLAY_NAME = "Standard";
        public const string SETTING_COLOR1 = "Color1";
        public const string SETTING_COLOR2 = "Color2";

        public static Color COLOR1 = Color.FromArgb(28, 91, 149);
        public static Color COLOR2 = Color.FromArgb(53, 152, 200);

        //Sound sound = new Sound("SystemNotification");

        LayoutManager lm = new LayoutManager(LayoutManager.AutoPositionDirection.UpLeft, 10, 10);

        public StandardDisplay()
        {
            this.SettingsPanel = new StandardSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Standard";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notifications as a small window near the system tray.";
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

        protected override LayoutManager GetLayoutManager(NotificationWindow win)
        {
            return this.lm;
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            StandardWindow win = new StandardWindow();
            win.SetNotification(notification);
            win.Color1 = GetColorFromSetting(SETTING_COLOR1, COLOR1);
            win.Color2 = GetColorFromSetting(SETTING_COLOR2, COLOR2);

            this.Show(win);

            //this.sound.Play();
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
