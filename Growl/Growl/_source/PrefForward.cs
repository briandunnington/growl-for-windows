using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PrefForward : DefaultablePreference, ISerializable
    {
        private const string DONT_FORWARD_DISPLAY_LABEL = "Don't Forward";
        private const string CUSTOM_DISPLAY_LABEL = "Choose...";

        public static PrefForward Default = new PrefForward(null, DEFAULT_DISPLAY_LABEL, true);
        private static PrefForward DontForward = new PrefForward(false, DONT_FORWARD_DISPLAY_LABEL);
        private static PrefForward Custom = new PrefForward(true, CUSTOM_DISPLAY_LABEL);

        private bool? forward = null;


        protected PrefForward(bool? forward, string name)
            : this(forward, name, false)
        {
        }

        private PrefForward(bool? forward, string name, bool isDefault)
            : base(name, isDefault)
        {
            this.forward = forward;
        }

        public bool? Forward
        {
            get
            {
                return this.forward;
            }
        }

        public bool IsCustom
        {
            get
            {
                return (this.Name == CUSTOM_DISPLAY_LABEL);
            }
        }

        public static PrefForward[] GetList(bool allowDefault)
        {
            int i = 0;
            int c = 2;
            if (allowDefault) c++;
            PrefForward[] arr = new PrefForward[c];
            if (allowDefault)
            {
                arr[i++] = Default;
            }
            arr[i++] = DontForward;
            arr[i++] = Custom;
            return arr;
        }

        public override int GetHashCode()
        {
            return this.forward.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefForward f = obj as PrefForward;
            if (f != null)
            {
                return this.Name == f.Name;
            }
            else
                return base.Equals(obj);
        }

        private static PrefForward GetByName(string name)
        {
            switch (name)
            {
                case DONT_FORWARD_DISPLAY_LABEL:
                    return PrefForward.DontForward;
                case CUSTOM_DISPLAY_LABEL:
                    return PrefForward.Custom;
                default:
                    return PrefForward.Default;
            }
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(PrefForwardSerializationHelper));
            info.AddValue("name", this.Name, typeof(string));
        }

        #endregion

        [Serializable]
        private class PrefForwardSerializationHelper : IObjectReference
        {
            private string name = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return PrefForward.GetByName(this.name);
            }

            #endregion
        }
    }
}
