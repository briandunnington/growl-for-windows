using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class GrowlException : Exception
    {
        public GrowlException()
            : this("Growl encountered an unhandled exception")
        {
        }

        public GrowlException(string message)
            : base(message)
        {
        }

        public GrowlException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GrowlException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
