using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Represents the interface required for displays that want to provide 
    /// muliple monitor support in GfW.
    /// </summary>
    /// <remarks>
    /// Most developers should simply inherit from the <see cref="VisualDisplay"/> class, 
    /// which provide implementation for this interface.
    /// </remarks>
    public interface IDisplayMultipleMonitor
    {
        /// <summary>
        /// Sets the preferred display.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        /// <remarks>
        /// This method will be called by GfW when the user selects the monitor that they
        /// prefer the notification to be displayed on (and also again each time GfW starts up).
        /// 
        /// When a notification is later passed to the display, the display can use the
        /// <paramref name="deviceName"/> to determine which monitor to render itself on.
        /// </remarks>
        void SetPreferredDisplay(string deviceName);
    }
}
