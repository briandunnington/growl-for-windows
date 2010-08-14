using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Sample_Display
{
    /// <summary>
    /// This settings panel will be displayed to the user when they want to configure
    /// your display. Important things to keep in mind:
    /// 
    ///     1. BackColor should be set to Transparent. Growl will do this automatically, but
    ///        it is a good idea to do it explicitly here as well to ensure your control ends
    ///        up looking how you intended
    /// 
    ///     2. Any settings you want to save in the SettingsCollection must be of a serializable
    ///        type
    /// 
    ///     3. The Growl client will ensure that the settings panel is never resized smaller than
    ///        450*155 pixels. Your panel should be layed out to work at this size, as well as handle
    ///        larger sizes (usually simply designing for 450*155 and setting .Anchor = Top | Left
    ///        is sufficient)
    /// 
    ///     4. Do *not* call GetSettings() or SaveSetting() from within the constructor. Your plugin
    ///        is instantiated by GfW using reflection and the values for the plugin directory path
    ///        and settings directory path are not yet available. Any initialization can instead be
    ///        done in the Load() event.
    /// </summary>
    public partial class SampleSettingsPanel : SettingsPanelBase
    {
        public SampleSettingsPanel()
        {
            InitializeComponent();

            this.Load +=new EventHandler(SampleSettingsPanel_Load);

            // uncomment this section if you want to use a custom ISettingsProvider
            //this.SettingsProvider = new CustomSettingsProvider();
        }

        void SampleSettingsPanel_Load(object sender, EventArgs e)
        {
            string filename = GetFileFromSetting(@"c:\growl_display_sdk_sample.txt");
            this.textBox1.Text = filename;

            this.openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
        }

        void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // update the text box display
            this.textBox1.Text = this.openFileDialog1.FileName;

            // persist the setting
            this.SaveSetting(SampleDisplay.SETTING_FILE_NAME, this.textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = this.openFileDialog1.ShowDialog();
        }

        private string GetFileFromSetting(string defaultFileName)
        {
            bool isDefault = true;
            string filename = defaultFileName;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(SampleDisplay.SETTING_FILE_NAME))
            {
                try
                {
                    object val = settings[SampleDisplay.SETTING_FILE_NAME];
                    if (val is string)
                    {
                        filename = (string)val;
                        isDefault = false;
                    }
                }
                catch
                {
                }
            }

            // if the setting was not found, save the default value
            if (isDefault) SaveSetting(SampleDisplay.SETTING_FILE_NAME, filename);

            return filename;
        }
    }
}
