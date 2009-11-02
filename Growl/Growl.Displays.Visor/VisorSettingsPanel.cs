using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Visor
{
    public partial class VisorSettingsPanel : SettingsPanelBase
    {
        public VisorSettingsPanel()
        {
            InitializeComponent();
        }

        private void VisorSettingsPanel_Load(object sender, EventArgs e)
        {
            Color defaultColor = this.GetColorFromSetting(VisorDisplay.SETTING_BGCOLOR, VisorDisplay.BG_COLOR);   // for backwards-compatibility
            this.pictureBoxEmergency.BackColor = this.GetColorFromSetting(VisorDisplay.SETTING_BG_EMERGENCY, defaultColor);
            this.pictureBoxHigh.BackColor = this.GetColorFromSetting(VisorDisplay.SETTING_BG_HIGH, defaultColor);
            this.pictureBoxNormal.BackColor = this.GetColorFromSetting(VisorDisplay.SETTING_BG_NORMAL, defaultColor);
            this.pictureBoxLow.BackColor = this.GetColorFromSetting(VisorDisplay.SETTING_BG_LOW, defaultColor);
            this.pictureBoxVeryLow.BackColor = this.GetColorFromSetting(VisorDisplay.SETTING_BG_VERYLOW, defaultColor);
            this.colorDialog1.AllowFullOpen = true;
            this.colorDialog1.FullOpen = true;
        }

        private void ChangeColor(PictureBox pb, string settingName)
        {
            Color newColor = PickColor(pb.BackColor);
            this.SaveSetting(settingName, newColor);
            pb.BackColor = newColor;
        }

        private Color PickColor(Color currentColor)
        {
            this.colorDialog1.CustomColors = new int[] { GetCorrectedColor(this.pictureBoxEmergency.BackColor),
                GetCorrectedColor(this.pictureBoxHigh.BackColor),
                GetCorrectedColor(this.pictureBoxNormal.BackColor),
                GetCorrectedColor(this.pictureBoxLow.BackColor),
                GetCorrectedColor(this.pictureBoxVeryLow.BackColor) };
            this.colorDialog1.Color = currentColor;

            DialogResult result = this.colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                return this.colorDialog1.Color;
            }
            else
            {
                return currentColor;
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

        private int GetCorrectedColor(Color color)
        {
            return (color.R + (color.G << 8) + (color.B << 16));
        }

        private void pictureBoxEmergency_Click(object sender, EventArgs e)
        {
            ChangeColor(pictureBoxEmergency, VisorDisplay.SETTING_BG_EMERGENCY);
        }

        private void pictureBoxHigh_Click(object sender, EventArgs e)
        {
            ChangeColor(pictureBoxHigh, VisorDisplay.SETTING_BG_HIGH);
        }

        private void pictureBoxNormal_Click(object sender, EventArgs e)
        {
            ChangeColor(pictureBoxNormal, VisorDisplay.SETTING_BG_NORMAL);
        }

        private void pictureBoxLow_Click(object sender, EventArgs e)
        {
            ChangeColor(pictureBoxLow, VisorDisplay.SETTING_BG_LOW);
        }

        private void pictureBoxVeryLow_Click(object sender, EventArgs e)
        {
            ChangeColor(pictureBoxVeryLow, VisorDisplay.SETTING_BG_VERYLOW);
        }
    }
}
