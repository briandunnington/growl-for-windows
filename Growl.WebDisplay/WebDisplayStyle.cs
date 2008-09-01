using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Growl.WebDisplay
{
    internal class WebDisplayStyle
    {
        private string name;
        private static string templateFileName;
        private string templatePath;
        private string baseUrl;
        private string templateHTML;

        public WebDisplayStyle(DirectoryInfo d)
        {
            this.name = d.Name;
            this.templatePath = String.Format(@"{0}\Resources\{1}", d.FullName, TemplateFileName);
            this.templateHTML = ReadTemplate();

            System.Uri baseUri = new Uri(this.templatePath);
            this.baseUrl = baseUri.ToString().Replace(TemplateFileName, "");
        }

        public static string TemplateFileName
        {
            get
            {
                return templateFileName;
            }
            set
            {
                templateFileName = value;
            }
        }

        public string TemplatePath
        {
            get
            {
                return this.templatePath;
            }
        }

        public string TemplateHTML
        {
            get
            {
                return this.templateHTML;
            }
        }

        public string BaseUrl
        {
            get
            {
                return this.baseUrl;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        private string ReadTemplate()
        {
            string path = this.TemplatePath;
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            string data = reader.ReadToEnd();
            reader.Close();
            stream.Close();
            return data;
        }
    }
}
