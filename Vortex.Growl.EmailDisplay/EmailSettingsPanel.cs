using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Vortex.Growl.DisplayStyle;

namespace Vortex.Growl.EmailDisplay
{
    public partial class EmailSettingsPanel : SettingsPanelBase
    {
        private const string SETTING_EMAILADDRESS = "EmailAddress";
        private const string SETTING_PRIORITYEMERGENCY = "PriorityEmergency";
        private const string SETTING_PRIORITYHIGH = "PriorityHigh";
        private const string SETTING_PRIORITYNORMAL = "PriorityNormal";
        private const string SETTING_PRIORITYMODERATE = "PriorityModerate";
        private const string SETTING_PRIORITYVERYLOW = "PriorityVeryLow";

        public EmailSettingsPanel()
        {
            InitializeComponent();
        }

        private void EmailSettingsPanel_Load(object sender, EventArgs e)
        {
            this.SettingsPanelSelected += new EventHandler(EmailSettingsPanel_SettingsPanelSelected);
            this.SettingsPanelDeselected += new EventHandler(EmailSettingsPanel_SettingsPanelDeselected);

            // set settings
            Dictionary<string, object> settings = this.GetSettings();
            this.emailTextBox.Text = (settings.ContainsKey(SETTING_EMAILADDRESS) ? settings[SETTING_EMAILADDRESS].ToString() : "");
            this.priorityEmergencyCheckBox.Checked = (settings.ContainsKey(SETTING_PRIORITYEMERGENCY) ? Convert.ToBoolean(settings[SETTING_PRIORITYEMERGENCY].ToString()) : false);
            this.priorityHighCheckBox.Checked = (settings.ContainsKey(SETTING_PRIORITYHIGH) ? Convert.ToBoolean(settings[SETTING_PRIORITYHIGH].ToString()) : false);
            this.priorityNormalCheckBox.Checked = (settings.ContainsKey(SETTING_PRIORITYNORMAL) ? Convert.ToBoolean(settings[SETTING_PRIORITYNORMAL].ToString()) : false);
            this.priorityModerateCheckBox.Checked = (settings.ContainsKey(SETTING_PRIORITYMODERATE) ? Convert.ToBoolean(settings[SETTING_PRIORITYMODERATE].ToString()) : false);
            this.priorityVeryLowCheckBox.Checked = (settings.ContainsKey(SETTING_PRIORITYVERYLOW) ? Convert.ToBoolean(settings[SETTING_PRIORITYVERYLOW].ToString()) : false);
        }

        void EmailSettingsPanel_SettingsPanelDeselected(object sender, EventArgs e)
        {
            SaveEmailAddressSetting();
            SavePriorityEmergencySetting();
            SavePriorityHighSetting();
            SavePriorityNormalSetting();
            SavePriorityModerateSetting();
            SavePriorityVeryLowSetting();
        }

        void EmailSettingsPanel_SettingsPanelSelected(object sender, EventArgs e)
        {
            // do nothing
        }

        private void SaveEmailAddressSetting()
        {
            SaveSetting(SETTING_EMAILADDRESS, this.emailTextBox.Text);
        }

        private void SavePriorityEmergencySetting()
        {
            SaveSetting(SETTING_PRIORITYEMERGENCY, this.priorityEmergencyCheckBox.Checked);
        }

        private void SavePriorityHighSetting()
        {
            SaveSetting(SETTING_PRIORITYHIGH, this.priorityHighCheckBox.Checked);
        }

        private void SavePriorityNormalSetting()
        {
            SaveSetting(SETTING_PRIORITYNORMAL, this.priorityNormalCheckBox.Checked);
        }

        private void SavePriorityModerateSetting()
        {
            SaveSetting(SETTING_PRIORITYMODERATE, this.priorityModerateCheckBox.Checked);
        }

        private void SavePriorityVeryLowSetting()
        {
            SaveSetting(SETTING_PRIORITYVERYLOW, this.priorityVeryLowCheckBox.Checked);
        }

        private void emailTextBox_TextChanged(object sender, EventArgs e)
        {
            SaveEmailAddressSetting();
        }

        private void priorityEmergencyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePriorityEmergencySetting();
        }

        private void priorityHighCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePriorityHighSetting();
        }

        private void priorityNormalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePriorityModerateSetting();
        }

        private void priorityModerateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePriorityNormalSetting();
        }

        private void priorityVeryLowCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePriorityVeryLowSetting();
        }
    }
}
