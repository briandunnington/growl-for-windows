using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Vortex.Growl.AppBridge
{
    [Serializable]
    public class DefaultablePreference
    {
        public const string DEFAULT_DISPLAY_LABEL = "[Default]";
        protected string name;
        protected bool isDefault = false;

        public string DefaultDisplayLabel
        {
            get
            {
                return DEFAULT_DISPLAY_LABEL;
            }
        }

        public string Name
        {
            get
            {
                string s = this.name;
                if (this.isDefault) s = DEFAULT_DISPLAY_LABEL;
                return s;
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
