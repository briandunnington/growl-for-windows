using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.Displays.Smokestack
{
    public partial class SmokestackSettingsPanel : SettingsPanelBase
    {
        PositionSettingScaler pss;
        Point overlayPosition;

        public SmokestackSettingsPanel()
        {
            InitializeComponent();
        }

        private void SmokestackSettingsPanel_Load(object sender, EventArgs e)
        {
            this.computerScreenPictureBox.Image = global::Growl.Displays.Smokestack.Properties.Resources.My_Computer;

            pss = new PositionSettingScaler(this.computerScreenPictureBox, global::Growl.Displays.Smokestack.Properties.Resources.overlay);

            this.overlayPosition = GetLocation();

            this.computerScreenPictureBox.Paint += new PaintEventHandler(computerScreenPictureBox_Paint);
        }

        void computerScreenPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(global::Growl.Displays.Smokestack.Properties.Resources.overlay, this.overlayPosition);
        }

        private Point GetLocation()
        {
            Point p = pss.TopRight;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(SmokestackDisplay.SETTING_DISPLAYLOCATION))
            {
                try
                {
                    object val = settings[SmokestackDisplay.SETTING_DISPLAYLOCATION];
                    int i = Convert.ToInt32(val);
                    switch (i)
                    {
                        case 2:
                            p = pss.TopRight;
                            break;
                        case 3:
                            p = pss.BottomLeft;
                            break;
                        case 4:
                            p = pss.BottomRight;
                            break;
                        default:
                            p = pss.TopLeft;
                            break;
                    }
                }
                catch
                {
                }
            }
            return p;
        }

        private void SaveLocation()
        {
            int i = 1;

            if (this.overlayPosition == pss.TopRight) i = 2;
            else if (this.overlayPosition == pss.BottomLeft) i = 3;
            else if (this.overlayPosition == pss.BottomRight) i = 4;
            else i = 1;

            this.SaveSetting(SmokestackDisplay.SETTING_DISPLAYLOCATION, i);
        }

        private void computerScreenPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            this.overlayPosition = pss.GetQuadrantFromCoordinates(e.X, e.Y);
            SaveLocation();
            this.computerScreenPictureBox.Invalidate();
        }
    }
}
