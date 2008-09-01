using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Text;

namespace Growl.AppBridge
{
    [Serializable]
    public class DisplayPreferences : ISerializable
    {
        protected Display display;

        public DisplayPreferences(Display display)
        {
            this.display = display;
        }

        protected DisplayPreferences(SerializationInfo info, StreamingContext context)
        {
            // generic way to deserialize all members (thus handling inherited types as well).
            // we cant be sure of the order of the data in the 'info' object, so we must do two loops to sort it out
            int i = 0;
            Dictionary<string, object> d = new Dictionary<string, object>();
            SerializationInfoEnumerator e = info.GetEnumerator();
            while (i < info.MemberCount)
            {
                e.MoveNext();
                d.Add(e.Name, e.Value);
                i++;
            }
            MemberInfo[] mi = FormatterServices.GetSerializableMembers(this.GetType(), context);
            object[] data = new object[mi.Length];
            for (int m = 0; m < mi.Length; ++m)
            {
                FieldInfo fi = ((FieldInfo)mi[m]);
                if (d.ContainsKey(fi.Name))
                    data[m] = d[fi.Name];
            }     
            FormatterServices.PopulateObjectMembers(this, mi, data);

            // handle custom values
            bool isDefault = info.GetBoolean("IsDefault");
            if (isDefault) this.display = Display.Default;
        }

        public Display Display
        {
            get
            {
                return this.display;
            }
            set
            {
                this.display = value;
            }
        }

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // generic way to serialize all members of an object (thus handling inherited types as well)
            MemberInfo[] mi = FormatterServices.GetSerializableMembers(this.GetType());
            object[] oi = FormatterServices.GetObjectData(this, mi);
            for(int i=0;i<oi.Length;i++)
            {
                info.AddValue(mi[i].Name, oi[i]);
            }

            // add custom values
            info.AddValue("IsDefault", this.display.IsDefault);
        }

        #endregion
    }
}
