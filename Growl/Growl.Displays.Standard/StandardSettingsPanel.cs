using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Standard
{
    public partial class StandardSettingsPanel : SettingsPanelBase
    {
        public StandardSettingsPanel()
        {
            InitializeComponent();
        }

        private void StandardSettingsPanel_Load(object sender, EventArgs e)
        {
            this.color1PictureBox.BackColor = this.GetColorFromSetting(StandardDisplay.SETTING_COLOR1, StandardDisplay.COLOR1);
            this.color2PictureBox.BackColor = this.GetColorFromSetting(StandardDisplay.SETTING_COLOR2, StandardDisplay.COLOR2);

            this.colorDialog.AllowFullOpen = true;
            this.colorDialog.FullOpen = true;
        }

        private void color1PictureBox_Click(object sender, EventArgs e)
        {
            this.colorDialog.CustomColors = new int[] { GetCorrectedColor(this.color1PictureBox.BackColor), GetCorrectedColor(this.color2PictureBox.BackColor) };
            this.colorDialog.Color = this.color1PictureBox.BackColor;

            DialogResult result = this.colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(StandardDisplay.SETTING_COLOR1, this.colorDialog.Color);
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
            this.colorDialog.CustomColors = new int[] { GetCorrectedColor(this.color1PictureBox.BackColor), GetCorrectedColor(this.color2PictureBox.BackColor) };
            this.colorDialog.Color = this.color2PictureBox.BackColor;

            DialogResult result = this.colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(StandardDisplay.SETTING_COLOR2, this.colorDialog.Color);
                this.color2PictureBox.BackColor = this.colorDialog.Color;
            }
        }

        private void resetLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.SaveSetting(StandardDisplay.SETTING_COLOR1, StandardDisplay.COLOR1);
            this.color1PictureBox.BackColor = StandardDisplay.COLOR1;

            this.SaveSetting(StandardDisplay.SETTING_COLOR2, StandardDisplay.COLOR2);
            this.color2PictureBox.BackColor = StandardDisplay.COLOR2;
        }

        private int GetCorrectedColor(Color color)
        {
            return (color.R + (color.G << 8) + (color.B << 16));
        }
    }
}
