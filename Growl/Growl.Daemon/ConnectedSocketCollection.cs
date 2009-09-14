using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Daemon
{
    class ConnectedSocketCollection : System.Collections.ObjectModel.KeyedCollection<AsyncSocket, ConnectedSocket>
    {
        protected override AsyncSocket GetKeyForItem(ConnectedSocket item)
        {
            if (item != null)
                return item.Socket;
            else
                return null;
        }
    }
}
