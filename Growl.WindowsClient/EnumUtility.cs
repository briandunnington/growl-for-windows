using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Growl.WindowsClient
{
    public static class EnumUtility
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] mi = type.GetMember(en.ToString());
            if (mi != null && mi.Length > 0)
            {
                object[] attrs = mi[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public static Dictionary<string, Enum> GetValues(Type en)
        {
            Dictionary<string, Enum> list = new Dictionary<string, Enum>();
            Array values = Enum.GetValues(en);
            Array.Sort(values); //TODO: control sort order?
            for (int i = 0; i < values.Length; i++)
            {
                Enum x = (Enum) values.GetValue(i);
                list.Add(GetDescription(x), x);
            }
            return list;
        }
    }
}
