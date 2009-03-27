using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Error
    {
        private int code;
        private string desc;

        public Error(int code, string desc)
        {
            this.code = code;
            this.desc = desc;
        }

        public int Code
        {
            get
            {
                return this.code;
            }
        }

        public string Description
        {
            get
            {
                return this.desc;
            }
        }
    }
}
