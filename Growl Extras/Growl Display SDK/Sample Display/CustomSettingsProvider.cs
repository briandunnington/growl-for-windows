using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_Display
{
    /// <summary>
    /// This is a very simple example of creating your own custom ISettingsProvider.
    /// In this sample, all settings are assumed to be strings and simply written to a text file,
    /// one setting/value pair per line.
    /// 
    /// The SettingsPanelBase class provides its own implementation of ISettingsProvider by
    /// default, so you do not need to create your own ISettingsProvider unless you want to
    /// have complete control over where/how your settings are persisted.
    /// </summary>
    internal class CustomSettingsProvider : Growl.DisplayStyle.ISettingsProvider
    {
        private const string PATH = @"c:\growl_sdk_sample_display_settings.txt";

        #region ISettingsProvider Members

        public Dictionary<string, object> Load()
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();

            string[] lines = System.IO.File.ReadAllLines(PATH);

            if (lines != null)
            {
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    settings.Add(parts[0], parts[1]);
                }
            }

            return settings;
        }

        public void Save(Dictionary<string, object> settings)
        {
            System.IO.StreamWriter stream = System.IO.File.CreateText(PATH);

            foreach (KeyValuePair<string, object> item in settings)
            {
                stream.WriteLine(String.Format("{0}|{1}", item.Key, item.Value.ToString()));
            }

            stream.Close();
        }

        #endregion
    }
}
