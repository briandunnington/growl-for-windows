using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Scripty
{
    public partial class ScriptySettingsPanel : SettingsPanelBase
    {
        public ScriptySettingsPanel()
        {
            InitializeComponent();

            this.Load +=new EventHandler(ScriptySettingsPanel_Load);
        }

        protected override void ReadSettings()
        {
            base.ReadSettings();

            Dictionary<string, object> settings = this.GetSettings();
            if (!settings.ContainsKey(ScriptyDisplay.SETTING_FILE_NAME))
            {
                string filename = System.IO.Path.Combine(this.Directory, "scripty_sample.bat");
                this.SaveSetting(ScriptyDisplay.SETTING_FILE_NAME, filename);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = this.openFileDialog1.ShowDialog();
        }

        private void ScriptySettingsPanel_Load(object sender, EventArgs e)
        {
            this.textBoxFile.Text = GetFileFromSetting();

            this.openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
        }

        void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.textBoxFile.Text = this.openFileDialog1.FileName;
            this.SaveSetting(ScriptyDisplay.SETTING_FILE_NAME, this.textBoxFile.Text);
        }

        private string GetFileFromSetting()
        {
            string filename = String.Empty;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(ScriptyDisplay.SETTING_FILE_NAME))
            {
                try
                {
                    object val = settings[ScriptyDisplay.SETTING_FILE_NAME];
                    if (val is string)
                    {
                        filename = (string)val;
                    }
                }
                catch
                {
                }
            }
            return filename;
        }
    }
}
