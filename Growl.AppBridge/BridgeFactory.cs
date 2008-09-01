using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AppBridge
{
    public class BridgeFactory
    {
        private static AppBridge appBridge;

        public static AppBridge GetAppBridge()
        {
            if (appBridge == null)
                appBridge = new AppBridge();
            return appBridge;
        }
    }
}
