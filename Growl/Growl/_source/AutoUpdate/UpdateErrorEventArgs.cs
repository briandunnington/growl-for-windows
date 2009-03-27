using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AutoUpdate
{
    public class UpdateErrorEventArgs : EventArgs
    {
        private Exception exception;
        private string userMessage;

        public UpdateErrorEventArgs(Exception exception, string userMessage)
        {
            this.exception = exception;
            this.userMessage = userMessage;
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        public string UserMessage
        {
            get
            {
                return this.userMessage;
            }
        }
    }
}
