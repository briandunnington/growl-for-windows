using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl
{
    [Serializable]
    public class DisplaySerializationHelper : IObjectReference
    {
        #region IObjectReference Members

        public object GetRealObject(StreamingContext context)
        {
            //DisplayStyleManager.FindDisplayStyle("by name");
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
