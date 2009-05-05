using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PrefDuration : DefaultablePreference
    {
        private static string SECONDS = Properties.Resources.LiteralString_Seconds;

        private static int[] options = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30 };

        public static PrefDuration Default = new PrefDuration(0, DEFAULT_DISPLAY_LABEL, true);
        private int duration;

        private PrefDuration(int duration)
            : this(duration, GetNameFromDuration(duration), false)
        {
        }

        private PrefDuration(int duration, string name, bool isDefault)
            : base(name, isDefault)
        {
            this.duration = duration;
        }

        public int Duration
        {
            get
            {
                return this.duration;
            }
        }

        public static PrefDuration[] GetList(bool allowDefault)
        {
            List<PrefDuration> list = new List<PrefDuration>();
            if (allowDefault) list.Add(Default);

            // read available choices from options list
            for(int i=0;i<options.Length;i++)
            {
                int s = options[i];
                PrefDuration pd = new PrefDuration(s);
                list.Add(pd);
            }

            PrefDuration[] arr = list.ToArray();
            return arr;
        }

        public override int GetHashCode()
        {
            return this.duration.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefDuration d = obj as PrefDuration;
            if (d != null)
            {
                return this.Duration == d.Duration;
            }
            else
                return base.Equals(obj);
        }

        private static string GetNameFromDuration(int duration)
        {
            return String.Format("{0} {1}", duration, SECONDS);
        }
    }
}
