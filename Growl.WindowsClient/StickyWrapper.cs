using System;
using System.Collections.Generic;
using System.Text;
using Growl.AppBridge;
using Growl.Framework;

namespace Growl.WindowsClient
{
    [Serializable]
    public class StickyWrapper : DefaultablePreference
    {
        public static StickyWrapper Default;
        private static StickyWrapper True;
        private static StickyWrapper False;
        private bool? sticky;

        static StickyWrapper()
        {
            Default = new StickyWrapper(null, DEFAULT_DISPLAY_LABEL, true);
            True = new StickyWrapper(true, Boolean.TrueString);
            False = new StickyWrapper(false, Boolean.FalseString);
        }

        private StickyWrapper(bool? sticky, string name)
            : this(sticky, name, false)
        {
        }

        private StickyWrapper(bool? sticky, string name, bool isDefault)
        {
            this.sticky = sticky;
            this.name = name;
            this.isDefault = isDefault;
        }

        public bool? Sticky
        {
            get
            {
                return this.sticky;
            }
        }

        public static StickyWrapper[] GetList()
        {
            StickyWrapper[] arr = new StickyWrapper[2];
            arr[0] = True;
            arr[1] = False;
            return arr;
        }

        public static StickyWrapper GetByName(string name)
        {
            if (name == Boolean.TrueString)
                return True;
            else if (name == Boolean.FalseString)
                return False;
            else
                return StickyWrapper.Default;
        }

        public static StickyWrapper GetByValue(bool? sticky)
        {
            string name = null;
            if(sticky.HasValue) name = sticky.Value.ToString();
            return GetByName(name);
        }
    }
}
