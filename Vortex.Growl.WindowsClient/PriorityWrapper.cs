using System;
using System.Collections.Generic;
using System.Text;
using Vortex.Growl.AppBridge;
using Vortex.Growl.Framework;

namespace Vortex.Growl.WindowsClient
{
    [Serializable]
    public class PriorityWrapper : DefaultablePreference
    {
        public static PriorityWrapper Default;
        private static Dictionary<string, PriorityWrapper> all;
        private Priority? priority;

        static PriorityWrapper()
        {
            Default = new PriorityWrapper(null, DEFAULT_DISPLAY_LABEL, true);
        }

        private PriorityWrapper(Priority? priority, string name)
            : this(priority, name, false)
        {
        }

        private PriorityWrapper(Priority? priority, string name, bool isDefault)
        {
            this.priority = priority;
            this.name = name;
            this.isDefault = isDefault;
        }

        public Priority? Priority
        {
            get
            {
                return this.priority;
            }
        }

        public static PriorityWrapper[] GetList()
        {
            all = new Dictionary<string, PriorityWrapper>();
            Dictionary<string, Enum> values = EnumUtility.GetValues(typeof(Priority));
            foreach (KeyValuePair<string, Enum> pair in values)
            {
                all.Add(pair.Key, new PriorityWrapper((Priority)pair.Value, pair.Key));
            }

            PriorityWrapper[] arr = new PriorityWrapper[all.Count];
            all.Values.CopyTo(arr, 0);
            return arr;
        }

        public static PriorityWrapper GetByName(string name)
        {
            if (name != null && all != null && all.ContainsKey(name))
                return all[name];
            else
                return PriorityWrapper.Default;
        }

        public static PriorityWrapper GetByValue(Priority? priority)
        {
            string name = null;
            if(priority.HasValue) name = EnumUtility.GetDescription(priority);
            return GetByName(name);
        }
    }
}
