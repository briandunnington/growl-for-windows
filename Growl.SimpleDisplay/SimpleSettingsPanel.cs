using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.SimpleDisplay
{
    public partial class SimpleSettingsPanel : SettingsPanelBase
    {
        private const string SETTING_COLOR1 = "Color1";
        private const string SETTING_COLOR2 = "Color2";

        public SimpleSettingsPanel()
        {
            InitializeComponent();
        }

        private void SimpleSettingsPanel_Load(object sender, EventArgs e)
        {
            this.color1PictureBox.BackColor = this.GetColorFromSetting(SETTING_COLOR1, Color.SkyBlue);
            this.color2PictureBox.BackColor = this.GetColorFromSetting(SETTING_COLOR2, Color.White);
        }

        private void color1PictureBox_Click(object sender, EventArgs e)
        {
            this.colorDialog.Color = this.color1PictureBox.BackColor;
            DialogResult result = this.colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(SETTING_COLOR1, this.colorDialog.Color);
                this.color1PictureBox.BackColor = this.colorDialog.Color;
            }
        }

        private Color GetColorFromSetting(string settingName, Color defaultColor)
        {
            Color color = defaultColor;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(settingName))
            {
                try
                {
                    object val = settings[settingName];
                    if (val is Color)
                    {
                        color = (Color)val;
                    }
                }
                catch
                {
                }
            }
            return color;
        }

        private void color2PictureBox_Click(object sender, EventArgs e)
        {
            this.colorDialog.Color = this.color2PictureBox.BackColor;
            DialogResult result = this.colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(SETTING_COLOR2, this.colorDialog.Color);
                this.color2PictureBox.BackColor = this.colorDialog.Color;
            }
        }
    }
}
