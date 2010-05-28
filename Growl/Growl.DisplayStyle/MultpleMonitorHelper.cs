using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Growl.DisplayStyle
{
    public static class MultipleMonitorHelper
    {
        /// <summary>
        /// Gets the <see cref="Screen"/> associated with the <paramref name="preferredDeviceName"/>
        /// </summary>
        /// <param name="preferredDeviceName">Name of the preferred device.</param>
        /// <returns><see cref="Screen"/> - if the <paramref name="preferredDeviceName"/> is not valid or not
        /// associated with a currently available <see cref="Screen"/>, the <see cref="Screen.PrimaryScreen"/>
        /// is returned instead.</returns>
        public static Screen GetScreen(string preferredDeviceName)
        {
            try
            {
                if (!String.IsNullOrEmpty(preferredDeviceName))
                {
                    foreach (Screen screen in Screen.AllScreens)
                    {
                        string deviceName = GetDeviceName(screen);
                        if (deviceName == preferredDeviceName)
                            return screen;
                    }
                }
            }
            catch
            {
                // if anything at all goes wrong, we will just return the PrimaryScreen
            }
            return Screen.PrimaryScreen;
        }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        /// <param name="screen">The <see cref="Screen"/></param>
        /// <returns>string</returns>
        /// <remarks>
        /// Since the device name normally contains null bytes and other unnecessary characters,
        /// this method only returns the normalized device name with unnecessary characters
        /// removed.
        /// </remarks>
        public static string GetDeviceName(Screen screen)
        {
            string name = null;
            if (screen != null)
            {
                name = screen.DeviceName;
                int pos = name.IndexOf('\0');
                if (pos >= 0)
                    name = name.Substring(0, pos);
            }
            return name;
        }
    }
}
