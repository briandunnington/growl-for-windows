using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    internal class DisplayNone : Display
    {
        public DisplayNone()
        {
            NoneDisplay noneDisplay = new NoneDisplay();
            Initialize(noneDisplay.Name, noneDisplay, false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.SetType(typeof(DisplayNoneSerializationHelper));
        }

        [Serializable]
        private class DisplayNoneSerializationHelper : IObjectReference
        {
            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return Display.None;
            }

            #endregion
        }
    }
}
