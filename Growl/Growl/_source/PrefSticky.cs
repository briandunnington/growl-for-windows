using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PrefSticky : DefaultablePreference, ISerializable
    {
        private const string NEVER_NAME = "Never";
        private const string ALWAYS_NAME = "Always";
        private const string WHENIDLE_NAME = "When Idle";

        public static PrefSticky Default = new PrefSticky(null, DEFAULT_DISPLAY_LABEL, true);
        private static PrefSticky Never = new PrefSticky(StickyPreferenceOption.Never, NEVER_NAME);
        private static PrefSticky Always = new PrefSticky(StickyPreferenceOption.Always, ALWAYS_NAME);
        private static PrefSticky WhenIdle = new PrefSticky(StickyPreferenceOption.WhenIdle, WHENIDLE_NAME);
        private StickyPreferenceOption? sticky;

        private PrefSticky(StickyPreferenceOption? sticky, string name)
            : this(sticky, name, false)
        {
        }

        private PrefSticky(StickyPreferenceOption? sticky, string name, bool isDefault)
            : base(name, isDefault)
        {
            this.sticky = sticky;
        }

        public bool? ShouldStayOnScreen(bool stayWhenIdle, bool isUserIdle, bool requested)
        {
            if (this.IsDefault)
                return requested;
            else
            {
                if(this.sticky == StickyPreferenceOption.Always ||
                    (this.sticky == StickyPreferenceOption.WhenIdle && isUserIdle))
                    return true;
                else
                    return false;
            }
        }

        public static PrefSticky[] GetList(bool allowDefault)
        {
            int i = 0;
            int c = 3;
            if (allowDefault) c++;
            PrefSticky[] arr = new PrefSticky[c];
            if (allowDefault)
            {
                arr[i++] = PrefSticky.Default;
            }
            arr[i++] = PrefSticky.Never;
            arr[i++] = PrefSticky.Always;
            arr[i++] = PrefSticky.WhenIdle;
            return arr;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefSticky s = obj as PrefSticky;
            if (s != null)
            {
                return this.sticky == s.sticky;
            }
            else
                return base.Equals(obj);
        }

        public enum StickyPreferenceOption
        {
            Never = 0,
            Always = 1,
            WhenIdle = 2
        }

        private static PrefSticky GetByValue(StickyPreferenceOption? spo)
        {
            if (spo == null)
                return PrefSticky.Default;

            switch (spo.Value)
            {
                case StickyPreferenceOption.Always :
                    return PrefSticky.Always;
                case StickyPreferenceOption.Never:
                    return PrefSticky.Never;
                case StickyPreferenceOption.WhenIdle:
                    return PrefSticky.WhenIdle;
                default :
                    return PrefSticky.Default;
            }
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(PrefStickySerializationHelper));
            info.AddValue("spo", this.sticky, typeof(StickyPreferenceOption?));
        }

        #endregion

        [Serializable]
        private class PrefStickySerializationHelper : IObjectReference
        {
            private StickyPreferenceOption? spo = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return PrefSticky.GetByValue(this.spo);
            }

            #endregion
        }
    }
}
