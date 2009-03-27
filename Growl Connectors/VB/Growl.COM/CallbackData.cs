using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.COM
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class CallbackData
    {
        private string notificationID;
        private string data;
        private string type;
        private string result;

        // this class cannot be created from VB
        public CallbackData(Growl.Connector.CallbackData callbackData)
        {
            this.notificationID = callbackData.NotificationID;
            this.data = callbackData.Data;
            this.type = callbackData.Type;
            this.result = callbackData.Result.ToString();
        }

        public string NotificationID
        {
            get
            {
                return this.notificationID;
            }
        }

        public string Data
        {
            get
            {
                return this.data;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
        }

        public string Result
        {
            get
            {
                return this.result;
            }
        }
    }
}
