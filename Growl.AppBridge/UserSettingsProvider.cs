using System;
using System.Configuration;
using System.Windows.Forms;

namespace Growl.AppBridge
{
    public class UserSettingsProvider : SettingsProvider
    {
        SettingSaver ss = new SettingSaver("user.config");

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
            // read in any saved values
            System.Collections.Hashtable savedSettings = null;
            try
            {
                savedSettings = (System.Collections.Hashtable)this.ss.Load();
            }
            catch
            {
            }

            // gather all default values, overwriting any with any previously saved values
            SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();
            foreach (SettingsProperty prop in collection)
            {
                SettingsPropertyValue spv = new SettingsPropertyValue(prop);
                if (savedSettings != null)
                {
                    object val = savedSettings[spv.Name];
                    if (val != null) spv.PropertyValue = val;
                }
                settings.Add(spv);
            }

            return settings;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            System.Collections.Hashtable settings = new System.Collections.Hashtable(collection.Count);
            foreach (SettingsPropertyValue spv in collection)
            {
                settings.Add(spv.Name, spv.PropertyValue);
            }

            this.ss.Save(settings);
        }
    }
}
