using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class CallbackContext
    {
        private string data;
        private string type;
        private string url;

        public CallbackContext()
        {
        }

        public string Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        public string Method;

        internal Growl.Connector.CallbackContext UnderlyingCallbackContext
        {
            get
            {
                if (!String.IsNullOrEmpty(this.url))
                    return new Growl.Connector.CallbackContext(this.Url);
                else
                    return new Growl.Connector.CallbackContext(this.Data, this.Type);
            }
        }
    }
}
