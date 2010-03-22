using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Provides a means for plugins to write to GfW's debug log
    /// </summary>
    public static class DebugInfo
    {
        /// <summary>
        /// Handles the <see cref="DebugInfo.Write"/> event
        /// </summary>
        public delegate void WriteEventHandler(string info);

        /// <summary>
        /// Occurs when a plugin write's debug info to the log
        /// </summary>
        public static event WriteEventHandler Write;

        /// <summary>
        /// Writes the specified info the the debug log
        /// </summary>
        /// <param name="info">The info to log</param>
        public static void WriteLine(string info)
        {
            if (Write != null)
            {
                Write(info);
            }
        }
    }
}
