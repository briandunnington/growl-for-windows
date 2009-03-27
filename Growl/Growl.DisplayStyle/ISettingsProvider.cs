using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the interface used by displays to manage and persist their settings.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Saves the settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <remarks>
        /// All settings contained in the settings dictionary must be of a serializable type.
        /// </remarks>
        void Save(Dictionary<string, object> settings);

        /// <summary>
        /// Loads the saved settings
        /// </summary>
        /// <returns>Dictionary of settings</returns>
        Dictionary<string, object> Load();
    }
}
