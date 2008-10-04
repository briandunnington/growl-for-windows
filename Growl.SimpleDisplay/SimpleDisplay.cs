using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.SimpleDisplay
{
    public class SimpleDisplay : Display
    {
        private const string SETTING_COLOR1 = "Color1";
        private const string SETTING_COLOR2 = "Color2";

        public static Color COLOR1 = Color.FromArgb(30, 102, 164);
        public static Color COLOR2 = Color.FromArgb(82, 167, 209);

        private List<SimpleWindow> activeWindows = new List<SimpleWindow>();


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
            win.Color1 = GetColorFromSetting(SETTING_COLOR1, COLOR1);
            win.Color2 = GetColorFromSetting(SETTING_COLOR2, COLOR2);

            Screen screen = Screen.FromControl(win);
            int x = screen.WorkingArea.Right - win.Size.Width;
            int y = screen.WorkingArea.Bottom - win.Size.Height;
            win.Location = new Point(x, y);

            win.Shown += new EventHandler(win_Shown);
            win.FormClosed += new FormClosedEventHandler(win_FormClosed);

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

        void win_Shown(object sender, EventArgs e)
        {
            SimpleWindow win = (SimpleWindow)sender;

            foreach (SimpleWindow sw in this.activeWindows)
            {
                sw.DesktopLocation = new Point(sw.DesktopLocation.X, sw.DesktopLocation.Y - win.Size.Height);
            }

            this.activeWindows.Add(win);
        }

        void win_FormClosed(object sender, FormClosedEventArgs e)
        {
            SimpleWindow win = (SimpleWindow)sender;
            this.activeWindows.Remove(win);
        }
    }
}
