using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AppBridge
{
    class LocalReceiver : BaseReceiver
    {
        internal LocalReceiver()
        {
            // this class is like a singleton.
            // instances should only be created by the parent AppBridge
            this.port = Growl.Framework.Growler.DEFAULT_LOCAL_PORT;
            this.localMessagesOnly = true;
        }
    }
}
