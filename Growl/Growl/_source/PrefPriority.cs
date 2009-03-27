using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Growl.Connector;

namespace Growl
{
    [Serializable]
    public class PrefPriority : DefaultablePreference, ISerializable
    {
        public static PrefPriority Default = new PrefPriority(null, DEFAULT_DISPLAY_LABEL, true);
        private static PrefPriority Emergency = new PrefPriority(Growl.Connector.Priority.Emergency, "Emergency");
        private static PrefPriority High = new PrefPriority(Growl.Connector.Priority.High, "High");
        private static PrefPriority Normal = new PrefPriority(Growl.Connector.Priority.Normal, "Normal");
        private static PrefPriority Low = new PrefPriority(Growl.Connector.Priority.Moderate, "Low");
        private static PrefPriority VeryLow = new PrefPriority(Growl.Connector.Priority.VeryLow, "Very Low");

        private Priority? priority = null;

        private PrefPriority(Priority? priority, string name)
            : this(priority, name, false)
        {
        }

        private PrefPriority(Priority? priority, string name, bool isDefault)
            : base(name, isDefault)
        {
            this.priority = priority;
        }

        public Priority? Priority
        {
            get
            {
                return this.priority;
            }
        }

        public static PrefPriority[] GetList(bool allowDefault)
        {
            int i = 0;
            int c = 5;
            if (allowDefault) c++;
            PrefPriority[] arr = new PrefPriority[c];
            if (allowDefault)
            {
                arr[i++] = Default;
            }
            arr[i++] = Emergency;
            arr[i++] = High;
            arr[i++] = Normal;
            arr[i++] = Low;
            arr[i++] = VeryLow;
            return arr;
        }

        public override int GetHashCode()
        {
            return this.priority.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefPriority p = obj as PrefPriority;
            if (p != null)
            {
                return this.priority == p.priority;
            }
            else
                return base.Equals(obj);
        }

        private static PrefPriority GetByValue(Priority? val)
        {
            if (val == null)
                return PrefPriority.Default;

            switch (val.Value)
            {
                case Growl.Connector.Priority.Emergency:
                    return PrefPriority.Emergency;
                case Growl.Connector.Priority.High:
                    return PrefPriority.High;
                case Growl.Connector.Priority.Normal:
                    return PrefPriority.Normal;
                case Growl.Connector.Priority.Moderate:
                    return PrefPriority.Low;
                case Growl.Connector.Priority.VeryLow:
                    return PrefPriority.VeryLow;
                default:
                    return PrefPriority.Default;
            }
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(PrefPrioritySerializationHelper));
            info.AddValue("priority", this.Priority, typeof(Priority?));
        }

        #endregion

        [Serializable]
        private class PrefPrioritySerializationHelper : IObjectReference
        {
            private Priority? priority = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return PrefPriority.GetByValue(this.priority);
            }

            #endregion
        }
    }
}
