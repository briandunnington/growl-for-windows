using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vortex.Growl.DisplayStyle;

namespace Vortex.Growl.SimpleDisplay
{
    public class SimpleDisplay : Display
    {
        private const string SETTING_COLOR1 = "Color1";
        private const string SETTING_COLOR2 = "Color2";

        public SimpleDisplay()
        {
            this.SettingsPanel = new SimpleSettingsPanel();
        }

        public override string Name
        {
            get
            {
                return "Simple";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notifications in a simple notification box.";
            }
        }

        public override string Author
        {
            get
            {
                return "Vortex Software";
            }
        }

        public override void Load()
        {
            base.Load();
        }

        public override void HandleNotification(Notification notification, string displayName)
        {
            SimpleWindow win = new SimpleWindow();
            win.SetNotification(notification);
            win.Color1 = GetColorFromSetting(SETTING_COLOR1, Color.SkyBlue);
            win.Color2 = GetColorFromSetting(SETTING_COLOR2, Color.White);
            win.Show();
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
