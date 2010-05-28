using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    public abstract class MultiMonitorVisualDisplay : VisualDisplay, IDisplayMultipleMonitor
    {
        /// <summary>
        /// The device name for the preferred monitor to display this type of notifications on
        /// </summary>
        string preferredDisplayDeviceName;

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
