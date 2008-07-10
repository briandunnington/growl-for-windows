using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vortex.Growl.DisplayStyle
{
    /// <summary>
    /// Provides the base class for all settings panels for all displays.
    /// Settings panels provide the user interface for the end user to set
    /// and modify any user-configurable settings.
    /// </summary>
    public partial class SettingsPanelBase : UserControl
    {
        /// <summary>
        /// Fired when the panel is selected in the Growl application.
        /// </summary>
        public event EventHandler SettingsPanelSelected;

        /// <summary>
        /// Fired when the panel is deselected in the Growl application.
        /// </summary>
        public event EventHandler SettingsPanelDeselected;

        /// <summary>
        /// Fired whenever a user-configurable setting is changed.
        /// </summary>
        public event EventHandler SettingsChanged;

        /// <summary>
        /// Provided for the Growl application to associated display-specific information with this panel.
        /// </summary>
        private object display;

        /// <summary>
        /// A collection of user-configurable setting values.
        /// </summary>
        private Dictionary<string, object> settingsCollection = null;

        /// <summary>
        /// Indicates if any settings have changed.
        /// </summary>
        private bool haveSettingsChanged = false;

        /// <summary>
        /// A local instance of the <see cref="SettingSaver"/> used to persist settings to disk.
        /// </summary>
        private SettingSaver ss;

        /// <summary>
        /// The directory where the display associated with this panel is installed.
        /// </summary>
        private string directory;


        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public SettingsPanelBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The path to the folder where the display associated with this panel is installed.
        /// </summary>
        public string Directory
        {
            set
            {
                this.directory = value;
            }
        }

        /// <summary>
        /// Provided for the Growl application to associated display-specific information with this panel.
        /// </summary>
        public object Display
        {
            get
            {
                return this.display;
            }
            set
            {
                this.display = value;
            }
        }

        /// <summary>
        /// Ensures that the internal <see cref="SettingsSaver"/> class has been instantiated.
        /// </summary>
        private void EnsureSettingsSaver()
        {
            if (this.ss == null)
                this.ss = new SettingSaver(this.directory, "display.settings");
        }

        /// <summary>
        /// Returns the list of user-configured setting values.
        /// </summary>
        /// <returns><see cref="Dictionary{string, object}"/></returns>
        public Dictionary<string, object> GetSettings()
        {
            if (this.settingsCollection == null)
                ReadSettings();

            return this.settingsCollection;
        }

        /// <summary>
        /// Saves the setting value <paramref name="val"/> identified by the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The setting name.</param>
        /// <param name="val">The setting value.</param>
        protected void SaveSetting(string key, object val)
        {
            this.haveSettingsChanged = true;
            if (!this.settingsCollection.ContainsKey(key))
                this.settingsCollection.Add(key, val);
            else
                this.settingsCollection[key] = val;
            OnSettingsChanged(null);
        }

        /// <summary>
        /// Selects the panel.
        /// </summary>
        public void SelectPanel()
        {
            OnSettingsPanelSelected(null);
        }

        /// <summary>
        /// Deselects the panel.
        /// </summary>
        public void DeselectPanel()
        {
            OnSettingsChanged(null);
            OnSettingsPanelDeselected(null);
        }

        /// <summary>
        /// Reads the settings from the persisted file on disk.
        /// </summary>
        private void ReadSettings()
        {
            EnsureSettingsSaver();
            this.settingsCollection = (Dictionary<string, object>) this.ss.Load();
            if (this.settingsCollection == null) this.settingsCollection = new Dictionary<string, object>();
        }

        /// <summary>
        /// Persists the settings to a file on disk.
        /// </summary>
        private void PersistSettings()
        {
            if (this.settingsCollection != null && this.settingsCollection.Count > 0)
            {
                EnsureSettingsSaver();
                this.ss.Save(this.settingsCollection);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:SettingsPanelSelected"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSettingsPanelSelected(EventArgs args)
        {
            if (this.SettingsPanelSelected != null)
            {
                this.SettingsPanelSelected(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:SettingsPanelDeselected"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSettingsPanelDeselected(EventArgs args)
        {
            if (this.SettingsPanelDeselected != null)
            {
                this.SettingsPanelDeselected(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:SettingsChanged"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSettingsChanged(EventArgs args)
        {
            if (this.SettingsChanged != null && this.haveSettingsChanged)
            {
                this.SettingsChanged(this, args);
            }
            this.haveSettingsChanged = false;
        }

        /// <summary>
        /// Handles the Load event of the SettingsPanelBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SettingsPanelBase_Load(object sender, EventArgs e)
        {
            this.SettingsPanelSelected += new EventHandler(SettingsPanelBase_SettingsPanelSelected);
            this.SettingsPanelDeselected += new EventHandler(SettingsPanelBase_SettingsPanelDeselected);
        }

        /// <summary>
        /// Handles the SettingsPanelSelected event of the SettingsPanelBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SettingsPanelBase_SettingsPanelSelected(object sender, EventArgs e)
        {
            // do nothing
        }

        /// <summary>
        /// Handles the SettingsPanelDeselected event of the SettingsPanelBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SettingsPanelBase_SettingsPanelDeselected(object sender, EventArgs e)
        {
            PersistSettings();
        }
    }
}
