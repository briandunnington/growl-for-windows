using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AppBridge
{
    [Serializable]
    public class ForwardComputer
    {
        protected string description;
        protected bool enabled = true;
        protected string ipAddress;
        protected int port;
        protected string password;

        public ForwardComputer(string description, bool enabled, string ipAddress, int port, string password)
        {
            this.description = description;
            this.enabled = enabled;
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            set
            {
                this.ipAddress = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        public string Display
        {
            get
            {
                return String.Format("{0} ({1}:{2})", this.description, this.ipAddress, this.port);
            }
        }
    }
}
