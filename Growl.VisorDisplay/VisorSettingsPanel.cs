using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.VisorDisplay
{
    public partial class VisorSettingsPanel : SettingsPanelBase
    {
        private const string SETTING_BGCOLOR = "BackgroundColor";

        public VisorSettingsPanel()
        {
            InitializeComponent();
        }

        private void VisorSettingsPanel_Load(object sender, EventArgs e)
        {
            this.currentBgColorPictureBox.BackColor = this.GetBgColor();
        }

        private void currentBgColorPictureBox_Click(object sender, EventArgs e)
        {
            DialogResult result = this.colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(SETTING_BGCOLOR, this.colorDialog1.Color);
                this.currentBgColorPictureBox.BackColor = this.colorDialog1.Color;
            }
        }

        private Color GetBgColor()
        {
            Color bgColor = Color.Black;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(SETTING_BGCOLOR))
            {
                try
                {
                    object val = settings[SETTING_BGCOLOR];
                    if (val is Color)
                    {
                        bgColor = (Color)val;
                    }
                }
                catch
                {
                }
            }
            return bgColor;
        }
    }
}
