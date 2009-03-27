using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl
{
    [Serializable]
    public class DefaultablePreference
    {
        public const string DEFAULT_DISPLAY_LABEL = "[Default]";
        private string name;
        private bool isDefault;

        protected DefaultablePreference()
        {
        }

        protected DefaultablePreference(string name, bool isDefault)
        {
            this.name = name;
            this.isDefault = isDefault;
        }

        public static string DefaultDisplayLabel
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
            protected set
            {
                this.name = value;
            }
        }

        public virtual string ActualName
        {
            get
            {
                return this.name;
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
            protected set
            {
                this.isDefault = value;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
