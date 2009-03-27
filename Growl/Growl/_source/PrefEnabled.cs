using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PrefEnabled    // NOTE: this class does *not* inherit from DefaultablePreference since it has no default setting
    {
        public static PrefEnabled True = new PrefEnabled(true, Boolean.TrueString);
        public static PrefEnabled False = new PrefEnabled(false, Boolean.FalseString);

        private string name;
        private bool enabled;

        private PrefEnabled(bool enabled, string name)
        {
            this.name = name;
            this.enabled = enabled;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }

        public static PrefEnabled[] GetList()
        {
            int i = 0;
            int c = 2;
            PrefEnabled[] arr = new PrefEnabled[c];
            arr[i++] = True;
            arr[i++] = False;
            return arr;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override int GetHashCode()
        {
            return this.enabled.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefEnabled e = obj as PrefEnabled;
            if (e != null)
            {
                return this.enabled == e.enabled;
            }
            else
                return base.Equals(obj);
        }

        private static PrefEnabled GetByName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                bool result;
                bool valid = bool.TryParse(name, out result);
                if (valid)
                {
                    return (result ? PrefEnabled.True : PrefEnabled.False);
                }
            }
            return PrefEnabled.False;
        }

        static public implicit operator PrefEnabled(bool val)
        {
            if (val)
                return PrefEnabled.True;
            else
                return PrefEnabled.False;
        }
    }
}