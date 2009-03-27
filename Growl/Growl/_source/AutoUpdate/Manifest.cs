using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Growl.AutoUpdate
{
    public class Manifest
    {
        private string version;
        private bool required;
        private string updateLocation;
        private string installerLocation;

        public static Manifest Parse(string data)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);

            XmlElement root = xml.DocumentElement;
            XmlElement versionNode = root["version"];
            XmlElement requiredNode = root["required"];
            XmlElement updateLocationNode = root["updateLocation"];
            XmlElement installerLocationNode = root["installerLocation"];

            string version = versionNode.InnerText;
            bool required = Convert.ToBoolean(requiredNode.InnerText);
            string updateLocation = updateLocationNode.InnerText;
            string installerLocation = installerLocationNode.InnerText;

            Manifest manifest = new Manifest(version, required, updateLocation, installerLocation);
            return manifest;
        }

        private Manifest() { }

        internal Manifest(string version, bool required, string updateLocation, string installerLocation)
        {
            this.version = version;
            this.required = required;
            this.updateLocation = updateLocation;
            this.installerLocation = installerLocation;
        }

        public string Version
        {
            get
            {
                return this.version;
            }
        }

        public bool Required
        {
            get
            {
                return this.required;
            }
        }

        public string UpdateLocation
        {
            get
            {
                return this.updateLocation;
            }
        }

        public string InstallerLocation
        {
            get
            {
                return this.installerLocation;
            }
        }

        public override string ToString()
        {
            XmlDocument xml = new XmlDocument();

            XmlElement version = xml.CreateElement("version");
            version.InnerText = this.Version;

            XmlElement required = xml.CreateElement("required");
            required.InnerText = this.Required.ToString();

            XmlElement updateLocation = xml.CreateElement("updateLocation");
            updateLocation.InnerText = this.UpdateLocation;

            XmlElement installerLocation = xml.CreateElement("installerLocation");
            installerLocation.InnerText = this.InstallerLocation;

            XmlElement manifest = xml.CreateElement("manifest");
            manifest.AppendChild(version);
            manifest.AppendChild(required);
            manifest.AppendChild(updateLocation);
            manifest.AppendChild(installerLocation);
            
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
            xml.AppendChild(manifest);

            return xml.OuterXml;
        }
    }
}
