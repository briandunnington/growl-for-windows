using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Daemon
{
    class ConnectedSocket
    {
        private AsyncSocket socket;
        private bool safeToDisconnect = false;

        public ConnectedSocket(AsyncSocket socket)
        {
            this.socket = socket;
        }

        public bool SafeToDisconnect
        {
            get
            {
                return this.safeToDisconnect;
            }
            set
            {
                this.safeToDisconnect = value;
            }
        }

        public AsyncSocket Socket
        {
            get
            {
                return this.socket;
            }
        }
    }
}
