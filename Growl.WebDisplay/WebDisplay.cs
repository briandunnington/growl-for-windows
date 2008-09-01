using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.WebDisplay
{
    public class WebDisplay : Growl.DisplayStyle.Display
    {
        public const string DEFAULT_DISPLAY_NAME = "Standard";
        private const string TEMPLATE_FILENAME = "template.html";
        private bool defaultOnly = false;
        private Dictionary<string, WebDisplayStyle> webDisplayStyles = new Dictionary<string, WebDisplayStyle>();
        private List<NotificationWindow> activeWindows = new List<NotificationWindow>();

        public WebDisplay()
        {
        }

        public WebDisplay(bool defaultOnly)
        {
            this.defaultOnly = defaultOnly;
        }

        public string WebDisplayDirectory
        {
            get
            {
                string path = String.Format(@"{0}\Displays\WebDisplay", this.GrowlApplicationPath);
                return path;
            }
        }

        public string StylesPath
        {
            get
            {
                string path = String.Format(@"{0}\Styles\", this.WebDisplayDirectory);
                return path;
            }
        }

        private string GetTemplate(string styleName, out string baseUrl)
        {
            string template = "";
            baseUrl = null;
            if (this.webDisplayStyles.ContainsKey(styleName))
            {
                WebDisplayStyle wds = this.webDisplayStyles[styleName];
                template = wds.TemplateHTML;
                baseUrl = wds.BaseUrl;
            }
            return template;
        }

        private string Merge(Notification n, string displayName, out string baseUrl)
        {
            string template = GetTemplate(displayName, out baseUrl);

            // TODO: allow handling of line breaks to be configured
            //string text = rn.Description;
            //if (this.ConvertLineBreaks) text = text.Replace("\n", "<br>");
            string text = n.Description.Replace("\n", "<br>");

            // TODO: replace placeholders with data from rn
            MergeFields fields = new MergeFields(true);
            fields.ApplicationName.Value = n.ApplicationName;
            fields.BaseUrl.Value = baseUrl;
            fields.Opacity.Value = ""; // TODO:
            fields.Priority.Value = n.Priority.ToString(); // TODO:
            fields.Image.Value = ""; // TODO:
            fields.Title.Value = n.Title;
            fields.Text.Value = text;
            return Merge(template, fields);
        }

        private static string Merge(string template, MergeFields fields)
        {
            string output = template;
            output = output.Replace(fields.ApplicationName.Key, fields.ApplicationName.Value);
            output = output.Replace(fields.BaseUrl.Key, fields.BaseUrl.Value);
            output = output.Replace(fields.Opacity.Key, fields.Opacity.Value);
            output = output.Replace(fields.Priority.Key, fields.Priority.Value);
            output = output.Replace(fields.Image.Key, fields.Image.Value);
            output = output.Replace(fields.Title.Key, fields.Title.Value);
            output = output.Replace(fields.Text.Key, fields.Text.Value);
            return output;
        }

        #region ISerializable Members

        #endregion

        private struct MergeFields
        {
            public MergeFields(bool fake)
            {
                this.ApplicationName = new MergeField("%applicationname%");
                this.BaseUrl = new MergeField("%baseurl%");
                this.Opacity = new MergeField("%opacity%");
                this.Priority = new MergeField("%priority%");
                this.Image = new MergeField("%image%");
                this.Title = new MergeField("%title%");
                this.Text = new MergeField("%text%");
            }

            public MergeField ApplicationName;
            public MergeField BaseUrl;
            public MergeField Opacity;
            public MergeField Priority;
            public MergeField Image;
            public MergeField Title;
            public MergeField Text;
        }

        private struct MergeField
        {
            public MergeField(string key)
            {
                this.Key = key;
                this.Value = "";
            }
            public string Key;
            public string Value;
        }

        /*
         * 
         %baseurl% 
         A file:// URL that points to the Resources directory of your style bundle. 
         %opacity% 
         The user's opacity setting. 
         %priority% 
         The priority name. 
         %image% 
         A unique identifier for the notification image. In order to display the image, use <img src="growlimage://%image%" />. 
         %title% 
         The notification title. 
         %text% 
         The notification text. 
         * */

        #region IDisplay Members

        public override string Name
        {
            get
            {
                return "WebKit Display";
            }
        }

        public override string Description
        {
            get
            {
                return "Displays notificaitons using an HTML rendering engine.";
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
            WebDisplayStyle.TemplateFileName = TEMPLATE_FILENAME;

            string[] stylesDirectories = Directory.GetDirectories(this.StylesPath);
            foreach(string name in stylesDirectories)
            {
                DirectoryInfo d = new DirectoryInfo(name);
                if (!this.defaultOnly || d.Name == DEFAULT_DISPLAY_NAME)
                {
                    WebDisplayStyle wds = new WebDisplayStyle(d);
                    webDisplayStyles.Add(wds.Name, wds);
                }
            }
        }

        public override void HandleNotification(Notification notification, string displayName)
        {
            string baseUrl;
            string html = Merge(notification, displayName, out baseUrl);
            
            NotificationWindow nw = new NotificationWindow();
            nw.TopMost = true;
            nw.Sticky = notification.Sticky;
            nw.SetHtml(html, baseUrl);
            nw.Size = new Size(320, 110);
            Screen screen = Screen.FromControl(nw);
            int x = screen.WorkingArea.Right - nw.Size.Width;
            int y = screen.WorkingArea.Bottom - nw.Size.Height;
            nw.DesktopLocation = new Point(x, y);
            nw.Shown += new EventHandler(nw_Shown);
            nw.FormClosed += new FormClosedEventHandler(nw_FormClosed);
            nw.Show();
        }

        void nw_Shown(object sender, EventArgs e)
        {
            NotificationWindow nw = (NotificationWindow)sender;
             
            foreach (NotificationWindow enw in this.activeWindows)
            {
                enw.DesktopLocation = new Point(enw.DesktopLocation.X, enw.DesktopLocation.Y - nw.Size.Height);
            }

           this.activeWindows.Add(nw);
         }

        void nw_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificationWindow nw = (NotificationWindow)sender;
            this.activeWindows.Remove(nw);

            foreach (NotificationWindow enw in this.activeWindows)
            {
               enw.DesktopLocation = new Point(enw.DesktopLocation.X, enw.DesktopLocation.Y + nw.Size.Height);
            }
        }

        public override string[] GetListOfAvailableDisplays()
        {
            List<string> styleList = new List<string>();
            foreach (WebDisplayStyle wds in webDisplayStyles.Values)
            {
                styleList.Add(wds.Name);
            }
            styleList.Sort();
            return styleList.ToArray();
        }

        #endregion
    }
}
