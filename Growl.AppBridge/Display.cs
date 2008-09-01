using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using Growl.Framework;
using Growl;

namespace Growl.AppBridge
{
    [Serializable]
    public class Display : DefaultablePreference, ISerializable
    {
        public static Display Default;
        private Growl.DisplayStyle.IDisplay display;

        static Display()
        {
            // use a special instance of the WebDisplay as the default fallback
            WebDisplay.WebDisplay webDisplay = new WebDisplay.WebDisplay(true);
            webDisplay.GrowlApplicationPath = Application.StartupPath;
            webDisplay.DisplayStylePath = webDisplay.WebDisplayDirectory;
            webDisplay.Load();

            Default = new Display(WebDisplay.WebDisplay.DEFAULT_DISPLAY_NAME, webDisplay, true);
        }

        public Display(string name, Growl.DisplayStyle.IDisplay display) : this(name, display, false)
        {
        }

        private Display(string name, Growl.DisplayStyle.IDisplay display, bool isDefault)
        {
            this.name = name;
            this.isDefault = isDefault;
            this.display = display;
        }

        protected Display(SerializationInfo info, StreamingContext context)
        {
            string name = info.GetString("name");
            bool isDefault = info.GetBoolean("isDefault");
            string directory = info.GetString("directory");
            LoadedDisplayStyle lds = DisplayStyleManager.FindDisplayStyle(directory);

            this.name = name;
            this.isDefault = isDefault;
            this.display = (lds != null ? lds.Display : null);
        }

        public void HandleNotification(ReceivedRegistration rr)
        {
            try
            {
                this.display.HandleNotification(rr, this.name);
            }
            catch
            {
                // suppress any exceptions here (in case the display fails for some reason)
            }
        }

        public void HandleNotification(ReceivedNotification rn)
        {
            try
            {
                this.display.HandleNotification(rn, this.name);
            }
            catch
            {
                // suppress any exceptions here (in case the display fails for some reason)
            }
        }

        public Growl.DisplayStyle.SettingsPanelBase SettingsPanel
        {
            get
            {
                return DisplayStyleManager.GetSettingsPanel(this.display.DisplayStylePath);
            }
        }

        public Dictionary<string, object> SettingsCollection
        {
            set
            {
                this.display.SettingsCollection = value;
            }
        }

        public string Description
        {
            get
            {
                return this.display.Description;
            }
        }

        public string Author
        {
            get
            {
                return this.display.Author;
            }
        }

        public string Version
        {
            get
            {
                return this.display.Version.ToString();
            }
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", this.name);
            info.AddValue("isDefault", this.isDefault);
            info.AddValue("directory", this.display.DisplayStylePath);
        }

        #endregion
    }
}
