using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
            this.currentBgColorPictureBox.BackColor = this.GetBgColor();
            this.colorDialog1.AllowFullOpen = true;
            this.colorDialog1.FullOpen = true;
        }

        private void currentBgColorPictureBox_Click(object sender, EventArgs e)
        {
            this.colorDialog1.CustomColors = new int[] { GetCorrectedColor(this.currentBgColorPictureBox.BackColor) };
            this.colorDialog1.Color = this.currentBgColorPictureBox.BackColor;

            DialogResult result = this.colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SaveSetting(VisorDisplay.SETTING_BGCOLOR, this.colorDialog1.Color);
                this.currentBgColorPictureBox.BackColor = this.colorDialog1.Color;
            }
        }

        private Color GetBgColor()
        {
            Color bgColor = Color.Black;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(VisorDisplay.SETTING_BGCOLOR))
            {
                try
                {
                    object val = settings[VisorDisplay.SETTING_BGCOLOR];
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

        private int GetCorrectedColor(Color color)
        {
            return (color.R + (color.G << 8) + (color.B << 16));
        }
    }
}
