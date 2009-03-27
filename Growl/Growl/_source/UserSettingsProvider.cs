using System;
using System.Configuration;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Xml;

namespace Growl
{
    public class UserSettingsProvider : SettingsProvider
    {
        private const string USER_SETTINGS_SECTION_NAME = "userSettings";
        string path = SettingSaver.GetPath("user.config");
        string pathAlt = SettingSaver.GetPathAlt("user.config");
        XmlEscaper escaper;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name)) name = "UserSettingsProvider";
            base.Initialize(name, config);
        }

        public override string ApplicationName
        {
            get
            {
                return Application.ProductName;
            }
            set
            {
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            // set all of the inherited default values first in case we have failure later
            SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();
            foreach (SettingsProperty prop in collection)
            {
                SettingsPropertyValue spv = new SettingsPropertyValue(prop);
                spv.SerializedValue = prop.DefaultValue;
                settings.Add(spv);
            }

            // now read in overridden user settings
            try
            {
                Configuration config = null;
                ClientSettingsSection clientSettings = GetUserSettings(out config, true);
                foreach (SettingsPropertyValue spv in settings)
                {
                    DeserializeFromXmlElement(spv.Property, spv, clientSettings);
                }
            }
            catch
            {
                // suppress
            }

            return settings;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            try
            {
                Configuration config = null;
                ClientSettingsSection clientSettings = GetUserSettings(out config, false);
                clientSettings.Settings.Clear();
                foreach (SettingsPropertyValue spv in collection)
                {
                    SettingValueElement sve = new SettingValueElement();
                    sve.ValueXml = SerializeToXmlElement(spv.Property, spv);
                    SettingElement se = new SettingElement(spv.Name, spv.Property.SerializeAs);
                    se.Value = sve;
                    clientSettings.Settings.Add(se);
                }
                config.Save();
            }
            catch
            {
                // suppress
            }
        }

        private ClientSettingsSection GetUserSettings(out Configuration config, bool allowAltPath)
        {
            string p = this.path;
            if (!System.IO.File.Exists(p) && allowAltPath) p = this.pathAlt;

            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = p;
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            ConfigurationSection configSection = config.GetSection(USER_SETTINGS_SECTION_NAME);
            ClientSettingsSection clientSettings = null;
            if (configSection != null)
            {
                clientSettings = (ClientSettingsSection)configSection;
            }
            else
            {
                clientSettings = new ClientSettingsSection();
                config.Sections.Add(USER_SETTINGS_SECTION_NAME, clientSettings);
            }

            return clientSettings;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private XmlNode SerializeToXmlElement(SettingsProperty setting, SettingsPropertyValue value)
        {
            XmlElement element = new XmlDocument().CreateElement("value");
            string serializedValue = value.SerializedValue as string;
            if ((serializedValue == null) && (setting.SerializeAs == SettingsSerializeAs.Binary))
            {
                byte[] inArray = value.SerializedValue as byte[];
                if (inArray != null)
                {
                    serializedValue = Convert.ToBase64String(inArray);
                }
            }
            if (serializedValue == null)
            {
                serializedValue = string.Empty;
            }
            if (setting.SerializeAs == SettingsSerializeAs.String)
            {
                serializedValue = this.Escaper.Escape(serializedValue);
            }
            element.InnerXml = serializedValue;
            XmlNode oldChild = null;
            foreach (XmlNode node2 in element.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.XmlDeclaration)
                {
                    oldChild = node2;
                    break;
                }
            }
            if (oldChild != null)
            {
                element.RemoveChild(oldChild);
            }
            return element;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private void DeserializeFromXmlElement(SettingsProperty property, SettingsPropertyValue spv, ClientSettingsSection clientSettings)
        {
            if (clientSettings != null)
            {
                SettingElement se = clientSettings.Settings.Get(spv.Name);
                if (se != null)
                {
                    string innerXml = se.Value.ValueXml.InnerXml;
                    if (se.SerializeAs == SettingsSerializeAs.String)
                    {
                        innerXml = this.Escaper.Unescape(innerXml);
                    }
                    spv.SerializedValue = innerXml;
                }
                else if (property.DefaultValue != null)
                {
                    spv.SerializedValue = property.DefaultValue;
                }
                else
                {
                    spv.PropertyValue = null;
                }
            }
            else
            {
                spv.PropertyValue = null;
            }
        }

        private XmlEscaper Escaper
        {
            get
            {
                if (this.escaper == null)
                {
                    this.escaper = new XmlEscaper();
                }
                return this.escaper;
            }
        }
 


        private class XmlEscaper
        {
            // Fields
            private XmlDocument doc = new XmlDocument();
            private XmlElement temp;

            // Methods
            internal XmlEscaper()
            {
                this.temp = this.doc.CreateElement("temp");
            }

            internal string Escape(string xmlString)
            {
                if (string.IsNullOrEmpty(xmlString))
                {
                    return xmlString;
                }
                this.temp.InnerText = xmlString;
                return this.temp.InnerXml;
            }

            internal string Unescape(string escapedString)
            {
                if (string.IsNullOrEmpty(escapedString))
                {
                    return escapedString;
                }
                this.temp.InnerXml = escapedString;
                return this.temp.InnerText;
            }
        }
    }
}
