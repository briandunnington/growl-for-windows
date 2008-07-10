using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vortex.Growl.DisplayStyle;

namespace Vortex.Growl.VisorDisplay
{
    public class VisorDisplay : Display
    {
        private const string SETTING_BGCOLOR = "BackgroundColor";

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
                return "Vortex Software";
            }
        }

        public override void Load()
        {
            base.Load();
        }

        public override void HandleNotification(Notification notification, string displayName)
        {
            VisorWindow vw = new VisorWindow();
            vw.SetNotification(notification);
            vw.BackColor = GetBgColor();
            vw.Show();
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
    }
}
