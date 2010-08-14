using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the base implementation for on-screen (visual) displays that provide multiple-monitor support.
    /// </summary>
    /// <remarks>
    /// Most developers should inherit their displays from this class if they are
    /// going to show a notification on-screen and want to support multiple monitors.
    /// </remarks>
    public abstract class MultiMonitorVisualDisplay : VisualDisplay, IDisplayMultipleMonitor
    {
        /// <summary>
        /// The device name for the preferred monitor to display this type of notifications on
        /// </summary>
        string preferredDisplayDeviceName;

        /// <summary>
        /// Gets the preferred display.
        /// </summary>
        /// <returns>The <see cref="System.Windows.Forms.Screen"/> to show the display on.</returns>
        public System.Windows.Forms.Screen GetPreferredDisplay()
        {
            return MultipleMonitorHelper.GetScreen(this.preferredDisplayDeviceName);
        }

        #region IDisplayMultipleMonitor Members

        /// <summary>
        /// Sets the preferred display.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        /// <remarks>
        /// This method will be called by GfW when the user selects the monitor that they
        /// prefer the notification to be displayed on.
        /// When a notification is later passed to the display, the display can use the
        /// <paramref name="deviceName"/> to determine which monitor to render itself on.
        /// </remarks>
        public void SetPreferredDisplay(string deviceName)
        {
            this.preferredDisplayDeviceName = deviceName;
        }

        #endregion
    }
}
