using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class PortConflictEventArgs : EventArgs
    {
        private int port;

        public PortConflictEventArgs(int port)
        {
            this.port = port;
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }
    }
}
