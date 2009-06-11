using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Degree
{
    public partial class DegreeSettingsPanel : SettingsPanelBase
    {
        private static Point TOP_LEFT = new Point(19, 14);
        private static Point TOP_RIGHT = new Point(77, 14);
        private static Point BOTTOM_LEFT = new Point(17, 74);
        private static Point BOTTOM_RIGHT = new Point(82, 74);

        private Point overlayPosition = TOP_RIGHT;

        public DegreeSettingsPanel()
        {
            InitializeComponent();
        }

        private void DegreeSettingsPanel_Load(object sender, EventArgs e)
        {
            this.computerScreenPictureBox.Image = global::Degree.Properties.Resources.my_computer;
            this.overlayPosition = GetLocation();

            this.computerScreenPictureBox.Paint += new PaintEventHandler(computerScreenPictureBox_Paint);
        }

        void computerScreenPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(global::Degree.Properties.Resources.overlay, this.overlayPosition);
        }

        private Point GetLocation()
        {
            Point p = TOP_RIGHT;
            Dictionary<string, object> settings = this.GetSettings();
            if (settings != null && settings.ContainsKey(DegreeDisplay.SETTING_DISPLAYLOCATION))
            {
                try
                {
                    object val = settings[DegreeDisplay.SETTING_DISPLAYLOCATION];
                    int i = Convert.ToInt32(val);
                    switch (i)
                    {
                        case 2:
                            p = TOP_RIGHT;
                            break;
                        case 3:
                            p = BOTTOM_LEFT;
                            break;
                        case 4:
                            p = BOTTOM_RIGHT;
                            break;
                        default:
                            p = TOP_LEFT;
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

            if (this.overlayPosition == TOP_RIGHT) i = 2;
            else if (this.overlayPosition == BOTTOM_LEFT) i = 3;
            else if (this.overlayPosition == BOTTOM_RIGHT) i = 4;
            else i = 1;

            this.SaveSetting(DegreeDisplay.SETTING_DISPLAYLOCATION, i);
        }

        private void computerScreenPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            int leftRightLine = this.computerScreenPictureBox.Width / 2;
            int topBottomLine = (this.computerScreenPictureBox.Height - 40) / 2;

            this.overlayPosition = TOP_RIGHT;

            if (e.X > leftRightLine)
            {
                this.overlayPosition = (e.Y > topBottomLine ? BOTTOM_RIGHT : TOP_RIGHT);
            }
            else
            {
                this.overlayPosition = (e.Y > topBottomLine ? BOTTOM_LEFT : TOP_LEFT);
            }

            SaveLocation();

            this.computerScreenPictureBox.Invalidate();
        }
    }
}
