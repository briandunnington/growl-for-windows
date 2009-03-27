using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class CallbackContext
    {
        private Growl.Connector.CallbackContext cbc;

        public CallbackContext()
        {
            this.cbc = new Growl.Connector.CallbackContext();
        }

        public string Data
        {
            get
            {
                return this.cbc.Data;
            }
            set
            {
                this.cbc.Data = value;
            }
        }

        public string Type
        {
            get
            {
                return this.cbc.Type;
            }
            set
            {
                this.cbc.Type = value;
            }
        }

        public void SetUrlCallback(string url, string method)
        {
            Growl.Connector.UrlCallbackTarget target = new Growl.Connector.UrlCallbackTarget();
            target.Url = url;
            target.Method = method;
            this.cbc.SetUrlCallbackTarget(target);
        }

        public string Method;

        internal Growl.Connector.CallbackContext UnderlyingCallbackContext
        {
            get
            {
                return this.cbc;
            }
        }
    }
}
