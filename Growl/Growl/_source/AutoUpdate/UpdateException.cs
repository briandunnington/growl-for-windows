using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AutoUpdate
{
    [Serializable]
    public class UpdateException : Exception
    {
        public UpdateException()
            : this("Growl's automatic updating mechanism encountered an unexpected error.")
        {
        }

        public UpdateException(string message)
            : base(message)
        {
        }

        public UpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected UpdateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
