using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Application
    {
        private Growl.Connector.Application app;

        public Application()
        {
            this.app = new Growl.Connector.Application("Unidentified Application");
        }

        public string Name
        {
            get
            {
                return this.app.Name;
            }
            set
            {
                this.app.Name = value;
            }
        }

        public string IconPath
        {
            get
            {
                if (this.app.Icon.IsUrl)
                    return this.app.Icon.Url;
                else
                    return null;
            }
            set
            {
                this.app.Icon = value;
            }
        }

        public byte[] IconBytes
        {
            get
            {
                if (this.app.Icon.IsRawData)
                    return this.app.Icon.Data.Data;
                else
                    return null;
            }
            set
            {
                this.app.Icon = new Growl.CoreLibrary.BinaryData(value);
            }
        }

        internal Growl.Connector.Application UnderlyingApplication
        {
            get
            {
                return this.app;
            }
        }
    }
}
